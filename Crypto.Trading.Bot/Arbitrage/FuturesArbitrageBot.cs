using Crypto.Interface.Futures;
using Crypto.Interface;
using Crypto.Trading.Bot.Common;
using System.Text;
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

        private const string USDT = "USDT";

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
            if (m_oChance == null) return;
            if( m_oChance.BuyPosition == null || m_oChance.SellPosition == null) return;
            if (m_oChance.Money == null) return;
            try
            {

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
                        m_oChance.Money.BuyOpenPrice = oPosition.AveragePrice;
                    }

                }
                if (oPosition.Symbol.ToString() == m_oChance.SellPosition.Symbol.ToString())
                {
                    if (m_oChance.SellPosition.Position == null)
                    {
                        m_oChance.SellPosition.Position = oPosition;
                        m_oChance.Money.SellOpenPrice = oPosition.AveragePrice;
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
                        m_oChance.Money.SellOpenPrice = oPosition.AveragePrice;
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

            foreach (var oChance in aChances.Where(p=> p.ChanceStatus == ChanceStatus.None || p.ChanceStatus == ChanceStatus.Leverage ) )
            {
                if( !oChance.CalculateArbitrage(nMoney) ) continue;
                if( oChance.Money == null ) continue;   
                if( oBest == null )
                {
                    if( oChance.Money.Percent > 0 ) oBest = oChance;
                }
                else
                {
                    if (oBest.Money != null && oChance.Money.Percent > oBest.Money.Percent)
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

            Dictionary<string, List<IOrderbook>> oDictCurrencies = new Dictionary<string, List<IOrderbook>>();

            foreach (var eType in aOrderbooks.Keys )
            {
                IOrderbook[] aBooks = aOrderbooks[eType];   
                foreach ( var oBook in aBooks)
                {
                    if (oBook.Symbol.Quote != USDT) continue;
                    if (!oDictCurrencies.ContainsKey(oBook.Symbol.Base))
                    {
                        oDictCurrencies[oBook.Symbol.Base] = new List<IOrderbook>();    
                    }
                    oDictCurrencies[oBook.Symbol.Base].Add(oBook);  
                }

            }

            // Now we create chances
            foreach( string strKey in  oDictCurrencies.Keys )
            {
                IOrderbook[] aBooks = oDictCurrencies[strKey].ToArray();
                if (aBooks.Length < 2) continue;
                decimal nPriceMin = aBooks.Where(p=> p.Asks.Length > 0 ).Select(p=> p.Asks[0].Price).Min();
                decimal nPriceMax = aBooks.Where(p => p.Asks.Length > 0).Select(p => p.Asks[0].Price).Max();
                decimal nDiff = (nPriceMax - nPriceMin) * 100.0M / nPriceMin;
                if (nDiff >= 10M) continue;
                aResult.Add(new ArbitrageChance(aBooks));
            }
            return aResult.ToArray();
        }


        private void LogResults( string strData, string? strErrorMessage, Exception? oException )
        {
            if ( oException != null )
            {
                Logger.Error(strData, oException);
            }
            if( strErrorMessage != null )
            {
                Logger.Error($"{strData} : [{strErrorMessage}]");
            }
            return;
        }

        private async Task<bool> SetLeverages( IArbitrageChance oChance )
        {
            if (oChance.Money == null || oChance.BuyPosition == null || oChance.SellPosition == null) return false;
            Logger.Info($"{oChance.Money.Percent} % Setting leverage for {oChance.BuyPosition.Symbol.ToString()} / {oChance.SellPosition.Symbol.ToString()}");
            List<Task<ITradingResult<bool>>> aTasksLeverage = new List<Task<ITradingResult<bool>>>();

            aTasksLeverage.Add(oChance.BuyPosition.Symbol.Exchange.Trading.SetLeverage(oChance.BuyPosition.Symbol, Setup.Leverage));
            aTasksLeverage.Add(oChance.SellPosition.Symbol.Exchange.Trading.SetLeverage(oChance.SellPosition.Symbol, Setup.Leverage));
            await Task.WhenAll(aTasksLeverage);

            ITradingResult<bool> [] aResultsWrong = aTasksLeverage.Select(p=> p.Result).Where(p=> !p.Success).ToArray();
            if (aResultsWrong.Any())
            {
                foreach( var oWrong in aResultsWrong ) LogResults("Set Leverage Error ...", oWrong.Message, oWrong.Exception ); 
                return false;
            }
            return true;    
        }

        /// <summary>
        /// Act on chance
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task<IArbitrageChance?> ActOnChance(IArbitrageChance oChance, decimal nMoney)
        {
            if ( !oChance.CalculateArbitrage(nMoney)) return null;
            if( oChance.Money == null || oChance.BuyPosition == null || oChance.SellPosition == null ) return null;    
            if (oChance.Money.Percent < 0.3M) return null;

            if( oChance.ChanceStatus == ChanceStatus.None)
            {
                bool bLeverageSet = await SetLeverages(oChance);
                if( !bLeverageSet ) return null;    
                oChance.ChanceStatus = ChanceStatus.Leverage;
                if (!oChance.CalculateArbitrage(nMoney)) return null;
                if (oChance.Money.Percent < 0.3M) return null;
            }
            if (oChance.ChanceStatus != ChanceStatus.Leverage) return null;
            Logger.Info($"{oChance.Money.Percent} % Leverage ok for {oChance.BuyPosition.Symbol.ToString()} / {oChance.SellPosition.Symbol.ToString()}");
            // Buy
            List<Task<ITradingResult<IFuturesOrder?>>> aTasks = new List<Task<ITradingResult<IFuturesOrder?>>>();

            aTasks.Add(oChance.BuyPosition.Symbol.Exchange.Trading.CreateLimitOrder(oChance.BuyPosition.Symbol, true, oChance.Money.Quantity, oChance.Money.BuyOpenPrice));
            aTasks.Add(oChance.SellPosition.Symbol.Exchange.Trading.CreateLimitOrder(oChance.SellPosition.Symbol, false, oChance.Money.Quantity, oChance.Money.SellOpenPrice));
            await Task.WhenAll(aTasks);
            IArbitrageChance? oResult = oChance;
            if(!aTasks[0].Result.Success) { oResult = null; LogResults($"Error creating order {oChance.BuyPosition.Symbol.ToString()}", aTasks[0].Result.Message, aTasks[0].Result.Exception); }
            if (!aTasks[1].Result.Success) { oResult = null; LogResults($"Error creating order {oChance.SellPosition.Symbol.ToString()}", aTasks[1].Result.Message, aTasks[1].Result.Exception); }
            if( oResult != null )
            {
                Logger.Info($" Active chance Buy {oChance.BuyPosition.Symbol.ToString()} at {oChance.Money.BuyOpenPrice} Sell {oChance.SellPosition.Symbol.ToString()} at {oChance.Money.SellOpenPrice} Percent {oChance.Money.Percent}");
                oChance.ChanceStatus = ChanceStatus.Position;
            }

            return oResult;
        }


        /// <summary>
        /// Act on position
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task ActOnPosition( IArbitrageChance oChance )
        {
            if (oChance.ChanceStatus != ChanceStatus.Position) return;
            if( oChance.Money == null ) return;
            if( oChance.BuyPosition == null || oChance.SellPosition == null ) return;
            oChance.CalculateProfit();

            decimal nPercent = Math.Round( 100.0M * oChance.Money.Profit / ( oChance.Money.Quantity * oChance.Money.BuyOpenPrice ), 3);
            if (nPercent < oChance.Money.Percent)
            {
                DateTime dNow = DateTime.Now;
                if( (dNow - m_dLastInfo).TotalMinutes >= 1 )
                {
                    m_dLastInfo = dNow;
                    Logger.Info($"   Actual profit {Math.Round(oChance.Money.Profit,2)} ({nPercent} %)");
                }
                return;
            }

            if( oChance.BuyPosition.Position == null || oChance.SellPosition.Position == null ) return;

            List<Task<ITradingResult<bool>>> aTasksClose = new List<Task<ITradingResult<bool>>>();
            aTasksClose.Add(oChance.BuyPosition.Symbol.Exchange.Trading.ClosePosition(oChance.BuyPosition.Position ));
            aTasksClose.Add(oChance.SellPosition.Symbol.Exchange.Trading.ClosePosition(oChance.SellPosition.Position ));
            await Task.WhenAll(aTasksClose);
            Logger.Info($"   Profit reached. Closing on profit {oChance.Money.Profit} ({nPercent} %)");
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
                decimal nBalance = Math.Round(oFound.Equity, 2);
                oBuildBalance.Append($" {oExchange.ExchangeType.ToString()} : {nBalance}");
                nResult += nBalance;
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
                if( oBuild.Length > 0) oBuild.Append(", ");
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
            decimal nMoney = Setup.Leverage * Setup.Amount;
            decimal? nBestProfit = null;    
            while ( !m_oTokenSource.IsCancellationRequested )
            {
                if( m_oChance == null )
                {
                    IArbitrageChance? oChance = FindChance(aChances);
                    if (oChance != null)
                    {
                        m_oChance = await ActOnChance(oChance, nMoney);
                        if( m_oChance == null )
                        {
                            if( oChance.Money != null && oChance.Money.Percent > nBestPercent )
                            {
                                nBestPercent = oChance.Money.Percent;
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
                        if( m_oChance.Money != null )
                        {
                            if (nBestProfit == null)
                            {
                                nBestProfit = m_oChance.Money.Profit;
                            }
                            else if (nBestProfit.Value < m_oChance.Money.Profit)
                            {
                                nBestProfit = m_oChance.Money.Profit;
                                Logger.Info($" Best profit {nBestProfit}");
                            }
                            await ActOnPosition(m_oChance);
                        }
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
