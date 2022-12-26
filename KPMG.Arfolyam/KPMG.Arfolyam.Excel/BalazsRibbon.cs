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

namespace KPMG.Arfolyam.Excel
{
    public partial class BalazsRibbon
    {
        OleDbConnection conn = null;
        private void BalazsRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            conn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=I:\IT\Coding\gitHubRepo\KPMG\KPMG.Arfolyam\KPMG.Arfolyam.Excel\ExchangeRates.accdb");
        }

        private void btnGetExchangeUnits_Click(object sender, RibbonControlEventArgs e)
        {

            ExchangeRate exchangeRate1 = new ExchangeRate(new MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient());



            
            #region full program
            using (MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient client = new MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient())
            {
                client.Open();

                var currenciesRequestBody = new GetCurrenciesRequestBody();

                var currencyUnitsRequestBody = new GetCurrencyUnitsRequestBody();

                var exchangeRatesRequestBody = new GetExchangeRatesRequestBody();

                var currencies = client.GetCurrencies(currenciesRequestBody);

                #region get currencies to list
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(currencies.GetCurrenciesResult);

                List<string> currencyList = new List<string>();
                XmlNodeList xnList = xml.SelectNodes("/MNBCurrencies/Currencies/Curr");
                foreach (XmlNode xn in xnList)
                {
                    currencyList.Add(xn.InnerText);
                }
                #endregion

                currencyUnitsRequestBody.currencyNames = string.Join(",", currencyList);

                var currencyUnits = client.GetCurrencyUnits(currencyUnitsRequestBody);

                //System.Data.DataTable exchangeRatesTable = new System.Data.DataTable();
                //exchangeRatesTable.Columns.Add("Datum", typeof(string));

                //foreach (var currency in currencyList)
                //{
                //    exchangeRatesTable.Columns.Add(currency, typeof(string));
                //}

                exchangeRatesRequestBody.startDate = "2015-01-01";
                exchangeRatesRequestBody.endDate = "2015-01-06";
                exchangeRatesRequestBody.currencyNames = string.Join(",", currencyList.Take(15));

                var exchangeRates = client.GetExchangeRates(exchangeRatesRequestBody);

                string currenciesXMLString = XElement.Parse(currencies.GetCurrenciesResult).ToString();

                Console.WriteLine("currencies: ");
                Console.WriteLine();
                Console.WriteLine(currenciesXMLString);
                Console.WriteLine();

                string currencyUnitsXMLString = XElement.Parse(currencyUnits.GetCurrencyUnitsResult).ToString();
                Console.WriteLine("currency units : ");
                Console.WriteLine();
                Console.WriteLine(currencyUnitsXMLString);
                Console.WriteLine();

                #region get exchange rates into list
                List<ExchangeRateDailyModel> exchangeRateDailyModels = new List<ExchangeRateDailyModel>();

                XmlReader rdr = XmlReader.Create(new StringReader(exchangeRates.GetExchangeRatesResult));
                ExchangeRateDailyModel exchangeRateDailyModel = null;
                ExchangeRateModel exchangeRateModel = null;
                int i = 0;
                while (rdr.Read())
                {

                    if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == "Day")
                    {
                        exchangeRateDailyModel = new ExchangeRateDailyModel();
                        exchangeRateDailyModel.Date = rdr.GetAttribute("date");
                    }

                    if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == "Rate")
                    {
                        exchangeRateModel = new ExchangeRateModel();
                        exchangeRateModel.Currency = rdr.GetAttribute("curr");
                    }

                    if (rdr.NodeType == XmlNodeType.Text)
                    {
                        exchangeRateModel.ExchangeRate = rdr.Value;
                    }

                    if (rdr.NodeType == XmlNodeType.EndElement && rdr.LocalName == "Rate")
                    {
                        ExchangeRateModel finalExchangeRateModel = new ExchangeRateModel();
                        finalExchangeRateModel.Currency = exchangeRateModel.Currency;
                        finalExchangeRateModel.ExchangeRate = exchangeRateModel.ExchangeRate;
                        exchangeRateDailyModel.ExchangeRate.Add(finalExchangeRateModel);
                        i++;
                    }

                    if (rdr.NodeType == XmlNodeType.EndElement && rdr.LocalName == "Day")
                    {
                        exchangeRateDailyModels.Add(exchangeRateDailyModel);
                        i = 0;
                    }
                }

