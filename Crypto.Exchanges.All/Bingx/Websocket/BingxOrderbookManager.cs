using BingX.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
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
    internal class BingxOrderbookManager : OrderbookHandler, IOrderbookManager
    {

        private IFuturesWebsocketPublic m_oWebsocket;

        private ConcurrentDictionary<string, IOrderbook> m_aOrderbooks = new ConcurrentDictionary<string, IOrderbook>();
        public int ReceiveCount { get; private set; } = 0;
        public BingxOrderbookManager(IFuturesWebsocketPublic oWebsocket) 
        { 
            m_oWebsocket = oWebsocket;
        }

        private IOrderbookPrice? GetBestPrice(IOrderbookPrice[] aPrices, decimal nMoney)
        {
            decimal nVolume = 0;
            IOrderbookPrice? oResult = null;    
            for( int i = 0; i < aPrices.Length; i++ )
            {
                decimal nPrice = aPrices[i].Price;
                nVolume += aPrices[i].Volume;

                decimal nMoneyTotal = nPrice * nVolume;
                if( nMoneyTotal >= nMoney )
                {
                    oResult = new BingxOrderbookPrice() { Price = nPrice, Volume = nVolume };
                    break;
                }
            }

            return oResult;
        }
        public IOrderbookPrice? GetBestAsk(string strSymbol, decimal nMoney)
        {
            IOrderbook? oOrderbook = GetData(strSymbol);
            if (oOrderbook == null) return null;
            return GetBestPrice(oOrderbook.Asks, nMoney);   
        }

        public IOrderbookPrice? GetBestBid(string strSymbol, decimal nMoney)
        {
            IOrderbook? oOrderbook = GetData(strSymbol);
            if (oOrderbook == null) return null;
            return GetBestPrice(oOrderbook.Bids, nMoney);
        }

        /// <summary>
        /// Get all orderbooks
        /// </summary>
        /// <returns></returns>
        public IOrderbook[] GetData()
        {
            List<IOrderbook> aResults = new List<IOrderbook>(); 
            foreach( string strKey in m_aOrderbooks.Keys )
            {
                IOrderbook? oFound = GetData( strKey );
                if (oFound == null) continue;
                aResults.Add( oFound ); 
            }
            return aResults.ToArray();  
        }

        public IOrderbook? GetData(string strSymbol)
        {
            IOrderbook? oResult = null;
            if( m_aOrderbooks.TryGetValue(strSymbol, out oResult) ) return oResult;
            return null;    
        }

        public void Put( string strSymbol, DateTime dDate, BingXOrderBook oParsedBook )
        {

            IFuturesSymbol? oSymbol = m_oWebsocket.FuturesSymbols.FirstOrDefault( p => p.Symbol == strSymbol );
            if (oSymbol == null) return;
            ReceiveCount++;
            IOrderbook oBook = new BingxOrderbook(oSymbol, dDate, oParsedBook);

            m_aOrderbooks.AddOrUpdate(strSymbol, p => oBook, (s, o) => oBook); 

            return;
        }
    }
}
