using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates
{
    internal class FundingBotExchangeData
    {
        public FundingBotExchangeData( ICryptoFuturesExchange oExchange ) 
        {
            Exchange = oExchange;
        }

        public ICryptoFuturesExchange Exchange { get; }
        public ICryptoWebsocket? Websocket { get; internal set; } = null;
        public IFuturesSymbol[]? Symbols { get; internal set; } = null;

        public IFundingRate[]? FundingSnapshots { get; internal set; } = null; 
    }
}
