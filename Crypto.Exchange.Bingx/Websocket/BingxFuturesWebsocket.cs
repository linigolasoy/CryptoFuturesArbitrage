using Crypto.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Websocket
{
    /// <summary>
    /// Futures websocket
    /// </summary>
    internal class BingxFuturesWebsocket : ICryptoWebsocket
    {

        // Constants
        private const string URL_PUBLIC = "wss://open-api-swap.bingx.com/swap-market";


        private ICryptoFuturesExchange m_oExchange;
        private IFuturesSymbol[]? m_aSymbols = null;

        private ICommonWebsocket? m_oMarketWs = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private ConcurrentQueue<JToken> m_oReceiveQueue = new ConcurrentQueue<JToken>();
        private Task? m_oReceiveTask = null;

        private int m_nId = 1000;

        private List<ISymbol> m_aSubscribed = new List<ISymbol>();
        private TickerManager m_oTickerManager;

        public BingxFuturesWebsocket(ICryptoFuturesExchange oExchange)
        {
            m_oExchange = oExchange;
            m_oTickerManager = new TickerManager(m_oExchange);
        }

        public IExchange Exchange { get => m_oExchange; }

        public IWebsocketManager<ITicker> TickerManager { get => m_oTickerManager; }

        /// <summary>
        /// Start websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();
            m_oCancelSource = new CancellationTokenSource();
            m_oMarketWs = CommonFactory.CreateWebsocket(URL_PUBLIC, 0);
            m_oMarketWs.OnConnect += MarketOnConnect;
            m_oMarketWs.OnDisConnect += MarketOnDisConnect;
            m_oMarketWs.OnReceived += MarketOnReceived;
            m_oReceiveTask = MainReceiveLoop();

            bool bResult = await m_oMarketWs.Start();
            return bResult;
        }

        /// <summary>
        /// Stop websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Stop()
        {
            if (m_oMarketWs != null)
            {
                await m_oMarketWs.Stop();
                await Task.Delay(1000);
                m_oMarketWs = null;
            }

            if (m_oReceiveTask != null)
            {
                m_oCancelSource.Cancel();
                await m_oReceiveTask;
                m_oReceiveTask = null;
            }
        }

        /// <summary>
        /// Receive loop so enqueuing never ends
        /// </summary>
        /// <returns></returns>
        private async Task MainReceiveLoop()
        {
            while (!m_oCancelSource.IsCancellationRequested)
            {
                JToken? oReceived = null;
                if (m_oReceiveQueue.TryDequeue(out oReceived))
                {

                    if (m_aSymbols == null) m_aSymbols = await m_oExchange.GetSymbols();
                    IWebsocketMessage? oMessage = WsMessageFactory.Parse(oReceived, m_aSymbols!);
                    if (oMessage == null)
                    {
                        continue;
                    }
                    switch (oMessage.MessageType)
                    {
                        case WebsocketMessageType.Ticker:
                            m_oTickerManager.Put(oMessage, m_aSymbols!);
                            break;
                        default:
                            break;
                    }
                    continue;
                }
                await Task.Delay(200);
            }
            await Task.Delay(300);
        }

        /// <summary>
        /// Id of request
        /// </summary>
        /// <returns></returns>
        private string GetId()
        {
            return string.Format("FuturesWs{0}", m_nId++);
        }
        /// <summary>
        /// Subscribe to market
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<bool> SubscribeToMarket(ISymbol[] aSymbols)
        {
            if (m_oMarketWs == null) return false;
            ISymbol[] aDelete = m_aSubscribed.Where(p => !aSymbols.Any(q => p.Symbol == q.Symbol)).ToArray();
            ISymbol[] aAdd = aSymbols.Where(p => !m_aSubscribed.Any(q => p.Symbol == q.Symbol)).ToArray();

            if (aDelete.Length > 0)
            {
                // TODO: Unsubscribe
            }
            if (aAdd.Length > 0)
            {
                // TODO: Subscribe
                int nSubscribed = 0;
                foreach (ISymbol oSymbol in aAdd)
                {
                    SubscribeToTicker oSubs = new SubscribeToTicker( GetId(), oSymbol.Symbol, false );
                    JObject oObject = JObject.FromObject(oSubs);
                    await m_oMarketWs.Send(oObject.ToString());
                    nSubscribed++;
                }

            }
            return true;
        }

        /// <summary>
        /// Simply parse to Json and add to queue
        /// </summary>
        /// <param name="strMessage"></param>
        private void MarketOnReceived(string strMessage)
        {
            JToken? oParsed = JToken.Parse(strMessage);
            if (oParsed != null) m_oReceiveQueue.Enqueue(oParsed);
        }
        private void MarketOnDisConnect()
        {
            return;
        }


        private void MarketOnConnect()
        {
            return;
        }

    }
}
