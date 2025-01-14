using BingX.Net.Clients;
using CoinEx.Net.Clients;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Common;
using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.CoinEx.Websocket;
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

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexFutures : IFuturesExchange
    {

        public const int TASK_COUNT = 20;
        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;

        private IFuturesSymbol[]? m_aSymbols = null;
        private ApiCredentials m_oApiCredentials;

        public CoinexFutures( ICryptoSetup oSetup ) 
        {
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;

            m_oApiCredentials = new ApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret);

            CoinExRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = m_oApiCredentials;
            });
            m_oGlobalClient = new ExchangeRestClient();
            // m_oBarFeeder = new BingxBarFeeder(this);
            Trading = new CoinexTrading(this, m_oGlobalClient);
            Account = new CoinexAccount(this, m_oGlobalClient);
            Market = new CoinexMarket(this);
            History = new CoinexHistory(this);  
        }

        public IFuturesHistory History { get; }
        public IFuturesMarket Market { get; }
        public IFuturesTrading Trading { get; }
        public IFuturesAccount Account { get; }
        internal ApiCredentials ApiCredentials { get => m_oApiCredentials; }
        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.CoinExFutures; }


        /// <summary>
        /// Creates websocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesWebsocketPublic?> CreateWebsocket()
        {
            IFuturesSymbol[]? aSymbols = await Market.GetSymbols();    
            if( aSymbols == null ) return null; 
            return new CoinexWebsocket(this, aSymbols); 
        }



    }
}
