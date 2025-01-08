using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Enums.V2;
using Bitget.Net.Objects;
using Crypto.Interface.Futures;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetTrading: IFuturesTrading
    {

        private const string USDT = "USDT";
        private IExchangeRestClient m_oGlobalClient;
        public BitgetTrading(ICryptoFuturesExchange oExchange, BitgetApiCredentials oCredentials)
        {
            Exchange = oExchange;
            BitgetRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = oCredentials;
            });
            m_oGlobalClient = new ExchangeRestClient();
        }

        public ICryptoFuturesExchange Exchange { get; }

        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, bool bLong, decimal nQuantity, decimal nPrice)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bBuy, bool bLong, decimal nQuantity)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesLeverage?> GetLeverage(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
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
            if (oResultLong.Success && oResultShort.Success) return true;
            return false;
        }
        public async Task<bool> CancelOrder(IFuturesOrder oOrder)
        {
            throw new NotImplementedException();
        }
    }
}
