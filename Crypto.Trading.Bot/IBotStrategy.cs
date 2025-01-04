using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot
{
    public interface IBotStrategy
    {
        public ITradingBot Bot { get; } 
        public bool EvalSymbol(IBotSymbolData oData);

        public IBotSymbolData? CreateSymbolData(IBotExchangeData oData, IFuturesSymbol oSymbol);
        public Task Process(IBotExchangeData[] aData);

    }
}
