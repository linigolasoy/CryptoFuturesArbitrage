using Bitget.Net.Enums;
using Crypto.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetMarket : IFuturesMarket
    {

        private IFuturesSymbol[]? m_aSymbols = null;
        private IExchangeRestClient m_oGlobalClient;

        private BitgetFutures m_oExchange;

        public BitgetMarket(BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oGlobalClient = new ExchangeRestClient();
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public IFuturesWebsocketPublic? Websocket { get => throw new NotImplementedException(); }
        public async Task<bool> StartSockets()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> EndSockets()
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
            ITaskManager<IFundingRateSnapShot?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRateSnapShot?>(BitgetFutures.TASK_COUNT);
            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetFundingRates(oSymbol));
            }

            var aTaskResults = await oTaskManager.GetResults();
            if (aTaskResults == null) return null;
            foreach (var oResult in aTaskResults)
            {
                if (oResult == null) continue;
                aResult.Add(oResult);
            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Get symbol list
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if (m_aSymbols != null) return m_aSymbols;
            var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetContractsAsync(BitgetProductTypeV2.UsdtFutures);
            if (oResult == null || oResult.Data == null) return null;
            if (!oResult.Success) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach (var oParsed in oResult.Data)
            {
                aResult.Add(new BitgetSymbol(this.Exchange, oParsed));
            }
            m_aSymbols = aResult.ToArray();
            return m_aSymbols;
        }
    }
}
