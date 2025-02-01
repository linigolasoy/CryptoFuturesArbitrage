using Crypto.Interface.Futures;
using CryptoClients.Net.Interfaces;
using CoinEx.Net.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XT.Net.Objects.Models;
using CryptoExchange.Net.Objects;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexTrading : IFuturesTrading
    {

        private IExchangeRestClient m_oGlobalClient;
        private const int RETRIES = 5;

        private static ConcurrentDictionary<string, IFuturesLeverage> m_aLeverages = new ConcurrentDictionary<string, IFuturesLeverage>();
        public CoinexTrading(IFuturesExchange oExchange, IExchangeRestClient oClient) 
        { 
            Exchange = oExchange;
            m_oGlobalClient = oClient;
        }
        public IFuturesExchange Exchange { get; }


        private OrderSide GetOrderSide( bool bBuy, bool bLong )
        {
            OrderSide eSide = OrderSide.Buy;
            if (bLong)
            {
                eSide = (bBuy ? OrderSide.Buy : OrderSide.Sell);
            }
            else
            {
                eSide = (bBuy ? OrderSide.Sell : OrderSide.Buy);
            }
            return eSide;
        }
        /// <summary>
        /// Creates a limit order
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
            int nRetries = 0;
            while (nRetries++ <= RETRIES)
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol,
                    eSide,
                    OrderTypeV2.Limit,
                    nQuantity,
                    nPrice
                );

                if (oResult == null) return null;
                if (oResult.Success)
                {
                    if (oResult.Data == null) return null;
                    IFuturesOrder oOrder = new CoinexOrder(oSymbol, bLong, bLong, oResult.Data, OrderUpdateType.Put);

                    return oOrder;
                }
                //|| !oResult.Success) return null;
                if (!HasToRetry(oResult.Error)) return null;
                await Task.Delay(500);
            }
            return null;
        }

        public async Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity)
        {
            OrderSide eSide = (bLong ? OrderSide.Buy : OrderSide.Sell);
            int nRetries = 0;
            while (nRetries++ <= RETRIES)
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol,
                    eSide,
                    OrderTypeV2.Market,
                    nQuantity
                );

                if (oResult == null) return null;
                if (oResult.Success)
                {
                    if (oResult.Data == null) return null;
                    IFuturesOrder oOrder = new CoinexOrder(oSymbol, bLong, bLong, oResult.Data, OrderUpdateType.Put);

                    return oOrder;
                }
                //|| !oResult.Success) return null;
                if (!HasToRetry(oResult.Error)) return null;
            }
            return null;
        }

        public async Task<bool> ClosePosition(IFuturesPosition oPositon, decimal? nPrice = null)
        {
            OrderSide eSide = (oPositon.Direction == FuturesPositionDirection.Long ? OrderSide.Sell : OrderSide.Buy);
            if ( nPrice == null )
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Trading.PlaceOrderAsync(
                    oPositon.Symbol.Symbol,
                    eSide,
                    OrderTypeV2.Market,
                    oPositon.Quantity
                );
                if (oResult == null || !oResult.Success) return false;
                return true;

            }
            else
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Trading.PlaceOrderAsync(
                    oPositon.Symbol.Symbol,
                    eSide,
                    OrderTypeV2.Limit,
                    oPositon.Quantity,
                    nPrice.Value
                );
                if (oResult == null || !oResult.Success ) return false;
                return true;

            }
        }

        /// <summary>
        /// Get leverage single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFuturesLeverage?> GetLeverage(IFuturesSymbol oSymbol)
        {
            IFuturesLeverage? oResult = null;

            if( m_aLeverages.TryGetValue(oSymbol.Symbol, out oResult) ) 
            { 
                return oResult; 
            }
            oResult = new CoinexLeverage(oSymbol);
            m_aLeverages.AddOrUpdate(oSymbol.Symbol, p=> oResult, (s,p)=> oResult); 

            return oResult; 
        }

        /// <summary>
        /// Get leverage multiple
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null)
        {
            List<IFuturesLeverage> aResult = new List<IFuturesLeverage>();
            IFuturesSymbol[]? aMatch = aSymbols;
            if( aMatch == null )
            {
                aMatch = Exchange.SymbolManager.GetAllValues();
            }
            if (aMatch == null) return null;
            foreach( var oSymbol in aMatch )
            {
                IFuturesLeverage? oFound = await GetLeverage(oSymbol);
                if( oFound != null ) aResult.Add(oFound);
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
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Trading.GetOpenOrdersAsync();
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null || oResult.Data.Items == null ) return null;
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();
            foreach( var oData in oResult.Data.Items )
            {
                IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue;
                bool bBuy = false;
                if (oData.Side == OrderSide.Buy) bBuy = true;
                IFuturesOrder oNew = new CoinexOrder(oSymbol, bBuy, true, oData, OrderUpdateType.Put);
                aResult.Add(oNew);  
            }
            return aResult.ToArray();
        }


        private bool HasToRetry(Error? oError)
        {
            if (oError == null) return false;
            if (oError.Code == 3008)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Sets leverage of specific symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="nLeverage"></param>
        /// <returns></returns>
        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            IFuturesLeverage? oLeverage = await GetLeverage(oSymbol);
            if( oLeverage == null ) return false;
            int nRetries = 0;
            while( nRetries ++ <= RETRIES )
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, MarginMode.Cross, nLeverage);
                if (oResult == null) return false;
                if (oResult.Success) break;
                if( !HasToRetry(oResult.Error )) return false;  
                await Task.Delay(500);

            }
            ((CoinexLeverage)oLeverage).LongLeverage = nLeverage;
            ((CoinexLeverage)oLeverage).ShortLeverage = nLeverage;
            m_aLeverages.AddOrUpdate(oSymbol.Symbol, p => oLeverage, (s, p) => oLeverage);
            return true;
        }
        public async Task<bool> CancelOrder(IFuturesOrder oOrder)
        {
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Trading.CancelOrderAsync(oOrder.Symbol.Symbol, long.Parse(oOrder.Id));
            if( oResult == null || !oResult.Success ) return false;
            return true;
        }
    }
}
