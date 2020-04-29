using ExtractTransactionID.ConsoleHelper;
using ExtractTransactionID.Constant;
using ExtractTransactionID.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ExtractTransactionID.Database.OracleHelper;

/// <summary>
/// This code handles reading log files from Smallworld
/// </summary>
namespace ExtractTransactionID.Reader
{
    public class ReadHelper
    {
        private static string ServerPath1
        {
            get
            {
                string folderPath = ConfigurationManager.AppSettings["ServerPath1"].ToString();
                return folderPath;
            }
        }

        private static string ServerPath2
        {
            get
            {
                string folderPath = ConfigurationManager.AppSettings["ServerPath2"].ToString();
                return folderPath;
            }
        }

        private static string ServerPath3
        {
            get
            {
                string folderPath = ConfigurationManager.AppSettings["ServerPath3"].ToString();
                return folderPath;
            }
        }

        private static string GetTransactionLogFolderPath(ServiceHelper sh, int serverNumber)
        {
            string folderName = string.Empty;
            string serverPath = string.Empty;
            switch (serverNumber)
            {
                case 3:
                    serverPath = ServerPath3;
                    break;
                case 2:
                    serverPath = ServerPath2;
                    break;
                case 1:
                default:
                    serverPath = ServerPath1;
                    break;
            }

            string folderPath = serverPath + sh.FolderName + "\\";
            return folderPath;

        }

        public static List<FileInfo> Read_SW_Logs(DateTime selDate, ServiceHelper sh)
        {
            PrintHelper.Trace(Messages.ReadStart_Log);

            List<FileInfo> filter1 = new List<FileInfo>();
            //get fileInfo from all the servers
            List<FileInfo> fileInfo1 = ReadSW_FileLogs_ByServerName(selDate, sh, 1);
            if (fileInfo1.Count > 0)
            {
                filter1.AddRange(fileInfo1);
            }

            List<FileInfo> fileInfo2 = ReadSW_FileLogs_ByServerName(selDate, sh, 2);
            if (fileInfo2.Count > 0)
            {
                filter1.AddRange(fileInfo2);
            }

            List<FileInfo> fileInfo3 = ReadSW_FileLogs_ByServerName(selDate, sh, 3);
            if (fileInfo3.Count > 0)
            {
                filter1.AddRange(fileInfo3);
            };

            //remove duplicates
            PrintHelper.Trace(Messages.RemoveDuplicates);
            filter1 = filter1.Distinct().ToList();
            PrintHelper.Trace(string.Format(Messages.ReadTotal_Log, filter1.Count().ToString()));

            return filter1;
        }

        private static List<FileInfo> ReadSW_FileLogs_ByServerName(DateTime selDate, ServiceHelper sh, int serverNumber)
        {
            List<FileInfo> filter1 = new List<FileInfo>();
            DateTime newDate = new DateTime(selDate.Year, selDate.Month, selDate.Day);
            //get path from servers
            string path = GetTransactionLogFolderPath(sh, serverNumber);
            try
            {
                PrintHelper.Trace(string.Format(Messages.GetFolderLocation, path));
                PrintHelper.Trace(Messages.PleaseWait);
                //get list of fileNames from the path provided
                List<string> files = Directory.GetFiles(path).ToList();

                //use foreach loop to convert files to fileInfo
                List<FileInfo> allFiles = new List<FileInfo>();
                foreach (string filePath in files)
                {
                    FileInfo file = new FileInfo(filePath);
                    allFiles.Add(file);
                }

                if(allFiles.Count > 0)
                {
                    //filter which files to take based on Modified date
                    filter1 = allFiles
                        .Where(s => s.LastWriteTime.Date == newDate.Date
                        && s.LastWriteTime.Month == newDate.Month
                        && s.LastWriteTime.Year == newDate.Year).ToList();
                }
                PrintHelper.Trace(string.Format(Messages.ModifiedDateTime, newDate.ToShortDateString()));
                PrintHelper.Trace(string.Format(Messages.ReadTotal_Log, filter1.Count.ToString()));
            }
            catch(Exception ex)
            {
                PrintHelper.Error(string.Format(Messages.ReadFileError, path));
                PrintHelper.Error(ex.ToString());
            }
            return filter1;
        }

