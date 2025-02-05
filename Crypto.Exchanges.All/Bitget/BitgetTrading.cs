using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Enums.V2;
using Bitget.Net.Objects;
using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XT.Net.Objects.Models;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetTrading: IFuturesTrading
    {


        private static ConcurrentDictionary<string, IFuturesLeverage> m_aLeverages = new ConcurrentDictionary<string, IFuturesLeverage>();

        public const string USDT = "USDT";
        private IExchangeRestClient m_oGlobalClient;
        public BitgetTrading(IFuturesExchange oExchange, BitgetApiCredentials oCredentials)
        {
            Exchange = oExchange;
            BitgetRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = oCredentials;
            });
            m_oGlobalClient = new ExchangeRestClient();
        }

        public IFuturesExchange Exchange { get; }


        private OrderSide GetOrderSide(bool bBuy, bool bLong)
        {
            OrderSide eSide = OrderSide.Buy;
            if (bLong)
            {
                eSide = (bBuy ? OrderSide.Buy : OrderSide.Buy);
            }
            else
            {
                eSide = (bBuy ? OrderSide.Sell : OrderSide.Buy);
            }
            return eSide;
        }

        private TradeSide GetTradeSide( bool bBuy, bool bLong)
        {
            if (bLong) return (bBuy ? TradeSide.Open : TradeSide.Close);
            return (!bBuy ? TradeSide.Open : TradeSide.Close);
        }

        /// <summary>
        /// Create limit order
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bBuy"></param>
        /// <param name="bLong"></param>
        /// <param name="nQuantity"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<ITradingResult<IFuturesOrder?>> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal nPrice)
        {
            try
            {
                OrderSide eSide = (bLong ? OrderSide.Buy : OrderSide.Sell);
                TradeSide eTradeSide = TradeSide.Open;
                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.PlaceOrderAsync(
                    BitgetProductTypeV2.UsdtFutures,
                    oSymbol.Symbol,
                    USDT,
                    eSide,
                    OrderType.Limit,
                    MarginMode.CrossMargin,
                    nQuantity,
                    nPrice,
                    null
                    ,
                    eTradeSide
                );
                if (oResult == null) return new TradingResult<IFuturesOrder?>("Result returned null");
                if (!oResult.Success) return new TradingResult<IFuturesOrder?>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<IFuturesOrder?>("Result returned data null");

                IFuturesOrder oOrder = new BitgetOrder(oSymbol, oResult.Data, bLong, bLong, FuturesOrderType.Limit, nQuantity, nPrice);
                return new TradingResult<IFuturesOrder?>(oOrder);
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
                OrderSide eSide = (bLong ? OrderSide.Buy : OrderSide.Sell);
                TradeSide eTradeSide = TradeSide.Open;
                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.PlaceOrderAsync(
                    BitgetProductTypeV2.UsdtFutures,
                    oSymbol.Symbol,
                    USDT,
                    eSide,
                    OrderType.Market,
                    MarginMode.CrossMargin,
                    nQuantity,
                    null,
                    null
                    ,
                    eTradeSide
                );
                if (oResult == null) return new TradingResult<IFuturesOrder?>("Result returned null");
                if (!oResult.Success) return new TradingResult<IFuturesOrder?>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<IFuturesOrder?>("Result returned data null");
                IFuturesOrder oOrder = new BitgetOrder(oSymbol, oResult.Data, bLong, bLong, FuturesOrderType.Market, nQuantity, null);
                return new TradingResult<IFuturesOrder?>(oOrder);
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
                OrderSide eSide = (oPositon.Direction == FuturesPositionDirection.Long ? OrderSide.Buy : OrderSide.Sell);
                TradeSide eTradeSide = TradeSide.Close;
                WebCallResult<BitgetOrderId>? oResult = null;
                if (nPrice == null)
                {
                    oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.PlaceOrderAsync(
                            BitgetProductTypeV2.UsdtFutures,
                            oPositon.Symbol.Symbol,
                            USDT,
                            eSide,
                            OrderType.Market,
                            MarginMode.CrossMargin,
                            oPositon.Quantity,
                            null,
                            null
                            ,
                            eTradeSide
                        );

                }
                else
                {
                    oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.PlaceOrderAsync(
                            BitgetProductTypeV2.UsdtFutures,
                            oPositon.Symbol.Symbol,
                            USDT,
                            eSide,
                            OrderType.Limit,
                            MarginMode.CrossMargin,
                            oPositon.Quantity,
                            nPrice.Value,
                            null
                            ,
                            eTradeSide
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
        /// Get leverage single
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesLeverage?> GetLeverage(IFuturesSymbol oSymbol)
        {
            IFuturesLeverage? oResult = null;
            if (m_aLeverages.TryGetValue(oSymbol.Symbol, out oResult))
            {
                return oResult;
            }
            oResult = new BitgetLeverage(oSymbol);
            m_aLeverages.AddOrUpdate(oSymbol.Symbol, p => oResult, (s, p) => oResult);

            return oResult;
        }

        /// <summary>
        /// Get leverage multiple
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null)
        {
            List<IFuturesLeverage> aResult = new List<IFuturesLeverage>();
            IFuturesSymbol[]? aMatch = aSymbols;
            if (aMatch == null)
            {
                aMatch = Exchange.SymbolManager.GetAllValues();
            }
            if (aMatch == null) return null;
            foreach (var oSymbol in aMatch)
            {
                IFuturesLeverage? oFound = await GetLeverage(oSymbol);
                if (oFound != null) aResult.Add(oFound);
            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Get orders
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesOrder[]?> GetOrders()
        {
            var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.GetOpenOrdersAsync(BitgetProductTypeV2.UsdtFutures);
            if( oResult == null || !oResult.Success ) return null;  
            if( oResult.Data == null || oResult.Data.Orders == null ) return null;
            IFuturesSymbol[]? aSymbols = Exchange.SymbolManager.GetAllValues();
            if (aSymbols == null) return null;
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();    
            foreach( var oData in oResult.Data.Orders )
            {
                if (oData == null) continue;
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p=> p.Symbol == oData.Symbol);
                if (oFound == null) continue;
                aResult.Add(new BitgetOrder(oFound, oData));
            }

            return aResult.ToArray();
        }

        public async Task<ITradingResult<bool>> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {

            try
            { 
                var oResultMargin = await m_oGlobalClient.Bitget.FuturesApiV2.Account.SetMarginModeAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, USDT, MarginMode.CrossMargin);
                if (oResultMargin == null) return new TradingResult<bool>("Result returned null");
                if (!oResultMargin.Success) return new TradingResult<bool>(oResultMargin.Error!.ToString());
                if (oResultMargin.Data == null) return new TradingResult<bool>("Result returned data null");


                var oResultLeverage = await m_oGlobalClient.Bitget.FuturesApiV2.Account.SetLeverageAsync(
                    BitgetProductTypeV2.UsdtFutures,
                    oSymbol.Symbol,
                    USDT,
                    nLeverage
                    );

                if (oResultLeverage == null) return new TradingResult<bool>("Result returned null");
                if (!oResultLeverage.Success) return new TradingResult<bool>(oResultLeverage.Error!.ToString());
                if (oResultLeverage.Data == null) return new TradingResult<bool>("Result returned data null");

                IFuturesLeverage? oFound = await GetLeverage(oSymbol);
                if (oFound == null) return new TradingResult<bool>("Leverage not found");
                ((BitgetLeverage)oFound).ShortLeverage = nLeverage;
                ((BitgetLeverage)oFound).LongLeverage = nLeverage;  
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
        public async Task<ITradingResult<bool>> CancelOrder(IFuturesOrder oOrder)
        {
            try
            {
                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.CancelOrderAsync(BitgetProductTypeV2.UsdtFutures, oOrder.Symbol.Symbol, oOrder.Id.ToString());
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
    }
}
