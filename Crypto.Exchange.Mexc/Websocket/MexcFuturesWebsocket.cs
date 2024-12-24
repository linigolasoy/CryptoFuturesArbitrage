using Crypto.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Websocket
{
    internal class MexcFuturesWebsocket : ICryptoWebsocket
    {

        private const string URL_MAIN = "wss://contract.mexc.com/edge";

        private const string PING_MESSAGE = "{\r\n  \"method\": \"ping\"\r\n}";
        private const string ALLTICKERS_SUBSCRIBE = "{\r\n  \"method\": \"sub.tickers\",\r\n  \"param\": {}\r\n}";

        private int m_nPingSeconds = 20;
        private bool m_bSubscribed = false;

        private ICommonWebsocket? m_oMarketWs = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private ConcurrentQueue<JToken> m_oReceiveQueue = new ConcurrentQueue<JToken>();
        private Task? m_oReceiveTask = null;


        private List<ISymbol> m_aSubscribed = new List<ISymbol>();

        private TickerManager m_oTickerManager = new TickerManager();   

        public MexcFuturesWebsocket(IExchange oExchange)
        {
            Exchange = oExchange;
        }



        public IExchange Exchange { get; }

        private IFuturesSymbol[]? m_aSymbols = null;

        public IWebsocketManager<ITicker> TickerManager { get => m_oTickerManager; }

        public IWebsocketManager<IFuturesOrder> FuturesOrderManager => throw new NotImplementedException();

        /// <summary>
        /// Receive loop so enqueuing never ends
        /// </summary>
        /// <returns></returns>
        private async Task MainReceiveLoop()
        {
            while( !m_oCancelSource.IsCancellationRequested )
            {
                JToken? oReceived = null;   
                if( m_oReceiveQueue.TryDequeue( out oReceived ) )
                {
                    if (m_aSymbols == null) m_aSymbols = (IFuturesSymbol[]?)await Exchange.GetRawSymbols();
                    IWebsocketMessage? oMessage = WsMessageFactory.Parse(oReceived);
                    if( oMessage == null )
                    {
                        continue;
                    }
                    switch( oMessage.MessageType )
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
        /// Start market and private websocket
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            await Stop();
            m_oCancelSource = new CancellationTokenSource();
            m_oMarketWs = CommonFactory.CreateWebsocket(URL_MAIN, m_nPingSeconds);
            m_oMarketWs.OnConnect += MarketOnConnect;
            m_oMarketWs.OnPing += MarketOnPing;
            m_oMarketWs.OnDisConnect += MarketOnDisConnect;
            m_oMarketWs.OnReceived += MarketOnReceived;
            m_oReceiveTask = MainReceiveLoop();

            bool bResult = await m_oMarketWs.Start();
            return bResult;
        }

        /// <summary>
        /// Simply parse to Json and add to queue
        /// </summary>
        /// <param name="strMessage"></param>
        private void MarketOnReceived(string strMessage)
        {
            JToken? oParsed = JToken.Parse(strMessage);
            if( oParsed != null ) m_oReceiveQueue.Enqueue(oParsed);
        }


        private void MarketOnDisConnect()
        {
            return;
        }

        private string MarketOnPing()
        {
            return PING_MESSAGE;
        }

        private void MarketOnConnect()
        {
            return;
        }

        /// <summary>
        /// Stop market and private websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Stop()
        {
            if( m_oMarketWs != null )
            {
                await m_oMarketWs.Stop();
                await Task.Delay(1000);
                m_oMarketWs = null;
            }

            if( m_oReceiveTask != null )
            {
                m_oCancelSource.Cancel();
                await m_oReceiveTask;
                m_oReceiveTask = null;
            }
            m_bSubscribed = false;  
            
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

            if( aDelete.Length > 0 )
            {
                // TODO: Unsubscribe
            }
            if( aAdd.Length > 0 )
            {
                // TODO: Subscribe
                int nSubscribed = 0;
                foreach( ISymbol oSymbol in aAdd )
                {
                    SubscribeTicker oSubs = new SubscribeTicker(oSymbol.Symbol, false);
                    JObject oObject = JObject.FromObject(oSubs);
                    await m_oMarketWs.Send(oObject.ToString());
                    nSubscribed++;  
                }

            }
            return true;
        }
    }
}
