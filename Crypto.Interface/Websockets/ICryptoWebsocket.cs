using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Websockets
{
    public interface ICryptoWebsocket
    {

        public IExchange Exchange { get; }  

        public IFuturesSymbol[] FuturesSymbols { get; }

        public Task<bool> Start();

        public Task Stop();

        public Task<bool> SubscribeToMarket(ISymbol[] aSymbols);

        // public IWebsocketManager<ITicker> TickerManager { get; }    

        public IWebsocketManager<IFuturesOrder> FuturesOrderManager { get; }
        public IWebsocketManager<IFuturesPosition> FuturesPositionManager { get; }
        public IOrderbookManager OrderbookManager { get; }
        public IWebsocketManager<IFundingRateSnapShot> FundingRateManager { get; }  

        public IWebsocketManager<IFuturesBalance> BalanceManager { get; }   
    }
}
