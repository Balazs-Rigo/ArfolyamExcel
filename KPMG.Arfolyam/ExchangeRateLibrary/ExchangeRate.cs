using ExchangeRateLibrary.MNBArfolyamServiceSoapClient;
using ExchangeRateLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExchangeRateLibrary
{
    public class ExchangeRate
    {
        private MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient _MNBArfolyamServiceSoap { get; set; }

        public ExchangeRate(MNBArfolyamServiceSoapClient.MNBArfolyamServiceSoapClient service)
        {
            _MNBArfolyamServiceSoap = service;
        }

        public DataTable GetExchangeRates(string startDate, string endDate, int numberOfCurrencies)
        {
            if (!IsValidDates(startDate, endDate))
                throw new ArgumentException("A kezdő vagy a vég dátum nem megfelelő formátumban van! A helyes formátum: yyyy-MM-dd");


            DataTable output = CreateDatatable();

            int i = 1;
            foreach (var dailyModel in GetExchangeRates(numberOfCurrencies, startDate, endDate))
            {
                output.Rows.Add();
                output.Rows[i]["Datum/ISO"] = dailyModel.Date;

                foreach (var exchangeRate in dailyModel.ExchangeRate)
                {
                    output.Rows[i][exchangeRate.Currency] = exchangeRate.ExchangeRate;
                }
                i++;
            }

            return output;
        }

        private bool IsValidDates(string startDate, string endDate)
        {
            string format = "yyyy-MM-dd";
            DateTime start;
            DateTime end;

            bool isStartValid = DateTime.TryParseExact(startDate, format, null, System.Globalization.DateTimeStyles.None, out start);
            bool isEndValid = DateTime.TryParseExact(endDate, format, null, System.Globalization.DateTimeStyles.None, out end);

            if (start > end)
                throw new ArgumentException("A kezdődátum nem lehet nagyobb, mint a végdátum.");

            return isStartValid && isEndValid;
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

        private List<ExchangeRateDailyModel> GetExchangeRates(int numberOfCurrencies, string startDate, string endDate)
        {
            List<ExchangeRateDailyModel> output = new List<ExchangeRateDailyModel>();

            var exchangeRatesRequestBody = new GetExchangeRatesRequestBody();

            exchangeRatesRequestBody.startDate = startDate;
            exchangeRatesRequestBody.endDate = endDate;
            exchangeRatesRequestBody.currencyNames = string.Join(",", GetCurrencies().Take(numberOfCurrencies + 1));

            var exchangeRates = _MNBArfolyamServiceSoap
                            .GetExchangeRates(exchangeRatesRequestBody).GetExchangeRatesResult;

            output = LoadExchangeRatesToList(exchangeRates);

            return output;
        }

        private List<ExchangeRateDailyModel> LoadExchangeRatesToList(string exchangeRates)
        {
            List<ExchangeRateDailyModel> output = new List<ExchangeRateDailyModel>();

            XmlReader rdr = XmlReader.Create(new StringReader(exchangeRates));
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
                    output.Add(exchangeRateDailyModel);
                    i = 0;
                }
            }

            return output;
        }

        private DataTable CreateDatatable()
        {
            DataTable output = new DataTable();

            output.Columns.Add("Datum/ISO");

            foreach (var currency in GetCurrencies())
            {
                output.Columns.Add(currency);
            }

            output.Rows.Add();
            output.Rows[0][0] = "Egység";

            foreach (var currenciesUnit in GetCurrencyUnits())
            {
                output.Rows[0][currenciesUnit.Currency] = currenciesUnit.Unit;
            }

            return output;
        }
    }
}
