using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XT.Net.Objects.Models;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartTrading : IFuturesTrading
    {

        private BitmartFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;

        private static ConcurrentDictionary<string, IFuturesLeverage> m_aLeverages = new ConcurrentDictionary<string, IFuturesLeverage>();

        public BitmartTrading(BitmartFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oGlobalClient = oExchange.GlobalClient;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }


        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="oOrder"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CancelOrder(IFuturesOrder oOrder)
        {
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.CancelOrderAsync(oOrder.Symbol.Symbol, oOrder.Id);
            if( oResult == null || !oResult.Success ) return false;
            return true;
        }

        public async Task<bool> ClosePosition(IFuturesPosition oPosition, decimal? nPrice = null)
        {
            decimal nContractSize = ((BitmartSymbol)oPosition.Symbol).ContractSize;
            int nNewQuantity = (int)(oPosition.Quantity / nContractSize);
            if (nNewQuantity <= 0) return false;
            var eSide = (oPosition.Direction == FuturesPositionDirection.Long ? BitMart.Net.Enums.FuturesSide.SellCloseLong : BitMart.Net.Enums.FuturesSide.BuyCloseShort);
            if( nPrice == null )
            {
                var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.PlaceOrderAsync(
                        oPosition.Symbol.Symbol,
                        eSide,
                        BitMart.Net.Enums.FuturesOrderType.Market,
                        nNewQuantity
                    );
                if (oResult == null || !oResult.Success) return false;
                if (oResult.Data == null) return false;

            }
            else
            {
                var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.PlaceOrderAsync(
                        oPosition.Symbol.Symbol,
                        eSide,
                        BitMart.Net.Enums.FuturesOrderType.Limit,
                        nNewQuantity,
                        nPrice
                    );
                if (oResult == null || !oResult.Success) return false;
                if (oResult.Data == null) return false;

            }
            return true;
        }


        /// <summary>
        /// Create limit order
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nQuantity"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal nPrice)
        {
            decimal nContractSize = ((BitmartSymbol)oSymbol).ContractSize;
            int nNewQuantity = (int) (nQuantity / nContractSize);
            if (nNewQuantity <= 0) return null;
            var eSide = (bLong ? BitMart.Net.Enums.FuturesSide.BuyOpenLong : BitMart.Net.Enums.FuturesSide.SellOpenShort);
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.PlaceOrderAsync( 
                    oSymbol.Symbol, 
                    eSide,
                    BitMart.Net.Enums.FuturesOrderType.Limit,
                    nNewQuantity,
                    nPrice
                );
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null ) return null;

            BitmartOrder oResultOrder = new BitmartOrder(oSymbol, oResult.Data.OrderId);
            oResultOrder.Quantity = nQuantity;
            oResultOrder.PositionDirection = (bLong ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            oResultOrder.Price = nPrice;
            oResultOrder.OrderType = FuturesOrderType.Limit;
            return oResultOrder;
        }

        public async Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity)
        {
            decimal nContractSize = ((BitmartSymbol)oSymbol).ContractSize;
            int nNewQuantity = (int)(nQuantity / nContractSize);
            if (nNewQuantity <= 0) return null;
            var eSide = (bLong ? BitMart.Net.Enums.FuturesSide.BuyOpenLong : BitMart.Net.Enums.FuturesSide.SellOpenShort);
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol,
                    eSide,
                    BitMart.Net.Enums.FuturesOrderType.Market,
                    nNewQuantity
                );
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;

            BitmartOrder oResultOrder = new BitmartOrder(oSymbol, oResult.Data.OrderId);
            oResultOrder.Quantity = nQuantity;
            oResultOrder.PositionDirection = (bLong ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            oResultOrder.Price = null;
            oResultOrder.OrderType = FuturesOrderType.Market;
            return oResultOrder;
        }

        public async Task<IFuturesLeverage?> GetLeverage(IFuturesSymbol oSymbol)
        {

            IFuturesLeverage? oResult = null;

            if (m_aLeverages.TryGetValue(oSymbol.Symbol, out oResult))
            {
                return oResult;
            }
            oResult = new BitmartLeverage(oSymbol);
            m_aLeverages.AddOrUpdate(oSymbol.Symbol, p => oResult, (s, p) => oResult);
            return oResult;
        }

        public async Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null)
        {
            IFuturesSymbol[]? aCheckSymbols = aSymbols;
            if( aCheckSymbols == null)
            {
                aSymbols = await Exchange.Market.GetSymbols();
            }
            if (aCheckSymbols == null) return null;
            List<IFuturesLeverage> aResult = new List<IFuturesLeverage>();
            foreach( var oSymbol in aCheckSymbols)
            {
                IFuturesLeverage? oFound = await GetLeverage(oSymbol);
                if (oFound != null) { aResult.Add(oFound); }
            }
            return aResult.ToArray();
        }

        public async Task<IFuturesOrder[]?> GetOrders()
        {
            IFuturesSymbol[]? aSymbols = await Exchange.Market.GetSymbols();
            if( aSymbols == null) return null;  
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.GetOpenOrdersAsync();
            if( oResult == null || !oResult.Success ) return null;
            if (oResult.Data == null) return null;
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();
            foreach( var oData in oResult.Data )
            {
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p=> p.Symbol == oData.Symbol );
                if (oFound == null) continue;
                aResult.Add( new BitmartOrder(oFound, oData) );
            }

            return aResult.ToArray(); 
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            IFuturesLeverage? oLeverage = await GetLeverage(oSymbol);
            if( oLeverage == null ) return false;
            if( oLeverage.ShortLeverage == nLeverage && oLeverage.LongLeverage == nLeverage ) return true;

            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, (decimal)nLeverage, BitMart.Net.Enums.MarginType.CrossMargin);
            if (oResult == null || !oResult.Success) return false;
            ((BitmartLeverage)oLeverage).ShortLeverage = nLeverage;
            ((BitmartLeverage)oLeverage).LongLeverage = nLeverage;

            return true;
        }
    }
}
