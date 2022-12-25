using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPMG.Arfolyam
{
    public class ExchangeRateDailyModel
    {
        public string Date { get; set; }
        public List<ExchangeRateModel> ExchangeRate { get; set; } = new List<ExchangeRateModel>();
    }
}
