using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxLeverage : IFuturesLeverage
    {
        public BingxLeverage(IFuturesSymbol oSymbol, BingXLeverage oParsed) 
        { 
            Symbol = oSymbol;   
            LongLeverage = oParsed.LongLeverage;
            ShortLeverage = oParsed.ShortLeverage;  
        }
        public IFuturesSymbol Symbol { get; }

        public decimal LongLeverage { get; }

        public decimal ShortLeverage { get; }
    }
}
