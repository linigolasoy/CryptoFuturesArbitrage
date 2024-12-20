using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Websockets
{
    public interface IWebsocketManager<T>
    {

        public T[] GetData();

        public T? GetData(string strSymbol);
    }
}
