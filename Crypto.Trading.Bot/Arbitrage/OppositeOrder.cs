﻿using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{


    internal class OppositeCloseResult : ICloseResult
    {
        public bool Success { get; internal set; } = false;

        public decimal ProfitOrLoss { get; internal set; } = 0;
    }

    internal class OppositeOrder : IOppositeOrder
    {
        private bool m_bLeverageSet = false;
        private const string USDT = "USDT";
        public OppositeOrder(IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort, int nLeverage) 
        {
            Leverage = nLeverage;   
            LongData = new ArbitrageOrderData(oSymbolLong);
            ShortData = new ArbitrageOrderData(oSymbolShort);
        }

        public int Leverage { get; }
        public IArbitrageOrderData LongData { get; }
        public IArbitrageOrderData ShortData { get; }

        public decimal Profit { get; private set; } = 0;
        public decimal Fees { get; private set; } = 0;

        public async Task<ICloseResult> TryCloseLimit()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update with orderbooks and rest if not having them
        /// </summary>
        public decimal Update( decimal nMoney )
        {
            decimal nResult = -9E10M;
            IFuturesWebsocketPublic? oWs = LongData.Symbol.Exchange.Market.Websocket;
            if ( oWs != null )
            {
                IOrderbook? oBook = oWs.OrderbookManager.GetData(LongData.Symbol.Symbol);
                if( oBook != null ) ((ArbitrageOrderData)LongData).Orderbook = oBook; 
            }
            oWs = ShortData.Symbol.Exchange.Market.Websocket;
            if (oWs != null)
            {
                IOrderbook? oBook = oWs.OrderbookManager.GetData(ShortData.Symbol.Symbol);
                if (oBook != null) ((ArbitrageOrderData)ShortData).Orderbook = oBook;
            }

            if (LongData.Orderbook == null || ShortData.Orderbook == null) return nResult;
            IOrderbookPrice? oPriceLong = LongData.Orderbook.GetBestPrice(true, null, nMoney);
            IOrderbookPrice? oPriceShort = ShortData.Orderbook.GetBestPrice(false, null, nMoney);

            if( oPriceLong == null || oPriceShort == null ) return nResult; 
            nResult = oPriceShort.Price - oPriceLong.Price;
            return nResult;
        }

        public async Task<ICloseResult> TryCloseMarket()
        {
            throw new NotImplementedException();
            /*
            OppositeCloseResult oResult = new OppositeCloseResult();
            if (PositionLong == null || PositionShort == null) return oResult;
            decimal nPnl = PositionLong.ProfitUnRealized + PositionShort.ProfitUnRealized;

            IFuturesBalance? oBalanceLong = SymbolLong.Exchange.Account.BalanceManager.GetData().FirstOrDefault(p => p.Currency == USDT);
            IFuturesBalance? oBalanceShort = SymbolShort.Exchange.Account.BalanceManager.GetData().FirstOrDefault(p => p.Currency == USDT);
            if( oBalanceLong != null && oBalanceShort != null )
            {
                this.ProfitBalance = oBalanceLong.ProfitUnrealized + oBalanceShort.ProfitUnrealized;
            }
            oResult.ProfitOrLoss = nPnl;
            this.Profit = nPnl;
            if ( nPnl > 0M )
            {
                List<Task<bool>> aTasks = new List<Task<bool>>();
                aTasks.Add(SymbolLong.Exchange.Trading.ClosePosition(PositionLong));
                aTasks.Add(SymbolShort.Exchange.Trading.ClosePosition(PositionShort));

                await Task.WhenAll(aTasks); 
                oResult.Success = true;
            }
            return oResult;
            */
        }

        /// <summary>
        /// Open limit order
        /// </summary>
        /// <param name="nMoney"></param>
        /// <returns></returns>
        public async Task<bool> TryOpenLimit(decimal nMoney)
        {
            decimal nDifference = Update(nMoney);
            if( nDifference < 0 ) return false; 
            IOrderbookPrice? oPriceLong = null;
            if (LongData.Orderbook != null) oPriceLong = LongData.Orderbook.GetBestPrice(true, null, nMoney);
            IOrderbookPrice? oPriceShort = null;
            if (ShortData.Orderbook != null) oPriceShort = ShortData.Orderbook.GetBestPrice(false, null, nMoney);

            if (oPriceLong == null || oPriceShort == null) return false;

            if( oPriceLong.Price > oPriceShort.Price ) return false;

            decimal nPrice = Math.Max(oPriceLong.Price, oPriceShort.Price);

            int nDecimals = Math.Min(LongData.Symbol.Decimals, ShortData.Symbol.Decimals);
            decimal nQuantity = Math.Round(nMoney / nPrice, nDecimals);


            if (nQuantity <= 0) return false;
            if( nQuantity < LongData.Symbol.Minimum ) return false;
            if( nQuantity < ShortData.Symbol.Minimum ) return false;    
            // Set leverages
            bool bResult = await SetLeverages();
            if (!bResult) return false;
            List<Task<IFuturesOrder?>> aTasks = new List<Task<IFuturesOrder?>>();
            aTasks.Add(LongData.Symbol.Exchange.Trading.CreateLimitOrder(LongData.Symbol, true, nQuantity, oPriceLong.Price));
            aTasks.Add(ShortData.Symbol.Exchange.Trading.CreateLimitOrder(ShortData.Symbol, false, nQuantity, oPriceShort.Price));


            await Task.WhenAll(aTasks);
            if (aTasks.Any(p => p.Result == null)) return false;

            return await UpdatePositions(nQuantity);
        }


        /// <summary>
        /// Set leverates
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SetLeverages()
        {
            if (m_bLeverageSet) return true;
            bool bResult = await LongData.Symbol.Exchange.Trading.SetLeverage(LongData.Symbol, Leverage);
            if (!bResult) return false;
            bResult = await ShortData.Symbol.Exchange.Trading.SetLeverage(LongData.Symbol, Leverage);
            if (!bResult) return false;
            m_bLeverageSet = true;  
            return true;
        }


        /// <summary>
        /// Update position data
        /// </summary>
        /// <param name="nQuantity"></param>
        /// <returns></returns>
        private async Task<bool> UpdatePositions(decimal nQuantity)
        {
            // TODO: Rollback
            // Wait until we have orders and positions on websockets
            ArbitrageOrderData oDataLong = (ArbitrageOrderData)LongData;    
            ArbitrageOrderData oDataShort = (ArbitrageOrderData)ShortData;
            int nRetries = 10;
            while (nRetries >= 0)
            {
                await Task.Delay(500);

                if (oDataLong.OpenOrder == null)
                {
                    IFuturesOrder[] aOrdersLong = oDataLong.Symbol.Exchange.Account.OrderManager.GetData();
                    oDataLong.OpenOrder = aOrdersLong.FirstOrDefault(p => p.Symbol.Symbol == oDataLong.Symbol.Symbol && p.Quantity == nQuantity && p.OrderDirection == FuturesOrderDirection.Buy);
                }
                if( oDataLong.Position == null) 
                {
                    IFuturesPosition[] aPositions = oDataLong.Symbol.Exchange.Account.PositionManager.GetData();
                    oDataLong.Position = aPositions.FirstOrDefault(p => p.Symbol.Symbol == oDataLong.Symbol.Symbol && p.Quantity == nQuantity && p.Direction == FuturesPositionDirection.Long);
                }
                if (oDataShort.OpenOrder == null)
                {
                    IFuturesOrder[] aOrdersShort = oDataShort.Symbol.Exchange.Account.OrderManager.GetData();
                    oDataShort.OpenOrder = aOrdersShort.FirstOrDefault(p => p.Symbol.Symbol == oDataLong.Symbol.Symbol && p.Quantity == nQuantity && p.OrderDirection == FuturesOrderDirection.Sell);
                }
                if( oDataShort.Position == null) 
                {
                    IFuturesPosition[] aPositions = oDataShort.Symbol.Exchange.Account.PositionManager.GetData();
                    oDataShort.Position = aPositions.FirstOrDefault(p => p.Symbol.Symbol == oDataShort.Symbol.Symbol && p.Quantity == nQuantity && p.Direction == FuturesPositionDirection.Short);

                }

                if (oDataLong.Position != null && oDataShort.Position != null)
                {
                    return true;
                }

                nRetries++;
            }
            return false;

        }
        public async Task<bool> TryOpenMarket(decimal nMoney)
        {
            decimal nDifference = Update(nMoney );
            if (nDifference < 0) return false;

            IOrderbookPrice? oPriceLong = null;
            if (LongData.Orderbook != null) oPriceLong = LongData.Orderbook.GetBestPrice(true, null, nMoney);
            IOrderbookPrice? oPriceShort = null;
            if (ShortData.Orderbook != null) oPriceShort = ShortData.Orderbook.GetBestPrice(false, null, nMoney);

            if( oPriceLong == null || oPriceShort == null ) return false;


            decimal nPrice = Math.Max(oPriceLong.Price, oPriceShort.Price); 

            int nDecimals = Math.Min(LongData.Symbol.Decimals, ShortData.Symbol.Decimals);
            decimal nQuantity = Math.Round(nMoney /  nPrice, nDecimals);

            
            if( nQuantity <= 0 ) return false;
            // Set leverages
            bool bResult = await SetLeverages();
            if (!bResult) return false;
            List<Task<IFuturesOrder?>> aTasks = new List<Task<IFuturesOrder?>>();
            aTasks.Add(LongData.Symbol.Exchange.Trading.CreateMarketOrder(LongData.Symbol, true, nQuantity));
            aTasks.Add(ShortData.Symbol.Exchange.Trading.CreateMarketOrder(ShortData.Symbol, false, nQuantity));


            await Task.WhenAll(aTasks);
            if (aTasks.Any(p => p.Result == null)) return false;

            return await UpdatePositions(nQuantity);
            /*
            */
        }

        /*
        /// <summary>
        /// Calculate fees based on position
        /// </summary>
        private void FeesOnPosition()
        {
            if (PositionLong == null || PositionShort == null) return;
            decimal nFeesLong = (PositionLong.Symbol.FeeTaker + PositionLong.Symbol.FeeMaker) * PositionLong.Quantity * PositionLong.AveragePrice;
            decimal nFeesShort = (PositionShort.Symbol.FeeTaker + PositionShort.Symbol.FeeMaker) * PositionShort.Quantity * PositionShort.AveragePrice;

            Fees = nFeesLong + nFeesShort;
        }
        */

        /// <summary>
        /// Create from positions
        /// </summary>
        /// <param name="aExchanges"></param>
        /// <returns></returns>
        public static async Task<IOppositeOrder[]?> CreateFromExchanges(IFuturesExchange[] aExchanges)
        {
            return null;
            // throw new NotImplementedException();
            /*
            await Task.Delay(2000);
            List<IOppositeOrder> aResult = new List<IOppositeOrder>();  
            for( int i = 0; i < aExchanges.Length; i++ )
            {
                IFuturesExchange oExchange1 = aExchanges[i];
                IFuturesPosition[] aPositions1 = oExchange1.Account.PositionManager.GetData();
                if (aPositions1.Length <= 0) continue;
                for( int j = i +1; j < aExchanges.Length; j++ )
                {
                    IFuturesExchange oExchange2 = aExchanges[j];
                    IFuturesPosition[] aPositions2 = oExchange2.Account.PositionManager.GetData();
                    if (aPositions2.Length <= 0) continue;

                    foreach( IFuturesPosition oPosition1 in aPositions1)
                    {
                        IFuturesPosition? oPosition2 = aPositions2
                            .FirstOrDefault(p => p.Symbol.Base == oPosition1.Symbol.Base &&
                                                p.Symbol.Quote == oPosition1.Symbol.Quote && 
                                                p.Quantity == oPosition1.Quantity &&
                                                p.Direction != oPosition1.Direction);
                        if( oPosition2 == null ) continue;  
                        if( oPosition1.Direction == FuturesPositionDirection.Long )
                        {
                            OppositeOrder oOrder = new OppositeOrder(oPosition1.Symbol, oPosition2.Symbol);
                            oOrder.PositionLong = oPosition1;
                            oOrder.PositionShort = oPosition2;
                            oOrder.FeesOnPosition();
                            aResult.Add(oOrder);    
                        }
                        else
                        {
                            OppositeOrder oOrder = new OppositeOrder(oPosition2.Symbol, oPosition2.Symbol);
                            oOrder.PositionLong = oPosition2;
                            oOrder.PositionShort = oPosition1;
                            oOrder.FeesOnPosition();
                            aResult.Add(oOrder);

                        }
                    }
                }
            }

            return aResult.ToArray();
            */
        }
    }
}
