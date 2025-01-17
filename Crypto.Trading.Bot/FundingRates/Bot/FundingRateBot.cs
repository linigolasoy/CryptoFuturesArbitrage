using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Trading.Bot.Arbitrage;
using Crypto.Trading.Bot.FundingRates.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Bot
{
    internal class FundingRateBot : ITradingBot
    {

        private IFundingSocketData? m_oSocketData = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private Task? m_oMainTask = null;   

        public FundingRateBot(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
        }
        public ICryptoSetup Setup { get; }

        public IFuturesExchange[]? Exchanges => throw new NotImplementedException();

        public ICommonLogger Logger { get; }


        /// <summary>
        /// Get chance
        /// </summary>
        /// <returns></returns>
        private async Task<IFundingPair?> GetChance()
        {
            if (m_oSocketData == null) return null;
            IFundingDate[]? aDates = await m_oSocketData.GetFundingDates();
            if (aDates == null) return null;

            IFundingPair? oBest = null;
            IFundingDate? oNext = await m_oSocketData.GetNext(null);
            while (oNext != null)
            {
                IFundingPair? oPair = oNext.GetBest();
                if (oPair != null)
                {
                    if (oBest == null)
                    {
                        oBest = oPair;
                    }
                    else if (oBest.Percent < oPair.Percent)
                    {
                        oBest = oPair;
                    }

                }
                oNext = await m_oSocketData.GetNext(oNext.DateTime);

            }

            if (oBest == null) return null;
            return oBest;
        }

        /// <summary>
        /// Log best
        /// </summary>
        /// <param name="oActual"></param>
        /// <param name="oBest"></param>
        private IFundingPair? LogBest( IFundingPair oActual, IFundingPair? oBest )
        {
            IFundingPair? oLog = null;
            if( oBest == null )
            {
                oLog = oActual; 
            }
            else
            {
                if( oActual.FundingDate.DateTime != oBest.FundingDate.DateTime ) 
                {
                    oLog = oActual;
                }
                else if( oActual.Percent > oBest.Percent )
                {
                    oLog = oBest;   
                }
            }

            if (oLog == null) return oBest;
            Logger.Info($"  {oLog.FundingDate.DateTime.ToShortTimeString()} {oLog.BuySymbol.Base} Buy on {oLog.BuySymbol.Exchange.ExchangeType.ToString()} Sell on {oLog.SellSymbol.Exchange.ExchangeType.ToString()} => {oLog.Percent} %");

            return oLog;
        }
        /// <summary>
        /// Main loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            IOppositeOrder? oActiveOrder = null;
            IFundingPair? oActiveChance = null;
            decimal nMoney = 30;
            IFundingPair? oBestChance = null;
            bool bTrade = true;

            while ( !m_oCancelSource.IsCancellationRequested )
            {
                if( oActiveOrder != null )
                {
                    if( oActiveOrder.LongData.Position == null && oActiveOrder.ShortData.Position == null )
                    {
                        decimal nDiff = oActiveOrder.Update(nMoney);
                        if( nDiff > 0 && bTrade )
                        {
                            bool bResult = await oActiveOrder.TryOpenLimit( nMoney ); 
                            Logger.Info($"Positive difference !!!! {nDiff}");
                            if( bResult )
                            {
                                bTrade = false;
                                Logger.Info("Se creó orden limit");
                            }
                        }
                        Logger.Info($"Negative difference {nDiff}");

                    }
                }
                else
                {

                    IFundingPair? oChance = await GetChance();    
                    if( oChance != null )
                    {
                        oBestChance = LogBest(oChance, oBestChance);

                        if( oChance.Percent > 0 && oActiveOrder == null )
                        {
                            double nMinutes = (oChance.FundingDate.DateTime - DateTime.Now).TotalMinutes;
                            if ( nMinutes > 0 && nMinutes < 60)
                            {
                                oActiveOrder = new OppositeOrder(oChance.BuySymbol, oChance.SellSymbol, 10);
                                oActiveChance = oChance;
                                Logger.Info($"  CHANCE !!!! {oActiveChance.FundingDate.DateTime.ToShortTimeString()} {oActiveChance.BuySymbol.Base} Buy on {oActiveChance.BuySymbol.Exchange.ExchangeType.ToString()} Sell on {oActiveChance.SellSymbol.Exchange.ExchangeType.ToString()} => {oActiveChance.Percent} %");
                            }
                        }
                    }
                }
                await Task.Delay(100);
            }
        }


        /// <summary>
        /// Starts socket data
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            await Stop();
            m_oCancelSource = new CancellationTokenSource();
            m_oSocketData = new FundingSocketData(Logger, Setup);
            bool bStarted = await m_oSocketData.Start();
            if (!bStarted) throw new Exception("Could not start socket data");
            await Task.Delay(1000);
            m_oMainTask = MainLoop();
        }
        public async Task Stop()
        {
            if (m_oSocketData != null)
            {
                m_oCancelSource.Cancel();   
                await m_oSocketData.Stop();
                await Task.Delay(1000);
                if( m_oMainTask != null )
                {
                    await m_oMainTask;
                    m_oMainTask = null;
                }
                m_oSocketData = null;
            }
        }
    }
}
