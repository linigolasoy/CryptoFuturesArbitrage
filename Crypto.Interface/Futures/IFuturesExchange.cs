using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;

namespace Crypto.Interface.Futures
{
    public interface IFuturesExchange 
    {

        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get; }


        public IFuturesMarket Market { get; }   
        public IFuturesHistory History { get; }


        public IFuturesTrading Trading { get; } 
        public IFuturesAccount Account { get; }
        public IFuturesSymbolManager SymbolManager { get; }
    }
}
