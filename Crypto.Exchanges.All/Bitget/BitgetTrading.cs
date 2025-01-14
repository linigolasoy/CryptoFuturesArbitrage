using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Enums.V2;
using Bitget.Net.Objects;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
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
        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal nPrice)
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
            if( oResult == null || !oResult.Success ) return null;
            if( oResult.Data == null ) return null; 
            IFuturesOrder oOrder = new BitgetOrder(oSymbol, oResult.Data, bLong, bLong, FuturesOrderType.Limit, nQuantity, nPrice);  
            return oOrder;  
        }

        public async Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity)
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
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            IFuturesOrder oOrder = new BitgetOrder(oSymbol, oResult.Data, bLong, bLong, FuturesOrderType.Market, nQuantity, null);
            return oOrder;
        }

        public async Task<bool> ClosePosition(IFuturesPosition oPositon, decimal? nPrice = null)
        {
            OrderSide eSide = (oPositon.Direction == FuturesPositionDirection.Long ? OrderSide.Buy : OrderSide.Sell);
            TradeSide eTradeSide = TradeSide.Close;
            if ( nPrice == null )
            {
                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.PlaceOrderAsync(
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
                if (oResult == null || !oResult.Success) return false;
                if (oResult.Data == null) return false;
                return true;

            }
            else
            {
                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.PlaceOrderAsync(
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
                if (oResult == null || !oResult.Success) return false;
                if (oResult.Data == null) return false;
                return true;

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
                aMatch = await Exchange.Market.GetSymbols();
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
            IFuturesSymbol[]? aSymbols = await Exchange.Market.GetSymbols();
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

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            var oResultLong = await m_oGlobalClient.Bitget.FuturesApiV2.Account.SetLeverageAsync(
                BitgetProductTypeV2.UsdtFutures,
                oSymbol.Symbol,
                USDT,
                nLeverage,
                PositionSide.Long
                );
            var oResultShort = await m_oGlobalClient.Bitget.FuturesApiV2.Account.SetLeverageAsync(
                BitgetProductTypeV2.UsdtFutures,
                oSymbol.Symbol,
                USDT,
                nLeverage,
                PositionSide.Long
                );

            if (oResultLong == null || oResultShort == null) return false;
            if (!oResultLong.Success || !oResultShort.Success) return false;
            IFuturesLeverage? oFound = await GetLeverage(oSymbol);
            if (oFound == null) return false;
            ((BitgetLeverage)oFound).ShortLeverage = nLeverage;
            ((BitgetLeverage)oFound).LongLeverage = nLeverage;  
            return true;
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="oOrder"></param>
        /// <returns></returns>
        public async Task<bool> CancelOrder(IFuturesOrder oOrder)
        {
            var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.CancelOrderAsync(BitgetProductTypeV2.UsdtFutures, oOrder.Symbol.Symbol, oOrder.Id.ToString());
            if( oResult == null || !oResult.Success) return false;
            return true;
        }
    }
}
