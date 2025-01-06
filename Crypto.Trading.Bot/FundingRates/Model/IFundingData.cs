using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{
    public interface IFundingData
    {
        public ICryptoFuturesExchange[] Exchanges { get; }

        public IFundingDate? GetNext(DateTime? dActual);
    }
}
