using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Websocket.Private
{
    internal class FuturesOrderManager : IWebsocketManager<IFuturesOrder>
    {
        private ICryptoWebsocket m_oWebsocket;

        private ConcurrentDictionary<long, IFuturesOrder> m_aOrders = new ConcurrentDictionary<long, IFuturesOrder>();

        public FuturesOrderManager( ICryptoWebsocket oWebsocket ) 
        {
            m_oWebsocket = oWebsocket;
        }

        /// <summary>
        /// Get all orders captured by websocket
        /// </summary>
        /// <returns></returns>
        public IFuturesOrder[] GetData()
        {
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();

            foreach( long nKey in m_aOrders.Keys ) 
            {
                IFuturesOrder? oFound = null;
                if( m_aOrders.TryGetValue( nKey, out oFound ) )
                {
                    aResult.Add( oFound );
                }
            }
            return aResult.ToArray();
        }

        public IFuturesOrder? GetData(string strSymbol)
        {
            IFuturesOrder[] aData = GetData();
            return aData.FirstOrDefault( p=> p.Symbol.Symbol == strSymbol );
        }

        public void Put( IWebsocketMessage oMessage )
        {
            if (!(oMessage is IFuturesOrder)) return;
            IFuturesOrder oOrder = ( IFuturesOrder )oMessage;

            m_aOrders.AddOrUpdate(oOrder.Id, (_) => oOrder, (_, o) => { ((OrderMessage)o).Update(oOrder); return o; });
            return;
        }
    }
}
