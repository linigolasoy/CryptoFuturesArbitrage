using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.SharedApis;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XT.Net.Objects.Models;

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
        public OppositeOrder(IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort, int nLeverage, DateTime dLimitDate) 
        {
            Leverage = nLeverage;   
            LongData = new ArbitrageOrderData(oSymbolLong);
            ShortData = new ArbitrageOrderData(oSymbolShort);
            LimitDate = dLimitDate;
        }

        public DateTime LimitDate { get; }
        public int Leverage { get; }
        public IArbitrageOrderData LongData { get; }
        public IArbitrageOrderData ShortData { get; }

        public decimal Profit { get; private set; } = 0;

        public int ProfitUpdates { get; private set; } = 0;
        public decimal Fees { get; private set; } = 0;

        public async Task<ICloseResult> TryCloseLimit()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Update with orderbooks and rest if not having them
        /// </summary>
        public void Update()
        {
            if( LongData.Orderbook == null )
            {
                if( LongData.Symbol.Exchange.Market.Websocket != null )
                {
                    IOrderbook? oOrderbook = LongData.Symbol.Exchange.Market.Websocket!.OrderbookManager.GetData(LongData.Symbol.Symbol);
                    if( oOrderbook != null ) LongData.Orderbook = oOrderbook;   
                }
            }
            if (ShortData.Orderbook == null)
            {
                if (ShortData.Symbol.Exchange.Market.Websocket != null)
                {
                    IOrderbook? oOrderbook = ShortData.Symbol.Exchange.Market.Websocket!.OrderbookManager.GetData(ShortData.Symbol.Symbol);
                    if (oOrderbook != null) ShortData.Orderbook = oOrderbook;
                }
            }

            // Put profit if we have positions
            if( LongData.Position != null && ShortData.Position != null ) 
            { 
                if( LongData.Orderbook != null && ShortData.Orderbook != null)
                {
                    IOrderbookPrice? oPriceLong = LongData.Orderbook.GetBestPrice(false, LongData.Quantity, null);
                    IOrderbookPrice? oPriceShort = ShortData.Orderbook.GetBestPrice(true, ShortData.Quantity, null);
                    if( oPriceLong != null &&  oPriceShort != null )
                    {
                        decimal nProfitLong = (oPriceLong.Price - LongData.Position.AveragePrice) * LongData.Position.Quantity;
                        decimal nProfitShort = (ShortData.Position.AveragePrice - oPriceShort.Price) * ShortData.Position.Quantity;
                        decimal nProfit = nProfitLong + nProfitShort;
                        ((ArbitrageOrderData)this.LongData).Profit = nProfitLong;
                        ((ArbitrageOrderData)this.ShortData).Profit = nProfitShort;

                        this.Profit = nProfit;
                        ProfitUpdates++;

                    }
                }
            }
            /*
            decimal nResult = -9E10M;
            IFuturesWebsocketPublic? oWs = LongData.Symbol.Exchange.Market.Websocket;
            if (oWs != null)
            {
                IOrderbook? oBook = oWs.OrderbookManager.GetData(LongData.Symbol.Symbol);
                if (oBook != null) ((ArbitrageOrderData)LongData).Orderbook = oBook;
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

            if (oPriceLong == null || oPriceShort == null) return nResult;
            nResult = oPriceShort.Price - oPriceLong.Price;
            return nResult;
            */
        }


        public async Task<ICloseResult> TryCloseMarket()
        {
            Update();
            OppositeCloseResult oResult = new OppositeCloseResult() { ProfitOrLoss = 0, Success = false };  
            if (this.Profit < 0) return oResult;
            oResult.ProfitOrLoss = this.Profit; 
            if( LongData.Position == null || ShortData.Position == null ) { return oResult; }   
            List<Task<ITradingResult<bool>>> aTasks = new List<Task<ITradingResult<bool>>>();
            aTasks.Add(LongData.Symbol.Exchange.Trading.ClosePosition(LongData.Position));
            aTasks.Add(ShortData.Symbol.Exchange.Trading.ClosePosition(ShortData.Position));

            await Task.WhenAll(aTasks); 
            oResult.Success = true;
            return oResult;
        }

        /// <summary>
        /// Open limit order
        /// </summary>
        /// <param name="nMoney"></param>
        /// <returns></returns>
        public async Task<bool> TryOpenLimit(decimal nMoney)
        {
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
            List<Task<ITradingResult<IFuturesOrder?>>> aTasks = new List<Task<ITradingResult<IFuturesOrder?>>>();
            aTasks.Add(LongData.Symbol.Exchange.Trading.CreateLimitOrder(LongData.Symbol, true, nQuantity, oPriceLong.Price));
            aTasks.Add(ShortData.Symbol.Exchange.Trading.CreateLimitOrder(ShortData.Symbol, false, nQuantity, oPriceShort.Price));


            await Task.WhenAll(aTasks);
            if (aTasks.Any(p => !p.Result.Success)) return false;

            return true;
        }


        /// <summary>
        /// Set leverates
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetLeverages()
        {
            if (m_bLeverageSet) return true;
            ITradingResult<bool> oResult = await LongData.Symbol.Exchange.Trading.SetLeverage(LongData.Symbol, Leverage);
            if (!oResult.Success) return false;
            oResult = await ShortData.Symbol.Exchange.Trading.SetLeverage(ShortData.Symbol, Leverage);
            if (!oResult.Success) return false;
            m_bLeverageSet = true;  
            return true;
        }

        /*
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
        */
        public async Task<bool> TryOpenMarket(decimal nMoney)
        {
            // decimal nDifference = Update(nMoney );
            // if (nDifference < 0) return false;

            IOrderbookPrice? oPriceLong = null;
            if (LongData.Orderbook != null) oPriceLong = LongData.Orderbook.GetBestPrice(true, null, nMoney);
            IOrderbookPrice? oPriceShort = null;
            if (ShortData.Orderbook != null) oPriceShort = ShortData.Orderbook.GetBestPrice(false, null, nMoney);

            if( oPriceLong == null || oPriceShort == null ) return false;


            decimal nPrice = Math.Max(oPriceLong.Price, oPriceShort.Price); 
            decimal nPriceMin = Math.Min(oPriceLong.Price, oPriceShort.Price);
            decimal nPercent = (nPrice - nPriceMin) * 100 / nPriceMin;
            if( nPercent > 0.2M ) return false; 
            int nDecimals = Math.Min(LongData.Symbol.Decimals, ShortData.Symbol.Decimals);
            decimal nQuantity = Math.Round(nMoney /  nPrice, nDecimals);

            
            if( nQuantity <= 0 ) return false;
            // Set leverages
            //bool bResult = await SetLeverages();
            // if (!bResult) return false;

            List<Task<ITradingResult<IFuturesOrder?>>> aTasks = new List<Task<ITradingResult<IFuturesOrder?>>>();
            aTasks.Add(LongData.Symbol.Exchange.Trading.CreateMarketOrder(LongData.Symbol, true, nQuantity));
            aTasks.Add(ShortData.Symbol.Exchange.Trading.CreateMarketOrder(ShortData.Symbol, false, nQuantity));


            await Task.WhenAll(aTasks);
            if (aTasks.Any(p => !p.Result.Success)) return false;
            return true;
            // return await UpdatePositions(nQuantity);
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
            // throw new NotImplementedException();
            await Task.Delay(2000);
            List<IOppositeOrder> aResult = new List<IOppositeOrder>();  
            for( int i = 0; i < aExchanges.Length; i++ )
            {
                IFuturesExchange oExchange1 = aExchanges[i];
                IFuturesPosition[]? aPositions1 = await oExchange1.Account.GetPositions();
                if (aPositions1 == null || aPositions1.Length <= 0) continue;
                for( int j = i +1; j < aExchanges.Length; j++ )
                {
                    IFuturesExchange oExchange2 = aExchanges[j];
                    IFuturesPosition[]? aPositions2 = await oExchange2.Account.GetPositions();
                    if (aPositions2 == null || aPositions2.Length <= 0) continue;

                    foreach( IFuturesPosition oPosition1 in aPositions1)
                    {
                        IFuturesPosition? oPosition2 = aPositions2
                            .FirstOrDefault(p => p.Symbol.Base == oPosition1.Symbol.Base &&
                                                p.Symbol.Quote == oPosition1.Symbol.Quote && 
                                                //Math.Round(p.Quantity,0) == Math.Round(oPosition1.Quantity,0) &&
                                                p.Direction != oPosition1.Direction);
                        if( oPosition2 == null ) continue;  
                        if( oPosition1.Direction == FuturesPositionDirection.Long )
                        {
                            OppositeOrder oOrder = new OppositeOrder(oPosition1.Symbol, oPosition2.Symbol, 10, DateTime.Now);
                            oOrder.LongData.Position = oPosition1;
                            oOrder.LongData.Quantity = oPosition1.Quantity; 
                            oOrder.ShortData.Position = oPosition2;
                            oOrder.ShortData.Quantity = oPosition2.Quantity;
                            aResult.Add(oOrder);    
                        }
                        else
                        {
                            OppositeOrder oOrder = new OppositeOrder(oPosition2.Symbol, oPosition1.Symbol, 10, DateTime.Now);
                            oOrder.LongData.Position = oPosition2;
                            oOrder.LongData.Quantity = oPosition2.Quantity;
                            oOrder.ShortData.Position = oPosition1;
                            oOrder.ShortData.Quantity = oPosition1.Quantity;
                            aResult.Add(oOrder);

                        }
                    }
                }
            }

            return aResult.ToArray();
        }

        public override string ToString()
        {
            StringBuilder oBuild = new StringBuilder();
            oBuild.Append($"Currency [{this.LongData.Symbol.Base}] ");
            oBuild.Append($"Long [{this.LongData.Symbol.Exchange.ExchangeType.ToString()}] ");
            oBuild.Append($"Short [{this.ShortData.Symbol.Exchange.ExchangeType.ToString()}] ");
            oBuild.Append($"Qty [{this.LongData.Quantity}] ");
            if( LongData.Position != null && ShortData.Position != null )
            {
                oBuild.Append($"Positions. Profit [{this.Profit}] ");
            }
            return oBuild.ToString();
        }
    }
}
