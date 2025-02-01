using BingX.Net.Clients;
using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Enums.V2;
using Bitget.Net.Objects;
using Crypto.Common;
using Crypto.Exchanges.All.Bitget.Websocket;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetFutures : IFuturesExchange
    {
        private const string USDT = "USDT";
        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;

        public const int TASK_COUNT = 20;

        private BitgetApiCredentials m_oApiCredentials;

        public BitgetFutures( ICryptoSetup oSetup ) 
        {
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;

            m_oApiCredentials = new BitgetApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret, "Cotton1234");
            BitgetRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = m_oApiCredentials;
            });
            m_oGlobalClient = new ExchangeRestClient();

            Task<IFuturesSymbol[]?> oTask = GetSymbols();
            oTask.Wait();
            if (oTask.Result == null) throw new Exception("No symbols");
            SymbolManager = new FuturesSymbolManager(oTask.Result);
            Trading = new BitgetTrading(this, m_oApiCredentials);
            Account = new BitgetAccount(this, m_oGlobalClient);
            Market = new BitgetMarket(this);
            History = new BitgetHistory(this);  
        }

        public IFuturesSymbolManager SymbolManager { get; }
        public IFuturesMarket Market { get; }
        public IFuturesHistory History { get; }
        public IFuturesTrading Trading { get; }
        public IFuturesAccount Account { get; }
        public BitgetApiCredentials ApiCredentials { get => m_oApiCredentials; }

        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitgetFutures; }


        /// <summary>
        /// Creates a new websocket
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesWebsocketPublic?> CreateWebsocket()
        {
            return new BitgetWebsocket(this); 
        }


        /// <summary>
        /// Get symbol list
        /// </summary>
        /// <returns></returns>
        private async Task<IFuturesSymbol[]?> GetSymbols()
        {
            var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetContractsAsync(BitgetProductTypeV2.UsdtFutures);
            if (oResult == null || oResult.Data == null) return null;
            if (!oResult.Success) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach (var oParsed in oResult.Data)
            {
                aResult.Add(new BitgetSymbol(this, oParsed));
            }
            return aResult.ToArray();
        }


    }
}
