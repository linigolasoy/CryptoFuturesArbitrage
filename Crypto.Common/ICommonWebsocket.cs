using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common
{

    /// <summary>
    /// Statistics
    /// </summary>
    public interface ICommonWebsocketStatistics
    {
        public int PingCount { get; }   
        public int ReceivedCount { get; }   
        public int SentCount { get; }
    }


    /// <summary>
    /// Basic websocket interface
    /// </summary>
    public interface ICommonWebsocket
    {
        public string Url { get; }
        public int    PingSeconds { get; }  
        public Task<bool> Start();
        public Task Stop();

        public delegate void ConnectDelegate();
        public delegate void DisconnectDelegate();
        public delegate void ReceivedDelegate( string strMessage );
        public delegate void ExceptionDelegate(Exception ex);
        public delegate string PingDelegate();  

        public event ConnectDelegate? OnConnect;
        public event DisconnectDelegate? OnDisConnect;
        public event ReceivedDelegate? OnReceived;
        public event ExceptionDelegate? OnException;
        public event PingDelegate? OnPing;

        public ICommonWebsocketStatistics Statistics { get; }
        public Task<bool> Send( string strMessage );    
    }
}
