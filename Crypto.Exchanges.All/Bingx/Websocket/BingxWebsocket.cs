using BingX.Net.Clients;
using BingX.Net.Objects.Models;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxWebsocket : ICryptoWebsocket
    {

        private class MarketSockets
        {
            public MarketSockets(BingXSocketClient oClient)
            {
                SocketClient = oClient;
            }
            public BingXSocketClient SocketClient { get; }

            public List<IFuturesSymbol> Symbols { get; } = new List<IFuturesSymbol>();
        }

        private List<MarketSockets> m_aMarketSockets = new List<MarketSockets>();
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private string? m_strListenKey = null;
        private IFuturesSymbol[] m_aSymbols;

        private BingxOrderbookManager m_oOrderbookManager;
        private BingxFundingRateManager m_oFundingManager;
        public BingxWebsocket(BingxFutures oExchange, IFuturesSymbol[] aSymbols)
        {
            m_oExchange = oExchange;
            m_aSymbols = aSymbols;
            m_oOrderbookManager = new BingxOrderbookManager(this);
            m_oFundingManager = new BingxFundingRateManager(oExchange, aSymbols);
        }
        private BingxFutures m_oExchange;
        public IExchange Exchange { get => m_oExchange; }
        public IFuturesSymbol[] FuturesSymbols { get => m_aSymbols; }

        public IWebsocketManager<IFuturesOrder> FuturesOrderManager => throw new NotImplementedException();

        public IOrderbookManager OrderbookManager { get => m_oOrderbookManager; }



        public IWebsocketManager<IFundingRate> FundingRateManager { get => m_oFundingManager; }




        /// <summary>
        /// Starts websockets
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();

            await m_oFundingManager.Start();


            return true;
        }



        /// <summary>
        /// Starts socket client
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Stop()
        {
            m_oCancelSource.Cancel();
            await m_oFundingManager.Stop();
            await Task.Delay(500);

            foreach (var oMarket in m_aMarketSockets)
            {
                await oMarket.SocketClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                oMarket.SocketClient.Dispose();
            }
            m_aMarketSockets.Clear();


        }

        /// <summary>
        /// Subscribe to market
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<bool> SubscribeToMarket(ISymbol[] aSymbols)
        {
            int nTotal = 0;
            int nMax = 200;

            while (nTotal < aSymbols.Length)
            {
                ISymbol[] aPartial = aSymbols.Skip(nTotal).Take(nMax).ToArray();
                nTotal += aPartial.Length;
                BingXSocketClient oClient = new BingXSocketClient();
                MarketSockets oMarketSocket = new MarketSockets(oClient);
                foreach (var oSymbol in aPartial)
                {
                    var oResult = await oClient.PerpetualFuturesApi.SubscribeToPartialOrderBookUpdatesAsync(oSymbol.Symbol, 10, 100, OnOrderbookUpdate);
                    if (oResult == null || !oResult.Success) return false;
                    oMarketSocket.Symbols.Add((IFuturesSymbol)oSymbol);
                }

                m_aMarketSockets.Add(oMarketSocket);
            }
            return true;
        }

        /// <summary>
        /// Update loop
        /// </summary>
        /// <param name="oUpdate"></param>
        private void OnOrderbookUpdate(DataEvent<BingXOrderBook> oUpdate)
        {
            if (oUpdate.Symbol == null) return;
            if (oUpdate.Data == null) return;
            BingXOrderBook oData = oUpdate.Data;
            m_oOrderbookManager.Put(oUpdate.Symbol, oUpdate.Timestamp.ToLocalTime(), oData);
        }


        public async Task<bool> SubscribeToFundingRates(IFuturesSymbol[] aSymbols)
        {
            await Task.Delay(500);
            return true;

        }

    }
}
