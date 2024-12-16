using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface ICryptoFuturesExchange
    {
        public ICryptoSetup Setup { get; }

        public Task<IFuturesSymbol[]?> GetSymbols();
    }
}
