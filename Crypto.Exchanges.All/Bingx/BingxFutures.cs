using BingX.Net.Clients;
using BingX.Net.Objects.Models;
using Crypto.Common;
using Crypto.Exchanges.All.Bingx.Websocket;
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

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxFutures : IFuturesExchange
    {


        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;
        public const int TASK_COUNT = 20;


        public BingxFutures( ICryptoSetup oSetup ) 
        {
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;

            BingXRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new ApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret);
            });
            m_oGlobalClient = new ExchangeRestClient();
            Trading = new BingxTrading(this, m_oGlobalClient);
            Account = new BingxAccount(this, m_oGlobalClient);
            Market = new BingxMarket(this);
            History = new BingxHistory(this);   
        }

        public IFuturesHistory History { get; }

        public IFuturesMarket Market { get; }
        public IFuturesTrading Trading { get; }
        public IFuturesAccount Account { get; }

        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BingxFutures; }

        public IExchangeRestClient GlobalClient { get => m_oGlobalClient; }




        public async Task<IFuturesWebsocketPublic?> CreateWebsocket()
        {
            IFuturesSymbol[]? aSymbols = await Market.GetSymbols();
            if (aSymbols == null) return null;
            return new BingxWebsocket(this, aSymbols);
        }





    }
}
