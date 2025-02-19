﻿using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    /*
    internal class BaseExchangeData : IBotExchangeData
    {

        public BaseExchangeData( IFuturesExchange oExchange ) 
        { 
            Exchange = oExchange;   
        }
        public IFuturesExchange Exchange { get; }

        public ICryptoWebsocket? Websocket { get; internal set; } = null;

        public IBotSymbolData[]? Symbols { get; internal set; } = null;

        public IFuturesBalance[]? Balances { get; internal set; } = null;

        /// <summary>
        /// Update all data from symbols
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            if (Websocket == null) return false;
            if (Symbols == null) return false;

            IFundingRate[]? aFundingRates = Websocket.FundingRateManager.GetData(); 
            if (aFundingRates == null || aFundingRates.Length <= (Symbols.Length / 2)) return false;

            IOrderbook[]? aOrderbooks = Websocket.OrderbookManager.GetData();
            // if (aOrderbooks == null || aOrderbooks.Length <= (Symbols.Length / 2)) return false;
            foreach( var oSymbolRaw in Symbols )
            {
                IFundingRate? oFunding = aFundingRates.FirstOrDefault(p=> p.Symbol.Symbol == oSymbolRaw.Symbol.Symbol );

                IOrderbook? oOrderbook = aOrderbooks.FirstOrDefault(p=> p.Symbol.Symbol == oSymbolRaw.Symbol.Symbol );

                BaseSymbolData oSymbolData = (BaseSymbolData)oSymbolRaw;
                oSymbolData.FundingRate = oFunding;
                oSymbolData.Orderbook = oOrderbook; 
            }
            return true;
        }

        public void Reset()
        {
            Balances = null;
            Websocket = null;
            Symbols = null; 
        }
    }
    */
}
