﻿using BitMart.Net.Clients;
using BitMart.Net.Objects;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartFutures : IFuturesExchange
    {

        private IApiKey m_oApiKey;
        private IExchangeRestClient m_oGlobalClient;
        public const int TASK_COUNT = 20;

        private BitMartApiCredentials m_oCredentials;
        public BitmartFutures( ICryptoSetup oSetup ) 
        {
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;


            m_oCredentials = new BitMartApiCredentials(m_oApiKey.ApiKey, m_oApiKey.ApiSecret, "IpfSecond");
            BitMartRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = m_oCredentials;
            });

            m_oGlobalClient = new ExchangeRestClient();
            Market = new BitmartMarket(this);
            History = new BitmartHistory(this); 
            Account = new BitmartAccount(this); 
            Trading = new BitmartTrading(this); 
        }

        internal BitMartApiCredentials Credentials { get => m_oCredentials; }
        internal IApiKey ApiKey { get => m_oApiKey; }   
        internal IExchangeRestClient GlobalClient { get => m_oGlobalClient; }
        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitmartFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History { get; }

        public IFuturesTrading Trading { get; }

        public IFuturesAccount Account { get; }
    }
}