                #endregion

                #region get currency untis into list
                List<CurrencyUnitModel> currencyUnitsList = new List<CurrencyUnitModel>();

                rdr = XmlReader.Create(new StringReader(currencyUnits.GetCurrencyUnitsResult));
                CurrencyUnitModel currencyUnitModel = null;
                while (rdr.Read())
                {

                    if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == "Unit")
                    {
                        currencyUnitModel = new CurrencyUnitModel();
                        currencyUnitModel.Currency = rdr.GetAttribute("curr");
                    }

                    if (rdr.NodeType == XmlNodeType.Text)
                    {
                        currencyUnitModel.Unit = rdr.Value;
                    }

                    if (rdr.NodeType == XmlNodeType.EndElement && rdr.LocalName == "Unit")
                    {
                        CurrencyUnitModel finalCurrencyUnitModel = new CurrencyUnitModel();
                        finalCurrencyUnitModel.Currency = currencyUnitModel.Currency;
                        finalCurrencyUnitModel.Unit = currencyUnitModel.Unit;
                        currencyUnitsList.Add(finalCurrencyUnitModel);
                    }
                }
                #endregion

                System.Data.DataTable dataTable = new System.Data.DataTable();
                dataTable.Columns.Add("Datum/ISO");

                foreach (var currency in currencyList)
                {
                    dataTable.Columns.Add(currency);
                }

                dataTable.Rows.Add();
                dataTable.Rows[0][0] = "Egység";

                foreach (var currenciesUnit in currencyUnitsList)
                {
                    dataTable.Rows[0][currenciesUnit.Currency] = currenciesUnit.Unit;
                }

                i = 1;
                foreach (var dailyModel in exchangeRateDailyModels)
                {
                    dataTable.Rows.Add();
                    dataTable.Rows[i]["Datum/ISO"] = dailyModel.Date;

                    foreach (var exchangeRate in dailyModel.ExchangeRate)
                    {
                        dataTable.Rows[i][exchangeRate.Currency] = exchangeRate.ExchangeRate;
                    }
                    i++;
                }


                string exchangeRatesXMLString = XElement.Parse(exchangeRates.GetExchangeRatesResult).ToString();
                XmlReader xmlReader = XmlReader.Create(new StringReader(exchangeRates.GetExchangeRatesResult));
                Console.WriteLine();
                Console.WriteLine("exchangeRatesXMLString: ");
                Console.WriteLine();
                Console.WriteLine(exchangeRatesXMLString);

                client.Close();

                Console.ReadLine();

                Microsoft.Office.Tools.Excel.Worksheet worksheet =
                    Globals.Factory
                    .GetVstoObject(Globals.ThisAddIn.Application.ActiveWorkbook.Worksheets[1]);

                for (int n = 0; n < dataTable.Columns.Count; n++)
                {
                    worksheet.Cells[1, n + 1] = dataTable.Columns[n].ColumnName;
                }

                for (int n = 0; n < dataTable.Rows.Count; n++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        worksheet.Cells[n + 2, j + 1] = dataTable.Rows[n][j].ToString();
                    }
                }
                var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                MessageBox.Show(timestamp.ToString());
                conn.Open();
                OleDbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $"insert into ExchangeRateAccessDB (UserName, Indoklas)" +
                    $" values ('{Environment.UserName}','indoklas teszt from code')";
                cmd.ExecuteNonQuery();
                conn.Close();
                #endregion
            
            }
        }
    }
}
