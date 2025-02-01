using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{

    internal class BaseOrderbookPrice : IOrderbookPrice
    {
        public BaseOrderbookPrice( IOrderbook oOrderbook, decimal nPrice, decimal nVolume ) 
        { 
            Orderbook = oOrderbook;
            Price = nPrice;
            Volume = nVolume;
        }
        public IOrderbook Orderbook { get; }  
        public decimal Price { get; } = 0;

        public decimal Volume { get; } = 0;
    }

    internal class BaseOrderbook: IOrderbook
    {
        public BaseOrderbook( IFuturesSymbol oSymbol, DateTime dUpdateDate, DateTime dReceiveDate )
        {
            Symbol = oSymbol;
            ReceiveDate = DateTime.Now;
            UpdateDate = dUpdateDate;   
            ReceiveDate = dReceiveDate;
        }
        public DateTime UpdateDate { get; internal set; }
        public DateTime ReceiveDate { get; internal set; }

        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice[] Asks { get; internal set; } = Array.Empty<IOrderbookPrice>();

        public IOrderbookPrice[] Bids { get; internal set; } = Array.Empty<IOrderbookPrice>();

        /// <summary>
        /// return best price
        /// </summary>
        /// <param name="aPrices"></param>
        /// <param name="nQuantity"></param>
        /// <param name="nMoney"></param>
        /// <returns></returns>
        private IOrderbookPrice? GetBestPriceArray(IOrderbookPrice[] aPrices, decimal? nQuantity = null, decimal? nMoney = null)
        {
            decimal nQuantityActual = 0;
            decimal nPriceActual = 0;
            foreach( var oPrice in aPrices )
            {
                nQuantityActual += oPrice.Volume;
                nPriceActual = oPrice.Price;
                if( nQuantity != null )
                {
                    if( nQuantityActual >= nQuantity.Value )
                    {
                        return new BaseOrderbookPrice(this, nPriceActual, nQuantityActual);
                    }
                }
                else if( nMoney != null )
                {
                    decimal nMoneyActual = nQuantityActual * nPriceActual;
                    if( nMoneyActual >= nMoney.Value )
                    {
                        return new BaseOrderbookPrice(this, nPriceActual, nQuantityActual);
                    }
                }
            }
            return null;
        }
        public IOrderbookPrice? GetBestPrice(bool bAsk, decimal? nQuantity = null, decimal? nMoney = null)
        {
            if( bAsk ) return GetBestPriceArray(Asks, nQuantity, nMoney);
            return GetBestPriceArray(Bids, nQuantity, nMoney);
        }
        public void Update(IOrderbook oNew)
        {
            this.UpdateDate = oNew.UpdateDate;
            this.ReceiveDate = DateTime.Now;
            this.Asks = oNew.Asks;
            this.Bids = oNew.Bids;
            return;
        }

    }
}
