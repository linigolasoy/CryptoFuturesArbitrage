using BingX.Net.Clients;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.V5;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bybit
{
    /*
    internal class BybitFutures : ICryptoFuturesExchange
    {

        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;

        private IFuturesSymbol[]? m_aSymbols = null;

        public BybitFutures(ICryptoSetup oSetup) 
        {
            Setup = oSetup;

            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;

            BybitRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new ApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret);
            });
            m_oGlobalClient = new ExchangeRestClient();

        }
        public IFuturesBarFeeder BarFeeder => throw new NotImplementedException();

        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.ByBitFutures; }

        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nMargin, int nLeverage, decimal nPrice)
        {
            throw new NotImplementedException();
        }

        public async Task<ICryptoWebsocket?> CreateWebsocket()
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesBalance[]?> GetBalances()
        {
            throw new NotImplementedException();
        }

        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            throw new NotImplementedException();
        }

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols)
        {
            throw new NotImplementedException();
        }

        public async Task<ISymbol[]?> GetRawSymbols()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get symbol list
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if (m_aSymbols != null) return m_aSymbols;

            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            string? strNextPage = null; 
            while (true) 
            {
                var oResult = await m_oGlobalClient.Bybit.V5Api.ExchangeData.GetLinearInverseSymbolsAsync(Category.Linear,null, null, null, 1000,strNextPage);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null) break;
                if (oResult.Data.List == null) break;
                foreach (var oSymbol in oResult.Data.List)
                {
                    if (!oSymbol.UnifiedMarginTrade) continue;
                    if (oSymbol.QuoteAsset != "USDT") continue;
                    aResult.Add(new BybitSymbol(oSymbol));
                }

                if (oResult.Data.NextPageCursor == null || string.IsNullOrEmpty(oResult.Data.NextPageCursor)) break;
                strNextPage = oResult.Data.NextPageCursor;
            }


            m_aSymbols = aResult.ToArray(); 
            return m_aSymbols;   
        }
    }
    */
}
