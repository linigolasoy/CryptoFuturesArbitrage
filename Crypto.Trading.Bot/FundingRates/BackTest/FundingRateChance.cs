using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Trading.Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.BackTest
{

    internal class FundingPosition
    {
        public FundingPosition(FundingRateChance oChance, IFuturesSymbol oSymbol) 
        { 
            Chance = oChance;   
            Symbol = oSymbol;
        }
        public FundingRateChance Chance { get; }

        public IFuturesSymbol Symbol { get; }
        public IFundingRate? FundingRate{ get; internal set; } = null;

        public IFuturesBar[]? Bars { get; internal set; } = null;

        public decimal Price { get; internal set; } = 0;

        public decimal ProfitRealized { get; internal set; } = 0;
        public decimal ProfitUnRealized { get; internal set; } = 0;

    }


    internal class FundingRateChance
    {
        public enum ChanceStatus
        {
            Open,
            Closed
        }
        
        public FundingRateChance( FundingRateData oData, decimal nMoney ) 
        {
            FundingData = oData;    
            Money = nMoney;
            IFundingRate? oBuy = oData.FundingRates.OrderBy(p=> p.Rate).First();
            IFundingRate? oSell = oData.FundingRates.OrderBy(p => p.Rate).Last();
            if (oBuy == null || oSell == null) return;

            if( oBuy.Symbol.Exchange.ExchangeType == oSell.Symbol.Exchange.ExchangeType )
            {
                // Select only one
                IFuturesSymbol oSymbol = oBuy.Symbol;
                if( oBuy.Rate > 0 ) oBuy = null;
                else oSell = null;  

                IFuturesSymbol? oOther = oData.EquivalentSymbols.FirstOrDefault(p=> p.Exchange.ExchangeType != oSymbol.Exchange.ExchangeType);
                if (oOther == null) return;
                if( oBuy == null )
                {
                    PositionBuy = new FundingPosition(this, oOther);
                    PositionSell = new FundingPosition(this, oSell!.Symbol);
                }
                else
                {
                    PositionBuy = new FundingPosition(this, oBuy!.Symbol);
                    PositionSell = new FundingPosition(this, oOther);
                }
            }
            else
            {
                PositionBuy = new FundingPosition(this, oBuy!.Symbol);
                PositionSell = new FundingPosition(this, oSell!.Symbol);
            }

            ProfitPercent = ((oSell == null ?  0 : oSell.Rate) - (oBuy == null? 0: oBuy.Rate)) * 100.0M;
            ProfitPercent = Math.Round(ProfitPercent, 3);
            PositionBuy.FundingRate = oBuy;
            PositionSell.FundingRate = oSell;    

            return;
        }

        public ChanceStatus Status { get; private set; } = ChanceStatus.Open;
        public decimal ProfitPercent { get; } = 0;
        public decimal Money { get; } = 0;
        public DateTime? DateOpen { get; internal set; } = null;
        public DateTime? DateClose { get; internal set; } = null;

        public decimal Quantity { get; internal set; } = 0;
        public decimal ProfitRealized { get; internal set; } = 0;
        public decimal ProfitUnrealized { get; internal set; } = 0;

        public FundingRateData FundingData { get; }

        public FundingPosition? PositionBuy { get; set; } = null;
        public FundingPosition? PositionSell { get; set; } = null;


        public bool Start(IFuturesBar[] aBarsBuy, IFuturesBar[] aBarsSell )
        {
            if( PositionBuy == null || PositionSell == null ) return false; 
            DateTime dLimit = FundingData.FundingDate.DateTime.AddMinutes(-15);
            IFuturesBar? oBarBuy = aBarsBuy.FirstOrDefault(p => p.DateTime < FundingData.FundingDate.DateTime && p.DateTime >= dLimit);
            IFuturesBar? oBarSell = aBarsSell.FirstOrDefault(p => p.DateTime < FundingData.FundingDate.DateTime && p.DateTime >= dLimit);
            if (oBarBuy == null || oBarSell == null) return false;


            PositionBuy.Bars = aBarsBuy.Where(p => p.DateTime > dLimit).OrderBy(p => p.DateTime).ToArray();
            PositionSell.Bars = aBarsSell.Where(p => p.DateTime > dLimit).OrderBy(p => p.DateTime).ToArray();

            PositionBuy.Price = oBarBuy.Open;
            PositionSell.Price = oBarSell.Open;
            decimal nPercentSpread = PriceUtil.SpreadPercent(PositionBuy.Price, PositionSell.Price);
            if (nPercentSpread > this.ProfitPercent) return false;
            int nDecimals = Math.Min( PositionSell.Symbol.Decimals, PositionBuy.Symbol.Decimals);
            decimal nPriceMax = Math.Max(PositionBuy.Price, PositionSell.Price);
            decimal nMoney = this.Money * FundingData.FundingDate.Setup.Leverage / 2.0M;
            decimal nQuantity = Math.Round(nMoney / nPriceMax, nDecimals);
            if( nQuantity <= 0 ) return false;
            Quantity = nQuantity;
            DateOpen = oBarBuy.DateTime;
            DateClose = DateOpen.Value;
            CalculatePnl(oBarBuy, oBarSell);    
            return true;
        }


        /// <summary>
        /// Calculate profit or loss
        /// </summary>
        /// <param name="oBarBuy"></param>
        /// <param name="oBarSell"></param>
        private void CalculatePnl( IFuturesBar oBarBuy, IFuturesBar oBarSell )
        {
            if (PositionBuy == null || PositionSell == null) return;
            PositionBuy.ProfitUnRealized = Math.Round( (oBarBuy.Open - PositionBuy.Price) * Quantity, 2);
            PositionSell.ProfitUnRealized = Math.Round((PositionSell.Price - oBarSell.Open) * Quantity, 2);

            ProfitUnrealized = PositionBuy.ProfitUnRealized + PositionSell.ProfitUnRealized;
        }


        /// <summary>
        /// Collect funding rates
        /// </summary>
        private void CollectFundingRates(IFuturesBar oBarBuy, IFuturesBar oBarSell)
        {
            decimal nMoneyBuyOpen = (PositionBuy!.Price * Quantity);
            decimal nMoneyBuyClose = nMoneyBuyOpen + PositionBuy!.ProfitUnRealized;

            decimal nMoneySellOpen = (PositionSell!.Price * Quantity);
            decimal nMoneySellClose = nMoneySellOpen + PositionSell!.ProfitUnRealized;

            decimal nFeeBuy = ( nMoneyBuyOpen * PositionBuy!.Symbol.FeeTaker ) + (nMoneyBuyClose * PositionBuy!.Symbol.FeeTaker);
            decimal nFeeSell = (nMoneySellOpen * PositionSell!.Symbol.FeeMaker) + (nMoneySellClose * PositionSell!.Symbol.FeeMaker);

            decimal nFundingBuy = (-1.0M * PositionBuy!.FundingRate!.Rate) * nMoneyBuyClose;
            decimal nFundingSell =  (PositionSell!.FundingRate!.Rate) * nMoneySellClose;

            PositionBuy.ProfitRealized = Math.Round( PositionBuy.ProfitUnRealized + nFundingBuy - nFeeBuy, 2);
            PositionBuy.ProfitUnRealized = 0;


            PositionSell.ProfitRealized = Math.Round(PositionSell.ProfitUnRealized + nFundingSell - nFeeSell, 2);
            PositionSell.ProfitUnRealized = 0;

            ProfitUnrealized = 0;
            ProfitRealized = PositionBuy.ProfitRealized + PositionSell.ProfitRealized;
            Status = ChanceStatus.Closed;

        }

        /// <summary>
        /// Step to next bar
        /// </summary>
        internal void Step()
        {
            if (DateClose == null) throw new Exception("No close date");
            if (PositionBuy == null || PositionSell == null) throw new Exception("No position");
            if (PositionBuy.Bars == null || PositionSell.Bars == null) throw new Exception("No bars");
            IFuturesBar? oNextBuy = PositionBuy.Bars.FirstOrDefault(p=> p.DateTime > DateClose.Value);
            IFuturesBar? oNextSell = PositionSell.Bars.FirstOrDefault(p => p.DateTime > DateClose.Value);
            if( oNextBuy == null || oNextSell == null ) throw new Exception("No next bar");
            if( oNextBuy.DateTime != oNextSell.DateTime ) throw new Exception("Different dates");
            DateClose = oNextBuy.DateTime;
            CalculatePnl(oNextBuy, oNextSell);
            if ( oNextBuy.DateTime > FundingData.FundingDate.DateTime || oNextBuy.DateTime > FundingData.FundingDate.DateTime )
            {
                // TODO Lock funding profit
                CollectFundingRates(oNextBuy, oNextSell);  
            }

        }
    }
}
