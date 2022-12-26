using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.Office.Tools.Ribbon;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using KPMG.Arfolyam.Excel.MNBArfolyamServiceSoapClient;
using System.Data.OleDb;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;

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
            ExchangeRate exchangeRate = new ExchangeRate(new MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient());

            DataTable dataTable = exchangeRate.GetExchangeRates("2017-03-11", "2017-03-27", 5);

            LoadExchangeRatesToExcelWorksheet(dataTable);

            LogActivity();

           
        }

        private void LoadExchangeRatesToExcelWorksheet(DataTable data)
        {
            Microsoft.Office.Tools.Excel.Worksheet worksheet =
                  Globals.Factory
                  .GetVstoObject(Globals.ThisAddIn.Application.ActiveWorkbook.Worksheets[1]);

            for (int n = 0; n < data.Columns.Count; n++)
            {
                worksheet.Cells[1, n + 1] = data.Columns[n].ColumnName;
            }

            for (int n = 0; n < data.Rows.Count; n++)
            {
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    worksheet.Cells[n + 2, j + 1] = data.Rows[n][j].ToString();
                }
            }
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
