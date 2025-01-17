using BingX.Net.Objects.Models;
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

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxPositionManager : BasePositionManager, IWebsocketPrivateManager<IFuturesPosition>
    {

        public BingxPositionManager( IFuturesWebsocketPrivate oWs) : base(oWs) 
        { 
        }

        public IFuturesSymbol[] FuturesSymbols { get; internal set; } = Array.Empty<IFuturesSymbol>();


        public void Put( IEnumerable<BingXFuturesPositionChange> aUpdated )
        {
            List<IFuturesPosition> aPositions = new List<IFuturesPosition>();
            foreach( var oPos in aUpdated)
            {
                IFuturesSymbol? oSymbol = FuturesSymbols.FirstOrDefault(p=> p.Symbol == oPos.Symbol );
                if (oSymbol == null) continue;
                IFuturesPosition oNew = new BingxPositionLocal(oSymbol, oPos);
                aPositions.Add(oNew);
            }
            PutData(aPositions.ToArray());

        }

    }
}
