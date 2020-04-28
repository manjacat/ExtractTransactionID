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
        private static string ServerPath
        {
            get
            {
                string folderPath = ConfigurationManager.AppSettings["ServerPath"].ToString();
                return folderPath;
            }
        }

        private static string GetTransactionLogFolderPath(ServiceHelper sh)
        {
            string folderName = string.Empty;
            string folderPath = ServerPath + sh.FolderName + "\\";
            return folderPath;

        }

        public static List<FileInfo> Read_SW_Logs(DateTime selDate, ServiceHelper sh)
        {
            DateTime newDate = new DateTime(selDate.Year, selDate.Month, selDate.Day);
            PrintHelper.Trace(Messages.ReadStart_Log);
            string path = GetTransactionLogFolderPath(sh);
            PrintHelper.Trace(string.Format(Messages.GetFolderLocation, path));
            PrintHelper.Trace(Messages.PleaseWait);
            //get list of fileNames
            List<string> files = Directory.GetFiles(path).ToList();
            //convert files to fileInfo
            List<FileInfo> allFiles = new List<FileInfo>();
            foreach (string filePath in files)
            {
                FileInfo file = new FileInfo(filePath);
                allFiles.Add(file);
            }

            //filter which files to take based on Created date
            List<FileInfo> filter1 = allFiles
                .Where(s => s.LastWriteTime.Date == newDate.Date 
                && s.LastWriteTime.Month == newDate.Month
                && s.LastWriteTime.Year == newDate.Year).ToList();
            PrintHelper.Trace(string.Format(Messages.ReadTotal_Log, filter1.Count.ToString()));
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
            return output;
        }

        public static string GetTransactionId(FileInfo file)
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
                transactionId = transactionId.Replace("\r\nINFO", "");
                transactionId = transactionId.Replace("\r\nWARN", "");
            }
            return transactionId;
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
