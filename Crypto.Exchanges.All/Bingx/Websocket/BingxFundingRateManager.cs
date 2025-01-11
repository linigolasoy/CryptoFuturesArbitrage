﻿using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxFundingRateManager : IWebsocketManager<IFundingRate>
    {

        private Task? m_oMainTask = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();


        private ConcurrentDictionary<string, IFundingRate> m_aFundingRates = new ConcurrentDictionary<string, IFundingRate>();

        private ICryptoFuturesExchange m_oExchange;
        private IFuturesSymbol[] m_aSymbols;
        public int ReceiveCount { get; private set; } = 0;
        public BingxFundingRateManager(ICryptoFuturesExchange oExchamge, IFuturesSymbol[] aSymbols)
        {
            m_oExchange = oExchamge;
            m_aSymbols = aSymbols;
        }


        /// <summary>
        /// Get all data
        /// </summary>
        /// <returns></returns>
        public IFundingRate[] GetData()
        {
            List<IFundingRate> aResult = new List<IFundingRate> (); 
            foreach( string strKey in m_aFundingRates.Keys ) 
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

        public async Task Start()
        {
            await Stop();
            m_oCancelSource = new CancellationTokenSource();
            m_oMainTask = MainRatesLoop();
        }

        public async Task Stop()
        {
            if (m_oMainTask != null)
            {
                m_oCancelSource.Cancel();
                await m_oMainTask;
                m_oMainTask = null;
            }
        }

        private async Task MainRatesLoop()
        {
            while (!m_oCancelSource.IsCancellationRequested)
            {
                IFundingRateSnapShot[]? aSnapshots = await m_oExchange.GetFundingRates(m_aSymbols);
                if (aSnapshots != null)
                {
                    ReceiveCount++;
                    foreach (IFundingRateSnapShot oShot in aSnapshots)
                    {
                        m_aFundingRates.AddOrUpdate(oShot.Symbol.Symbol, p => oShot, (p, s) => oShot);
                    }
                }
                await Task.Delay(2000);
            }
        }

    }
}
