using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc
{
    public class MexcFuturesExchange : ICryptoFuturesExchange
    {
        public MexcFuturesExchange( ICryptoSetup setup)
        {
            Setup = setup;
        }

        public ICryptoSetup Setup { get; }

        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            throw new NotImplementedException();
        }
    }
}
