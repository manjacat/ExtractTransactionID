using ExtractTransactionID.ConsoleHelper;
using ExtractTransactionID.Constant;
using ExtractTransactionID.Reader;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTransactionID.Database
{
    /// <summary>
    /// Connection to SWTNB (Oracle)
    /// </summary>
    public class SWTNBHelper : OracleHelper
    {
        public SWTNBHelper()
        {
            //connection to SWTNB
            ConnectionString = ConfigurationManager.AppSettings["SWTNBconnection"].ToString();
        }

        public int InsertTransLog(List<FileInfo> logFiles, ServiceHelper sh)
        {
            string query = string.Empty;
            int total = 0;
            foreach (FileInfo file in logFiles)
            {
                DateTime createDate = file.CreationTime;
                DateTime modifiedDate = file.LastWriteTime;
                string id = ReadHelper.GetTransactionId(file);

                //if(sh.ServiceName == "UpdateAssetAttribService")
                //{
                //    PrintHelper.Trace("UpdateAssetAttribService: Transaction ID is " + id);
                //}

                if (id != null || id != string.Empty)
                {
                    OracleParameter[] parameters = new OracleParameter[]
                    {
                        new OracleParameter("SERVICE_NAME", sh.ServiceName),
                        new OracleParameter("TRANSID", id),
                        new OracleParameter("TRANSTIME", modifiedDate)
                    };
                    query = string.Format("INSERT into SOA_TRANSACTION (ID, SERVICE, TRANSACTION_ID, TRANSACTION_TIME)" +
                   "(SELECT SQ_SOA_TRANSACTION.NEXTVAL ID, " +
                   ":SERVICE_NAME SERVICE, " +
                   ":TRANSID TRANSACTION_ID, " +
                   ":TRANSTIME TRANSACTION_TIME " +
                   "FROM DUAL)");
                    ExecNonQuery(query, parameters);
                    PrintHelper.Trace(string.Format(Messages.TransactionId_SUCCESS, id));
                    total++;
                }
            }
            return total;
        }

        public List<string> GetTransId(DateTime inputDate, ServiceHelper sh)
        {
            string query = string.Empty;
            DateTime startDate = new DateTime(inputDate.Year, inputDate.Month, inputDate.Day);
            DateTime endDate = startDate.AddDays(1);
            OracleParameter[] parameters = new OracleParameter[]
            {
                new OracleParameter("SERVICE_NAME", sh.ServiceName),
                new OracleParameter("STARTDATE", startDate),
                new OracleParameter("ENDDATE", endDate)
            };
            query = string.Format("SELECT TRANSACTION_ID from SOA_TRANSACTION WHERE 1=1 "
           + "AND SERVICE = :SERVICE_NAME "
           + "AND TRANSACTION_TIME >= :STARTDATE "
           + "AND TRANSACTION_TIME < :ENDDATE "
           + "AND TRANSACTION_ID IS NOT NULL");
            PrintHelper.TraceQuery(string.Format(Messages.GetOracleQuery, query));
            DataTable dt = QueryTable(query, parameters);

            List<string> retList = new List<string>();
            foreach(DataRow dr in dt.Rows)
            {
                string transId = dr["TRANSACTION_ID"].ToString();
                PrintHelper.Trace(string.Format(Messages.TransactionId_SUCCESS, transId));
                retList.Add(transId);
            }
            PrintHelper.Trace(Messages.ReadEnd_Log);
            return retList;
        }


        public void DeleteTransLog(DateTime selDate, ServiceHelper sh)
        {
            DateTime startDate = selDate;
            DateTime endDate = selDate.AddDays(1);
            OracleParameter[] parameters = new OracleParameter[]
            {
                new OracleParameter("SERVICE_NAME", sh.ServiceName),
                new OracleParameter("STARTDATE", startDate),
                new OracleParameter("ENDDATE", endDate)
            };
            string query = string.Format("DELETE FROM SOA_TRANSACTION WHERE 1=1 "
                + "AND SERVICE = :SERVICE_NAME "
                + "AND TRANSACTION_TIME >= :STARTDATE "
                + "AND TRANSACTION_TIME < :ENDDATE "
                );
            ExecNonQuery(query, parameters);
        }

        public void DeleteFailTrans(DateTime selDate, ServiceHelper sh, DateTime startDate)
        {
            DateTime endDate = startDate.AddDays(1);
            OracleParameter[] parameters = new OracleParameter[]
            {
                new OracleParameter("SERVICE_NAME", sh.ServiceName),
                new OracleParameter("STARTDATE", startDate),
                new OracleParameter("ENDDATE", endDate)
            };
            string query = string.Format("DELETE FROM KSF_SYSTEM_SOA_FAIL_TRANS WHERE 1=1 " +
                "AND SERVICE = :SERVICE_NAME " +
                "AND RECEIVED_DATE >= :STARTDATE " +
                "AND RECEIVED_DATE < :ENDDATE");
            //PrintHelper.TraceQuery("ServiceName is " + sh.ServiceName);
            //PrintHelper.TraceQuery("StartDate is " + startDate.ToShortDateString());
            //PrintHelper.TraceQuery("EndDate is " + endDate.ToShortDateString());
            //PrintHelper.TraceQuery(string.Format(Messages.GetOracleQuery, query));
            ExecNonQuery(query, parameters);
        }

        public int InsertFailTrans(IEnumerable<string> difference, DateTime selDate, ServiceHelper sh)
        {
           
            string query = string.Empty;
            int total = 0;
            foreach (string id in difference)
            {
                if (id != null)
                {
                    //PrintHelper.Trace(string.Format(Messages.InsertTransactionId_TRY, id));
                    OracleParameter[] parameters = new OracleParameter[]
                    {
                        new OracleParameter("SERVICE_NAME", sh.ServiceName),
                        new OracleParameter("TRANS_ID", id),
                        new OracleParameter("REC_DATE", selDate)
                    };
                    query = string.Format("INSERT into KSF_SYSTEM_SOA_FAIL_TRANS "
                   + "(ID, SERVICE, RECEIVED_DATE, TRANSACTION_ID) "
                   + "(SELECT KSF_SYSTEM_SOA_FAIL_TRANS_SEQ.NEXTVAL ID, "
                   + ":SERVICE_NAME SERVICE, "
                   + ":REC_DATE RECEIVED_DATE, "
                   + ":TRANS_ID TRANSACTION_ID "
                   + "FROM DUAL)");
                    try
                    {
                        ExecNonQuery(query, parameters);
                        PrintHelper.Trace(string.Format(Messages.TransactionId_SUCCESS, id));
                    }
                    catch
                    {
                        PrintHelper.Trace(Messages.InsertDataFail);
                    }
                    total++;
                }
            }
            return total;
        }
    }
}
