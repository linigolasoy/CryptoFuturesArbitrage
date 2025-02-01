using BitMart.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart.Websocket
{
    internal class BitmartFundingRateManager : IWebsocketManager<IFundingRate>
    {

        private ConcurrentDictionary<string, IFundingRate> m_aRates = new ConcurrentDictionary<string, IFundingRate> ();
        private IFuturesSymbolManager m_oSymbolManager;
        public BitmartFundingRateManager(IFuturesWebsocketPublic oWebsocket) 
        {
            m_oSymbolManager = oWebsocket.Exchange.SymbolManager;
        }

        public int Count { get => m_aRates.Count; }

        public int ReceiveCount { get; private set; } = 0;

        public DateTime LastUpdate { get; private set; } = DateTime.Now;

        public IFundingRate[] GetData()
        {
            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach( string strKey in m_aRates.Keys )
            {
                IFundingRate? oFound = GetData( strKey ); 
                if( oFound != null ) aResult.Add( oFound ); 
            }
            return aResult.ToArray();
        }

        public IFundingRate? GetData(string strSymbol)
        {
            IFundingRate? oFound = null;
            if( m_aRates.TryGetValue(strSymbol, out oFound) ) 
            { 
                return oFound;  
            }
            return null;
        }

        public void Put(DataEvent<BitMartFundingRateUpdate> oUpdate)
        {
            if (oUpdate == null || oUpdate.Data == null) return;
            // TODO: Delete???
            if( oUpdate.Data.NextFundingTime == null )
            {
                return; 
            }

            IFuturesSymbol? oSymbol = m_oSymbolManager.GetSymbol(oUpdate.Data.Symbol);
            if (oSymbol == null) return;
            ReceiveCount++;
            IFundingRate oNew = new BitmartFundingRateLocal(oSymbol, oUpdate.Data);
            LastUpdate = DateTime.Now;
            m_aRates.AddOrUpdate(oSymbol.Symbol, p => oNew, (s, p) => { p.Update(oNew); return p; });
        }
    }
}
