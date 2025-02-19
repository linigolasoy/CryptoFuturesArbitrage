using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Trading.Bot.BackTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.BackTest
{
    internal class FundingRatesTester : IBackTester
    {

        public FundingRatesTester(ICryptoSetup oSetup, ICommonLogger oLogger, DateTime dFrom, DateTime dTo) 
        {
            Setup = oSetup;
            Logger = oLogger;   
            From = dFrom;
            To = dTo;
        }
        public ICryptoSetup Setup { get; }

        public ICommonLogger Logger { get; }
        public DateTime From { get; }
        public DateTime To { get; }

        public bool Ended { get; private set; } = true;

        private IFuturesExchange[] m_aExchanges = Array.Empty<IFuturesExchange>();  

        private FundingRateDate[] m_aFundingDates = Array.Empty<FundingRateDate>(); 
        /// <summary>
        /// Create exchanges
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CreateExchanges()
        {
            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            Logger.Info("  Creating exchanges...");
            foreach ( var eType in Setup.ExchangeTypes )
            {
                Logger.Info($"     {eType.ToString()}");
                IFuturesExchange? oExchange = await ExchangeFactory.CreateExchange(eType, Setup);
                if (oExchange == null) continue;
                aExchanges.Add(oExchange);
            }
            m_aExchanges = aExchanges.ToArray();
            Logger.Info("  Created exchanges");
            return true;
        }

        /// <summary>
        /// Load funding rates
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<ExchangeType, IFundingRate[]>?> LoadFundingHistory()
        {
            Dictionary<ExchangeType, IFundingRate[]> aResult = new Dictionary<ExchangeType, IFundingRate[]>();
            Logger.Info("  Loading funding rate history...");
            foreach ( var oExchange in m_aExchanges )
            {
                Logger.Info($"     {oExchange.ExchangeType.ToString()}");
                IFundingRate[]? aRates = await oExchange.History.GetFundingRatesHistory(oExchange.SymbolManager.GetAllValues(), From);
                if( aRates != null )
                {
                    aResult.Add(oExchange.ExchangeType, aRates);    
                }
            }
            Logger.Info("  Loaded.");
            return aResult;
        }


        /// <summary>
        /// Create funding dates
        /// </summary>
        private void CreateFundingDates(Dictionary<ExchangeType, IFundingRate[]> aFundingRates)
        {
            // Select start date
            DateTime dStart = From;

            foreach( var oData in aFundingRates )
            {
                DateTime dMin = oData.Value.Select(p=> p.SettleDate).Min();
                if( dMin > dStart ) dStart = dMin;  
            }




            return;

        }

        /// <summary>
        /// Creates exchanges
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            Logger.Info("FundingRatesTester Starting...");
            bool bOk = await CreateExchanges();
            if (!bOk) return false;

            Dictionary<ExchangeType, IFundingRate[]>? aFundingRates = await LoadFundingHistory();
            if (aFundingRates == null) return false;

            CreateFundingDates(aFundingRates);
            Logger.Info("FundingRatesTester Started.");

            await Task.Delay(1000);
            Ended = false;
            return true;
        }

        public async Task<IBackTestResult?> Step()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Stop()
        {
            throw new NotImplementedException();
        }
    }
}
