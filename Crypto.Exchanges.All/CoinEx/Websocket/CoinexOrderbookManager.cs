using CoinEx.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Bingx;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexOrderbookManager : IOrderbookManager
    {
        private ICryptoWebsocket m_oWebsocket;
        private IFuturesSymbol[] m_aSymbols;

        private ConcurrentDictionary<string, IOrderbook> m_aData = new ConcurrentDictionary<string, IOrderbook> ();
        public int ReceiveCount { get; private set; } = 0;
        public CoinexOrderbookManager( ICryptoWebsocket oWs, IFuturesSymbol[] aSymbols ) 
        { 
            m_oWebsocket = oWs;
            m_aSymbols = aSymbols;
        }

        private IOrderbookPrice? GetBestPrice(IOrderbookPrice[] aPrices, decimal nMoney)
        {
            decimal nVolume = 0;
            IOrderbookPrice? oResult = null;
            for (int i = 0; i < aPrices.Length; i++)
            {
                decimal nPrice = aPrices[i].Price;
                nVolume += aPrices[i].Volume;

                decimal nMoneyTotal = nPrice * nVolume;
                if (nMoneyTotal >= nMoney)
                {
                    oResult = new CoinexOrderbookPrice() { Price = nPrice, Volume = nVolume };
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
        /// Gets all data
        /// </summary>
        /// <returns></returns>
        public IOrderbook[] GetData()
        {
            List<IOrderbook> aResult = new List<IOrderbook> (); 
            foreach( string strKey in m_aData.Keys )
            {
                IOrderbook? oData = null;
                if( m_aData.TryGetValue(strKey, out oData ) ) aResult.Add( oData ); 
            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Get data syngle symbol
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <returns></returns>
        public IOrderbook? GetData(string strSymbol)
        {
            IOrderbook? oResult = null;
            if( m_aData.TryGetValue (strSymbol, out oResult) )
            {
                return oResult;
            }
            return null;
        }

        public void Put(CoinExOrderBook oParsed)
        {
            ReceiveCount++;
            IFuturesSymbol? oFound = m_aSymbols.FirstOrDefault(p=> p.Symbol == oParsed.Symbol);
            if (oFound == null) return;
            CoinexOrderbook oBook = new CoinexOrderbook(oFound, oParsed);

            m_aData.AddOrUpdate(oFound.Symbol, p=> oBook, (p, b)=> oBook);

            return;
        }
    }
}
