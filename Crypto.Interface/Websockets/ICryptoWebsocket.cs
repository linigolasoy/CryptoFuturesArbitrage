using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Websockets
{
    public interface ICryptoWebsocket
    {

        public IExchange Exchange { get; }  

        public Task<bool> Start();

        public Task Stop();

        public Task<bool> SubscribeToMarket(ISymbol[] aSymbols);

        public IWebsocketManager<ITicker> TickerManager { get; }    
    }
}
