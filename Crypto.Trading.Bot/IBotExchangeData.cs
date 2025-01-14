using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot
{

    public interface IBotSymbolData
    {
        public IBotExchangeData ExchangeData { get; }

        public IFuturesSymbol Symbol { get; }

        public IFundingRate? FundingRate { get; }

        public IOrderbook? Orderbook { get; }

        public IFuturesOrder[]? Orders { get; }

        public IFuturesPosition[]? Positions { get; }
    }

    public interface IBotExchangeData
    {

        public IFuturesExchange Exchange { get; }

        public IFuturesWebsocketPublic? Websocket { get; }

        public IBotSymbolData[]? Symbols { get; }

        public IFuturesBalance[]? Balances { get; }

        public Task<bool> Update();

        public void Reset();
    }
}
