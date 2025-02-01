using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{
    internal class ArbitrageChance : IArbitrageChance
    {
        private const decimal MAXIMUM_ORDERBOOK_DELAY = 500;
        public ArbitrageChance( IOrderbook oBookBuy, IOrderbook oBookSell ) 
        {
            BuyPosition = new ArbitragePosition(this, oBookBuy);
            SellPosition = new ArbitragePosition(this, oBookSell);
        }    
        public ChanceStatus ChanceStatus { get; set ; } = ChanceStatus.None;    

        public IArbitragePosition BuyPosition { get; private set; }

        public IArbitragePosition SellPosition { get; private set; }

        public decimal Profit { get; private set; } = 0;

        public decimal Quantity { get; private set; } = 0;

        public decimal BuyOpenPrice { get; set; } = 0;
        public decimal BuyClosePrice { get; private set; } = 0;

        public decimal SellOpenPrice { get; set; } = 0;
        public decimal SellClosePrice { get; private set; } = 0;

        public decimal Percent { get; private set; } = 0;


        private decimal CalculateDelay( IOrderbookPrice oPriceBuy, IOrderbookPrice oPriceSell )
        {
            double nDelayBuy = (oPriceBuy.Orderbook.ReceiveDate - oPriceBuy.Orderbook.UpdateDate).TotalMilliseconds;
            double nDelaySell = (oPriceSell.Orderbook.ReceiveDate - oPriceSell.Orderbook.UpdateDate).TotalMilliseconds;
            return (decimal) Math.Max(nDelayBuy, nDelaySell);
        }
        public bool CalculateArbitrage(decimal nMoney)
        {
            DateTime dNow = DateTime.Now;
            try
            {
                IOrderbookPrice? oPriceBuy = BuyPosition.Orderbook.GetBestPrice(true, null, nMoney);
                IOrderbookPrice? oPriceSell = SellPosition.Orderbook.GetBestPrice(false, null, nMoney);
                if (oPriceBuy == null || oPriceSell == null) return false;
                decimal nDelay = CalculateDelay(oPriceBuy, oPriceSell);

                if (oPriceBuy.Orderbook.Asks.Length < 5) return false;
                if (oPriceSell.Orderbook.Bids.Length < 5) return false;
                decimal nBuyPrice = Math.Max(oPriceBuy.Price, oPriceBuy.Orderbook.Asks[2].Price);
                decimal nSellPrice = Math.Min(oPriceSell.Price, oPriceSell.Orderbook.Bids[2].Price);
                int nPrecision = (BuyPosition.Symbol.QuantityDecimals < SellPosition.Symbol.QuantityDecimals ? BuyPosition.Symbol.QuantityDecimals : SellPosition.Symbol.QuantityDecimals);
                decimal nMaxPrice = Math.Max(oPriceBuy.Price, oPriceSell.Price);
                decimal nQuantity = Math.Round(nMoney / nMaxPrice, nPrecision);
                Quantity = nQuantity;
                BuyOpenPrice = nBuyPrice;
                SellOpenPrice = nSellPrice;
                Percent = Math.Round((SellOpenPrice - BuyOpenPrice) * 100.0M / SellOpenPrice, 3);
            }
            catch( Exception ex )
            {
                return false;
            }
            return true;
        }


        public bool CalculateProfit()
        {
            try
            {
                IOrderbookPrice? oPriceBuy = BuyPosition.Orderbook.GetBestPrice(false, Quantity, null);
                IOrderbookPrice? oPriceSell = SellPosition.Orderbook.GetBestPrice(true, Quantity, null);
                if (oPriceBuy == null || oPriceSell == null) return false;
                if (BuyPosition.Position == null || SellPosition.Position == null) return false;
                if (oPriceBuy.Orderbook.Bids.Length < 5) return false;
                if (oPriceSell.Orderbook.Asks.Length < 5) return false;


                decimal nBuyPrice = Math.Min(oPriceBuy.Price, oPriceBuy.Orderbook.Bids[2].Price);
                decimal nSellPrice = Math.Max(oPriceSell.Price, oPriceSell.Orderbook.Asks[2].Price);

                decimal nProfitBuy = (nBuyPrice - this.BuyOpenPrice) * Quantity;
                decimal nProfitSell = (this.SellOpenPrice - nSellPrice) * Quantity;
                Profit = nProfitBuy + nProfitSell;
                BuyClosePrice = nBuyPrice;
                SellClosePrice = nSellPrice;
            }
            catch( Exception ex ) { return false; }
            return true;
        }



        public void Reset()
        {
            this.ChanceStatus =  ChanceStatus.None;
            this.BuyClosePrice = 0;
            this.SellClosePrice = 0;
            this.Profit = 0;
            this.Quantity = 0;
            this.BuyOpenPrice = 0;
            this.BuyClosePrice = 0;
            this.SellOpenPrice = 0;
            this.SellClosePrice = 0;
            this.Percent = 0;
            this.BuyPosition.Position = null;
            this.SellPosition.Position = null;
        }
    }
}
