using BingX.Net.Clients;
using CoinEx.Net.Clients;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Common;
using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.CoinEx.Websocket;
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

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexFutures : ICryptoFuturesExchange
    {

        private const int TASK_COUNT = 20;
        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;

        private IFuturesSymbol[]? m_aSymbols = null;
        private ApiCredentials m_oApiCredentials;

        public CoinexFutures( ICryptoSetup oSetup ) 
        {
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;

            m_oApiCredentials = new ApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret);

            CoinExRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = m_oApiCredentials;
            });
            m_oGlobalClient = new ExchangeRestClient();
            // m_oBarFeeder = new BingxBarFeeder(this);
        }
        public IFuturesBarFeeder BarFeeder => throw new NotImplementedException();

        internal ApiCredentials ApiCredentials { get => m_oApiCredentials; }
        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.CoinExFutures; }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            throw new NotImplementedException();
        }
        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nQuantity, decimal nPrice)
        {
            throw new NotImplementedException();
        }
        public async Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nQuantity, decimal nPrice)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ICryptoWebsocket?> CreateWebsocket()
        {
            IFuturesSymbol[]? aSymbols = await GetSymbols();    
            if( aSymbols == null ) return null; 
            return new CoinexWebsocket(this, aSymbols); 
        }

        /// <summary>
        /// Get balances
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesBalance[]?> GetBalances()
        {
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Account.GetBalancesAsync();

            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null ) return null;
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach( var oData in oResult.Data )
            {
                aResult.Add( new CoinexBalance( oData ) );  
            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Get funding rates of specific symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            IFundingRateSnapShot[]? aResult = await GetFundingRates(new IFuturesSymbol[] { oSymbol });
            if (aResult == null) return null;
            if( aResult.Length <= 0 ) return null;
            return aResult[0];
        }

        /// <summary>
        /// Get all funding rates
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {

            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetFundingRatesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.Count() <= 0 ) return null;
            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();
            foreach( CoinExFundingRate oData in oResult.Data )
            {
                if( oData.LastFundingTime == null ) continue;
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p=> p.Symbol == oData.Symbol);
                if (oFound == null) continue;
                aResult.Add( new CoinexFundingRateSnapshot(oFound, oData) );    
            }
            return aResult.ToArray();
        }

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom)
        {

            DateTime dFromActual = dFrom.Date;
            DateTime dToActual = DateTime.Now;

            int nLimit = 1000;
            int nPage = 1;
            List<IFundingRate> aResult = new List<IFundingRate>();  
            while(true)
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetFundingRateHistoryAsync(oSymbol.Symbol, dFromActual, dToActual, nPage, nLimit);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null) break;

                if( oResult.Data.Items != null && oResult.Data.Items.Count() > 0 )
                {
                    foreach( CoinExFundingRateHistory oData in oResult.Data.Items ) 
                    {
                        if (oData.FundingTime == null) continue;
                        aResult.Add(new CoinexFundingRate(oSymbol, oData));
                    }
                }

                if (!oResult.Data.HasNext) break;
                nPage++;

            }
            return aResult.ToArray();   
        }

        /// <summary>
        /// Multi symbol history
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
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get symbol list
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if( m_aSymbols != null ) return m_aSymbols;
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetSymbolsAsync();
            if (oResult == null || !oResult.Success) return null;

            if( oResult.Data == null ) return null;
            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach( CoinExFuturesSymbol oParsed in oResult.Data ) 
            { 
                aResult.Add(new CoinexSymbol(this, oParsed)); 
            }
            m_aSymbols = aResult.ToArray();
            return m_aSymbols;

        }
    }
}
