using Binance.Net.Objects.Models.Spot;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
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

        private enum BotStatus
        {
            Start,
            FindChance,
            WaitForOpen,
            WaitForClose,
            Close
        }

        private IFundingSocketData? m_oSocketData = null;
        private IOppositeOrder? m_oActiveOrder = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private Task? m_oMainTask = null;   
        private BotStatus m_eStatus = BotStatus.Start;

        private DateTime m_dLastInfo = DateTime.Now;
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
            if( oNext == null ) return null;
            while (oNext != null)
            {
                IFundingPair[] aPairsSorted = oNext.Pairs.OrderByDescending(p => p.Percent).Take(10).ToArray();
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
                    oLog = oActual;   
                }
            }

            if (oLog == null) return oBest;
            Logger.Info($"  {oLog.FundingDate.DateTime.ToShortTimeString()} {oLog.BuySymbol.Base} Buy on {oLog.BuySymbol.Exchange.ExchangeType.ToString()} Sell on {oLog.SellSymbol.Exchange.ExchangeType.ToString()} => {oLog.Percent} %");

            return oLog;
        }


        private IOppositeOrder? CreateActiveOrder( IFundingPair oPair )
        {
            double nMinutes = (oPair.FundingDate.DateTime - DateTime.Now).TotalMinutes;
            if (nMinutes < 0 || nMinutes >= 60) return null;
            IOppositeOrder oResult = new OppositeOrder(oPair.BuySymbol, oPair.SellSymbol, this.Setup.Leverage);
            // Put orderbooks
            if (oPair.BuySymbol.Exchange.Market.Websocket == null) return null;
            IOrderbook? oFoundBuy = oPair.BuySymbol.Exchange.Market.Websocket.OrderbookManager.GetData().FirstOrDefault(p=> p.Symbol.Symbol == oPair.BuySymbol.Symbol);
            if( oFoundBuy == null ) return null;    
            if (oPair.SellSymbol.Exchange.Market.Websocket == null) return null;
            IOrderbook? oFoundSell = oPair.SellSymbol.Exchange.Market.Websocket.OrderbookManager.GetData().FirstOrDefault(p => p.Symbol.Symbol == oPair.SellSymbol.Symbol);
            if (oFoundSell == null) return null;

            oResult.LongData.Orderbook = oFoundBuy;
            oResult.ShortData.Orderbook = oFoundSell;
            return oResult;

        }

        /// <summary>
        /// Try open order
        /// </summary>
        /// <param name="oOrder"></param>
        /// <returns></returns>
        private async Task TryOpen( IOppositeOrder oOrder, decimal nMoney )
        {
            if( oOrder.LongData.Orderbook == null ) return;
            IOrderbookPrice? oBestBuy = oOrder.LongData.Orderbook.GetBestPrice(true, null, nMoney); 
            if( oBestBuy == null ) return;
            if (oOrder.ShortData.Orderbook == null) return;
            IOrderbookPrice? oBestSell = oOrder.ShortData.Orderbook.GetBestPrice(false, null, nMoney);
            if (oBestSell == null) return;
            decimal nDiff = oBestSell.Price - oBestBuy.Price;   
            DateTime dNow = DateTime.Now;
            if( (dNow - m_dLastInfo).TotalMinutes >= 1 )
            {
                Logger.Info($"  Waiting for buy {oOrder.LongData.Symbol.Base} Difference {nDiff}");
                m_dLastInfo = dNow; 
            }

            // if( oBestBuy.Price > oBestSell.Price ) return;
            Logger.Info($"  Found !!!!! buy {oOrder.LongData.Symbol.Base} Difference {nDiff}");

            // bool bOrder = await oOrder.TryOpenLimit(nMoney);
            bool bOrder = await oOrder.TryOpenMarket(nMoney);
            if ( bOrder )
            {
                m_eStatus = BotStatus.WaitForClose;
            }
        }

        /// <summary>
        /// Wait to close order
        /// </summary>
        /// <param name="oOrder"></param>
        /// <param name="oPair"></param>
        /// <returns></returns>
        private async Task WaitForClose( IOppositeOrder? oOrder, IFundingPair? oPair )
        {
            if( oOrder == null || oPair == null ) return;   
            if( oOrder.LongData.Position == null ) return;  
            if( oOrder.ShortData.Position == null ) return; 
        }

        /// <summary>
        /// Main loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            IFundingPair? oActiveChance = null;
            decimal nMoney = this.Setup.Leverage * this.Setup.Amount;
            IFundingPair? oBestChance = null;
            bool bTrade = true;

            BotStatus eActual = m_eStatus;
            while ( !m_oCancelSource.IsCancellationRequested )
            {
                switch( m_eStatus )
                {
                    case BotStatus.Start:
                        m_eStatus = BotStatus.FindChance;
                        break;
                    case BotStatus.FindChance:
                        oActiveChance = await GetChance();
                        if (oActiveChance != null)
                        {
                            oBestChance = LogBest(oActiveChance, oBestChance);

                            m_oActiveOrder = CreateActiveOrder(oActiveChance);    
                            if(m_oActiveOrder != null )
                            {
                                m_eStatus = BotStatus.WaitForOpen;
                            }
                        }
                        break;
                    case BotStatus.WaitForOpen:
                        if( m_oActiveOrder != null )
                        {
                            await TryOpen( m_oActiveOrder, nMoney );    
                        }
                        break;
                    case BotStatus.WaitForClose:
                        await WaitForClose( m_oActiveOrder, oActiveChance); 
                        break;
                    case BotStatus.Close:
                        break;
                }

                if( eActual != m_eStatus )
                {
                    eActual = m_eStatus;
                    Logger.Info($"New status {eActual.ToString()}");
                }
                /*
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
                */
                await Task.Delay(200);
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
            await CreateEvents();
            await Task.Delay(1000);
            m_oMainTask = MainLoop();
        }

        private async Task CreateEvents()
        {
            if (m_oSocketData == null) return;
            foreach( var oExchange in m_oSocketData.Exchanges )
            {
                oExchange.Account.OnPrivateEvent += OnPrivateEvent;
                await oExchange.Account.StartSockets(); 
            }
        }

        private async Task PutOrder(IWebsocketQueueItem oItem)
        {
            if (m_oActiveOrder == null) return;
            IFuturesOrder oOrder = (IFuturesOrder)oItem;    
            if( oOrder.Symbol.Exchange.ExchangeType == m_oActiveOrder.LongData.Symbol.Exchange.ExchangeType )
            {
                if( oOrder.Symbol.Symbol == m_oActiveOrder.LongData.Symbol.Symbol )
                {
                    if( oOrder.OrderDirection == FuturesOrderDirection.Buy )
                    {
                        m_oActiveOrder.LongData.OpenOrder = oOrder;
                        Logger.Info($"  Order open buy on {oOrder.Symbol.Exchange.ExchangeType.ToString()} {oOrder.Symbol.Symbol} Status {oOrder.OrderStatus.ToString()}");
                    }
                    else
                    {
                        m_oActiveOrder.LongData.CloseOrder = oOrder;
                        Logger.Info($"  Order close buy on {oOrder.Symbol.Exchange.ExchangeType.ToString()} {oOrder.Symbol.Symbol} Status {oOrder.OrderStatus.ToString()}");
                    }
                }
            }
            if (oOrder.Symbol.Exchange.ExchangeType == m_oActiveOrder.ShortData.Symbol.Exchange.ExchangeType)
            {
                if (oOrder.Symbol.Symbol == m_oActiveOrder.ShortData.Symbol.Symbol)
                {
                    if (oOrder.OrderDirection == FuturesOrderDirection.Sell)
                    {
                        m_oActiveOrder.ShortData.OpenOrder = oOrder;
                        Logger.Info($"  Order open sell on {oOrder.Symbol.Exchange.ExchangeType.ToString()} {oOrder.Symbol.Symbol} Status {oOrder.OrderStatus.ToString()}");
                    }
                    else
                    {
                        m_oActiveOrder.ShortData.CloseOrder = oOrder;
                        Logger.Info($"  Order close sell on {oOrder.Symbol.Exchange.ExchangeType.ToString()} {oOrder.Symbol.Symbol} Status {oOrder.OrderStatus.ToString()}");
                    }
                }

            }
        }

        private async Task PutPosition(IWebsocketQueueItem oItem)
        {
            if (m_oActiveOrder == null) return;
            IFuturesPosition oPosition = (IFuturesPosition)oItem;
            if (oPosition.Symbol.Exchange.ExchangeType == m_oActiveOrder.LongData.Symbol.Exchange.ExchangeType)
            {
                if (oPosition.Symbol.Symbol == m_oActiveOrder.LongData.Symbol.Symbol)
                {
                    if (oPosition.Direction == FuturesPositionDirection.Long )
                    {
                        m_oActiveOrder.LongData.Position = oPosition;
                        Logger.Info($"  Position long on {oPosition.Symbol.Exchange.ExchangeType.ToString()} {oPosition.Symbol.Symbol} ");
                    }
                }
            }
            if (oPosition.Symbol.Exchange.ExchangeType == m_oActiveOrder.ShortData.Symbol.Exchange.ExchangeType)
            {
                if (oPosition.Symbol.Symbol == m_oActiveOrder.ShortData.Symbol.Symbol)
                {
                    if (oPosition.Direction == FuturesPositionDirection.Short)
                    {
                        m_oActiveOrder.ShortData.Position = oPosition;
                        Logger.Info($"  Position short on  {oPosition.Symbol.Exchange.ExchangeType.ToString()} {oPosition.Symbol.Symbol}");
                    }
                }

            }
        }
        private async Task OnPrivateEvent(IWebsocketQueueItem oItem)
        {
            switch( oItem.QueueType )
            {
                case WebsocketQueueType.Poisition:
                    await PutPosition(oItem);
                    break;
                case WebsocketQueueType.Order:
                    await PutOrder(oItem);
                    break;
                case WebsocketQueueType.Balance:
                    break;
            }
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
