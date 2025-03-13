using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Trading.Bot.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Spread
{
    /// <summary>
    /// Market maker bot
    /// </summary>
    internal class SpreadBot : ITradingBot
    {

        private const double SECONDS_CANCEL = 20;
        private const int MAX_CHANCES = 3;

        private IFuturesExchange? m_oExchange = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private Task? m_oMainTask = null;

        private List<SpreadChance> m_aChances = new List<SpreadChance>();
        // private SpreadChance? m_oChance = null;

        private List<SpreadChance> m_aCompletedChances = new List<SpreadChance>();  

        private ConcurrentBag<IFuturesSymbol> m_aLeverages = new ConcurrentBag<IFuturesSymbol>();
        public SpreadBot(ICryptoSetup oSetup, ICommonLogger oLogger) 
        {
            Setup = oSetup;
            Logger = oLogger;
        }
        public ICryptoSetup Setup { get; }

        public ISocketManager SocketManager => throw new NotImplementedException();

        public ICommonLogger Logger { get; }


        private bool MustFind()
        {

            if (m_aChances.Where(p=> p.Status != SpreadStatus.Init && p.Status != SpreadStatus.Closed).Count() < MAX_CHANCES) return true;
            // if( m_oChance.Status == SpreadStatus.Init || m_oChance.Status == SpreadStatus.Closed) return true;
            return false;
        }


        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <returns></returns>
        private async Task CreateNewOrder(SpreadChance oChance)
        {
            if (m_oExchange == null ) return;
            oChance.Refresh();
            decimal nPrice = oChance.PriceBid;
            if (nPrice <= 0) return;

            decimal nMoney = ((decimal)Setup.Leverage * Setup.Amount) / (decimal)MAX_CHANCES;
            decimal nQuantity = nMoney / nPrice;
            nQuantity = Math.Round(nQuantity, oChance.Symbol.QuantityDecimals);

            var oResultTrade = await m_oExchange.Trading.CreateLimitOrder(oChance.Symbol, true, nQuantity, nPrice);
            if (oResultTrade == null || !oResultTrade.Success || oResultTrade.Result == null)
            {
                Logger.Error($"   Could not create order on {oChance.Symbol.ToString()}");
                return;
            }
            oChance.ChanceQuantity = nQuantity;
            oChance.Status = SpreadStatus.WaitForOpen;
            oChance.BuyOrderId = oResultTrade!.Result!.Id;
        }


        /// <summary>
        /// Creates open order
        /// </summary>
        /// <returns></returns>
        private async Task CreateOpenOrder(SpreadChance oChance)
        {
            if( m_oExchange == null ) return;

            if( !m_aLeverages.Any(p=> p.Symbol == oChance.Symbol.Symbol) )
            {
                var oResult = await m_oExchange.Trading.SetLeverage(oChance.Symbol, Setup.Leverage);
                if( oResult == null || !oResult.Success || !oResult.Result )
                {
                    Logger.Warning($"Could not set leverage for {oChance.Symbol.ToString()}");
                    return;
                }
                m_aLeverages.Add(oChance.Symbol);
                return;
            }

            await CreateNewOrder(oChance);

        }

        /// <summary>
        /// Waits for order open
        /// </summary>
        /// <returns></returns>
        private async Task WaitForOrderOpen(SpreadChance oChance)
        {
            if (m_oExchange == null) return;
            if (oChance.BuyOrderId == null) return;


            if (oChance.BuyOrder == null)
            {
                IFuturesOrder[] aOrders = m_oExchange.Account.OrderManager.GetData();
                IFuturesOrder? oFound = aOrders.FirstOrDefault(p => p.Id == oChance.BuyOrderId);
                if (oFound == null) return;
                oChance.BuyOrder = oFound;
                oChance.Status = SpreadStatus.WaitForPosition;
            }

        }

        /// <summary>
        /// Check for close order
        /// </summary>
        /// <param name="oOrder"></param>
        /// <returns></returns>
        private async Task<bool> CheckForCloseOrder( IFuturesOrder oOrder )
        {
            if (m_oExchange == null) return false;
            DateTime dNow = DateTime.Now;
            double nSeconds = (dNow - oOrder.TimeCreated).TotalSeconds;


            if (nSeconds > SECONDS_CANCEL)
            {
                // Cancel order
                var oResult = await m_oExchange.Trading.CancelOrder(oOrder);
                if (oResult == null || !oResult.Success)
                {
                    Logger.Error("  Could not cancel order!!");
                    return false;
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Waits for order close
        /// </summary>
        /// <returns></returns>
        private async Task WaitForPosition(SpreadChance oChance)
        {
            
            if ( m_oExchange == null) return;
            if (oChance.BuyOrder == null) return;
            // Create if needed
            if( oChance.BuyOrder.OrderStatus == FuturesOrderStatus.Filled || oChance.BuyOrder.OrderStatus == FuturesOrderStatus.PartialFilled)
            {
                bool bQuantity = (oChance.BuyOrder.OrderStatus == FuturesOrderStatus.Filled);
                IFuturesPosition[]? aPositions = m_oExchange.Account.PositionManager.GetData();
                if (aPositions == null || aPositions.Length <= 0) return;
                IFuturesPosition? oPosition = aPositions.FirstOrDefault( p=> !p.Closed && 
                                                                            p.Symbol.Symbol == oChance.Symbol.Symbol && 
                                                                            (!bQuantity || p.Quantity == oChance.ChanceQuantity) && 
                                                                            p.Direction == FuturesPositionDirection.Long);
                if (oPosition == null) return;
                oChance.Position = oPosition;
                oChance.Status = SpreadStatus.CreateClose;
                m_aCompletedChances.Add(oChance);   
                Logger.Info("Position found...");

                // If partially filled, cancel order but
                if( oChance.BuyOrder.OrderStatus == FuturesOrderStatus.PartialFilled )
                {
                    await m_oExchange.Trading.CancelOrder(oChance.BuyOrder);
                }
            }
            else if( oChance.BuyOrder.OrderStatus == FuturesOrderStatus.New)
            {
                bool bCanceled = await CheckForCloseOrder(oChance.BuyOrder);
                if( bCanceled)
                {
                    oChance.BuyOrder = null;
                    oChance.BuyOrderId = null;
                    oChance.Status = SpreadStatus.Init;
                }
            }

            
        }

        /// <summary>
        /// Waits for order close
        /// </summary>
        /// <returns></returns>
        private async Task CreateOrderClose(SpreadChance oChance)
        {
            if ( m_oExchange == null) return;
            if (oChance.BuyOrder == null) return;
            if (oChance.Position == null) return;
            if (oChance.BuyOrder.OrderStatus != FuturesOrderStatus.Filled) return;

            if (oChance.SellOrderId != null || oChance.SellOrder != null) return;

            oChance.Refresh();
            var oResult = await m_oExchange.Trading.ClosePosition(oChance.Position, oChance.PriceAsk);//.CreateLimitOrder(m_oChance.Symbol, false, m_oChance.Quantity, m_oChance.PriceAsk);
            if (oResult == null || !oResult.Success)
            {
                Logger.Error("Could not create close order!");
                return;
            }
            if (!oResult.Result)
            {
                Logger.Error("Close order returned null!");
                return;
            }
            oChance.Status = SpreadStatus.WaitForClose;
            oChance.SellOrder = null; 
            // await CreateOrderClose();
            return;

        }


        /// <summary>
        /// Waits for order close
        /// </summary>
        /// <returns></returns>
        private async Task WaitForOrderClose(SpreadChance oChance)
        {
            
            if ( m_oExchange == null) return;
            if (oChance.Position == null) return;


            if (oChance.SellOrder == null)
            {
                IFuturesOrder[] aOrders = m_oExchange.Account.OrderManager.GetData();
                IFuturesOrder? oFound = aOrders.FirstOrDefault(p => 
                    p.Symbol.Symbol == oChance.Symbol.Symbol && 
                    p.OrderStatus == FuturesOrderStatus.New && 
                    p.OrderDirection == FuturesOrderDirection.Sell && 
                    p.Quantity == oChance.Position.Quantity);
                if (oFound == null) return;
                // Logger.Info("Found sell order");
                oChance.SellOrder = oFound;
            }

            if( oChance.SellOrder.OrderStatus == FuturesOrderStatus.New)
            {
                bool bCanceled = await CheckForCloseOrder(oChance.SellOrder);
                if (bCanceled)
                {
                    oChance.SellOrder = null;
                    oChance.Status = SpreadStatus.CreateClose;
                    return;
                }

            }
            else if( oChance.SellOrder.OrderStatus == FuturesOrderStatus.Filled )
            {
                oChance.Status = SpreadStatus.Init;
                Logger.Info("SELL ORDER FILLED!!!!");
            }

            
        }

        /// <summary>
        /// Acts on chance if needed
        /// </summary>
        /// <returns></returns>
        private async Task ActOnChance(SpreadChance oChance)
        {
            
            SpreadStatus eStatusAct = oChance.Status;
            try
            {
                switch (oChance.Status)
                {
                    case SpreadStatus.Init:
                        await CreateOpenOrder(oChance);
                        break;
                    // break;
                    case SpreadStatus.WaitForOpen:
                        await WaitForOrderOpen(oChance);
                        break;
                    case SpreadStatus.WaitForPosition:
                        await WaitForPosition(oChance);
                        break;
                    case SpreadStatus.CreateClose:
                        await CreateOrderClose(oChance);
                        break;
                    case SpreadStatus.WaitForClose:
                        await WaitForOrderClose(oChance);
                        break;
                    case SpreadStatus.Closed:
                        break;
                }

                if( oChance.Status != eStatusAct )
                {
                    // Logger.Info($"   New chance status on {oChance.Symbol.ToString()}  {oChance.Status.ToString()}");
                    // if (m_oChance.Status == SpreadStatus.Init) m_oChance = null;
                }
            }
            catch(Exception ex)
            {
                Logger.Error(" Error acting on chance", ex);
            }
        }

        /// <summary>
        /// Find chance
        /// </summary>
        private void FindChance()
        {
            if( m_oExchange == null ) return;
            if (m_oExchange.Market.Websocket == null) return;
            IOrderbook[] aOrderbooks = m_oExchange.Market.Websocket.OrderbookManager.GetData();
            if (aOrderbooks.Length <= 0) return;

            // Remove chance
            for( int i = m_aChances.Count - 1; i >= 0; i-- ) 
            { 
                if(m_aChances[i].Status == SpreadStatus.Init || m_aChances[i].Status == SpreadStatus.Closed )
                {
                    m_aChances.RemoveAt(i);
                }
            }
            // int nToFind 
            List<SpreadChance> aFound= new List<SpreadChance>();    
            // SpreadChance? oBest = null;
            DateTime dNow = DateTime.Now;   
            foreach( var oOrderbook in aOrderbooks.Where(p=> p.Symbol.Quote == "USDT"))
            {
                SpreadChance oFound = new SpreadChance(oOrderbook);
                if (oFound.PercentSpread < Setup.ThresHold ) continue;
                if (oFound.PercentSpread > Setup.ThresHold * 2 ) continue;
                double nMilliSeconds = (dNow - m_oExchange.Market.Websocket.OrderbookManager.LastUpdate).TotalMilliseconds;
                if (nMilliSeconds > 100) continue;
                if (m_aChances.Any(p => p.Symbol.Symbol == oFound.Symbol.Symbol)) continue;

                aFound.Add(oFound); 
            }

            int nAdd = MAX_CHANCES - m_aChances.Count;
            m_aChances.AddRange( aFound.OrderByDescending(p=> p.PercentSpread).Take(nAdd) );
        }


        /// <summary>
        /// Chance main loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            Logger.Info("   Main loop started");
            DateTime dLastInfo = DateTime.Now;  
            while( !m_oCancelSource.IsCancellationRequested )
            {
                if ( MustFind() )
                {
                    FindChance();  
                }
                List<Task> aTasks = new List<Task>();   
                foreach( var oChance in m_aChances )
                {
                    aTasks.Add(ActOnChance(oChance));
                }
                await Task.WhenAll( aTasks );
                await Task.Delay(100);
                DateTime dNow = DateTime.Now;   
                double nMinutes = (DateTime.Now - dLastInfo).TotalMinutes;
                if( nMinutes >= 2 && dNow.Second < 5 )
                {
                    int nCompleted = m_aCompletedChances.Where(p=> p.Status == SpreadStatus.Init || p.Status == SpreadStatus.Closed).Count();
                    int nPending   = m_aCompletedChances.Where(p => p.Status != SpreadStatus.Init && p.Status != SpreadStatus.Closed).Count();

                    Logger.Info($"   Info. Chances pending : {nPending}, Completed : {nCompleted}");
                    dLastInfo = dNow;   
                }

            }
            Logger.Info("   Main loop ended");
        }


        public async Task<bool> Start()
        {
            Logger.Info("Spread bot starting...");
            await Stop();
            m_oExchange = await ExchangeFactory.CreateExchange(ExchangeType.CoinExFutures, Setup, Logger);
            if (m_oExchange == null) { Logger.Error("Could not create exchange"); return false; }
            Logger.Info($"   Created {m_oExchange.ExchangeType.ToString()} Exchange");

            await Task.Delay(1000);
            bool bSockets = await m_oExchange.Market.StartSockets();    
            if( !bSockets ) { Logger.Error("Could not start market sockets"); return false; }
            Logger.Info("   Started market sockets");

            m_oExchange.Account.OnPrivateEvent += OnPrivateEvent;

            await Task.Delay(1000);
            bSockets = await m_oExchange.Account.StartSockets();    
            if (!bSockets) { Logger.Error("Could not start account sockets"); return false; }
            Logger.Info("   Account sockets");


            await Task.Delay(1000);
            m_oCancelSource = new CancellationTokenSource();
            m_oMainTask = MainLoop();
            Logger.Info("   Main loop");
            await Task.Delay(2000);

            Logger.Info("Spread bot started...");
            return true;
        }



        /// <summary>
        /// Puts order
        /// </summary>
        /// <param name="oOrder"></param>
        private void PutOrder( IFuturesOrder oOrder )
        {
            if( oOrder.OrderStatus == FuturesOrderStatus.PartialFilled || oOrder.OrderStatus == FuturesOrderStatus.Filled )
            {
                Logger.Info($"   Order on {oOrder.Symbol.ToString()}, Type {oOrder.OrderDirection.ToString()}, Status {oOrder.OrderStatus.ToString()}, Qty {oOrder.Quantity.ToString()}, Price {oOrder.Price.ToString()}");
            }
        }

        /// <summary>
        /// Put position
        /// </summary>
        /// <param name="oPosition"></param>
        private void PutPosition(IFuturesPosition oPosition)
        {
            Logger.Info($"   Position on {oPosition.Symbol.ToString()} Quantity {oPosition.Quantity.ToString()} Price {oPosition.AveragePrice.ToString()} {(oPosition.Closed ? "CLOSED": string.Empty)}");
        }

        /// <summary>
        /// Private event
        /// </summary>
        /// <param name="oItem"></param>
        /// <returns></returns>
        private async Task OnPrivateEvent(Interface.Futures.Websockets.IWebsocketQueueItem oItem)
        {
            try
            {
                switch (oItem.QueueType)
                {
                    case Interface.Futures.Websockets.WebsocketQueueType.Order:
                        PutOrder((IFuturesOrder)oItem); 
                        break;
                    case Interface.Futures.Websockets.WebsocketQueueType.Poisition:
                        PutPosition((IFuturesPosition)oItem);
                        break;
                    default:
                        break;
                }

            }
            catch(Exception ex)
            {
                Logger.Error(" Error handling websocket event", ex);
            }
        }

        public async Task Stop()
        {
            if (m_oExchange == null) return;
            if( m_oMainTask != null )
            {
                m_oCancelSource.Cancel();
                await m_oMainTask;
                await Task.Delay(1000);
                m_oMainTask = null; 
            }
            Logger.Info("Spread bot ending...");
            await m_oExchange.Account.StopSockets();
            await Task.Delay(1000);

            await m_oExchange.Market.EndSockets();  
            await Task.Delay(1000);


            Logger.Info("Spread bot ended...");
            await Task.Delay(1000);
        }
    }
}
