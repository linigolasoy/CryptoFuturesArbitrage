using Crypto.Interface.Futures;
using Crypto.Interface;
using Crypto.Trading.Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Market;
using CryptoExchange.Net.CommonObjects;
using Crypto.Interface.Futures.Websockets;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Account;
using System.Linq.Expressions;

namespace Crypto.Trading.Bot.Arbitrage
{
    internal class FuturesArbitrageBot: ITradingBot
    {
        public FuturesArbitrageBot(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            SocketManager = new BaseSocketManager(oSetup, oLogger);
        }
        public ICryptoSetup Setup { get; }

        public ISocketManager SocketManager { get; }
        
        public ICommonLogger Logger { get; }

        private Task? m_oMainLoop = null;
        private CancellationTokenSource m_oTokenSource = new CancellationTokenSource();

        private IArbitrageChance? m_oChance = null;
        private List<IArbitrageChance> m_aChances = new List<IArbitrageChance>();

        private DateTime m_dLastInfo = DateTime.Now;
        private DateTime m_dLastBalance = DateTime.Now;
        private DateTime m_dLastPerformance = DateTime.Now;

        /// <summary>
        /// Start bot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            await Stop();
            bool bResult = await SocketManager.Start();
            if (!bResult) return false;

            foreach( var oExchange in SocketManager.Exchanges )
            {
                oExchange.Account.OnPrivateEvent += OnPrivateEvent;
            }
            // SocketManager.Exchanges[0].Market.
            m_oMainLoop = MainLoop();
            return true;
        }


        private async Task ProcessOrder(IFuturesOrder oOrder)
        {

        }

        private async Task ProcessPosition(IFuturesPosition oPosition)
        {
            try
            {

                if (m_oChance == null) return;
                if( oPosition.AveragePrice <= 0 )
                {
                    Logger.Info($"Positon Average Prize ZERO on {oPosition.Symbol.ToString()}");
                }
                if( oPosition.Symbol.ToString() == m_oChance.BuyPosition.Symbol.ToString() )
                {
                    if (m_oChance.BuyPosition.Position == null)
                    {
                        m_oChance.BuyPosition.Position = oPosition;
                        Logger.Info($" Position on {oPosition.Symbol.ToString()} Direction {oPosition.Direction.ToString()}");
                    }
                    else if( m_oChance.BuyPosition.Position.Closed )
                    {
                        Logger.Info($" Position CLOSED on {oPosition.Symbol.ToString()} Direction {oPosition.Direction.ToString()}");
                        if( m_oChance.SellPosition.Position != null && m_oChance.SellPosition.Position.Closed )
                        {
                            m_oChance.ChanceStatus = ChanceStatus.Closed;
                        }
                    }
                    if (oPosition.AveragePrice > 0)
                    {
                        m_oChance.BuyOpenPrice = oPosition.AveragePrice;
                    }

                }
                if (oPosition.Symbol.ToString() == m_oChance.SellPosition.Symbol.ToString())
                {
                    if (m_oChance.SellPosition.Position == null)
                    {
                        m_oChance.SellPosition.Position = oPosition;
                        m_oChance.SellOpenPrice = oPosition.AveragePrice;
                        Logger.Info($" Position on {oPosition.Symbol.ToString()} Direction {oPosition.Direction.ToString()}");
                    }
                    else if (m_oChance.SellPosition.Position.Closed)
                    {
                        Logger.Info($" Position CLOSED on {oPosition.Symbol.ToString()} Direction {oPosition.Direction.ToString()}");
                        if (m_oChance.BuyPosition.Position != null && m_oChance.BuyPosition.Position.Closed)
                        {
                            m_oChance.ChanceStatus = ChanceStatus.Closed;
                        }
                    }
                    if (oPosition.AveragePrice > 0)
                    {
                        m_oChance.SellOpenPrice = oPosition.AveragePrice;
                    }
                }
            }
            catch( Exception e)
            {
                Logger.Error($" Error processing queue position on {oPosition.Symbol.ToString()}", e);
            }

        }

        private async Task OnPrivateEvent(IWebsocketQueueItem oItem)
        {
            switch(oItem.QueueType)
            {
                case WebsocketQueueType.Order:
                    await ProcessOrder((IFuturesOrder)oItem);
                    break;
                case WebsocketQueueType.Poisition:
                    await ProcessPosition((IFuturesPosition)oItem);
                    break;
            }
        }

