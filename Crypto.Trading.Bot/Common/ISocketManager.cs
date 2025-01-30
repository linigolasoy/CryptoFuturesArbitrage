using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    public interface ISocketManager
    {

        public ICryptoSetup Setup { get; }
        public ICommonLogger Logger { get; }
        public ExchangeType[]? ExchangeTypes { get; set; }
        public IFuturesExchange[] Exchanges { get; }
        public IFuturesWebsocketPublic[]? SocketsPublic { get; }
        public IFuturesWebsocketPrivate[]? SocketsPrivate{ get; }

        public ReadOnlyDictionary<ExchangeType, IFundingRate[]> GetFundingRates();
        public ReadOnlyDictionary<ExchangeType, IOrderbook[]> GetOrderbooks();
        public Task<bool> Start();

        public Task Stop();



    }
}
