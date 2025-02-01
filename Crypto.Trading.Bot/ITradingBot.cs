using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Trading.Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot
{

    /// <summary>
    /// Create trading bot
    /// </summary>
    public interface ITradingBot
    {
        public ICryptoSetup Setup { get; }  

        public ISocketManager SocketManager { get; }

        // public IBotStrategy? Strategy { get; }

        public ICommonLogger Logger { get; }    
        public Task<bool> Start();


        public Task Stop(); 
    }
}
