using CoinEx.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexPoisitionManager : BasePositionManager, IWebsocketPrivateManager<IFuturesPosition>
    {

        public CoinexPoisitionManager(CoinexWebsocketPrivate oWebsocket): base(oWebsocket)
        {
        }
        public void Put(CoinExPositionUpdate oUpdate, bool bClose)
        {
            IFuturesSymbol? oSymbol = PrivateSocket.FuturesSymbols.FirstOrDefault(p => p.Symbol == oUpdate.Position.Symbol);
            if (oSymbol == null) return;
            IFuturesPosition oPosition = new CoinexPoisitionLocal(oSymbol, oUpdate);
            if (bClose) RemoveData(oPosition);
            else PutData(oPosition);
            return;
        }
    }
}
