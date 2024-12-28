using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{


    public enum ExchangeType
    {
        // MexcSpot,
        // MexcFutures,
        //BingxSpot,
        CoinExFutures,
        BingxFutures
    }
    /// <summary>
    /// Exchange base, independent of futures or spot
    /// </summary>
    public interface IExchange
    {
        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get; }   

        public Task<ICryptoWebsocket?> CreateWebsocket();

        public Task<ISymbol[]?> GetRawSymbols();

    }
}
