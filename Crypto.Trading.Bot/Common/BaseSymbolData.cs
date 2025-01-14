using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    internal class BaseSymbolData : IBotSymbolData
    {

        public BaseSymbolData(IBotExchangeData oData, IFuturesSymbol oSymbol) 
        { 
            ExchangeData = oData;
            Symbol = oSymbol;   
        }
        public IBotExchangeData ExchangeData { get; }

        public IFuturesSymbol Symbol { get; }

        public IFundingRate? FundingRate { get; internal set; } = null;

        public IOrderbook? Orderbook { get; internal set; } = null;

        public IFuturesOrder[]? Orders => throw new NotImplementedException();

        public IFuturesPosition[]? Positions => throw new NotImplementedException();
    }
}
