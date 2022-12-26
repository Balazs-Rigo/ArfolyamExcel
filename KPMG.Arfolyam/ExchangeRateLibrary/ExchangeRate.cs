using ExchangeRateLibrary.Constans;
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

        /// <summary>
        /// Egy Datatable-ban visszaadja a valuta árfolyamait adott időintervallumban
        /// </summary>
        /// <param name="startDate">Mikortól szeretném a lekérdezést.</param>
        /// <param name="endDate">Meddig szeretném a lekérdezést.</param>
        /// /// <param name="numberOfCurrencies">Hány valutát szeretnék lekérdezni.</param>
        public DataTable GetExchangeRates(string startDate, string endDate, int numberOfCurrencies)
        {
            if (!IsValidDates(startDate, endDate))
                throw new ArgumentException("A kezdő vagy a vég dátum nem megfelelő formátumban van! A helyes formátum: yyyy-MM-dd");


            DataTable output = CreateDatatable();

            int i = 1;
            foreach (var dailyModel in GetExchangeRates(numberOfCurrencies, startDate, endDate))
            {
                output.Rows.Add();
                output.Rows[i][TableConstans.Datum] = dailyModel.Date;

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

            bool isStartValid = DateTime.TryParseExact(startDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime start);
            bool isEndValid = DateTime.TryParseExact(endDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime end);

            if (start > end)
                throw new ArgumentException("A kezdődátum nem lehet nagyobb, mint a végdátum.");

            return isStartValid && isEndValid;
        }

        private List<string> GetCurrencies()
        {
            var currenciesRequestBody = new GetCurrenciesRequestBody();

            var currencies = _MNBArfolyamServiceSoap.GetCurrencies(currenciesRequestBody);

            List<string> output = LoadCurrenciesToList(currencies.GetCurrenciesResult);

            return output;
        }

        private List<string> LoadCurrenciesToList(string currencies)
        {
            List<string> output = new List<string>();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(currencies);

            using (XmlNodeList xnList = xml.SelectNodes("/MNBCurrencies/Currencies/Curr"))
            {                
                foreach (XmlNode xn in xnList)
                {
                    output.Add(xn.InnerText);
                }
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

            using (XmlReader rdr = XmlReader.Create(new StringReader(currencyUnits)))
            {
                CurrencyUnitModel currencyUnitModel = null;
                while (rdr.Read())
                {

                    if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == XMLNodeConstans.Unit)
                    {
                        currencyUnitModel = new CurrencyUnitModel
                        {
                            Currency = rdr.GetAttribute(XMLAttributeConstans.Curr)
                        };
                    }

                    if (rdr.NodeType == XmlNodeType.Text)
                    {
                        currencyUnitModel.Unit = rdr.Value;
                    }

                    if (rdr.NodeType == XmlNodeType.EndElement && rdr.LocalName == XMLNodeConstans.Unit)
                    {
                        CurrencyUnitModel finalCurrencyUnitModel = new CurrencyUnitModel
                        {
                            Currency = currencyUnitModel.Currency,
                            Unit = currencyUnitModel.Unit
                        };
                        currencyUnitsList.Add(finalCurrencyUnitModel);
                    }
                }
            }

            
            return currencyUnitsList;
        }

        private List<ExchangeRateDailyModel> GetExchangeRates(int numberOfCurrencies, string startDate, string endDate)
        {
            var exchangeRatesRequestBody = new GetExchangeRatesRequestBody
            {
                startDate = startDate,
                endDate = endDate,
                currencyNames = string.Join(",", GetCurrencies().Take(numberOfCurrencies + 1))
            };

            var exchangeRates = _MNBArfolyamServiceSoap
                            .GetExchangeRates(exchangeRatesRequestBody).GetExchangeRatesResult;

            List<ExchangeRateDailyModel>  output = LoadExchangeRatesToList(exchangeRates);

            return output;
        }

        private List<ExchangeRateDailyModel> LoadExchangeRatesToList(string exchangeRates)
        {
            List<ExchangeRateDailyModel> output = new List<ExchangeRateDailyModel>();

            using (XmlReader rdr = XmlReader.Create(new StringReader(exchangeRates)))
            {
                ExchangeRateDailyModel exchangeRateDailyModel = null;
                ExchangeRateModel exchangeRateModel = null;
                int i = 0;
                while (rdr.Read())
                {

                    if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == XMLNodeConstans.Day)
                    {
                        exchangeRateDailyModel = new ExchangeRateDailyModel
                        {
                            Date = rdr.GetAttribute(XMLAttributeConstans.Date)
                        };
                    }

                    if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName == XMLNodeConstans.Rate)
                    {
                        exchangeRateModel = new ExchangeRateModel
                        {
                            Currency = rdr.GetAttribute(XMLAttributeConstans.Curr)
                        };
                    }

                    if (rdr.NodeType == XmlNodeType.Text)
                    {
                        exchangeRateModel.ExchangeRate = rdr.Value;
                    }

                    if (rdr.NodeType == XmlNodeType.EndElement && rdr.LocalName == XMLNodeConstans.Rate)
                    {
                        ExchangeRateModel finalExchangeRateModel = new ExchangeRateModel();
                        finalExchangeRateModel.Currency = exchangeRateModel.Currency;
                        finalExchangeRateModel.ExchangeRate = exchangeRateModel.ExchangeRate;
                        exchangeRateDailyModel.ExchangeRate.Add(finalExchangeRateModel);
                        i++;
                    }

                    if (rdr.NodeType == XmlNodeType.EndElement && rdr.LocalName == XMLNodeConstans.Day)
                    {
                        output.Add(exchangeRateDailyModel);
                        i = 0;
                    }
                }
            }           

            return output;
        }

        private DataTable CreateDatatable()
        {
            DataTable output = new DataTable();

            output.Columns.Add(TableConstans.Datum);

            foreach (var currency in GetCurrencies())
            {
                output.Columns.Add(currency);
            }

            output.Rows.Add();
            output.Rows[0][0] = TableConstans.Egyseg;

            foreach (var currenciesUnit in GetCurrencyUnits())
            {
                output.Rows[0][currenciesUnit.Currency] = currenciesUnit.Unit;
            }

            return output;
        }
    }
}
