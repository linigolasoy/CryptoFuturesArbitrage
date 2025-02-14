using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BaseOrderbookPartial : IOrderbook
    {
        public BaseOrderbookPartial(IFuturesSymbol oSymbol, DateTime dUpdateDate, DateTime dReceiveDate)
        {
            Symbol = oSymbol;
            UpdateDate = dUpdateDate;
            ReceiveDate = dReceiveDate;
        }
        public DateTime UpdateDate { get; internal set; }
        public DateTime ReceiveDate { get; internal set; }

        public IFuturesSymbol Symbol { get; }

        private ConcurrentDictionary<decimal, decimal> m_aAsks = new ConcurrentDictionary<decimal, decimal>();
        private ConcurrentDictionary<decimal, decimal> m_aBids = new ConcurrentDictionary<decimal, decimal>();  
        public IOrderbookPrice[] Asks { get => GetDict(true); } // internal set; } = Array.Empty<IOrderbookPrice>();

        public IOrderbookPrice[] Bids { get => GetDict(false); } // internal set; } = Array.Empty<IOrderbookPrice>();



        private IOrderbookPrice[] GetDict( bool bAsks )
        {
            List<IOrderbookPrice> aResult = new List<IOrderbookPrice>();
            ConcurrentDictionary<decimal, decimal> oDict = (bAsks ? m_aAsks: m_aBids);
            foreach(decimal nKey in oDict.Keys )
            {
                decimal nValue = 0;
                if( oDict.TryGetValue(nKey, out nValue) )
                {
                    aResult.Add( new BaseOrderbookPrice(this, nKey, nValue ) );  
                }
            }
            if( bAsks ) return aResult.OrderBy(p=> p.Price ).ToArray(); 
            return aResult.OrderByDescending(p=> p.Price ).ToArray();   
        }
        /// <summary>
        /// return best price
        /// </summary>
        /// <param name="aPrices"></param>
        /// <param name="nQuantity"></param>
        /// <param name="nMoney"></param>
        /// <returns></returns>
        private IOrderbookPrice? GetBestPriceArray(IOrderbookPrice[] aPrices, int nMinPosition, decimal? nQuantity = null, decimal? nMoney = null)
        {
            decimal nQuantityActual = 0;
            decimal nPriceActual = 0;
            
            for ( int i = 0; i < aPrices.Length; i++ )
            {
                IOrderbookPrice oPrice = aPrices[i];
                nQuantityActual += oPrice.Volume;
                nPriceActual = oPrice.Price;
                if (nQuantity != null)
                {
                    if (nQuantityActual >= nQuantity.Value && i >= nMinPosition)
                    {
                        return new BaseOrderbookPrice(this, nPriceActual, nQuantityActual);
                    }
                }
                else if (nMoney != null)
                {
                    decimal nMoneyActual = nQuantityActual * nPriceActual;
                    if (nMoneyActual >= nMoney.Value && i >= nMinPosition)
                    {
                        return new BaseOrderbookPrice(this, nPriceActual, nQuantityActual);
                    }
                }
            }
            return null;
        }
        public IOrderbookPrice? GetBestPrice(bool bAsk, int nMinPosition, decimal? nQuantity = null, decimal? nMoney = null)
        {
            if (bAsk) return GetBestPriceArray(Asks, nMinPosition, nQuantity, nMoney);
            return GetBestPriceArray(Bids, nMinPosition, nQuantity, nMoney);
        }
        public void Update(IOrderbook oNew)
        {
            this.UpdateDate = oNew.UpdateDate;
            PutAsks(oNew.Asks);  
            PutBids(oNew.Bids);
            return;
        }

        private void UpdateDict( ConcurrentDictionary<decimal,decimal> oDict, IOrderbookPrice[] aNews )
        {
            foreach( var oNew in aNews )
            {
                if( oNew.Volume <= 0 )
                {
                    decimal nOut = 0;
                    bool bResult = oDict.TryRemove(oNew.Price, out nOut );
                    if ( !bResult )
                    {
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    oDict.AddOrUpdate( oNew.Price, p=> oNew.Volume, (p, v)=> oNew.Volume );
                }
            }
        }
        internal void PutAsks(IOrderbookPrice[] aNewData )
        {
            UpdateDict(m_aAsks, aNewData);  
        }
        internal void PutBids(IOrderbookPrice[] aNewData)
        {
            UpdateDict(m_aBids, aNewData);
        }
    }
}
