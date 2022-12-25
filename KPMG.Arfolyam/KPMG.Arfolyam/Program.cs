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

            currencyUnitsRequestBody.currencyNames = string.Join(",",currencyList);

           var currencyUnits = client.GetCurrencyUnits(currencyUnitsRequestBody);
            
            DataTable exchangeRatesTable = new DataTable();
            exchangeRatesTable.Columns.Add("Datum", typeof(string));

            foreach (var currency in currencyList)
            {
                exchangeRatesTable.Columns.Add(currency, typeof(string));
            }

            exchangeRatesRequestBody.startDate = "2015-10-01";
            exchangeRatesRequestBody.endDate = "2015-10-07";
            exchangeRatesRequestBody.currencyNames = string.Join(",",currencyList.Take(10));

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
                }

                if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == "Rate")
                {
                    exchangeRateModel =new ExchangeRateModel();
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

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Datum");
            foreach (var currency in currencyList)
            {
                dataTable.Columns.Add(currency);
            }

            i = 0;
            foreach (var dailyModel in exchangeRateDailyModels)
            {
                dataTable.Rows.Add();
                dataTable.Rows[i]["Datum"] = dailyModel.Date;

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

        }        
    }
}
