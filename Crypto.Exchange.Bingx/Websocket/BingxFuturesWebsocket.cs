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

        private class WebsocketData
        {
            public WebsocketData( ICommonWebsocket oWebsocket )
            {
                Websocket = oWebsocket; 
            }

            public ICommonWebsocket Websocket { get; }

            public ISymbol[]? Subscribed { get; set; } = null;
        }

        // Constants
        private const string URL_PUBLIC = "wss://open-api-swap.bingx.com/swap-market";

        private const int MAX_SUBSCRIBED = 200;

        private ICryptoFuturesExchange m_oExchange;
        private IFuturesSymbol[]? m_aSymbols = null;

        private List<WebsocketData>? m_aMarketWs = null;
        // private ICommonWebsocket? m_oMarketWs = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private ConcurrentQueue<JToken> m_oReceiveQueue = new ConcurrentQueue<JToken>();
        private Task? m_oReceiveTask = null;

        private int m_nId = 1000;

        // private List<ISymbol> m_aSubscribed = new List<ISymbol>();
        private TickerManager m_oTickerManager;

        public BingxFuturesWebsocket(ICryptoFuturesExchange oExchange)
        {
            m_oExchange = oExchange;
            m_oTickerManager = new TickerManager(m_oExchange);
        }

        public IExchange Exchange { get => m_oExchange; }

        public IWebsocketManager<ITicker> TickerManager { get => m_oTickerManager; }

        /// <summary>
        /// Generates new websocket
        /// </summary>
        /// <returns></returns>
        private async Task<WebsocketData> CreateNewMarketWebsocket()
        {
            ICommonWebsocket oWs = CommonFactory.CreateWebsocket(URL_PUBLIC, 0);
            oWs.OnConnect += MarketOnConnect;
            oWs.OnDisConnect += MarketOnDisConnect;
            oWs.OnReceived += MarketOnReceived;
            await oWs.Start();
            return new WebsocketData(oWs);
        }
        /// <summary>
        /// Start websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();
            m_oCancelSource = new CancellationTokenSource();
            WebsocketData oData = await CreateNewMarketWebsocket();

            m_aMarketWs = new List<WebsocketData>() { oData };
            m_oReceiveTask = MainReceiveLoop();

            return true;
        }

        /// <summary>
        /// Stop websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Stop()
        {
            if (m_aMarketWs != null)
            {
                foreach( var oMarketWs in m_aMarketWs)
                {
                    await oMarketWs.Websocket.Stop();
                }
                await Task.Delay(1000);
                m_aMarketWs = null;
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
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task SubscribeSingleWs( WebsocketData oData, ISymbol[] aSymbols )
        {

            foreach (ISymbol oSymbol in aSymbols)
            {
                SubscribeToTicker oSubs = new SubscribeToTicker(GetId(), oSymbol.Symbol, false);
                JObject oObject = JObject.FromObject(oSubs);
                await oData.Websocket.Send(oObject.ToString());
            }
            oData.Subscribed = aSymbols;

        }

        /// <summary>
        /// Subscribe to market
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<bool> SubscribeToMarket(ISymbol[] aSymbols)
        {
            if (m_aMarketWs == null) return false;

            int nAdded = 0;
            foreach( var oWs in m_aMarketWs )
            {
                if( oWs.Subscribed != null ) throw new NotImplementedException();
                await SubscribeSingleWs(oWs, aSymbols.Skip(nAdded).Take(MAX_SUBSCRIBED).ToArray());
                nAdded += oWs.Subscribed!.Length;
            }

            while( nAdded < aSymbols.Length )
            {
                WebsocketData oNewData = await CreateNewMarketWebsocket();
                m_aMarketWs.Add(oNewData);
                await Task.Delay(1000);
                await SubscribeSingleWs(oNewData, aSymbols.Skip(nAdded).Take(MAX_SUBSCRIBED).ToArray());
                nAdded += oNewData.Subscribed!.Length;
            }
            /*
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
            */
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