        public static List<string> Read_QS_TransactionId(DateTime selDate, ServiceHelper sh)
        {
            PrintHelper.Trace(Messages.PleaseWait);
            DateTime newDate = new DateTime(selDate.Year, selDate.Month, selDate.Day);
            DateTime startDate = newDate.AddDays(0);
            DateTime endDate = newDate.AddDays(1);
            SOAHelper soa = new SOAHelper();
            DataTable dt = soa.Read_QS_Report(startDate, endDate, sh);
            List<string> output = new List<string>();
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string transactionId = dr["MSG_LABELS"].ToString();
                    if (transactionId.Contains("OutboundRequestSent-TransactionID="))
                    {
                        transactionId = transactionId.Replace("OutboundRequestSent-TransactionID=", "");
                    }
                    if (transactionId.Contains("OutboundRequestSend-TransactionID="))
                    {
                        transactionId = transactionId.Replace("OutboundRequestSend-TransactionID=", "");
                    }
                    if (transactionId.Contains("InboundRequestReceived-TransactionID="))
                    {
                        transactionId = transactionId.Replace("InboundRequestReceived-TransactionID=", "");
                    }
                    if (transactionId.Contains("\n"))
                    {
                        transactionId = transactionId.Replace("\n", "");
                    }
                    if (transactionId.Contains("\r"))
                    {
                        transactionId = transactionId.Replace("\r", "");
                    }
                    output.Add(transactionId.Trim());
                }
            }

            //remove duplicates
            output = output.Distinct().ToList();
            return output;
        }

        public static string GetTransactionId(FileInfo file)
        {
            try
            {
                //read logfiles as text
                string txtFile = File.ReadAllText(file.FullName);

                //look for the word "Transaction" and get the transactionId            
                string[] txtArray = txtFile.Split(' ');
                string transactionId = string.Empty;
                for (int i = 0; i < txtArray.Length; i++)
                {
                    string outString = txtArray[i];
                    if (outString.ToLower().Contains("transaction"))
                    {
                        // Transaction ID : XXXXXXXXXXXXXXXX
                        // Transaction ID : 221119020153251
                        // Transaction ID : 910919040027358
                        transactionId = txtArray[i + 3];
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(transactionId))
                {
                    //remove any possible Line Feed (\n) or Carriage Return (\r) inside transaction ID
                    transactionId = transactionId.Replace("\r\nINFO", "");
                    transactionId = transactionId.Replace("\r\nWARN", "");
                    transactionId = transactionId.Replace("\nINFO", "");
                    transactionId = transactionId.Replace("\nWARN", "");
                    transactionId = transactionId.Replace("\r", "");
                    transactionId = transactionId.Replace("\n", "");
                }
                return transactionId;
            }
            catch (Exception ex)
            {
                PrintHelper.Error(ex.ToString());
                return string.Empty;
            }
        }

        public static List<string> Read_SW_Transaction_ID(List<FileInfo> logFiles)
        {
            PrintHelper.Trace(Messages.PleaseWait);
            List<string> transactionIdList = new List<string>();

            foreach (FileInfo file in logFiles)
            {
                string id = GetTransactionId(file);
                if(id != null)
                {
                    transactionIdList.Add(id);
                }
            }
            //return the transactionId list for Export to Excel/Insert to Oracle DB
            return transactionIdList;
        }

        public static List<string> Read_SW_Transaction_ID(DateTime selDate, ServiceHelper sh)
        {
            SWTNBHelper tnbDB = new SWTNBHelper();
            List<string> transactionIdList = tnbDB.GetTransId(selDate, sh);

            return transactionIdList;
        }
    }
}
