using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal interface IOrderbookParser
    {

        public IOrderbook? Parse(string strMessage, IFuturesSymbolManager oSymbolManager);

        public string PingMessage { get; }  
    }
}
