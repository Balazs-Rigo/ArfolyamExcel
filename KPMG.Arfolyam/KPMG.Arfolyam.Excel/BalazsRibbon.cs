using System;
using System.Data;
using Microsoft.Office.Tools.Ribbon;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;
using Microsoft.Office.Interop.Excel;
using System.Text;
using System.Diagnostics;

namespace KPMG.Arfolyam.Excel
{
    public partial class BalazsRibbon
    {
        OleDbConnection conn = null;
        private void BalazsRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            conn = new OleDbConnection($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\ExchangeRates.accdb");
        }

        private void btnGetExchangeUnits_Click(object sender, RibbonControlEventArgs e)
        {
            Stopwatch watch = new Stopwatch();

            watch.Start();

            ExchangeRate exchangeRate = new ExchangeRate(new MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient());

            System.Data.DataTable dataTable = exchangeRate.GetExchangeRates("2020-01-01", "2021-01-10", 14);

            LoadExchangeRatesToExcelWorksheet(dataTable);

            watch.Stop();

            TimeSpan ts = watch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

            //MessageBox.Show(elapsedTime);

            //LogActivity();

           
        }

        private void LoadExchangeRatesToExcelWorksheet(System.Data.DataTable data)
        {
            Microsoft.Office.Tools.Excel.Worksheet worksheet =
                  Globals.Factory
                  .GetVstoObject(Globals.ThisAddIn.Application.ActiveWorkbook.Worksheets[1]);

            for (int i = 0; i < data.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = data.Columns[i].ColumnName;
            }

            StringBuilder stringBuilder = new StringBuilder();
            double numberValue;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                for (int j = 0; j < data.Columns.Count; j++)
                {

                    var currentValue = stringBuilder.Append(data.Rows[i][j].ToString()).ToString();

                    if (string.IsNullOrEmpty(currentValue) && j != 1)
                    {
                        break;
                    }

                    bool isValueNumber = double.TryParse(currentValue.ToString(), out numberValue);

                    if (isValueNumber)
                    {
                        worksheet.Cells[i + 2, j + 1] = numberValue;

                    }
                    else
                    {
                        worksheet.Cells[i + 2, j + 1] = currentValue;
                    }                  
                    stringBuilder.Clear();
                }
            }

            //Range r = (Range)worksheet.Cells[4, 4];
            //r.EntireColumn.NumberFormat = "0.0";

            //Range d = (Range)worksheet.Cells[3, 1];
            //d.EntireColumn.NumberFormat = "YYYY.MMMM.DD";  // date


            //r.NumberFormat = "$0.00"; // currency
        }

        private void LogActivity()
        {
            var now = DateTime.Now;
            var username = Environment.UserName;

            conn.Open();

            OleDbCommand cmd = conn.CreateCommand();
            cmd.Parameters.Add("@now",OleDbType.Date).Value = now;
            cmd.Parameters.Add("@username", OleDbType.VarWChar).Value = username;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"insert into ExchangeRateAccessDB (UserName, Indoklas, Timestamp)" +
                $" values (@username,'null', @now)";
            cmd.ExecuteNonQuery();

        }
    }
}
