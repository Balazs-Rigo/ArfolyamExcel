using KPMG.Arfolyam.MNBArfolyamServiceSoapClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace KPMG.Arfolyam
{
    internal class Program
    {
        static void Main(string[] args)
        {
           
            MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient client = new MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient();
           
            client.Open();

            var currenciesRequestBody = new GetCurrenciesRequestBody();

            var currencyUnitsRequestBody = new GetCurrencyUnitsRequestBody();

            var exchangeRatesRequestBody = new GetExchangeRatesRequestBody();


            var currencies = client.GetCurrencies(currenciesRequestBody);

            //currencyUnitsRequestBody.currencyNames = "HUF,AUD,EUR,USD,IDR,JPY,ITL,GRD";

           // var currencyUnits = client.GetCurrencyUnits(currencyUnitsRequestBody);

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

            DataTable exchangeRatesTable = new DataTable();
            exchangeRatesTable.Columns.Add("Datum", typeof(string));

            foreach (var currency in currencyList)
            {
                exchangeRatesTable.Columns.Add(currency, typeof(string));
            }

            exchangeRatesRequestBody.startDate = "2015-10-01";
            exchangeRatesRequestBody.endDate = "2015-10-5";
            exchangeRatesRequestBody.currencyNames = string.Join(",",currencyList.Take(10));

            var exchangeRates = client.GetExchangeRates(exchangeRatesRequestBody);

            string currenciesXMLString = XElement.Parse(currencies.GetCurrenciesResult).ToString();    

            Console.WriteLine("currencies: ");
            Console.WriteLine();
            Console.WriteLine(currenciesXMLString);
            Console.WriteLine();

            //string currencyUnitsXMLString = XElement.Parse(currencyUnits.GetCurrencyUnitsResult).ToString();
            //Console.WriteLine("currency units : ");
            //Console.WriteLine();
            //Console.WriteLine(currencyUnitsXMLString);
            //Console.WriteLine();

            #region get values from exchangeRates
            //XmlDocument xmlExchangeRates = new XmlDocument();
            //xml.LoadXml(exchangeRates.GetExchangeRatesResult);

            //List<string> excahangeRatesList = new List<string>();
            //XmlNodeList xnListExchangeRates = xml.SelectNodes("/MNBExchangeRates/Day/Rate");

            //List<ExchangeRateDailyModel> exchangeRateDailyModels = new List<ExchangeRateDailyModel>();
            //foreach (XmlNode xn in xnListExchangeRates)
            //{
            //    var parentNodeXelement = XElement.Parse(xn.ParentNode.OuterXml);
            //    var date = parentNodeXelement.FirstAttribute.Value;
            //    var CurrentNodeXelement = XElement.Parse(xn.OuterXml);
            //    var currency = CurrentNodeXelement.LastAttribute.Value;
            //    var excangeRate = xn.InnerText;

            //    ExchangeRateDailyModel exchangeRateDailyModel = new ExchangeRateDailyModel();
            //    ExchangeRateModel exchangeRateModel = new ExchangeRateModel();
            //    exchangeRateDailyModel.Date = date;
            //    exchangeRateModel.Currency = currency;
            //    exchangeRateModel.ExchangeRate = excangeRate;

            //    exchangeRateDailyModels.Add(exchangeRateDailyModel);
            //}
            #endregion

            //for (int i = 1; i < exchangeRateDailyModels.Count; i++)
            //{
            //    DataSet ds = new DataSet();
            //    ds.rowexchangeRatesTable.Rows.Add(new DataRow());
            //    exchangeRatesTable.Rows[i]["Datum"] = exchangeRateDailyModels[i].Date;
            //    exchangeRatesTable.Rows[i][exchangeRateDailyModels[i].ExchangeRate.Currency] =
            //        exchangeRateDailyModels[i].ExchangeRate.ExchangeRate;
            //}

            List<ExchangeRateDailyModel> exchangeRateDailyModels= new List<ExchangeRateDailyModel>();

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
                        Console.WriteLine("Day");
                        Console.WriteLine("Attribute count: "+rdr.GetAttribute("date"));
                }

                if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == "Rate")
                {
                    exchangeRateModel =new ExchangeRateModel();
                    exchangeRateModel.Currency = rdr.GetAttribute("curr");
                    //exchangeRateDailyModel.ExchangeRate.Add(new ExchangeRateModel
                    //{
                    //    Currency = rdr.GetAttribute("curr"),
                    //});      
                }

                if (rdr.NodeType == XmlNodeType.Text)
                {
                    exchangeRateModel.ExchangeRate = rdr.Value;
                   // exchangeRateDailyModel.ExchangeRate[i].ExchangeRate = rdr.Value;
                    //i++;
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

                //Console.WriteLine("NodeType: "+ rdr.NodeType);
                //if (rdr.NodeType == XmlNodeType.Element)
                //{
                //   // Console.WriteLine("node while: "+rdr.LocalName);
                //}
            }


            string exchangeRatesXMLString = XElement.Parse(exchangeRates.GetExchangeRatesResult).ToString();
            XmlReader xmlReader = XmlReader.Create(new StringReader(exchangeRates.GetExchangeRatesResult));
            Console.WriteLine();
            Console.WriteLine("exchangeRatesXMLString: ");
            Console.WriteLine();
            Console.WriteLine(exchangeRatesXMLString);

            StringReader stringReader = new StringReader(exchangeRates.GetExchangeRatesResult);
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(stringReader);

            

            client.Close();

            Console.ReadLine();

        }        
    }
}
