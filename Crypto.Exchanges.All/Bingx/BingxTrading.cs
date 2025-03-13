using BingX.Net.Objects.Models;
using Crypto.Common;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XT.Net.Objects.Models;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxTrading : IFuturesTrading
    {

        private IExchangeRestClient m_oGlobalClient;
        public BingxTrading(IFuturesExchange oExchange, IExchangeRestClient oClient) 
        { 
            Exchange = oExchange;   
            m_oGlobalClient = oClient;
        }

        public IFuturesExchange Exchange { get; }


        /// <summary>
        /// Creates order
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bBuy"></param>
        /// <param name="nQuantity"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<ITradingResult<IFuturesOrder?>> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal nPrice)
        {
            try
            {
                var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol,
                    (bLong ? BingX.Net.Enums.OrderSide.Buy : BingX.Net.Enums.OrderSide.Sell),
                    BingX.Net.Enums.FuturesOrderType.Limit,
                    (bLong ? BingX.Net.Enums.PositionSide.Long : BingX.Net.Enums.PositionSide.Short),
                    nQuantity,
                    nPrice
                );

                if (oResult == null) return new TradingResult<IFuturesOrder?>("Result returned null");
                if ( !oResult.Success) return new TradingResult<IFuturesOrder?>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<IFuturesOrder?>("Result returned data null");

                return new TradingResult<IFuturesOrder?>( new BingxOrder(oSymbol, oResult.Data));
            }
            catch (Exception ex)
            {
                return new TradingResult<IFuturesOrder?>(ex);
            }
        }

        public async Task<ITradingResult<IFuturesOrder?>> CreateMarketOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity)
        {
            try
            {
                var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.PlaceOrderAsync(
                        oSymbol.Symbol,
                        (bLong ? BingX.Net.Enums.OrderSide.Buy : BingX.Net.Enums.OrderSide.Sell),
                        BingX.Net.Enums.FuturesOrderType.Market,
                        (bLong ? BingX.Net.Enums.PositionSide.Long : BingX.Net.Enums.PositionSide.Short),
                        nQuantity
                    );
                if (oResult == null) return new TradingResult<IFuturesOrder?>("Result returned null");
                if (!oResult.Success) return new TradingResult<IFuturesOrder?>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<IFuturesOrder?>("Result returned data null");

                return new TradingResult<IFuturesOrder?>(new BingxOrder(oSymbol, oResult.Data));
            }
            catch (Exception ex)
            {
                return new TradingResult<IFuturesOrder?>(ex);
            }

        }


        public async Task<ITradingResult<bool>> ClosePosition(IFuturesPosition oPositon, decimal? nPrice = null)
        {
            try
            { 
                WebCallResult<BingXFuturesOrder>? oResult = null;
                if( nPrice == null )
                {
                    oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.PlaceOrderAsync(
                            oPositon.Symbol.Symbol,
                            (oPositon.Direction == FuturesPositionDirection.Short ? BingX.Net.Enums.OrderSide.Buy : BingX.Net.Enums.OrderSide.Sell),
                            BingX.Net.Enums.FuturesOrderType.Market,
                            (oPositon.Direction == FuturesPositionDirection.Long ? BingX.Net.Enums.PositionSide.Long : BingX.Net.Enums.PositionSide.Short),
                            oPositon.Quantity
                        );

                }
                else
                {
                    oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.PlaceOrderAsync(
                            oPositon.Symbol.Symbol,
                            (oPositon.Direction == FuturesPositionDirection.Short ? BingX.Net.Enums.OrderSide.Buy : BingX.Net.Enums.OrderSide.Sell),
                            BingX.Net.Enums.FuturesOrderType.Limit,
                            (oPositon.Direction == FuturesPositionDirection.Long ? BingX.Net.Enums.PositionSide.Long : BingX.Net.Enums.PositionSide.Short),
                            oPositon.Quantity,
                            nPrice.Value
                        );

                }
                if (oResult == null) return new TradingResult<bool>("Result returned null");
                if (!oResult.Success) return new TradingResult<bool>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<bool>("Result returned data null");

                return new TradingResult<bool>(true);
            }
            catch (Exception ex)
            {
                return new TradingResult<bool>(ex);
            }
        }
        /// <summary>
        /// Get leverage single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFuturesLeverage?> GetLeverage(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Account.GetLeverageAsync(oSymbol.Symbol);
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null ) return null; 
            return new BingxLeverage(oSymbol, oResult.Data);
        }

        /// <summary>
        /// Get leverage multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null)
        {
            IFuturesSymbol[]? aRequestSymbols;
            if( aSymbols == null )
            {
               aRequestSymbols = Exchange.SymbolManager.GetAllValues();
            }
            else
            {
                aRequestSymbols = aSymbols;
            }
            if (aRequestSymbols == null) return null;

            ITaskManager<IFuturesLeverage?> oTaskManager = CommonFactory.CreateTaskManager<IFuturesLeverage?>(BingxFutures.TASK_COUNT);
            List<IFuturesLeverage> aResult = new List<IFuturesLeverage>();

            foreach (IFuturesSymbol oSymbol in aRequestSymbols)
            {
                await oTaskManager.Add(GetLeverage(oSymbol));
            }

            var aTaskResults = await oTaskManager.GetResults();
            if (aTaskResults == null) return null;
            foreach (var oResult in aTaskResults)
            {
                if (oResult == null ) continue;
                aResult.Add(oResult);
            }
            return aResult.ToArray();

        }

        /// <summary>
        /// Get orders
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesOrder[]?> GetOrders()
        {
            IFuturesSymbol[]? aSymbols = Exchange.SymbolManager.GetAllValues();   
            if ( aSymbols == null ) return null;
            var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.GetOpenOrdersAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();    

            foreach( var oData in  oResult.Data) 
            { 
                IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p=> p.Symbol == oData.Symbol);    
                if (oSymbol == null) continue;
                aResult.Add( new BingxOrder(oSymbol, oData));
            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Set leverage
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="nLeverage"></param>
        /// <returns></returns>
        public async Task<ITradingResult<bool>> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            try
            {
                var oResultLong = await m_oGlobalClient.BingX.PerpetualFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, BingX.Net.Enums.PositionSide.Long, nLeverage);
                if (oResultLong == null) return new TradingResult<bool>("Result returned null");
                if (!oResultLong.Success) return new TradingResult<bool>(oResultLong.Error!.ToString());
                if (oResultLong.Data == null) return new TradingResult<bool>("Result returned data null");

                var oResultShort = await m_oGlobalClient.BingX.PerpetualFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, BingX.Net.Enums.PositionSide.Short, nLeverage);

                if (oResultShort == null) return new TradingResult<bool>("Result returned null");
                if (!oResultShort.Success) return new TradingResult<bool>(oResultShort.Error!.ToString());
                if (oResultShort.Data == null) return new TradingResult<bool>("Result returned data null");
                return new TradingResult<bool>(true);
            }
            catch (Exception ex)
            {
                return new TradingResult<bool>(ex);
            }
        }


        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="oOrder"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ITradingResult<bool>> CancelOrder( IFuturesOrder oOrder)
        {

            try
            { 
                var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.CancelOrderAsync(oOrder.Symbol.Symbol, long.Parse(oOrder.Id));

                if (oResult == null) return new TradingResult<bool>("Result returned null");
                if (!oResult.Success) return new TradingResult<bool>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<bool>("Result returned data null");

                return new TradingResult<bool>(true);
            }
            catch (Exception ex)
            {
                return new TradingResult<bool>(ex);
            }

        }


        public async Task<ITradingResult<bool>> SetStopLossTakeProfit(IFuturesPosition oPosition, decimal nStopLoss, decimal nTakeProfit)
        {
            throw new NotImplementedException();    
        }
    }
}
