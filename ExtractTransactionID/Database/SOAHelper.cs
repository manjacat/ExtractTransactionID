using ExtractTransactionID.ConsoleHelper;
using ExtractTransactionID.Constant;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTransactionID.Database
{
    /// <summary>
    /// Connection to PRODSOAINFRA (Oracle)
    /// Not Used
    /// </summary>
    public class SOAHelper : OracleHelper
    {

        public SOAHelper()
        {
            //Connection to SOA
            ConnectionString = ConfigurationManager.AppSettings["SOAconnection"].ToString();
        }

        public DataTable Read_QS_Report(DateTime startDate, DateTime endDate, ServiceHelper sh)
        {
            PrintHelper.Trace(Messages.ReadStart_QS);
            OracleParameter[] parameters = new OracleParameter[]
            {
                new OracleParameter("STARTDATE", startDate),
                new OracleParameter("ENDDATE", endDate)
            };

            //change parameter based on Service Name
            string inboundServiceName = sh.InboundServiceName;
            PrintHelper.Trace(string.Format(Messages.GetInboundServiceName, inboundServiceName));

            string query = "SELECT " +
                //" a.DB_TIMESTAMP, a.LOCALHOST_TIMESTAMP, a.HOST_NAME, a.STATE, " +
                //" SUBSTR(a.MSG_LABELS,35,15) AS \"MSG_LABELS\" " +
                "a.MSG_LABELS "
                + "FROM PROD_SOAINFRA.WLI_QS_REPORT_ATTRIBUTE a "
                //+ ", PROD_SOAINFRA.WLI_QS_REPORT_DATA b " +
                + "WHERE a.INBOUND_SERVICE_NAME like '"
                + inboundServiceName + "' "
                + "AND a.MSG_LABELS like 'OutboundRequest%TransactionID=%' "
                //+ "AND a.MSG_LABELS like '%Request%TransactionID=%' "
                + "AND a.LOCALHOST_TIMESTAMP >= :STARTDATE "
                + "AND a.LOCALHOST_TIMESTAMP < :ENDDATE "
                // + "AND a.MSG_GUID = b.MSG_GUID"
                ;
            PrintHelper.TraceQuery(string.Format(Messages.GetOracleQuery, query));
            DataTable dt = QueryTable(query, parameters);
            PrintHelper.Trace(Messages.ReadEnd_QS);
            return dt;
        }
    }
}
