using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetOrderManager : BaseOrderManager, IWebsocketPrivateManager<IFuturesOrder>
    {
        public BitgetOrderManager(BitgetWebsocketPrivate oWebsocket): base(oWebsocket) 
        {
        }


        /// <summary>
        /// Put data
        /// </summary>
        /// <param name="aOrders"></param>
        public void Put(IEnumerable<BitgetFuturesOrderUpdate> aOrders)
        {
            List<IFuturesOrder> aList = new List<IFuturesOrder>();

            foreach( var oParsed in aOrders )
            {
                IFuturesSymbol? oSymbol = this.PrivateSocket.Exchange.SymbolManager.GetSymbol( oParsed.Symbol);
                if (oSymbol == null) continue;
                IFuturesOrder oNew = new BitgetOrder(oSymbol, oParsed);
                aList.Add(oNew);    
            }

            PutData(aList.ToArray());

        }
    }
}
