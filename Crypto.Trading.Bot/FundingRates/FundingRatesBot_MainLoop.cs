using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoExchange.Net.CommonObjects;
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


        private List<FundingChance> m_aChances = new List<FundingChance>(); 
        /// <summary>
        /// Update funding rates
        /// </summary>
        /// <returns></returns>
        private void UpdateFundingRates()
        {
            if (m_aDictExchanges == null) return;
            foreach (ExchangeType eType in m_aDictExchanges.Keys)
            {
                FundingBotExchangeData oData = m_aDictExchanges[eType];

                if( oData.Websocket != null )
                {
                    oData.FundingSnapshots = oData.Websocket.FundingRateManager.GetData();
                }

            }

        }


        /// <summary>
        /// Chances 
        /// </summary>
        private void FindChances()
        {
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

            IOrderbookManager oManager1 = oData1.Websocket.OrderbookManager;
            IOrderbookManager oManager2 = oData2.Websocket.OrderbookManager;

            decimal nMoney = Setup.Leverage * Setup.Amount;

            foreach( IFundingRate oShot1 in oData1.FundingSnapshots ) 
            {
                IFundingRate? oShot2 = oData2.FundingSnapshots.FirstOrDefault(p => p.Symbol.Base == oShot1.Symbol.Base && p.Symbol.Quote == oShot1.Symbol.Quote);
                if (oShot2 == null) continue;

                decimal nRate = 0;
                bool bFirstBuy = (oShot1.Rate < oShot2.Rate);
                if ( oShot1.SettleDate < oShot2.SettleDate )
                {
                    nRate = Math.Abs( oShot1.Rate );
                    bFirstBuy = (oShot1.Rate < 0);
                }
                else if (oShot1.SettleDate  > oShot2.SettleDate)
                {
                    nRate = Math.Abs(oShot2.Rate);
                    bFirstBuy = (oShot2.Rate > 0);
                }
                else
                {
                    nRate = Math.Abs(oShot1.Rate - oShot2.Rate) * 100;
                }
                    
                if (nRate < Setup.PercentMinimum) continue;

                FundingChance? oChance = null;
                if ( bFirstBuy )
                {
                    IOrderbookPrice? oPrice1 = oData1.Websocket.OrderbookManager.GetBestAsk(oShot1.Symbol.Symbol, nMoney);
                    if (oPrice1 == null) continue;
                    IOrderbookPrice? oPrice2 = oData2.Websocket.OrderbookManager.GetBestBid(oShot2.Symbol.Symbol, nMoney);
                    if (oPrice2 == null) continue;
                    FundingChanceData oChanceBuyData = new FundingChanceData(oData1.Exchange.ExchangeType, oShot1.Symbol, oPrice1.Price, oShot1.Rate, oShot1.SettleDate);
                    FundingChanceData oChanceSellData = new FundingChanceData(oData2.Exchange.ExchangeType, oShot2.Symbol, oPrice2.Price, oShot2.Rate, oShot2.SettleDate);
                    oChance = new FundingChance(nRate, oChanceBuyData, oChanceSellData);
                }
                else
                {
                    IOrderbookPrice? oPrice1 = oData1.Websocket.OrderbookManager.GetBestBid(oShot1.Symbol.Symbol, nMoney);
                    if (oPrice1 == null) continue;
                    IOrderbookPrice? oPrice2 = oData2.Websocket.OrderbookManager.GetBestAsk(oShot2.Symbol.Symbol, nMoney);
                    if (oPrice2 == null) continue;
                    FundingChanceData oChanceSellData = new FundingChanceData(oData1.Exchange.ExchangeType, oShot1.Symbol, oPrice1.Price, oShot1.Rate, oShot1.SettleDate);
                    FundingChanceData oChanceBuyData = new FundingChanceData(oData2.Exchange.ExchangeType, oShot2.Symbol, oPrice2.Price, oShot2.Rate, oShot2.SettleDate);
                    oChance = new FundingChance(nRate, oChanceBuyData, oChanceSellData);

                }

                if (oChance != null)
                {
                    //if (oChance.BuyData.Price < oChance.SellData.Price)
                    {
                        aChances.Add(oChance);
                    }

                }


            }

            FundingChance[] aNewChances = aChances.Where( p=> !m_aChances.Any(q=> p.Base == q.Base && p.Quote == q.Quote)).ToArray(); 
            if( aNewChances.Length > 0 )
            {
                FundingChance oBest = aNewChances.OrderByDescending(p => p.RatePercent).First();
                m_aChances.Add(oBest);

                string strInfo = string.Format(" -- Best chance {0:0.000}% Buy {1} price {2}... Sell {3} price {4}",
                    oBest.RatePercent,
                    oBest.BuyData.Symbol.Symbol,
                    oBest.BuyData.Price,
                    oBest.SellData.Symbol.Symbol,
                    oBest.SellData.Price);
                Logger.Info(strInfo);
            }
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
                    UpdateFundingRates();
                    FindChances();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error on main loop",ex);
                }
                await Task.Delay(500);
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
