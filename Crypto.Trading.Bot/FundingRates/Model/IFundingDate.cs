using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{
    /// <summary>
    /// Single funding date with all its pairs
    /// </summary>
    public interface IFundingDate
    {
        public DateTime DateTime { get; }

        public IFundingPair[] Pairs { get; }

        public IFundingPair? GetBest();
    }
}
