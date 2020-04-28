using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTransactionID.Excel
{
    public class ExcelHelper
    {
        private static string ExcelOutputFolder
        {
            get
            {
                string folderPath = ConfigurationSettings.AppSettings["ExcelOutputFolder"].ToString();
                return folderPath;
            }
        }

        public static string ExportExcel(List<string> transactionIdList, List<string> soaTransactionList)
        {
            IWorkbook workbook = CreateExcel(transactionIdList, soaTransactionList);

            string filePathSettings = ExcelOutputFolder;
            string fileName = "ExcelReport_" + string.Format("{0:dd_MM_yy_hh_mm}", DateTime.Now) + ".xlsx";
            string filePath = filePathSettings + fileName;
            using (FileStream fs = File.Create(filePath))
            {
                MemoryStream ms = new MemoryStream();
                ms.Position = 0;
                workbook.Write(ms);
                byte[] data = ms.ToArray();
                fs.Write(data, 0, data.Length);
                ms.Close();
                fs.Close();
            }
            return fileName;
        }

        private static IWorkbook CreateExcel(List<string> swTransactionList, List<string> soaTransactionList)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("Sheet 1");

            int rowAdjust = 0;

            //create Header
            //IRow headerRow = sheet1.CreateRow(rowAdjust);
            //ICell headerCell0 = headerRow.CreateCell(1);
            //headerCell0.SetCellValue("List of TransactionID Log not in SOA: Total=" + swTransactionList.Count);
            rowAdjust++;

            //create table Header
            IRow thRow = sheet1.CreateRow(rowAdjust);
            ICell thCell1 = thRow.CreateCell(1);
            thCell1.SetCellValue("TransactionID");
            ICell thCell2 = thRow.CreateCell(2);
            thCell2.SetCellValue("In Database?");
            rowAdjust++;

            for (int i = 0; i < swTransactionList.Count; i++)
            {
                IRow nRow = sheet1.CreateRow(rowAdjust);
                string id = swTransactionList[i];
                //ICell nCell0 = nRow.CreateCell(0);
                //nCell0.SetCellValue("Transaction ID:");
                ICell nCell1 = nRow.CreateCell(1);
                nCell1.SetCellValue(id);
                ICell nCell2 = nRow.CreateCell(2);
                if (soaTransactionList.Contains(id))
                {
                    nCell2.SetCellValue("Yes");
                }
                else
                {
                    nCell2.SetCellValue("No");
                }
                
                rowAdjust++;
            }

            //rowAdjust = rowAdjust + 2;
            ////create Header for SOA Report
            //IRow headerRow2 = sheet1.CreateRow(rowAdjust);
            //ICell headerCell0_2 = headerRow2.CreateCell(1);
            //headerCell0_2.SetCellValue("List of TransactionID in SOA Database: Total=" + soaTransactionList.Count);
            //rowAdjust++;

            //for (int i = 0; i < soaTransactionList.Count; i++)
            //{
            //    IRow nRow = sheet1.CreateRow(rowAdjust);
            //    ICell nCell0 = nRow.CreateCell(0);
            //    nCell0.SetCellValue("Transaction ID:");
            //    ICell nCell1 = nRow.CreateCell(1);
            //    nCell1.SetCellValue(soaTransactionList[i]);
            //    rowAdjust++;
            //}
            sheet1.AutoSizeColumn(0);
            sheet1.AutoSizeColumn(1);
            return workbook;
        }
    }
}
