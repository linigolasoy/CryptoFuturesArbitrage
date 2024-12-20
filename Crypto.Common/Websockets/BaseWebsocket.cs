﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace Crypto.Common.Websockets
{

    /// <summary>
    /// Websockets
    /// </summary>
    internal class BaseWebsocket : ICommonWebsocket
    {

        public BaseWebsocket( string strUrl, int nPingSeconds ) 
        { 
            Url = strUrl;   
            PingSeconds = nPingSeconds;
        }
        public string Url { get; }
        public int PingSeconds { get; } 

        public event ICommonWebsocket.ConnectDelegate? OnConnect;
        public event ICommonWebsocket.DisconnectDelegate? OnDisConnect;
        public event ICommonWebsocket.ReceivedDelegate? OnReceived;
        public event ICommonWebsocket.ExceptionDelegate? OnException;
        public event ICommonWebsocket.PingDelegate? OnPing;

        private CancellationTokenSource m_oTokenSource = new CancellationTokenSource();


        private ConcurrentQueue<string> m_oQueueSend = new ConcurrentQueue<string>();

        private Task? m_oSendTask = null;
        private WatsonWsClient? m_oWsClient = null;
        private UTF8Encoding m_oUtf8Encoder = new UTF8Encoding();

        private BaseStatistics m_oStatistics = new BaseStatistics();

        public ICommonWebsocketStatistics Statistics { get => m_oStatistics; }
        /// <summary>
        /// Main send task
        /// </summary>
        /// <returns></returns>
        private async Task MainSendTask()
        {
            DateTime dLastPing = DateTime.Now;  
            while( !m_oTokenSource.IsCancellationRequested )
            {
                DateTime dNow = DateTime.Now;
                if( (dNow - dLastPing).TotalSeconds > PingSeconds )
                {
                    if( OnPing != null && m_oWsClient != null && m_oWsClient.Connected )
                    {
                        string strMessage = OnPing();
                        await m_oWsClient.SendAsync( strMessage );
                        m_oStatistics.PingCount++;
                        m_oStatistics.SentCount++;  
                    }
                    dLastPing = dNow;
                    continue;
                }

                string? oDequeued = null;
                if( m_oQueueSend.TryDequeue( out oDequeued ) && m_oWsClient != null && m_oWsClient.Connected )
                {
                    await m_oWsClient.SendAsync(oDequeued);
                    m_oStatistics.SentCount++;
                }
                await Task.Delay( 100 );
            }
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="strMessage"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Send(string strMessage)
        {
            m_oQueueSend.Enqueue( strMessage ); 
            await Task.Delay( 100 );
            return true;
        }

        /// <summary>
        /// Starts a new websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();

            m_oTokenSource = new CancellationTokenSource();
            m_oSendTask = MainSendTask();
            m_oWsClient = new WatsonWsClient(new Uri(Url));
            m_oWsClient.ServerConnected += ClientOnServerConnected;
            m_oWsClient.ServerDisconnected += ClientOnServerDisconnected;
            m_oWsClient.MessageReceived += ClientOnMessageReceived;
            m_oWsClient.Start();
            await Task.Delay(1000);
            return true;    
        }

        private void ClientOnMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            m_oStatistics.ReceivedCount++;
            if (e.MessageType != System.Net.WebSockets.WebSocketMessageType.Text) return;
            string strText = m_oUtf8Encoder.GetString(e.Data.ToArray());
            if( OnReceived != null )
            {
                try
                {
                    OnReceived(strText);    
                }
                catch (Exception ex) { }
            }
        }

        private void ClientOnServerDisconnected(object? sender, EventArgs e)
        {
            if( OnDisConnect != null ) { OnDisConnect(); }
        }

        private void ClientOnServerConnected(object? sender, EventArgs e)
        {
            if( OnConnect != null ) { OnConnect(); }    
        }

        public async Task Stop()
        {
            if (m_oWsClient == null) return;

            if( m_oSendTask != null )
            {
                m_oTokenSource.Cancel();
                await Task.Delay(1000);
                await m_oSendTask;
                m_oSendTask = null; 
            }

            if( m_oWsClient.Connected )
            {
                m_oWsClient.Stop(); 
                await Task.Delay(1000);
            }
            m_oWsClient = null;
        }
    }
}
