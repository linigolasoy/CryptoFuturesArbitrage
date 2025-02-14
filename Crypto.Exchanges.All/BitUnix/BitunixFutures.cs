using CoinEx.Net.Clients;
using Crypto.Exchanges.All.BitUnix.Rest;
using Crypto.Exchanges.All.Common;
using Crypto.Exchanges.All.IpfCryptoClients.Interface;
using Crypto.Exchanges.All.IpfCryptoClients.Rest;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.BitUnix
{
    internal class BitunixFutures : IFuturesExchange
    {

        private ICryptoRestClient m_oRestClient;
        private ICryptoRestParser m_oRestParser;

        public BitunixFutures( ICryptoSetup oSetup ) 
        { 
            Setup = oSetup;
            m_oRestParser = new BitunixRestParser(this);
            m_oRestClient = new CryptoRestClient(BitunixRestParser.URL_BASE, m_oRestParser);
            IFuturesSymbol[]? aSymbols = GetSymbols();
            if (aSymbols == null) throw new Exception("No symbols");
            SymbolManager = new FuturesSymbolManager(aSymbols);

            History = new BitunixHistory(this);
        }
        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitUnixFutures; }

        public IFuturesMarket Market => throw new NotImplementedException();

        public IFuturesHistory History { get; }

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account => throw new NotImplementedException();

        public IFuturesSymbolManager SymbolManager { get; }

        private IFuturesSymbol[] GetSymbols()
        {

            Task<ICryptoRestResult<IFuturesSymbol[]>> oTask =
                m_oRestClient.DoGetArray<IFuturesSymbol>(BitunixRestParser.ENDPOINT_SYMBOLS, p => BitunixSymbol.Parse(this,p)) ;
            oTask.Wait();
            if (oTask.Result == null || !oTask.Result.Success || oTask.Result.Data == null ) throw new Exception("Could not get symbols!!!");
            return oTask.Result.Data;
        }

    }
}