        /// <summary>
        /// Stop bot
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            if( m_oMainLoop != null)
            {
                m_oTokenSource.Cancel();
                await m_oMainLoop;
                m_oMainLoop = null;
            }
            await SocketManager.Stop();
            await Task.Delay(1000); 
        }


        /// <summary>
        /// Find chance
        /// </summary>
        /// <returns></returns>
        private IArbitrageChance? FindChance(IArbitrageChance[] aChances)
        {
            decimal nMoney = Setup.Leverage * Setup.Amount;
            IArbitrageChance? oBest = null;

            foreach (var oChance in aChances)
            {
                if( !oChance.CalculateArbitrage(nMoney) ) continue;
                if( oBest == null )
                {
                    if( oChance.Percent > 0 ) oBest = oChance;
                }
                else
                {
                    if (oChance.Percent > oBest.Percent)
                    {
                        oBest = oChance;
                        // Logger.Info($"Best {oBest.Percent}");
                    }
                }
            }


            return oBest;
        }

        /// <summary>
        /// Create chances
        /// </summary>
        /// <returns></returns>
        private IArbitrageChance[] CreateChances()
        {
            List<IArbitrageChance> aResult = new List<IArbitrageChance>();
            var aOrderbooks = SocketManager.GetOrderbooks();
            decimal nMoney = Setup.Leverage * Setup.Amount;

            foreach (var eTypeBuy in aOrderbooks.Keys)
            {
                foreach (var eTypeSell in aOrderbooks.Keys)
                {
                    if (eTypeBuy == eTypeSell) continue;
                    foreach (var oBookBuy in aOrderbooks[eTypeBuy])
                    {
                        IOrderbookPrice? oPriceBuy = oBookBuy.GetBestPrice(true, null, nMoney);
                        if (oPriceBuy == null) continue;
                        IOrderbook? oBookSell = aOrderbooks[eTypeSell].FirstOrDefault(p => p.Symbol.Base == oBookBuy.Symbol.Base && p.Symbol.Quote == oBookBuy.Symbol.Quote);
                        if (oBookSell == null) continue;
                        IOrderbookPrice? oPriceSell = oBookSell.GetBestPrice(false, null, nMoney);
                        if (oPriceSell == null) continue;
                        

                        // if (oPriceBuy.Price > oPriceSell.Price) continue;
                        decimal nMinimum = Math.Min(oPriceSell.Price, oPriceBuy.Price); 
                        decimal nPercent = Math.Round(Math.Abs(oPriceSell.Price - oPriceBuy.Price) * 100M / nMinimum, 3);
                        if (nPercent > 10.0M) continue;
                        IArbitrageChance oChance = new ArbitrageChance(oPriceBuy.Orderbook, oPriceSell.Orderbook);
                        aResult.Add(oChance);   
                    }

                }
            }

            return aResult.ToArray();   
        }


        /// <summary>
        /// Act on chance
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task<IArbitrageChance?> ActOnChance(IArbitrageChance oChance, decimal nMoney)
        {
            if( !oChance.CalculateArbitrage(nMoney)) return null;
            if (oChance.Percent < 0.3M) return null;

            if( oChance.ChanceStatus == ChanceStatus.None)
            {
                List<Task<bool>> aTasksLeverage = new List<Task<bool>>();

                aTasksLeverage.Add(oChance.BuyPosition.Symbol.Exchange.Trading.SetLeverage(oChance.BuyPosition.Symbol, Setup.Leverage));
                aTasksLeverage.Add(oChance.SellPosition.Symbol.Exchange.Trading.SetLeverage(oChance.SellPosition.Symbol, Setup.Leverage));
                await Task.WhenAll(aTasksLeverage); 
                if( aTasksLeverage.Any(p=> !p.Result)) return null;
                oChance.ChanceStatus = ChanceStatus.Leverage;
                return null;
            }
            if (oChance.ChanceStatus != ChanceStatus.Leverage) return null;
            // Buy
            List<Task<IFuturesOrder?>> aTasks = new List<Task<IFuturesOrder?>>();

            aTasks.Add(oChance.BuyPosition.Symbol.Exchange.Trading.CreateMarketOrder(oChance.BuyPosition.Symbol, true, oChance.Quantity));
            aTasks.Add(oChance.SellPosition.Symbol.Exchange.Trading.CreateMarketOrder(oChance.SellPosition.Symbol, false, oChance.Quantity));
            await Task.WhenAll(aTasks); 

            Logger.Info($" Active chance Buy {oChance.BuyPosition.Symbol.ToString()} Sell {oChance.SellPosition.Symbol.ToString()} Percent {oChance.Percent}");
            oChance.ChanceStatus = ChanceStatus.Position;

            return oChance;
        }


        /// <summary>
        /// Act on position
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task ActOnPosition( IArbitrageChance oChance )
        {
            if (oChance.ChanceStatus != ChanceStatus.Position) return;
            oChance.CalculateProfit();

            decimal nPercent = Math.Round( 100.0M * oChance.Profit / ( oChance.Quantity * oChance.BuyOpenPrice ), 3);
            if (nPercent < oChance.Percent)
            {
                DateTime dNow = DateTime.Now;
                if( (dNow - m_dLastInfo).TotalMinutes >= 1 )
                {
                    m_dLastInfo = dNow;
                    Logger.Info($"   Actual profit {Math.Round(oChance.Profit,2)} ({nPercent} %)");
                }
                return;
            }

            if( oChance.BuyPosition.Position == null || oChance.SellPosition.Position == null ) return;

            List<Task<bool>> aTasksClose = new List<Task<bool>>();
            aTasksClose.Add(oChance.BuyPosition.Symbol.Exchange.Trading.ClosePosition(oChance.BuyPosition.Position /*, oChance.BuyClosePrice*/));
            aTasksClose.Add(oChance.SellPosition.Symbol.Exchange.Trading.ClosePosition(oChance.SellPosition.Position /*, oChance.SellClosePrice*/));
            await Task.WhenAll(aTasksClose);
            Logger.Info($"   Profit reached. Closing on profit {oChance.Profit} ({nPercent} %)");
            oChance.ChanceStatus = ChanceStatus.OrderClose;
        }

        /// <summary>
        /// Start balances
        /// </summary>
        /// <returns></returns>
        private decimal GetAndLogBalances(decimal? nStartBalance)
        {
            if (SocketManager == null) return 0;
            decimal nResult = 0;
            StringBuilder oBuildBalance = new StringBuilder();  
            foreach( var oExchange in SocketManager.Exchanges )
            {
                if(oExchange.Account == null || oExchange.Account.BalanceManager == null) continue;
                IFuturesBalance[]? aBalances = oExchange.Account.BalanceManager.GetData();
                if (aBalances == null) continue;
                IFuturesBalance? oFound = aBalances.FirstOrDefault(p => p.Currency == "USDT");
                if( oFound == null ) continue;
                if( oBuildBalance.Length > 0 ) { oBuildBalance.Append(", "); }
                oBuildBalance.Append($" {oExchange.ExchangeType.ToString()} : {oFound.Equity}");
                nResult += oFound.Equity;
            }

            DateTime dNow = DateTime.Now;
            if( (dNow - m_dLastBalance).TotalMinutes >= 2 )
            {
                Logger.Info($" Balance {nResult} => {oBuildBalance.ToString()}");
                if (nStartBalance != null)
                {
                    decimal nProfit = nResult - nStartBalance.Value;
                    Logger.Info($" Profit {nProfit}");
                }
                m_dLastBalance = dNow;
            }

            return nResult; 
        }
        private void LogDelays()
        {
            DateTime dNow = DateTime.Now;
            if ((dNow - m_dLastPerformance).TotalMinutes <= 2) return;
            m_dLastPerformance = dNow;
            Dictionary<ExchangeType, decimal> aDelays = new Dictionary<ExchangeType, decimal>();    
            foreach( var oExchange in SocketManager.Exchanges)
            {
                if(oExchange.Market.Websocket == null) continue;    
                double nDelay = 0;
                int nCount = 0;

                IOrderbook[] aBooks = oExchange.Market.Websocket.OrderbookManager.GetData();
                nCount = aBooks.Length; 
                foreach( var oBook in aBooks )
                {
                    double nDiff = (oBook.ReceiveDate - oBook.UpdateDate).TotalMilliseconds;
                    nDelay += nDiff;    
                }

                decimal nAverage = Math.Round( (decimal)nDelay / (decimal)nCount, 1);
                aDelays[oExchange.ExchangeType] = nAverage;  
            }

            StringBuilder oBuild = new StringBuilder();
            foreach( var oType in aDelays.Keys )
            {
                if( oBuild.Length <= 0) oBuild.Append(", ");
                oBuild.Append($"{oType.ToString()} ({aDelays[oType] / 1000M})");
            }

            Logger.Info($" DELAYS : {oBuild.ToString()}");
        }
        /// <summary>
        /// Main loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            await Task.Delay(5000);
            IArbitrageChance[] aChances = CreateChances();
            await Task.Delay(2000);
            decimal nStartBalance = GetAndLogBalances(null);
            Logger.Info($" START BALANCE : {nStartBalance}");
            // IArbitrageChance? oActive = null;
            decimal nBestPercent = -100.0M;
            decimal? nBestProfit = null;    
            while ( !m_oTokenSource.IsCancellationRequested )
            {
                if( m_oChance == null )
                {
                    IArbitrageChance? oChance = FindChance(aChances);
                    if (oChance != null)
                    {
                        // m_oChance = await ActOnChance(oChance, Setup.Leverage * Setup.Amount);
                        if( m_oChance == null )
                        {
                            if( oChance.Percent > nBestPercent )
                            {
                                nBestPercent = oChance.Percent;
                                Logger.Info($"Best percent found {nBestPercent} %");
                            }
                        }
                    }
                }
                else
                {
                    if( m_oChance.ChanceStatus == ChanceStatus.Closed )
                    {
                        m_oChance.Reset();
                        nBestPercent = -100.0M;
                        m_oChance = null;
                        Logger.Info("Closed chance....About to find another...");

                    }
                    else if( m_oChance.CalculateProfit() )
                    {
                        if( nBestProfit == null )
                        {
                            nBestProfit = m_oChance.Profit;
                        }
                        else if( nBestProfit.Value < m_oChance.Profit )
                        {
                            nBestProfit = m_oChance.Profit;
                            Logger.Info($" Best profit {nBestProfit}");
                        }
                        await ActOnPosition(m_oChance);
                    }
                }
                GetAndLogBalances(nStartBalance);
                LogDelays();
                await Task.Delay(100);
            }

            decimal nEndBalance = GetAndLogBalances(nStartBalance);
            Logger.Info($" TOTAL PROFIT : {nEndBalance - nStartBalance}");
            await Task.Delay(1000);
        }
    }
}
