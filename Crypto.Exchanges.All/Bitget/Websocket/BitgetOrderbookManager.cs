using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Bingx;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetOrderbookManager : IOrderbookManager
    {

        private ConcurrentDictionary<string, IOrderbook> m_aOrderbooks = new ConcurrentDictionary<string, IOrderbook>();

        private IFuturesSymbol[] m_aSymbols;
        public int ReceiveCount { get; private set; } = 0;
        public BitgetOrderbookManager(IFuturesSymbol[] aSymbols) 
        { 
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

        public IOrderbook[] GetData()
        {
            List<IOrderbook> aResult = new List<IOrderbook>();  
            foreach( string strKey in m_aOrderbooks.Keys )
            {
                IOrderbook? oData = GetData(strKey);
                if( oData != null ) aResult.Add(oData); 
            }
            return aResult.ToArray();
        }

        public IOrderbook? GetData(string strSymbol)
        {
            IOrderbook? oResult = null;
            if( m_aOrderbooks.TryGetValue(strSymbol, out oResult) )
            {
                return oResult;
            }
            return null;
        }

        public void Put(string strSymbol, BitgetOrderBookUpdate oUpdate)
        {
            ReceiveCount++;
            IFuturesSymbol? oFound = m_aSymbols.FirstOrDefault(p => p.Symbol == strSymbol);
            if (oFound == null) return;
            IOrderbook oOrderbook = new BitgetOrderbook(oFound,oUpdate);
            m_aOrderbooks.AddOrUpdate(oFound.Symbol, p => oOrderbook, (s, p) => oOrderbook);
            return;
        }
    }
}
