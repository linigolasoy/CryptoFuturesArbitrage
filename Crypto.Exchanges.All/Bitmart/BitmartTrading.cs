using Crypto.Exchanges.All.Common;
using Crypto.Interface;
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
        public async Task<ITradingResult<bool>> CancelOrder(IFuturesOrder oOrder)
        {
            try
            {
                var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.CancelOrderAsync(oOrder.Symbol.Symbol, oOrder.Id);
                if (oResult == null) return new TradingResult<bool>("Result returned null");
                if (!oResult.Success) return new TradingResult<bool>(oResult.Error!.ToString());

                return new TradingResult<bool>(true);
            }
            catch (Exception ex)
            {
                return new TradingResult<bool>(ex);
            }
        }

        public async Task<ITradingResult<bool>> ClosePosition(IFuturesPosition oPosition, decimal? nPrice = null)
        {
            try
            {
                decimal nContractSize = ((BitmartSymbol)oPosition.Symbol).ContractSize;
                int nNewQuantity = (int)(oPosition.Quantity / nContractSize);
                if (nNewQuantity <= 0) return new TradingResult<bool>("Quantity less than zero");
                var eSide = (oPosition.Direction == FuturesPositionDirection.Long ? BitMart.Net.Enums.FuturesSide.SellCloseLong : BitMart.Net.Enums.FuturesSide.BuyCloseShort);
                var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.PlaceOrderAsync(
                        oPosition.Symbol.Symbol,
                        eSide,
                        ( nPrice == null ? BitMart.Net.Enums.FuturesOrderType.Market : BitMart.Net.Enums.FuturesOrderType.Limit),
                        nNewQuantity,
                        nPrice
                    );
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
        /// Create limit order
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nQuantity"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<ITradingResult<IFuturesOrder?>> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal nPrice)
        {
            try
            {
                decimal nContractSize = ((BitmartSymbol)oSymbol).ContractSize;
                int nNewQuantity = (int) (nQuantity / nContractSize);
                if (nNewQuantity <= 0) return new TradingResult<IFuturesOrder?>("Quantity zero");
                var eSide = (bLong ? BitMart.Net.Enums.FuturesSide.BuyOpenLong : BitMart.Net.Enums.FuturesSide.SellOpenShort);
                var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.PlaceOrderAsync( 
                    oSymbol.Symbol, 
                    eSide,
                    BitMart.Net.Enums.FuturesOrderType.Limit,
                    nNewQuantity,
                    nPrice
                );
                if (oResult == null) return new TradingResult<IFuturesOrder?>("Result returned null");
                if (!oResult.Success) return new TradingResult<IFuturesOrder?>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<IFuturesOrder?>("Result returned data null");

                BitmartOrder oResultOrder = new BitmartOrder(oSymbol, oResult.Data.OrderId);
                oResultOrder.Quantity = nQuantity;
                oResultOrder.PositionDirection = (bLong ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
                oResultOrder.Price = nPrice;
                oResultOrder.OrderType = FuturesOrderType.Limit;
                return new TradingResult<IFuturesOrder?>(oResultOrder);
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
                decimal nContractSize = ((BitmartSymbol)oSymbol).ContractSize;
                int nNewQuantity = (int)(nQuantity / nContractSize);
                if (nNewQuantity <= 0) return new TradingResult<IFuturesOrder?>("Quantity zero");
                var eSide = (bLong ? BitMart.Net.Enums.FuturesSide.BuyOpenLong : BitMart.Net.Enums.FuturesSide.SellOpenShort);
                var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol,
                    eSide,
                    BitMart.Net.Enums.FuturesOrderType.Market,
                    nNewQuantity
                );
                if (oResult == null) return new TradingResult<IFuturesOrder?>("Result returned null");
                if (!oResult.Success) return new TradingResult<IFuturesOrder?>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<IFuturesOrder?>("Result returned data null");

                BitmartOrder oResultOrder = new BitmartOrder(oSymbol, oResult.Data.OrderId);
                oResultOrder.Quantity = nQuantity;
                oResultOrder.PositionDirection = (bLong ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
                oResultOrder.Price = null;
                oResultOrder.OrderType = FuturesOrderType.Market;
                return new TradingResult<IFuturesOrder?>(oResultOrder);
            }
            catch (Exception ex)
            {
                return new TradingResult<IFuturesOrder?>(ex);
            }
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
                aSymbols = Exchange.SymbolManager.GetAllValues();
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
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.GetOpenOrdersAsync();
            if( oResult == null || !oResult.Success ) return null;
            if (oResult.Data == null) return null;
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();
            foreach( var oData in oResult.Data )
            {
                IFuturesSymbol? oFound = Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oFound == null) continue;
                aResult.Add( new BitmartOrder(oFound, oData) );
            }

            return aResult.ToArray(); 
        }

        public async Task<ITradingResult<bool>> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            try
            {
                IFuturesLeverage? oLeverage = await GetLeverage(oSymbol);
                if ( oLeverage == null) return new TradingResult<bool>("No Leverage found");
                if ( oLeverage.ShortLeverage == nLeverage && oLeverage.LongLeverage == nLeverage ) return new TradingResult<bool>(true);

                var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, (decimal)nLeverage, BitMart.Net.Enums.MarginType.CrossMargin);
                if (oResult == null) return new TradingResult<bool>("Result returned null");
                if (!oResult.Success) return new TradingResult<bool>(oResult.Error!.ToString());
                if (oResult.Data == null) return new TradingResult<bool>("Result returned data null");
                ((BitmartLeverage)oLeverage).ShortLeverage = nLeverage;
                ((BitmartLeverage)oLeverage).LongLeverage = nLeverage;
                return new TradingResult<bool>(true);
            }
            catch (Exception ex)
            {
                return new TradingResult<bool>(ex);
            }
        }
    }
}
