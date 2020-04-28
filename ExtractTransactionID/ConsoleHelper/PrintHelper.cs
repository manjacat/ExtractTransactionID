using ExtractTransactionID.Constant;
using ExtractTransactionID.Reader;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTransactionID.ConsoleHelper
{
    /// <summary>
    /// related to Printing process to a console, or to a Trace Log (uses NLog)
    /// Not related to main program, only useful when debugging or checking trace
    /// </summary>
    public class PrintHelper
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// print list of transaction ID from 
        /// </summary>
        /// <param name="selDate"></param>
        /// <param name="serviceName"></param>
        public static void Print_QS_TransactionId(DateTime selDate, 
            ServiceHelper sh)
        {
            List<string> transactionList = ReadHelper.Read_QS_TransactionId(selDate, sh);
            if (transactionList.Count > 0)
            {
                foreach (string id in transactionList)
                {
                    Console.WriteLine("QS ID is : {0}", id);
                }
            }
            Console.WriteLine("Total is {0}", transactionList.Count);
            Console.ReadLine();
        }

        /// <summary>
        /// Print List of Transaction ID from Smallworld
        /// </summary>
        /// <param name="selDate"></param>
        /// <param name="serviceName"></param>
        private static void Print_SW_TransactionId(DateTime selDate,
            ServiceHelper sh)
        {
            List<string> transactionList = ReadHelper.Read_SW_Transaction_ID(selDate,
                sh);
            if (transactionList.Count > 0)
            {
                foreach (string id in transactionList)
                {
                    Console.WriteLine("Transaction ID: {0}", id);
                }
            }
            int total = transactionList.Count();
            Console.WriteLine("{0} file(s) found.", total.ToString());
            Console.ReadLine();
        }

        public static void Print(string message, LogLevel lgLevel = null)
        {
            Console.WriteLine(message);
            if(lgLevel == null)
            {
                lgLevel = LogLevel.Trace;
            }
            //no need to show "Please Wait" in log
            if(message != Messages.PleaseWait)
            {
                logger.Log(lgLevel, message);
            }
        }

        public static void Error(string message)
        {
            //show error message in console
            Trace(Messages.ProgramError);
            //hide stacktrace from console message.
            logger.Error(message);            
        }

        public static void ErrorLog(string message)
        {
            //this will hide Query displayed to the console. only appear in log
            logger.Error(message);
        }

        public static void Trace(string message)
        {
            Print(message, LogLevel.Trace);
        }

        public static void TraceQuery(string message)
        {
            //this will hide Query displayed to the console. only appear in log
            logger.Trace(message);
        }

        public static void Info(string message)
        {
            Print(message, LogLevel.Info);
        }
    }

    
}
