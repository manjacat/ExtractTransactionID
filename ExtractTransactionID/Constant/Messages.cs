using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTransactionID.Constant
{
    /// <summary>
    /// List of messages to be displayed on Console/Trace
    /// </summary>
    public static class Messages
    {
        public const string NewLine = "------------------------------";
        public const string StartProgram = "Application has started. ";
        public const string GetServiceName = "Transaction has started for {0}";
        public const string PleaseWait = "Please Wait....";
        public const string ReadFileError = "Unable to read file location at {0}. Please refer ERROR log for details";
        public const string ModifiedDateTime = "Last Modified Date was {0}";
        public const string CreatedDateTime = "Created Date was {0}";
        public const string RemoveDuplicates = "Removing any duplicate Transaction ID...";
        public const string ProgramError = "Application has encountered an error. Please refer ERROR log for details.";
        public const string GetCurrentDate = "Selected Date is {0:dd-MMM-yyyy}.";
        public const string InsertData = "New Data has been inserted.";
        public const string GetOracleQuery = "Query is : {0}";
        public const string InsertDataFail = "Fail to Insert New Data. Refer Stacktrace Below.";
        public const string ExcelError = "Failed To Generate Excel. Refer Stacktrace Below. ";
        public const string ExcelSuccess = "Successly Generate Excel. ";
        public const string EndProgram = "Application has been terminated. ";
        public const string GetFolderLocation = "Log Folder Location is: {0} ";
        public const string GetInboundServiceName = "InboundServiceName is: {0}";
        public const string ReadStart_Log = "Reading Log Files START. ";
        public const string ReadEnd_Log = "Reading Log Files END. ";
        public const string ReadTotal_Log = "Read Completed. {0} files found. ";
        public const string ReadStart_QS = "Reading START from PROD_SOAINFRA.WLI_QS_REPORT_ATTRIBUTE.";
        public const string ReadEnd_QS = "Reading END from PROD_SOAINFRA.WLI_QS_REPORT_ATTRIBUTE.";
        public const string TotalTransId_QS = "Total TransactionId from SOA is {0}";
        public const string TotalTransId_SW = "Total TransactionId from SW is {0}";
        public const string InsertSOA_TRANSACTION = "Insert to SWTNBGIS.SOA_TRANSACTION.";
        public const string TotalRowsInserted = "Total Rows Inserted: {0}.";
        public const string TransactionId_TRY = "Trying to Insert TransactionId: {0}.";
        public const string GetTransactionId = "TransactionId found: {0}.";
        public const string TransactionId_SUCCESS = "TransactionId: {0} has been inserted.";
        public const string InsertKSF_SYSTEM_SOA_FAIL_TRANS = "Insert to SWTNBGIS.KSF_SYSTEM_SOA_FAIL_TRANS.";
        public const string TotalDifference = "Total TransactionId to insert = {0}";
    }
}
