using BingX.Net.Clients;
using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Objects;
using Crypto.Common;
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

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetFutures : ICryptoFuturesExchange
    {

        private const int TASK_COUNT = 20;
        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;

        private IFuturesSymbol[]? m_aSymbols = null;

        public BitgetFutures( ICryptoSetup oSetup ) 
        {
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;

            BitgetRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new BitgetApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret, "Cotton12$$");
            });
            m_oGlobalClient = new ExchangeRestClient();
        }
        public IFuturesBarFeeder BarFeeder => throw new NotImplementedException();

        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitgetFutures; }

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

        /// <summary>
        /// Funding rate snapshot single
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            var oResultRateTask = m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetFundingRateAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol);
            var oResultTimeTask = m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetNextFundingTimeAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol);

            var oResultRate = await oResultRateTask;
            var oResultTime = await oResultTimeTask;

            if (oResultRate == null || oResultRate.Data == null) return null;
            if (!oResultRate.Success) return null;
            if (oResultTime == null || oResultTime.Data == null) return null;
            if (!oResultTime.Success) return null;

            return new BitgetFuturesFundingRateSnap(oSymbol, oResultRate.Data, oResultTime.Data);
        }

        /// <summary>
        /// Funding rate snapshot multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            ITaskManager<IFundingRateSnapShot?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRateSnapShot?>(TASK_COUNT);
            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetFundingRates(oSymbol));
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
        /// Get funding rate history on single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom)
        {
            int nPage = 1;
            int nPageSize = 100;
            DateTime dLimit = dFrom.Date;
            List<IFundingRate> aResult = new List<IFundingRate>();

            bool bLimit = false;
            while (!bLimit) 
            {
                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetHistoricalFundingRateAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, nPageSize, nPage);
                if (oResult == null || oResult.Data == null) break;
                if (!oResult.Success) break;
                if (oResult.Data.Count() <= 0) break;

                foreach( var oData in oResult.Data )
                {
                    if (oData.FundingTime == null) continue;
                    aResult.Add(new BitgetFuturesFundingRate(oSymbol, oData));
                    if( oData.FundingTime.Value.Date <= dLimit )
                    {
                        bLimit = true;
                        break;
                    }
                }
                nPage++;    
            }


            return aResult.ToArray();
        }

        /// <summary>
        /// Funding rate history multiple
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(TASK_COUNT);
            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetFundingRatesHistory(oSymbol, dFrom));
            }

            var aTaskResults = await oTaskManager.GetResults();
            if (aTaskResults == null) return null;
            foreach (var oResult in aTaskResults)
            {
                if (oResult == null || oResult.Length <= 0) continue;
                aResult.AddRange(oResult);
            }
            return aResult.ToArray();
        }

        public async Task<ISymbol[]?> GetRawSymbols()
        {
            return await GetSymbols();
        }

        /// <summary>
        /// Get symbol list
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if( m_aSymbols != null ) return m_aSymbols;
            var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetContractsAsync(BitgetProductTypeV2.UsdtFutures);
            if( oResult == null || oResult.Data == null )  return null;
            if (!oResult.Success) return null;
            if( oResult.Data.Count() <= 0 ) return null;

            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach( var oParsed in oResult.Data )
            {
                aResult.Add(new BitgetSymbol(this, oParsed));
            }
            m_aSymbols = aResult.ToArray();
            return m_aSymbols;
        }
    }
}
