using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates
{
    internal partial class FundingRatesBot 
    {

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private Task? m_oMainTask = null;

        /// <summary>
        /// Update funding rates
        /// </summary>
        /// <returns></returns>
        private async Task UpdateFundingRates()
        {
            if (m_aDictExchanges == null) return;
            foreach (ExchangeType eType in m_aDictExchanges.Keys)
            {
                FundingBotExchangeData oData = m_aDictExchanges[eType];
                if( oData.FundingSnapshots != null )
                {
                    DateTime dNow = DateTime.Now;
                    IFundingRateSnapShot[] aExpired = oData.FundingSnapshots.Where(p => p.NextSettle <= dNow).ToArray();
                    IFundingRateSnapShot[] aCurrent = oData.FundingSnapshots.Where(p => p.NextSettle > dNow).ToArray();
                    if (aExpired.Length <= 0) continue;
                    Logger.Info(string.Format("             Get funding rates of {0}.- {1} dates expired", eType.ToString(), aExpired.Length));
                    IFundingRateSnapShot[]? aNewExpired = await oData.Exchange.GetFundingRates( aExpired.Select(p=> p.Symbol).ToArray() ); 
                    if( aNewExpired != null )
                    {
                        List<IFundingRateSnapShot> aNewFundings = new List<IFundingRateSnapShot>();
                        aNewFundings.AddRange( aNewExpired );   
                        aNewFundings.AddRange( aCurrent );
                        oData.FundingSnapshots = aNewFundings.ToArray();
                        Logger.Info(string.Format("             Got funding rates of {0}.- {1} dates expired", eType.ToString(), aExpired.Length));
                    }
                    continue;
                }

                Logger.Info(string.Format("             Get funding rates of {0}", eType.ToString()));
                IFundingRateSnapShot[]? oNew = await oData.Exchange.GetFundingRates(oData.Symbols!);
                if( oNew == null ) { Logger.Error("No funding rates"); }
                else { oData.FundingSnapshots = oNew; } 

            }

        }


        /// <summary>
        /// Chances 
        /// </summary>
        private void FindChances()
        {
            /*
            List<FundingChance> aChances = new List<FundingChance>();

            if (m_aDictExchanges == null) return;
            ExchangeType eType1 = m_aDictExchanges.Keys.First();
            ExchangeType eType2 = m_aDictExchanges.Keys.Last();

            FundingBotExchangeData oData1 = m_aDictExchanges[eType1];
            if (oData1.Websocket == null) return;
            if (oData1.FundingSnapshots == null) return;

            FundingBotExchangeData oData2 = m_aDictExchanges[eType2];
            if (oData2.Websocket == null) return;
            if (oData2.FundingSnapshots == null) return;

            IWebsocketManager<ITicker> oManager1 = oData1.Websocket.TickerManager;
            IWebsocketManager<ITicker> oManager2 = oData2.Websocket.TickerManager;

            ITicker[] aTikers2 = oManager2.GetData();
            int nFound1 = 0;
            int nFound2 = 0;

            foreach( ITicker oTicker1 in oManager1.GetData())
            {
                nFound1++;  
                ITicker? oTicker2 = aTikers2.FirstOrDefault(p => p.Symbol.Base == oTicker1.Symbol.Base && p.Symbol.Quote == oTicker1.Symbol.Quote);
                if (oTicker2 == null) continue;
                if( oTicker1.FundingRate == 0 || oTicker2.FundingRate == 0 ) continue;
                nFound2++;  
                bool bFirstBuy = (oTicker1.FundingRate < oTicker2.FundingRate);
                decimal nRate = Math.Abs(oTicker1.FundingRate - oTicker2.FundingRate) * 100;

                if (nRate < Setup.PercentMinimum) continue;
                FundingChance? oChance = null;
                if( bFirstBuy)
                {
                    FundingChanceData oChanceBuyData = new FundingChanceData(oData1.Exchange.ExchangeType, (IFuturesSymbol)oTicker1.Symbol, oTicker1.Ask, oTicker1.FundingRate, DateTime.Now);
                    FundingChanceData oChanceSellData = new FundingChanceData(oData2.Exchange.ExchangeType, (IFuturesSymbol)oTicker2.Symbol, oTicker2.Bid, oTicker2.FundingRate, DateTime.Now);
                    oChance = new FundingChance(nRate, oChanceBuyData, oChanceSellData);
                }
                else
                {
                    FundingChanceData oChanceBuyData = new FundingChanceData(oData2.Exchange.ExchangeType, (IFuturesSymbol)oTicker2.Symbol, oTicker2.Ask, oTicker2.FundingRate, DateTime.Now);
                    FundingChanceData oChanceSellData = new FundingChanceData(oData1.Exchange.ExchangeType, (IFuturesSymbol)oTicker1.Symbol, oTicker1.Bid, oTicker1.FundingRate, DateTime.Now);
                    oChance = new FundingChance(nRate, oChanceBuyData, oChanceSellData);
                }
                if (oChance != null)
                {
                    if( oChance.BuyData.Price < oChance.SellData.Price) 
                    {
                        aChances.Add(oChance);
                    }

                }
            }
            if( aChances.Count > 0 )
            {
                FundingChance oBest = aChances.OrderByDescending(p => p.RatePercent).First();

                string strInfo = string.Format(" -- Best chance {0:0.000}% Buy {1} price {2}... Sell {3} price {4}",
                    oBest.RatePercent,
                    oBest.BuyData.Symbol.Symbol,
                    oBest.BuyData.Price,
                    oBest.SellData.Symbol.Symbol,
                    oBest.SellData.Price);
                Logger.Info(strInfo);
            }
            */
        }


        /// <summary>
        /// Task loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            DateTime dLastRun = DateTime.Now;
            while( !m_oCancelSource.IsCancellationRequested )
            {
                try
                {
                    DateTime dNow = DateTime.Now;   
                    double nTotal = (dNow - dLastRun).TotalMinutes;
                    if( nTotal >= 5 )
                    {
                        Logger.Info("     ....");
                        dLastRun = dNow;    
                    }
                    await UpdateFundingRates();
                    FindChances();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error on main loop",ex);
                }
                await Task.Delay(5000);
            }
        }

        /// <summary>
        /// Starts main loop
        /// </summary>
        /// <returns></returns>
        private async Task CreateMainLoop()
        {
            await StopMainLoop();
            m_oCancelSource = new CancellationTokenSource();
            m_oMainTask = MainLoop();

        }

        /// <summary>
        /// Stops main loop
        /// </summary>
        /// <returns></returns>
        private async Task StopMainLoop()
        {
            if (m_oMainTask == null) return;
            m_oCancelSource.Cancel();
            await m_oMainTask;

        }

    }
}
