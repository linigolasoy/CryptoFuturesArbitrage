using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Market
{
    public interface IFuturesMarket
    {
        public IFuturesExchange Exchange { get; }

        public Task<IFuturesSymbol[]?> GetSymbols();

        public Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol);
        public Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols);

        public IFuturesWebsocketPublic? Websocket { get; }
        public Task<bool> StartSockets();
        public Task<bool> EndSockets();
    }
}
