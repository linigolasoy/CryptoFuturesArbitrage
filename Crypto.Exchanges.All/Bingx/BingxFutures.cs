using BingX.Net.Clients;
using BingX.Net.Objects.Models;
using Crypto.Common;
using Crypto.Exchanges.All.Bingx.Websocket;
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

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxFutures : ICryptoFuturesExchange
    {

        public const int TASK_COUNT = 20;

        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;

        private IFuturesSymbol[]? m_aSymbols = null;

        private IFuturesBarFeeder m_oBarFeeder;

        public BingxFutures( ICryptoSetup oSetup ) 
        {
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;

            BingXRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new ApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret);
            });
            m_oGlobalClient = new ExchangeRestClient();
            m_oBarFeeder = new BingxBarFeeder(this);

        }
        public IFuturesBarFeeder BarFeeder { get => m_oBarFeeder; }

        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BingxFutures; }

        public IExchangeRestClient GlobalClient { get => m_oGlobalClient; } 

        /// <summary>
        /// Set leverage
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bBuy"></param>
        /// <param name="nMargin"></param>
        /// <param name="nLeverage"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nMargin, int nLeverage, decimal nPrice)
        {
            // m_oGlobalClient.BingX.PerpetualFuturesApi.Account
            m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.PlaceOrderAsync()
            throw new NotImplementedException();
        }

        public async Task<ICryptoWebsocket?> CreateWebsocket()
        {
            IFuturesSymbol[]? aSymbols = await GetSymbols();
            if (aSymbols == null) return null;
            return new BingxWebsocket(this, aSymbols);
        }


        /// <summary>
        /// Get account balances
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesBalance[]?> GetBalances()
        {
            var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Account.GetBalancesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;

            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach( BingXFuturesBalance oData in oResult.Data )
            {
                aResult.Add( new BingxBalance(oData) );
            }

            return aResult.ToArray();   
        }

        /// <summary>
        /// Funding rates single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            IFundingRateSnapShot[]? aResults =  await GetFundingRates(new IFuturesSymbol[] { oSymbol });

            if (aResults == null || aResults.Length <= 0 ) return null;
            return aResults.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
        }

        /// <summary>
        /// Get funding rates actual
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetFundingRatesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;

            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();
            foreach( BingXFundingRate oData in oResult.Data )
            {
                if (oData == null) continue;
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p=> p.Symbol == oData.Symbol); 
                if (oFound == null) continue;
                aResult.Add(new BingxFundingRateSnapshot(oFound, oData));

            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Gets funding rates
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol)
        {
            DateTime dFromActual = DateTime.Today.AddYears(-2);
            DateTime dToActual = DateTime.Now;

            int nLimit = 1000;

            List<IFundingRate> aResult = new List<IFundingRate>();
            while(true)
            {
                var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetFundingRateHistoryAsync(oSymbol.Symbol, dFromActual, dToActual, nLimit);
                if (oResult == null || !oResult.Success ) break;
                if( oResult.Data == null ) break;

                List<IFundingRate> aPartial = new List<IFundingRate>();

                foreach( BingXFundingRateHistory oData in oResult.Data )
                {
                    aPartial.Add( new BingxFundingRate(oSymbol, oData) );
                }

                if (aPartial.Count <= 0) break;
                DateTime dMinimum = aPartial.Select(p => p.DateTime).Min();
                dToActual = dMinimum.AddHours(-1);
                aResult.AddRange(aPartial);
                if (dMinimum.Date <= dFromActual.Date) break;
                if( aPartial.Count < nLimit ) break;    

            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Get funding rate history, multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols)
        {

            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(TASK_COUNT);
            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetFundingRatesHistory(oSymbol));
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


        /// <summary>
        /// Get Symbols raw
        /// </summary>
        /// <returns></returns>
        public async Task<ISymbol[]?> GetRawSymbols()
        {
            return await GetSymbols();    
        }

        /// <summary>
        /// Get futures symbols
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if( m_aSymbols != null ) return m_aSymbols;
            var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetContractsAsync();
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null ) return null; 
            if( oResult.Data.Count() <= 0  ) return null;

            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach( BingXContract oData in oResult.Data )
            {
                aResult.Add( new BingxSymbol( oData ) );    
            }

            m_aSymbols = aResult.ToArray(); 
            return m_aSymbols;
        }
    }
}
