using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class FundingJson
    {
        public FundingJson()
        {
        }

        public string Symbol { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }

        public decimal Rate { get; set; }
    }
}
