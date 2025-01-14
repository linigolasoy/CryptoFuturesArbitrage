using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Websockets
{
    public interface IFuturesWebsocketPublic
    {
        public IFuturesExchange Exchange { get; }

        public IFuturesSymbol[] FuturesSymbols { get; }

        public Task<bool> Start();

        public Task Stop();

        public Task<bool> SubscribeToMarket(IFuturesSymbol[] aSymbols);

        public Task<bool> SubscribeToFundingRates(IFuturesSymbol[] aSymbols);

        // public IWebsocketManager<ITicker> TickerManager { get; }    

        public IOrderbookManager OrderbookManager { get; }
        public IWebsocketManager<IFundingRate> FundingRateManager { get; }
    }
}
