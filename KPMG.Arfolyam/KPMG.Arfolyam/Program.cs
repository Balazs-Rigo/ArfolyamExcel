using KPMG.Arfolyam.MNBArfolyamServiceSoapClient;
using System;
using System.Collections.Generic;
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

            var currentExchangeRatesRequestBody = new GetCurrentExchangeRatesRequestBody();

            var dateIntervalRequestBody = new GetDateIntervalRequestBody();

            var dateInterval = client.GetDateInterval(dateIntervalRequestBody);

            var intervalDataResult = dateInterval.GetDateIntervalResult;

            var currencies = client.GetCurrencies(currenciesRequestBody);

            currencyUnitsRequestBody.currencyNames = "HUF,AUD,EUR,USD,IDR,JPY,ITL,GRD";

            var currencyUnits = client.GetCurrencyUnits(currencyUnitsRequestBody);

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

            exchangeRatesRequestBody.startDate = "2022-10-01";
            exchangeRatesRequestBody.endDate = "2022-12-02";
            exchangeRatesRequestBody.currencyNames = string.Join(",",currencyList.Take(11));

            var exchangeRates = client.GetExchangeRates(exchangeRatesRequestBody);

            var currentExchangeRates = client.GetCurrentExchangeRates(currentExchangeRatesRequestBody);

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

            string exchangeRatesXMLString = XElement.Parse(exchangeRates.GetExchangeRatesResult).ToString();
            
            Console.WriteLine();
            Console.WriteLine("exchangeRatesXMLString: ");
            Console.WriteLine();
            Console.WriteLine(exchangeRatesXMLString);

            client.Close();

            Console.ReadLine();

        }       
    }
}
