using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{
    internal class FundingPair: IFundingPair
    {
        internal FundingPair(IFundingDate oDate, IFuturesSymbol oSymbol1, IFuturesSymbol oSymbol2, IFundingRate oRate1, IFundingRate? oRate2)
        {
            FundingDate = oDate;
            decimal nBuyRate = 0;
            decimal nSellRate = 0;
            if (oRate2 == null)
            {
                if (oRate1.Rate > 0)
                {
                    SellSymbol = oSymbol1;
                    SellFunding = oRate1;
                    BuySymbol = oSymbol2;
                    BuyFunding = null;
                    nSellRate = oRate1.Rate;
                }
                else
                {
                    BuySymbol = oSymbol1;
                    BuyFunding = oRate1;
                    SellSymbol = oSymbol2;
                    SellFunding = null;
                    nBuyRate = oRate1.Rate;

                }
            }
            else
            {
                if (oRate1.Rate > oRate2.Rate)
                {
                    BuySymbol = oSymbol2;
                    BuyFunding = oRate2;
                    SellSymbol = oSymbol1;
                    SellFunding = oRate1;
                }
                else
                {
                    BuySymbol = oSymbol1;
                    BuyFunding = oRate1;
                    SellSymbol = oSymbol2;
                    SellFunding = oRate2;
                }
                nBuyRate = BuyFunding.Rate;
                nSellRate = SellFunding.Rate;
            }
            decimal nRate = nSellRate - nBuyRate;
            if (nRate < 0)
            {
                return;
            }
            decimal nFees = BuySymbol.FeeTaker + BuySymbol.FeeMaker + SellSymbol.FeeMaker + SellSymbol.FeeTaker;
            Percent = Math.Round( (nRate - nFees) * 100M, 3);
            return;
        }
        public IFundingDate FundingDate { get; }

        public decimal Percent { get; }

        public IFuturesSymbol BuySymbol { get; }

        public IFuturesSymbol SellSymbol { get; }

        public IFundingRate? BuyFunding { get; }

        public IFundingRate? SellFunding { get; }

        /// <summary>
        /// Create different pairs
        /// </summary>
        /// <param name="oDate"></param>
        /// <param name="aSymbols"></param>
        /// <param name="aRates"></param>
        /// <returns></returns>
        public static FundingPair[]? Create(IFundingDate oDate, IFuturesSymbol[] aSymbols, IFundingRate[] aRates)
        {
            IFuturesSymbol[] aNonExistent = aSymbols.Where(p => !aRates.Any(q => q.Symbol.Exchange.ExchangeType == p.Exchange.ExchangeType)).ToArray();
            IFuturesSymbol[] aExistent = aSymbols.Where(p => !aNonExistent.Any(q => q.Exchange.ExchangeType == p.Exchange.ExchangeType)).ToArray();

            List<FundingPair> aResult = new List<FundingPair>();
            // Match existent
            for (int i = 0; i < aExistent.Length; i++)
            {
                for (int j = i + 1; j < aExistent.Length; j++)
                {
                    IFuturesSymbol oSymbol1 = aExistent[i];
                    IFuturesSymbol oSymbol2 = aExistent[j];
                    IFundingRate oRate1 = aRates.First(p => p.Symbol.Exchange.ExchangeType == oSymbol1.Exchange.ExchangeType);
                    IFundingRate oRate2 = aRates.First(p => p.Symbol.Exchange.ExchangeType == oSymbol2.Exchange.ExchangeType);
                    aResult.Add(new FundingPair(oDate, oSymbol1, oSymbol2, oRate1, oRate2));
                }
            }
            // Match non existent
            foreach (IFuturesSymbol oNonExistent in aNonExistent)
            {
                foreach (IFuturesSymbol oExistent in aExistent)
                {
                    IFundingRate oRateExist = aRates.First(p => p.Symbol.Exchange.ExchangeType == oExistent.Exchange.ExchangeType);
                    aResult.Add(new FundingPair(oDate, oExistent, oNonExistent, oRateExist, null));

                }
            }
            return aResult.ToArray();
        }
    }
}
