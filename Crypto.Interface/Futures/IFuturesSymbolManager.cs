using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{
    public interface IFuturesSymbolManager
    {
        public IFuturesSymbol? GetSymbol(string strSymbol);
        public string[] GetAllKeys();
        public IFuturesSymbol[] GetAllValues();
    }
}
