using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{

    internal class ArbitrageMoney : IArbitrageMoney
    {
        public ArbitrageMoney( IArbitrageChance oChance ) 
        { 
            Chance = oChance;
        }
        public IArbitrageChance Chance { get; }

        public decimal Profit { get; set; } = 0;

        public decimal Quantity { get; set; } = 0;

        public decimal BuyOpenPrice { get; set; } = 0;
        public decimal SellOpenPrice { get; set; } = 0;

        public decimal BuyClosePrice { get; set; } = 0;

        public decimal SellClosePrice { get; set; } = 0;

        public decimal Percent { get; internal set; } = 0;
    }

    internal class ArbitrageChance : IArbitrageChance
    {
        private const decimal MAXIMUM_ORDERBOOK_DELAY = 500;
        private const int ORDERBOOK_POS = 1;
        public ArbitrageChance( IOrderbook[] aOrderbooks ) /* oBookBuy, IOrderbook oBookSell ) */
        {
            // BuyPosition = new ArbitragePosition(this, oBookBuy);
            // SellPosition = new ArbitragePosition(this, oBookSell);
            Currency = aOrderbooks[0].Symbol.Base;
            Money = new ArbitrageMoney(this);
            Orderbooks = aOrderbooks;   
        }

        public string Currency { get; }
        public ChanceStatus ChanceStatus { get; set ; } = ChanceStatus.None;    
        public IOrderbook[] Orderbooks { get; }
        public IArbitragePosition? BuyPosition { get; private set; }

        public IArbitragePosition? SellPosition { get; private set; }

        public IArbitrageMoney Money { get; private set; }


        public bool SetPositions(IFuturesPosition oLongPosition, IFuturesPosition oShortPosition)
        {
            IOrderbook? oLongBook = Orderbooks.FirstOrDefault(p => p.Symbol.Symbol == oLongPosition.Symbol.Symbol && p.Symbol.Exchange.ExchangeType == oLongPosition.Symbol.Exchange.ExchangeType);
            if (oLongBook == null) return false;

            IOrderbook? oShortBook = Orderbooks.FirstOrDefault(p => p.Symbol.Symbol == oShortPosition.Symbol.Symbol && p.Symbol.Exchange.ExchangeType == oShortPosition.Symbol.Exchange.ExchangeType);
            if (oShortBook == null) return false;

            var oPosBuy = new ArbitragePosition(this, oLongBook);
            oPosBuy.Position = oLongPosition;
            BuyPosition = oPosBuy;

            var oPosSell = new ArbitragePosition(this, oShortBook);
            oPosSell.Position = oShortPosition;
            SellPosition = oPosSell;
            ChanceStatus = ChanceStatus.Position;

            ArbitrageMoney oMoney = new ArbitrageMoney(this);
            oMoney.Quantity = oLongPosition.Quantity;
            oMoney.BuyOpenPrice = oLongPosition.AveragePrice;
            oMoney.SellOpenPrice = oShortPosition.AveragePrice;
            oMoney.Percent = Math.Round((oMoney.SellOpenPrice - oMoney.BuyOpenPrice) * 100.0M / oMoney.SellOpenPrice, 3);
            this.Money = oMoney;    
            return true;
        }
        private decimal CalculateDelay( IOrderbookPrice oPriceBuy, IOrderbookPrice oPriceSell )
        {
            double nDelayBuy = (oPriceBuy.Orderbook.ReceiveDate - oPriceBuy.Orderbook.UpdateDate).TotalMilliseconds;
            double nDelaySell = (oPriceSell.Orderbook.ReceiveDate - oPriceSell.Orderbook.UpdateDate).TotalMilliseconds;
            return (decimal) Math.Max(nDelayBuy, nDelaySell);
        }
        public bool CalculateArbitrage(decimal nMoney)
        {
            BuyPosition = null;
            SellPosition = null;

            DateTime dNow = DateTime.Now;
            try
            {
                IOrderbookPrice? oBestBuy = null;
                IOrderbookPrice? oBestSell = null;

                foreach (IOrderbook oBook in Orderbooks)
                {
                    IOrderbookPrice? oPriceBuy = oBook.GetBestPrice(true, ORDERBOOK_POS, null, nMoney);
                    IOrderbookPrice? oPriceSell = oBook.GetBestPrice(false, ORDERBOOK_POS, null, nMoney);
                    if (oPriceBuy == null || oPriceSell == null) continue;

                    if (oBestBuy == null)
                    {
                        oBestBuy = oPriceBuy;
                    }
                    else if (oPriceBuy.Price < oBestBuy.Price )
                    {
                        oBestBuy = oPriceBuy;
                    }
                    if (oBestSell == null)
                    {
                        oBestSell = oPriceSell;
                    }
                    else if (oPriceSell.Price > oBestSell.Price)
                    {
                        oBestSell = oPriceSell;
                    }
                }

                if (oBestBuy == null || oBestSell == null) return false;
                if( oBestBuy.Orderbook.Symbol.Exchange.ExchangeType == oBestSell.Orderbook.Symbol.Exchange.ExchangeType ) return false;
                if( oBestBuy.Price >= oBestSell.Price ) return false;
                BuyPosition = new ArbitragePosition(this, oBestBuy.Orderbook);
                SellPosition = new ArbitragePosition(this, oBestSell.Orderbook);

                int nPrecision = (BuyPosition.Symbol.QuantityDecimals < SellPosition.Symbol.QuantityDecimals ? BuyPosition.Symbol.QuantityDecimals : SellPosition.Symbol.QuantityDecimals);
                decimal nMaxPrice = Math.Max(oBestBuy.Price, oBestSell.Price);
                decimal nQuantity = Math.Round(nMoney / nMaxPrice, nPrecision);
                ArbitrageMoney oMoney = new ArbitrageMoney(this);
                oMoney.Quantity = nQuantity;
                oMoney.BuyOpenPrice = oBestBuy.Price;
                oMoney.SellOpenPrice = oBestSell.Price;
                oMoney.Percent = Math.Round((oMoney.SellOpenPrice - oMoney.BuyOpenPrice) * 100.0M / oMoney.SellOpenPrice, 3);

                Money = oMoney;

                /*
                IOrderbookPrice? oPriceBuy = BuyPosition.Orderbook.GetBestPrice(true, null, nMoney);
                IOrderbookPrice? oPriceSell = SellPosition.Orderbook.GetBestPrice(false, null, nMoney);
                if (oPriceBuy == null || oPriceSell == null) return false;
                decimal nDelay = CalculateDelay(oPriceBuy, oPriceSell);

                if (oPriceBuy.Orderbook.Asks.Length < 5) return false;
                if (oPriceSell.Orderbook.Bids.Length < 5) return false;
                decimal nBuyPrice = Math.Max(oPriceBuy.Price, oPriceBuy.Orderbook.Asks[2].Price);
                decimal nSellPrice = Math.Min(oPriceSell.Price, oPriceSell.Orderbook.Bids[2].Price);
                */
            }
            catch ( Exception ex )
            {
                return false;
            }
            return true;
        }


        public bool CalculateProfit()
        {
            try
            {
                if (Money == null || BuyPosition == null || SellPosition == null ) return false;
                IOrderbookPrice? oPriceBuy = BuyPosition.Orderbook.GetBestPrice(false, ORDERBOOK_POS, Money.Quantity, null);
                IOrderbookPrice? oPriceSell = SellPosition.Orderbook.GetBestPrice(true, ORDERBOOK_POS, Money.Quantity, null);
                if (oPriceBuy == null || oPriceSell == null) return false;
                if (BuyPosition.Position == null || SellPosition.Position == null) return false;
                // if (oPriceBuy.Orderbook.Bids.Length < 5) return false;
                // if (oPriceSell.Orderbook.Asks.Length < 5) return false;


                decimal nBuyPrice = oPriceBuy.Price; // Math.Min(oPriceBuy.Price, oPriceBuy.Orderbook.Bids[2].Price);
                decimal nSellPrice = oPriceSell.Price; //  Math.Max(oPriceSell.Price, oPriceSell.Orderbook.Asks[2].Price);

                decimal nProfitBuy = (nBuyPrice - this.Money.BuyOpenPrice) * Money.Quantity;
                decimal nProfitSell = (this.Money.SellOpenPrice - nSellPrice) * Money.Quantity;
                Money.Profit = nProfitBuy + nProfitSell;
                Money.BuyClosePrice = nBuyPrice;
                Money.SellClosePrice = nSellPrice;
            }
            catch( Exception ex ) { return false; }
            return true;
        }



        public void Reset()
        {
            this.ChanceStatus =  ChanceStatus.None;
            this.Money = new ArbitrageMoney(this);  
            this.BuyPosition = null;
            this.SellPosition = null;
        }
    }
}
