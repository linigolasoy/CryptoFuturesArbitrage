using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using CoinEx.Net.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Crypto.Interface.Futures.Market;
using Crypto.Exchanges.All.Common;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexOrderManager : BaseOrderManager, IWebsocketPrivateManager<IFuturesOrder>
    {

        public CoinexOrderManager(CoinexWebsocketPrivate oWs) : base(oWs)
        { 
        }


        public void Put(CoinExFuturesOrderUpdate oOrder)
        {
            IFuturesSymbol? oSymbol = PrivateSocket.Exchange.SymbolManager.GetSymbol(oOrder.Order.Symbol);
            if (oSymbol == null) return;
            bool bBuy = false;
            if( oOrder.Order.Side == OrderSide.Buy ) bBuy = true;
            OrderUpdateType eType = oOrder.Event;

            IFuturesOrder oNew = new CoinexOrder(oSymbol, bBuy, true, oOrder.Order, oOrder.Event );
            PutData(oNew.Id, oNew);
            return;
        }
    }
}
