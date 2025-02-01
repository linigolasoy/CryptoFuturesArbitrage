using Crypto.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BaseOrderbookSocket
    {
        private IFuturesSymbolManager m_oSymbolManager;
        public BaseOrderbookSocket(string strUrl, IFuturesWebsocketPublic oWebsockets, IOrderbookParser oParser) 
        { 
            Parser = oParser;
            m_oSymbolManager = oWebsockets.Exchange.SymbolManager;
            CommonSocket = CommonFactory.CreateWebsocket(strUrl, 20);
            CommonSocket.OnReceived += CommonSocket_OnReceived;
            CommonSocket.OnPing += CommonSocket_OnPing;
            CommonSocket.OnDisConnect += CommonSocket_OnDisConnect;
            OrderbookManager = oWebsockets.OrderbookManager;
        }

        private void CommonSocket_OnDisConnect()
        {
            return;
        }

        private string CommonSocket_OnPing()
        {
            return Parser.PingMessage;
        }

        /// <summary>
        /// Receive parsed
        /// </summary>
        /// <param name="strMessage"></param>
        private void CommonSocket_OnReceived(string strMessage)
        {
            IOrderbook? oOrderbook = Parser.Parse(strMessage, m_oSymbolManager);
            if (oOrderbook != null)
            {
                OrderbookManager.Update(oOrderbook);
            }
        }

        public ICommonWebsocket CommonSocket { get; }
        public IOrderbookManager OrderbookManager { get; }
        internal IOrderbookParser Parser { get; }
        // public m Task<bool> Subscribe() 
        

    }
}
