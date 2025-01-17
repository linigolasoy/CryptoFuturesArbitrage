using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Websockets
{
    public interface IWebsocketPrivateManager<T> : IWebsocketManager<T>
    {
        public IFuturesWebsocketPrivate PrivateSocket { get; }

        public T AddFunction(T oNew);
        public T UpdateFunction(T oOld, T oNew);
    }
}
