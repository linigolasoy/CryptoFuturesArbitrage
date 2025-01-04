using Crypto.Interface.Futures;
using Crypto.Trading.Bot.Common;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates
{
    internal class FundingRateStrategy : IBotStrategy
    {
        public FundingRateStrategy( ITradingBot oBot ) 
        { 
            Bot = oBot;
        }
        public ITradingBot Bot { get; }


        public IBotSymbolData? CreateSymbolData( IBotExchangeData oData, IFuturesSymbol oSymbol )
        {
            return new BaseSymbolData(oData, oSymbol);  
        }
        /// <summary>
        /// Evaluate if symbol has to do
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        public bool EvalSymbol(IBotSymbolData oData)
        {
            if( oData.Symbol.Quote != "USDT") return false;
            return true;
        }



        private static decimal nMaxBuy = -999;
        private static decimal nMaxSell = -999;
        /// <summary>
        /// Return percentage to gain
        /// </summary>
        /// <param name="oBuySymbol"></param>
        /// <param name="oSellSymbol"></param>
        /// <returns></returns>
        private decimal? PercentGain(IBotSymbolData oBuySymbol, IBotSymbolData oSellSymbol)
        {
            if (oBuySymbol.FundingRate == null) return null;
            if (oSellSymbol.FundingRate == null) return null;
            decimal nRateBuy = oBuySymbol.FundingRate.Rate;
            decimal nRateSell = oSellSymbol.FundingRate.Rate;
            decimal nBuyPercent = (-nRateBuy) * 100M;
            decimal nSellPercent = (nRateSell) * 100M;

            if (nBuyPercent > nMaxBuy)
            {
                nMaxBuy = nBuyPercent;
                Bot.Logger.Info($"   MaxBuy {nMaxBuy}");
            }
            if (nSellPercent > nMaxSell)
            {
                nMaxSell = nSellPercent;
                Bot.Logger.Info($"   MaxSell {nMaxSell}");
            }

            if (oBuySymbol.FundingRate.NextSettle < oSellSymbol.FundingRate.NextSettle)
            {
                return nBuyPercent;
            }
            if (oBuySymbol.FundingRate.NextSettle > oSellSymbol.FundingRate.NextSettle)
            {
                return nSellPercent;
            }

            return nBuyPercent + nSellPercent;


        }

        private static decimal MaxRateFound = -999M;
        /// <summary>
        /// Find chances
        /// </summary>
        /// <param name="oBuyExchange"></param>
        /// <param name="oSellExchange"></param>
        private void FindChances( IBotExchangeData oBuyExchange, IBotExchangeData oSellExchange ) 
        {
            IBotSymbolData[] aBuySymbols = oBuyExchange.Symbols!.Where( p=> p.FundingRate != null /* && p.Orderbook != null */ ).ToArray();
            IBotSymbolData[] aSellSymbols = oSellExchange.Symbols!.Where(p => p.FundingRate != null /* && p.Orderbook != null */).ToArray();

            decimal nMaxRate = -999;

            foreach( IBotSymbolData oBuySymbol in aBuySymbols )
            {
                IBotSymbolData? oSellSymbol = aSellSymbols.FirstOrDefault(p=> p.Symbol.Base == oBuySymbol.Symbol.Base && p.Symbol.Quote == oBuySymbol.Symbol.Quote);
                if (oSellSymbol == null) continue;

                decimal? nPercentGain = PercentGain(oBuySymbol, oSellSymbol);
                if( nPercentGain == null ) continue;
                /*
                if(oBuySymbol.FundingRate == null || oSellSymbol.FundingRate == null ) continue;    
                // Not valid funding rates
                if (oSellSymbol.FundingRate.Rate <= 0 && oBuySymbol.FundingRate.Rate >= 0) continue;
                decimal nRate = 0;
                if( oBuySymbol.FundingRate.NextSettle < oSellSymbol.FundingRate.NextSettle )
                {
                    nRate = -oBuySymbol.FundingRate.Rate;
                    continue;
                }
                else if (oBuySymbol.FundingRate.NextSettle > oSellSymbol.FundingRate.NextSettle)
                {
                    nRate = oSellSymbol.FundingRate.Rate;
                    continue;
                }
                else
                {
                    if( oBuySymbol.FundingRate.Rate < 0 && oSellSymbol.FundingRate.Rate >= 0 )
                    {
                        nRate = oSellSymbol.FundingRate.Rate - oBuySymbol.FundingRate.Rate;
                    }
                    else if( oBuySymbol.FundingRate.Rate < 0 && oSellSymbol.FundingRate.Rate < 0 )
                    {
                        nRate = oBuySymbol.FundingRate.Rate - oSellSymbol.FundingRate.Rate;
                    }
                    else if (oBuySymbol.FundingRate.Rate >= 0 && oSellSymbol.FundingRate.Rate >= 0)
                    {
                        nRate = oSellSymbol.FundingRate.Rate - oBuySymbol.FundingRate.Rate;
                    }
                    else if (oBuySymbol.FundingRate.Rate >= 0 && oSellSymbol.FundingRate.Rate < 0)
                    {
                        continue;
                    }
                }
                if (nRate <= 0) continue;
                nRate *= 100;
                */
                decimal nRate = nPercentGain.Value;
                if (nRate > nMaxRate)
                {
                    nMaxRate = nRate;
                }
                if (nRate <= Bot.Setup.PercentMinimum) continue;

            }
            if( nMaxRate > 0  && nMaxRate > MaxRateFound )
            {
                MaxRateFound = nMaxRate;
                Bot.Logger.Info($"Found max {MaxRateFound}");
            }


        }


        /// <summary>
        /// Two side changes
        /// </summary>
        /// <param name="aData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Process(IBotExchangeData[] aData)
        {
            foreach( IBotExchangeData oBuyExchange in aData ) 
            { 
                foreach( IBotExchangeData oSellExchange in aData )
                {
                    if (oSellExchange.Exchange.ExchangeType == oBuyExchange.Exchange.ExchangeType) continue;
                    FindChances( oBuyExchange, oSellExchange ); 
                }
            }

            return;
        }
    }
}
