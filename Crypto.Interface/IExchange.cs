using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{

    /// <summary>
    /// Exchange base, independent of futures or spot
    /// </summary>
    public interface IExchange
    {
        public ICryptoSetup Setup { get; }

        public Task<ICryptoWebsocket?> CreateWebsocket();

        public Task<ISymbol[]?> GetRawSymbols();
    }
}
