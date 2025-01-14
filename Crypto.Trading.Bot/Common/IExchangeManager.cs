using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    public interface IExchangeManager
    {

        public ICryptoSetup Setup { get; }

        public IFuturesExchange[] Exchanges { get; }  
    }
}
