using BingX.Net.Objects.Models;
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

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxOrderManager : BaseOrderManager, IWebsocketPrivateManager<IFuturesOrder>
    {


        public BingxOrderManager(IFuturesWebsocketPrivate oWs) : base(oWs) 
        { 
        }





        public void Put(BingXFuturesOrderUpdate oUpdate)
        {
            IFuturesSymbol? oFound = this.PrivateSocket.Exchange.SymbolManager.GetSymbol(oUpdate.Symbol);
            if (oFound == null) return;
            IFuturesOrder oNewOrder = new BingxOrder(oFound, oUpdate);
            PutData(oUpdate.OrderId.ToString(), oNewOrder);    

        }

    }
}
