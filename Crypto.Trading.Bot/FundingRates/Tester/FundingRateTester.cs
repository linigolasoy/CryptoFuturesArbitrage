using Crypto.Interface.Futures;
using Crypto.Trading.Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Tester
{
    internal class FundingRateTester : IBotStrategy
    {

        private enum TesterStatus
        {
            None,
            FundingHistory,
            SelectStartDate,
            SelectBest,
            BarLoop
        }

        private TesterStatus m_eStatus = TesterStatus.None;

        private DateTime m_dStartDate  = DateTime.Today.AddDays(1);
        private DateTime m_dActualDate = DateTime.Today.AddDays(1);

        private FundingRateChance? m_oActualChance = null;  

        public FundingRateTester(ITradingBot oBot)
        {
            Bot = oBot;
        }
        public ITradingBot Bot { get; }


        public IBotSymbolData? CreateSymbolData(IBotExchangeData oData, IFuturesSymbol oSymbol)
        {
            return new FundingDataHistory(oData, oSymbol);
        }

        public bool EvalSymbol(IBotSymbolData oData)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Load funding rate history
        /// </summary>
        /// <param name="aData"></param>
        /// <returns></returns>
        private async Task LoadFundingHistory(IBotExchangeData[] aData)
        {
            try
            {
                DateTime dFrom = DateTime.Today.AddMonths(-2);
                foreach (IBotExchangeData oData in aData)
                {
                    if (oData.Symbols == null) continue;
                    Bot.Logger.Info($"   Load funding hisory for {oData.Exchange.ExchangeType.ToString()}");
                    int nTotal = oData.Symbols.Length;
                    int nActual = 0;
                    int nPercent = 0;
                    int nLastPercent = 0;
                    foreach ( var oSymbolData in oData.Symbols) 
                    { 
                        FundingDataHistory oDataHistory = (FundingDataHistory)oSymbolData;
                        await oDataHistory.LoadHistory(dFrom);
                        nActual++;
                        nPercent = (100 * nActual) / nTotal;    
                        if( nLastPercent != nPercent && nPercent % 5 == 0)
                        {
                            nLastPercent = nPercent;
                            Bot.Logger.Info($"      {nLastPercent} %");
                        }
                    }
                    await Task.Delay(1000); 
                }
                m_eStatus = TesterStatus.SelectStartDate;
            }
            catch( Exception e)
            {
                Bot.Logger.Error("   Error loading Funding History", e);
                await Task.Delay(2000);
            }
        }


        /// <summary>
        /// Select start date
        /// </summary>
        /// <returns></returns>
        private async Task SelectStartDate(IBotExchangeData[] aData)
        {
            DateTime? dFound = null;
            try
            {
                foreach (IBotExchangeData oData in aData)
                {
                    if (oData.Symbols == null) continue;
                    foreach (IBotSymbolData oSymbolData in oData.Symbols)
                    {
                        FundingDataHistory oHistory = (FundingDataHistory)oSymbolData;
                        if (oHistory.FundingHistory == null || oHistory.FundingHistory.Length <= 0 ) continue;

                        DateTime dMinimum = oHistory.FundingHistory.Select(p => p.DateTime).Min().Date;
                        if (dFound == null || dMinimum < dFound.Value)
                        {
                            dFound = dMinimum;
                        }

                    }
                }
                if (dFound != null)
                {
                    m_dStartDate = dFound.Value;
                    m_dActualDate = dFound.Value;
                    Bot.Logger.Info($"   Start Date is {m_dStartDate.ToShortDateString()}");
                    m_eStatus = TesterStatus.SelectBest;
                }
            }
            catch( Exception ex )
            {
                Bot.Logger.Error("   ERROR on getting start date", ex);
            }
            await Task.Delay(1000);
            return;
        }



        /// <summary>
        /// Next date
        /// </summary>
        /// <param name="aData"></param>
        /// <returns></returns>
        private DateTime? NextDate(IBotExchangeData[] aData, DateTime dActual)
        {
            DateTime? dResult = null;
            foreach (IBotExchangeData oData in aData)
            {
                if (oData.Symbols == null) continue;
            }
            return dResult;
        }
        /// <summary>
        /// Select best chance
        /// </summary>
        /// <returns></returns>
            private async Task SelectBest(IBotExchangeData[] aData)
        {
            try
            {
                DateTime dActual = m_dActualDate;
                decimal nBestProfit = -100;
                while( m_oActualChance == null && dActual < DateTime.Today )
                {
                    FundingRateChance? oBest = null;
                    foreach( IBotExchangeData oDataBuy in aData  )
                    {
                        if (oDataBuy.Symbols == null) continue;
                        foreach( IBotExchangeData oDataSell in aData )
                        {
                            if (oDataSell.Symbols == null) continue;
                            if (oDataBuy.Exchange.ExchangeType == oDataSell.Exchange.ExchangeType) continue;
                            foreach( IBotSymbolData oSymbolBuy in oDataBuy.Symbols)
                            {
                                IBotSymbolData? oSymbolSell = oDataSell.Symbols.FirstOrDefault(p => p.Symbol.Base == oSymbolBuy.Symbol.Base && p.Symbol.Quote == oSymbolBuy.Symbol.Quote);
                                if (oSymbolSell == null) continue;

                                FundingRateChance oChance = new FundingRateChance(oSymbolBuy, oSymbolSell);

                                decimal nProfit = oChance.ProfitOnDate(dActual);
                                if (nProfit < 0) continue;
                                if( nProfit > nBestProfit )
                                {
                                    oBest = oChance;    
                                    nBestProfit = nProfit;  
                                }
                            }
                        }
                    }
                    if( oBest != null )
                    {
                        m_oActualChance = oBest;    
                    }
                    else
                    {
                        DateTime? dNext = NextDate(aData, dActual);
                        if (dNext == null) break;
                        dActual = dNext.Value;
                    }

                }
                await Task.Delay(1000);
            }
            catch ( Exception ex )
            {
                Bot.Logger.Error($"   ERROR selecting best for {m_dActualDate.ToShortDateString()}", ex);
            }
        }
        /// <summary>
        /// Switch status
        /// </summary>
        /// <param name="aData"></param>
        /// <returns></returns>
        public async Task Process(IBotExchangeData[] aData)
        {
            switch( m_eStatus )
            {
                case TesterStatus.None:
                    m_eStatus = TesterStatus.FundingHistory;
                    break;
                case TesterStatus.FundingHistory:
                    await LoadFundingHistory(aData);    
                    break;
                case TesterStatus.SelectStartDate:
                    await SelectStartDate(aData);   
                    break;
                case TesterStatus.SelectBest:
                    await SelectBest(aData);
                    break;
                case TesterStatus.BarLoop:
                    break;
            }
        }
    }
}
