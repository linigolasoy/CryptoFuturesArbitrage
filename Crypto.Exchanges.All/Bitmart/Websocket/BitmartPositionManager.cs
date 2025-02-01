using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Objects.Sockets;

namespace Crypto.Exchanges.All.Bitmart.Websocket
{
    internal class BitmartPositionManager : BasePositionManager, IWebsocketPrivateManager<IFuturesPosition>
    {

        public BitmartPositionManager( IFuturesWebsocketPrivate oWs) : base(oWs) 
        { 
        }



        public void Put(DataEvent<IEnumerable<BitMartPositionUpdate>> oUpdate)
        {
            List<IFuturesPosition> aPositions = new List<IFuturesPosition>();
            string? strSymbol = oUpdate.Symbol;
            if ( oUpdate.Data != null )
            {
                foreach (var oPos in oUpdate.Data)
                {
                    if( strSymbol == null ) strSymbol = oPos.Symbol;
                    if (strSymbol == null) continue;
                    IFuturesSymbol? oSymbol = this.PrivateSocket.Exchange.SymbolManager.GetSymbol(strSymbol);
                    if (oSymbol == null) continue;
                    IFuturesPosition oNew = new BitmartPositionLocal(oSymbol, oPos);
                    aPositions.Add(oNew);
                }

            }
            PutData(aPositions.ToArray());

        }

    }
}
