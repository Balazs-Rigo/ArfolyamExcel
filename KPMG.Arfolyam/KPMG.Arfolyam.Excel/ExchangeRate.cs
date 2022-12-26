using KPMG.Arfolyam.Excel.MNBArfolyamServiceSoapClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KPMG.Arfolyam.Excel
{
    public class ExchangeRate
    {
        private MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient _MNBArfolyamServiceSoap { get; set; }

        public ExchangeRate(MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient service)
        {
            _MNBArfolyamServiceSoap = service;

            GetCurrencyUnits();
        }

        private List<string> GetCurrencies()
        {
            List<string> output = new List<string>();

            var currenciesRequestBody = new GetCurrenciesRequestBody();

            var currencies = _MNBArfolyamServiceSoap.GetCurrencies(currenciesRequestBody);            

            output = LoadCurrenciesToList(currencies.GetCurrenciesResult);

            return output;
        }

        private List<string> LoadCurrenciesToList(string currencies)
        {
            List<string> output = new List<string>();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(currencies);

            XmlNodeList xnList = xml.SelectNodes("/MNBCurrencies/Currencies/Curr");
            foreach (XmlNode xn in xnList)
            {
                output.Add(xn.InnerText);
            }

            return output;
        }

        private List<CurrencyUnitModel> GetCurrencyUnits()
        {
            List<CurrencyUnitModel> output = new List<CurrencyUnitModel>();

            var currencyUnitsRequestBody = new GetCurrencyUnitsRequestBody();

            currencyUnitsRequestBody.currencyNames = string.Join(",", GetCurrencies());

            var currencyUnits = _MNBArfolyamServiceSoap.GetCurrencyUnits(currencyUnitsRequestBody).GetCurrencyUnitsResult;

            output = LoadCurrencyUnitsToList(currencyUnits);

            return output; 
        }

        private List<CurrencyUnitModel> LoadCurrencyUnitsToList(string currencyUnits)
        {
            List<CurrencyUnitModel> currencyUnitsList = new List<CurrencyUnitModel>();

            XmlReader rdr = XmlReader.Create(new StringReader(currencyUnits));
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
            return currencyUnitsList;
        }

        private List<ExchangeRateDailyModel> GetExchangeRates(string startDate, string endDate, int numberOfCurrencies)
        {
            List<ExchangeRateDailyModel> output = new List<ExchangeRateDailyModel>();

            var exchangeRatesRequestBody = new GetExchangeRatesRequestBody();

            exchangeRatesRequestBody.startDate = startDate;
            exchangeRatesRequestBody.endDate = endDate;
            exchangeRatesRequestBody.currencyNames = string.Join(",", GetCurrencies().Take(numberOfCurrencies));


            DataTable data = CreateDatatable();

            return output;
        }

        private DataTable CreateDatatable()
        {
            DataTable output = new DataTable();

            output.Columns.Add("");

            return output;
        }
    }
}
