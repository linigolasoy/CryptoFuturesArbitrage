using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.BackTest
{
    internal class CurrencySymbols
    {
        public CurrencySymbols(string strCurrency, IFuturesSymbol[] aSymbols) 
        {
            Currency = strCurrency;
            Symbols = aSymbols;
        }

        public string Currency { get; }

        public IFuturesSymbol[] Symbols { get;}
    }
}
