using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects.Sockets;
using System.Threading.Channels;

namespace Crypto.Exchanges.All.Bitmart.Websocket
{
    internal class BitmartOrderManager : BaseOrderManager, IWebsocketPrivateManager<IFuturesOrder>
    {


        public BitmartOrderManager(IFuturesWebsocketPrivate oWs) : base(oWs) 
        { 
        }





        public void Put(DataEvent<IEnumerable<BitMartFuturesOrderUpdateEvent>> oUpdate)
        {
            if (oUpdate.Data == null) return;
            List<IFuturesOrder> aOrders = new List<IFuturesOrder>();
            string? strSymbol = oUpdate.Symbol;
            foreach( var oData in oUpdate.Data )
            {
                if (strSymbol == null) strSymbol = oData.Order.Symbol;
                if (strSymbol == null) continue;
                IFuturesSymbol? oFound = this.PrivateSocket.Exchange.SymbolManager.GetSymbol(strSymbol);
                if (oFound == null) continue;

                IFuturesOrder oOrder = new BitmartOrder(oFound, oData.Event, oData.Order);
                // aOrders.Add(oOrder);
                // IFuturesOrder oNewOrder = new BitmartOrder(oFound, oUpdate);
                PutData(oOrder.Id, oOrder);

            }

        }

    }
}
