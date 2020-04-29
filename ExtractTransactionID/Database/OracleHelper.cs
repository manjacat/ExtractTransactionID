using ExtractTransactionID.ConsoleHelper;
using ExtractTransactionID.Constant;
using NLog;
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
    public class OracleHelper
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        public string ConnectionString { get; set; }

        //public enum ServiceName
        //{
        //    BCRM_Update,
        //    Create_CPP,
        //    Create_SNC,
        //    Update_CPP,
        //    Update_SNC,
        //    ERMS_Update
        //}
        
        public DataTable QueryTable(string query, OracleParameter[] parameters = null)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            using (OracleConnection oraconn = new OracleConnection(ConnectionString))
            {
                try
                {
                    if (oraconn.State == ConnectionState.Open)
                        oraconn.Close();
                    oraconn.Open();
                    OracleCommand cmd = new OracleCommand(query, oraconn);
                    cmd.CommandType = CommandType.Text;
                    cmd.BindByName = true;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    PrintHelper.Trace(ex.Message);
                    PrintHelper.ErrorLog(string.Format(Messages.GetOracleQuery,query));
                    if(parameters != null)
                    {
                        PrintHelper.ErrorLog("Parameter List: ");
                        for(int i = 0; i < parameters.Length; i++)
                        {
                            PrintHelper.ErrorLog
                                (string.Format("{0} = {1}", 
                                parameters[i].ParameterName, 
                                parameters[i].Value));
                        }                       
                    }
                    PrintHelper.Error(ex.ToString());
                    throw;
                }
                finally
                {
                    oraconn.Close();
                }
            }
            //return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable(); 
            return dt;
        }

        public void ExecNonQuery(string query, OracleParameter[] parameters = null)
        {
            using (OracleConnection oraconn = new OracleConnection(ConnectionString))
            {
                try
                {
                    if (oraconn.State == ConnectionState.Open)
                        oraconn.Close();
                    oraconn.Open();
                    OracleCommand cmd = new OracleCommand(query, oraconn);
                    cmd.CommandType = CommandType.Text;
                    cmd.BindByName = true;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    int retVal = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    PrintHelper.Trace(ex.Message);
                    PrintHelper.ErrorLog(string.Format(Messages.GetOracleQuery, query));
                    if (parameters != null)
                    {
                        PrintHelper.ErrorLog("Parameter List below: ");
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            PrintHelper.ErrorLog
                                (string.Format("{0} = {1}",
                                parameters[i].ParameterName,
                                parameters[i].Value));
                        }
                    }
                    PrintHelper.Error(ex.ToString());
                    throw;
                }
                finally
                {
                    oraconn.Close();
                }
            }
        }
    }
}
