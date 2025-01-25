using Crypto.Common;
using Crypto.Exchanges.All.Bitmart.Websocket;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartMarket : IFuturesMarket
    {
        private IExchangeRestClient m_oGlobalClient;

        private BitmartFutures m_oExchange;
        private IFuturesSymbol[]? m_aSymbols = null;
        private BitmartWebsocketPublic? m_oWebsocket = null;
        public BitmartMarket(BitmartFutures oExchange) 
        { 
            m_oExchange = oExchange;
            m_oGlobalClient = new ExchangeRestClient();
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IFuturesWebsocketPublic? Websocket { get => m_oWebsocket; }


        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.ExchangeData.GetCurrentFundingRateAsync(oSymbol.Symbol);
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null ) return null; 
            if( oResult.Data.NextFundingTime == null ) return null; 
            return new BitmartFundingRateSnapshot(oSymbol, oResult.Data);
        }

        /// <summary>
        /// Get all funding rates
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            ITaskManager<IFundingRateSnapShot?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRateSnapShot?>(BitmartFutures.TASK_COUNT);
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
        /// Get symbols
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if ( m_aSymbols != null ) return m_aSymbols;
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.ExchangeData.GetContractsAsync();
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null || oResult.Data.Count() <= 0 ) return null;    
            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();  
            foreach( var oData  in oResult.Data )
            {
                aResult.Add( new BitmartSymbol(Exchange, oData));   
            }
            m_aSymbols = aResult.ToArray();
            return m_aSymbols;
        }

        /// <summary>
        /// Starts websockets
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> StartSockets()
        {
            await EndSockets();
            if (m_aSymbols == null)
            {
                m_aSymbols = await Exchange.Market.GetSymbols();
                if (m_aSymbols == null) return false;

            }
            m_oWebsocket = new BitmartWebsocketPublic(m_oExchange, m_aSymbols);

            bool bResult = await m_oWebsocket.Start();
            if (!bResult) return false;
            bResult = await m_oWebsocket.SubscribeToMarket(m_aSymbols);
            return bResult;
        }

        /// <summary>
        /// Ends sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EndSockets()
        {
            if (m_oWebsocket == null) return true;

            await m_oWebsocket.Stop();
            await Task.Delay(2000);
            m_oWebsocket = null;
            return true;
        }

    }
}
