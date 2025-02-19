﻿using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexFundingRateManager : IWebsocketManager<IFundingRate>
    {

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();


        private ConcurrentDictionary<string, IFundingRate> m_aFundingRates = new ConcurrentDictionary<string, IFundingRate>();
        public int ReceiveCount { get; private set; } = 0;
        public int Count { get=> m_aFundingRates.Count; }

        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        private IFuturesSymbolManager m_oSymbolManager;
        public CoinexFundingRateManager(IFuturesWebsocketPublic oWebsocket)
        {
            m_oSymbolManager = oWebsocket.Exchange.SymbolManager;
        }
        /// <summary>
        /// Get all data
        /// </summary>
        /// <returns></returns>
        public IFundingRate[] GetData()
        {
            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach (string strKey in m_aFundingRates.Keys)
            {
                IFundingRate? oFound = GetData(strKey);
                if (oFound == null) continue;
                aResult.Add(oFound);
            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Get data single
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <returns></returns>
        public IFundingRate? GetData(string strSymbol)
        {
            IFundingRate? oFound = null;
            if (m_aFundingRates.TryGetValue(strSymbol, out oFound)) return oFound;
            return null;
        }


        /// <summary>
        /// Put data
        /// </summary>
        /// <param name="aData"></param>
        public void Put( IEnumerable<CoinExFuturesTickerUpdate> aData )
        {
            ReceiveCount++;
            foreach( var oData in aData )
            {
                if( oData.LastFundingTime  == null )
                {
                    continue;
                }
                IFuturesSymbol? oSymbol = m_oSymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue;
                IFundingRate oRate = new CoinexFundingRate(oSymbol, oData);

                m_aFundingRates.AddOrUpdate(oSymbol.Symbol, p => oRate, (s, p) => oRate);   

            }
            LastUpdate = DateTime.Now;
        }
    }
}
