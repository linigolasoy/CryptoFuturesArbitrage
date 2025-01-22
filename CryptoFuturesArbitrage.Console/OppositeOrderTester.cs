using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Crypto.Trading.Bot;
using Crypto.Trading.Bot.Arbitrage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoFuturesArbitrage.Console
{
    internal class OppositeOrderTester: ITradingBot
    {

        public ICryptoSetup Setup { get; }
        public ICommonLogger Logger { get; }

        public IFuturesExchange[]? Exchanges { get; private set; } = null;

        public OppositeOrderTester( ICryptoSetup oSetup, ICommonLogger oLogger) 
        { 
            Setup = oSetup;
            Logger = oLogger;
        }

        private CancellationTokenSource m_oTokenSource = new CancellationTokenSource();
        private Task? m_oMainTask = null;
        private IOppositeOrder? m_oActiveOrder = null;

        private DateTime m_dLastDate = DateTime.Now;
        public async Task Start()
        {
            await Stop();
            Logger.Info("Starting tester....");
            // Create exchanges
            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            foreach( var eType in Setup.ExchangeTypes )
            {
                IFuturesExchange oExchange = await ExchangeFactory.CreateExchange(eType, Setup);
                aExchanges.Add(oExchange);  
            }
            Exchanges = aExchanges.ToArray();
            // Create private sockets and market sockets
            Logger.Info("   Create sockets...");
            foreach ( var oExchange in Exchanges )
            {
                Logger.Info( $"      {oExchange.ExchangeType.ToString()}");
                oExchange.Account.OnPrivateEvent += OnPrivateEvent;
                bool bResultPrivate = await oExchange.Account.StartSockets();
                if (!bResultPrivate) throw new Exception("Could not start private sockets");
                bool bResultMarket = await oExchange.Market.StartSockets();
                if (!bResultMarket) throw new Exception("Could not start market sockets");
            }
            Logger.Info("   Create main task...");
            m_oMainTask = MainLoop();

            // Create main loop
            Logger.Info("Started....");
        }


        private async Task CheckPosition(IWebsocketQueueItem oItem )
        {
            if (m_oActiveOrder == null) return;
            IFuturesPosition oPosition = (IFuturesPosition)oItem;
            if( oPosition.Symbol.Exchange.ExchangeType == m_oActiveOrder.LongData.Symbol.Exchange.ExchangeType )
            {
                if( oPosition.Symbol.Symbol == m_oActiveOrder.LongData.Symbol.Symbol )
                {
                    if (m_oActiveOrder.LongData.Position == null)
                    {
                        Logger.Info($"  New Long position on {oPosition.Symbol.Exchange.ExchangeType.ToString()} {oPosition.Symbol.Symbol} Qty {oPosition.Quantity}");
                        m_oActiveOrder.LongData.Position = oPosition;
                    }
                }
            }
            if (oPosition.Symbol.Exchange.ExchangeType == m_oActiveOrder.ShortData.Symbol.Exchange.ExchangeType)
            {
                if (oPosition.Symbol.Symbol == m_oActiveOrder.ShortData.Symbol.Symbol)
                {
                    if (m_oActiveOrder.ShortData.Position == null)
                    {
                        Logger.Info($"  New Short position on {oPosition.Symbol.Exchange.ExchangeType.ToString()} {oPosition.Symbol.Symbol} Qty {oPosition.Quantity}");
                        m_oActiveOrder.ShortData.Position = oPosition;
                    }
                }
            }
        }

        private async Task CheckOrder(IWebsocketQueueItem oItem)
        {
            if (m_oActiveOrder == null) return;
            IFuturesOrder oOrder = (IFuturesOrder)oItem;    
        }

        private async Task OnPrivateEvent(IWebsocketQueueItem oItem)
        {
            switch(oItem.QueueType)
            {
                case WebsocketQueueType.Balance:
                    break;
                case WebsocketQueueType.Order:
                    await CheckOrder(oItem);
                    break;
                case WebsocketQueueType.Poisition:
                    await CheckPosition(oItem);
                    break;
            }
        }

        /// <summary>
        /// Create tester order
        /// </summary>
        /// <returns></returns>
        private async Task<IOppositeOrder?> CreateTesterOrder()
        {
            string strBase = "GMT";
            string strQuote = "USDT";
            if (Exchanges == null) return null;
            IFuturesExchange? oExchangeBuy = Exchanges.FirstOrDefault(p=> p.ExchangeType == ExchangeType.CoinExFutures);
            if( oExchangeBuy == null) return null;
            IFuturesExchange? oExchangeSell = Exchanges.FirstOrDefault(p => p.ExchangeType == ExchangeType.BingxFutures);
            if (oExchangeSell == null) return null;
            IFuturesSymbol[]? aSymbolsBuy = await oExchangeBuy.Market.GetSymbols();
            if (aSymbolsBuy == null) return null;
            IFuturesSymbol[]? aSymbolsSell = await oExchangeSell.Market.GetSymbols();
            if (aSymbolsSell == null) return null;

            IFuturesSymbol? oSymbolBuy = aSymbolsBuy.FirstOrDefault(p=> p.Base == strBase && p.Quote == strQuote);
            if (oSymbolBuy == null) return null;
            IFuturesSymbol? oSymbolSell = aSymbolsSell.FirstOrDefault(p => p.Base == strBase && p.Quote == strQuote);
            if (oSymbolSell == null) return null;

            IOppositeOrder oResult = ArbitrageFactory.CreateOppositeOrder(oSymbolBuy, oSymbolSell, 10);
            return oResult;
        }

        /// <summary>
        /// Logdata
        /// </summary>
        /// <returns></returns>
        private async Task LogData()
        {
            DateTime dNow = DateTime.Now;
            int nDelay = 2;
            double nMinutes = (dNow - m_dLastDate).TotalMinutes;    
            if( nMinutes < (double)nDelay ) return;  
            if( dNow.Second != 0 ) return;
            if( dNow.Minute % nDelay != 0 ) return; 

            m_dLastDate = dNow;
            if (m_oActiveOrder == null) return;
            if( m_oActiveOrder.LongData.Orderbook == null || m_oActiveOrder.ShortData.Orderbook == null ) return;

            string strLog = $"  Orderbook dates. Long {m_oActiveOrder.LongData.Orderbook.UpdateDate.ToShortTimeString()} Short "+
                           $"{m_oActiveOrder.ShortData.Orderbook.UpdateDate.ToShortTimeString()} LastProfit {m_oActiveOrder.Profit} "+
                           $" LongProfit({m_oActiveOrder.LongData.Profit}) ShortProfit({m_oActiveOrder.ShortData.Profit}) Updates {m_oActiveOrder.ProfitUpdates}";
            Logger.Info(strLog);    
        }
        private async Task MainLoop()
        {
            decimal nMoney = 1400;
            decimal nMaxProfit = -9E10M;
            decimal nProfitToClose = 20M;
            await Task.Delay(2000);
            bool bCreatedOrder = false; 
            IOppositeOrder[]? aOrders = await ArbitrageFactory.CreateOppositeOrderFromExchanges(Exchanges!);
            if( aOrders != null && aOrders.Length > 0 )
            {
                m_oActiveOrder = aOrders[0];  
                bCreatedOrder = true;
                Logger.Info("Found active order...");
            }
            else
            {
                m_oActiveOrder = await CreateTesterOrder();
            }
            if (m_oActiveOrder == null) return;
            while (!m_oTokenSource.IsCancellationRequested)
            {
                if (m_oActiveOrder != null)
                {
                    m_oActiveOrder.Update();
                    if (!bCreatedOrder)
                    {
                        bCreatedOrder = await m_oActiveOrder.TryOpenMarket(nMoney);
                    }
                    else
                    {
                        if (m_oActiveOrder.Profit > nMaxProfit)
                        {
                            nMaxProfit = m_oActiveOrder.Profit;
                            Logger.Info($"  Max profit {nMaxProfit} ==> {m_oActiveOrder.ToString()}");
                        }
                        if ( m_oActiveOrder.Profit >= nProfitToClose)
                        {
                            Logger.Info("   Trying to close....");
                            ICloseResult oResult = await m_oActiveOrder.TryCloseMarket();
                            if( oResult.Success )
                            {
                                Logger.Info("   CLOSED!!!!!!!!!....");
                                m_oActiveOrder = null;
                            }
                        }
                    }
                    await LogData();
                }

                await Task.Delay(500);
            }
            await Task.Delay(200);
        }


        /// <summary>
        /// Stop socket
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            if (Exchanges != null)
            {
                Logger.Info("Stopping tester....");
                foreach (var oExchange in Exchanges)
                {
                    if (oExchange.Market.Websocket != null)
                    {
                        await oExchange.Market.EndSockets();
                    }
                }
                Exchanges = null;
            }
            if( m_oMainTask != null )
            {
                m_oTokenSource.Cancel();
                await m_oMainTask;
                m_oMainTask = null;
            }
        }
    }
}
