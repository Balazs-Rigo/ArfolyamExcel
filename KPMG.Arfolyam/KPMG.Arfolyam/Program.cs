using KPMG.Arfolyam.MNBArfolyamServiceSoapClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KPMG.Arfolyam
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient client = new MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient();

            var currenciesRequestBody = new GetCurrenciesRequestBody();

            var currencyUnitsRequestBody = new GetCurrencyUnitsRequestBody();

            var currencyExchangeRatesRequestBody = new GetCurrentExchangeRatesRequestBody();

            var currencies = client.GetCurrencies(currenciesRequestBody);

            currencyUnitsRequestBody.currencyNames = "HUF,AUD,EUR,USD,IDR,JPY,ITL,GRD";

            var currencyUnits = client.GetCurrencyUnits(currencyUnitsRequestBody);

            var currencyExchangeRates = client.GetCurrentExchangeRates(currencyExchangeRatesRequestBody);

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

            string exchangeRatesXMLString = XElement.Parse(currencyExchangeRates.GetCurrentExchangeRatesResult).ToString();
            
            Console.WriteLine();
            Console.WriteLine("exchangeRatesXMLString: ");
            Console.WriteLine();
            Console.WriteLine(exchangeRatesXMLString);

            Console.ReadLine();

        }
    }
}
