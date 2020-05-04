using ExtractTransactionID.ConsoleHelper;
using ExtractTransactionID.Constant;
using ExtractTransactionID.Database;
using ExtractTransactionID.Excel;
using ExtractTransactionID.Reader;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTransactionID
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static DateTime inputDate
        {
            get
            {
                //testing only. change this to datetime.now
                //BCRM - 10.9.2019
                //CreateCPP - 22.11.2019
                //CreateSNC - 21.11.2019
                //UpdateCPP - 1.9
                //UpdateSNC - 11.11
                //ERMS - 14.9
                string _inputDate = ConfigurationManager.AppSettings["inputDate"].ToString();
                DateTime currDate = DateTime.Now.AddDays(-1);
                if (_inputDate.ToUpper() != "TODAY")
                {
                    try
                    {
                        currDate = DateTime.ParseExact(_inputDate, "dd-MM-yyyy",
                        CultureInfo.InvariantCulture);
                        currDate = currDate.AddDays(-1);
                    }
                    catch
                    {
                        currDate = DateTime.Now.AddDays(-1);
                    }
                }
                return currDate;
                //return DateTime.Now.AddMonths(-1);
            }
        }

        /// <summary>
        /// Main entry of the program. to debug and see what this program do, start here.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            PrintHelper.Trace(Messages.NewLine);
            PrintHelper.Trace(Messages.StartProgram);
            PrintHelper.Trace(string.Format(Messages.GetCurrentDate, inputDate));
            // get logs for all 6 types

            //TestProgramAll();
            RunProgramAll();

            PrintHelper.Trace(Messages.EndProgram);
            //Console.Read();
        }

        private static void TestProgramAll()
        {
            TestTransactionId(ServiceCode.ERMS_Update);
            TestTransactionId(ServiceCode.BCRM_Update);
            TestTransactionId(ServiceCode.Create_CPP);
            TestTransactionId(ServiceCode.Create_SNC);
            TestTransactionId(ServiceCode.Update_CPP);
            TestTransactionId(ServiceCode.Update_SNC);
            Console.ReadLine();
        }

        private static void RunProgramAll()
        {
            StartProgram(ServiceCode.ERMS_Update);
            StartProgram(ServiceCode.ERMS_Update);
            StartProgram(ServiceCode.BCRM_Update);
            StartProgram(ServiceCode.Create_CPP);
            StartProgram(ServiceCode.Create_SNC);
            StartProgram(ServiceCode.Update_CPP);
            StartProgram(ServiceCode.Update_SNC);
        }

        /// <summary>
        /// test locally without saving to database
        /// for debugging purposes only: call this method in Main()
        /// </summary>
        /// <param name="myService">Service Code</param>
        private static void TestTransactionId(ServiceCode myService)
        {
            PrintHelper.Trace(string.Format(Messages.GetServiceName, myService.ToString()));
            ServiceHelper sh = new ServiceHelper(myService);
            List<FileInfo> logFiles = ReadHelper.Read_SW_Logs(inputDate, sh);

            foreach (FileInfo file in logFiles)
            {
                DateTime createDate = file.CreationTime;
                DateTime modifiedDate = file.LastWriteTime;
                string id = ReadHelper.GetTransactionId_StreamReader(file);
                PrintHelper.Trace(string.Format("the output transaction ID is {0}", id));
            }
        }

        /// <summary>
        /// Main will call this function. Start Here
        /// </summary>
        /// <param name="myService"></param>
        private static void StartProgram(ServiceCode myService)
        {
            PrintHelper.Trace(string.Format(Messages.GetServiceName, myService.ToString()));
            try
            {
                ServiceHelper sh = new ServiceHelper(myService);

                //SWTNBHelper sWTNB = new SWTNBHelper();
                //List<string> retString = sWTNB.GetTransId(inputDate, sh);

                //get logfiles from folder based on date and insert data to SOA Transaction
                List<FileInfo> logFiles = ReadHelper.Read_SW_Logs(inputDate, sh);
                Insert_SOA_Transaction(logFiles, sh);

                //Compare with QS Report and save difference in KSF_SYSTEM_SOA_FAIL_TRANS
                List<string> soaTransactionList = ReadHelper.Read_QS_TransactionId(inputDate, sh);
                List<string> swTransactionList = ReadHelper.Read_SW_Transaction_ID(logFiles);

                PrintHelper.Trace(Messages.RemoveDuplicates);
                soaTransactionList = soaTransactionList.Distinct().ToList();
                swTransactionList = swTransactionList.Distinct().ToList();
                
                PrintHelper.Trace(string.Format(Messages.TotalTransId_SW, swTransactionList.Count));
                PrintHelper.Trace(string.Format(Messages.TotalTransId_QS, soaTransactionList.Count));

                //this will insert the difference into KSF_SYSTEM_SOA_FAIL_TRANS table
                //var difference = soaTransactionList.Except(swTransactionList);
                var difference = swTransactionList.Except(soaTransactionList);
                if(difference.Count() == 0)
                {
                    PrintHelper.Trace(string.Format(Messages.TotalDifferenceZero, difference.Count().ToString()));
                }
                else
                {
                    PrintHelper.Trace(string.Format(Messages.TotalDifference, difference.Count().ToString()));
                    Insert_SOA_Fail_Trans(difference, sh, inputDate);
                }
            }
            catch (Exception ex)
            {
                PrintHelper.Error(ex.Message);
                PrintHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// this will insert the difference into KSF_SYSTEM_SOA_FAIL_TRANS table
        /// </summary>
        /// <param name="difference">List string of Transaction Id </param>
        /// <param name="sh">Instance of Service Helper</param>
        /// <param name="inputDate">selected Date</param>
        private static void Insert_SOA_Fail_Trans(IEnumerable<string> difference, ServiceHelper sh, DateTime inputDate)
        {
            PrintHelper.Trace(Messages.InsertKSF_SYSTEM_SOA_FAIL_TRANS);
            SWTNBHelper sw = new SWTNBHelper();
            if (difference.Count() > 0)
            {
                try
                {
                    //sw.DeleteFailTrans(DateTime.Now, sh, inputDate);
                    int totalRows = sw.Insert_SOAFailTrans(difference, inputDate, sh);
                    PrintHelper.Trace(string.Format(Messages.TotalRowsInserted,
                        totalRows.ToString()));
                }
                catch (Exception ex)
                {
                    PrintHelper.Error(Messages.InsertDataFail);
                    PrintHelper.Error(ex.ToString());
                }
            }
        }

        /// <summary>
        /// This will insert the transaction ID from Logs into SOA_TRANSACTION table
        /// </summary>
        /// <param name="logFiles">List of Log files from SW folder</param>
        /// <param name="sh">Instance of Service Helper</param>
        private static void Insert_SOA_Transaction(List<FileInfo> logFiles, ServiceHelper sh)
        {
            PrintHelper.Trace(Messages.InsertSOA_TRANSACTION);
            SWTNBHelper sw = new SWTNBHelper();
            if (logFiles.Count > 0)
            {
                try
                {
                    //sw.DeleteTransLog(inputDate, sh);
                    int totalRows = sw.Insert_Soa_Trans(logFiles.ToList(), sh);
                    PrintHelper.Trace(string.Format(Messages.TotalRowsInserted, totalRows));
                }
                catch (Exception ex)
                {
                    PrintHelper.Error(Messages.InsertDataFail);
                    PrintHelper.Error(ex.ToString());
                }
            }
        }

        //not used.
        private static void GenerateExcel(List<string> swTransactionList, List<string> soaTransactionList)
        {
            //Generate Excel
            Console.WriteLine("Generating Excel");
            try
            {
                string fileName = ExcelHelper.ExportExcel(swTransactionList, soaTransactionList);
                logger.Log(LogLevel.Trace, Messages.ExcelSuccess + fileName);
            }
            catch (Exception ex)
            {
                PrintHelper.Error(Messages.ExcelError);
                PrintHelper.Error(ex.ToString());
            }
        }

    }
}
