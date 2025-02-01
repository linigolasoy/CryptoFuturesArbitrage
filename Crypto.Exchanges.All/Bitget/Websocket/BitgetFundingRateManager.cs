using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetFundingRateManager : IWebsocketManager<IFundingRate>
    {

        private ConcurrentDictionary<string, IFundingRate> m_aFundingRates = new ConcurrentDictionary<string, IFundingRate>();
        public int ReceiveCount { get; private set; } = 0;

        public int Count { get=>  m_aFundingRates.Count; }
        public DateTime LastUpdate { get; private set; } = DateTime.Now;

        private IFuturesSymbolManager m_oSymbolManager;
        public BitgetFundingRateManager(IFuturesWebsocketPublic oWebsocket) 
        {
            m_oSymbolManager = oWebsocket.Exchange.SymbolManager;
        }


        public IFundingRate[] GetData()
        {
            List<IFundingRate> aResult = new List<IFundingRate>();  
            foreach( string strSymbol in m_aFundingRates.Keys )
            {
                IFundingRate? oFound = GetData(strSymbol);
                if( oFound != null ) aResult.Add( oFound ); 
            }
            return aResult.ToArray();   
        }

        public IFundingRate? GetData(string strSymbol)
        {
            IFundingRate? oResult = null;
            if( m_aFundingRates.TryGetValue(strSymbol, out oResult) ) 
            { 
                return oResult;
            }
            return null;
        }

        public void Put(BitgetFuturesTickerUpdate oTicker )
        {
            ReceiveCount++;
            IFuturesSymbol? oFound = m_oSymbolManager.GetSymbol( oTicker.Symbol);
            if (oFound == null) return;
            if (oTicker.FundingRate == null) return;
            if( oTicker.NextFundingTime == null) return;
            IFundingRate oRate = new BitgetFuturesFundingRate(oFound, oTicker);

            m_aFundingRates.AddOrUpdate(oFound.Symbol, p=> oRate, (s, r) => oRate);
            LastUpdate = DateTime.Now;
            return;
        }

    }
}
