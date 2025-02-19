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
        private const string USDT = "USDT";
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
        private void CreateFundingDates(CurrencySymbols[] aCurrencies,  Dictionary<ExchangeType, IFundingRate[]> aFundingRates)
        {
            // Select start date

            Logger.Info("  Creating funding dates...");

            DateTime dStart = From;

            Logger.Info("      Minimum date");
            foreach ( var oData in aFundingRates )
            {
                DateTime dMin = oData.Value.Select(p=> p.SettleDate).Min();
                if( dMin > dStart ) dStart = dMin;  
            }

            Dictionary<DateTime, Dictionary<string,List<IFundingRate>>> aDictDates = new Dictionary<DateTime, Dictionary<string, List<IFundingRate>>>();
            Logger.Info("      Split dates");

            foreach ( var oData in aFundingRates )
            {
                IFundingRate[] aRates = oData.Value.Where(p=> p.SettleDate >= dStart && p.Symbol.Quote == USDT).ToArray();    

                foreach( var oRate  in aRates )
                {
                    DateTime dDate = new DateTime(oRate.SettleDate.Year, oRate.SettleDate.Month, oRate.SettleDate.Day, oRate.SettleDate.Hour, oRate.SettleDate.Minute, 0, DateTimeKind.Local);

                    if (!aCurrencies.Any(p => p.Currency == oRate.Symbol.Base)) continue;
                    if (!aDictDates.ContainsKey(dDate)) aDictDates.Add(dDate, new Dictionary<string, List<IFundingRate>>());
                    Dictionary<string, List<IFundingRate>> oDict = aDictDates[dDate];   
                    if( !oDict.ContainsKey(oRate.Symbol.Base) ) oDict.Add(oRate.Symbol.Base, new List<IFundingRate>());
                    oDict[oRate.Symbol.Base].Add(oRate);
                }
            }


            Logger.Info("      Funding data");
            List<FundingRateDate> aResult = new List<FundingRateDate>();
            foreach( var oDate in aDictDates)
            {

                Dictionary<string, List<IFundingRate>> oDict = oDate.Value;
                aResult.Add( new FundingRateDate(oDate.Key, oDate.Value) ); 
            }

            m_aFundingDates = aResult.OrderBy(p=> p.DateTime).ToArray();    
            Logger.Info("  Created.");



            return;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<CurrencySymbols[]?> CreateCurrencySymbols()
        {
            Dictionary<string, List<IFuturesTicker>> aDictTickers = new Dictionary<string, List<IFuturesTicker>>();

            // Dictionary<ExchangeType, IFuturesTicker[]> aDictTickers = new Dictionary<ExchangeType, IFuturesTicker[]>();
            Logger.Info("  Loading tickers...");
            foreach ( var oExchange in m_aExchanges )
            {
                Logger.Info($"     {oExchange.ExchangeType.ToString()}");
                IFuturesTicker[]? aTickers = await oExchange.Market.GetTickers();
                if (aTickers == null) continue;
                foreach( var oTicker in aTickers )
                {
                    if( oTicker.Symbol.Quote != USDT ) continue;
                    if (!aDictTickers.ContainsKey(oTicker.Symbol.Base)) aDictTickers.Add(oTicker.Symbol.Base, new List<IFuturesTicker>());// { oTicker } );
                    aDictTickers[oTicker.Symbol.Base].Add(oTicker); 
                }

            }
            Logger.Info("  Loaded.");

            List<CurrencySymbols> aResult = new List<CurrencySymbols>();
            foreach( var oDictElement in aDictTickers )
            {
                if( oDictElement.Value.Count < 2 ) continue;    
                IFuturesTicker oMinTicker = oDictElement.Value.OrderBy(p=> p.Price).First();
                IFuturesTicker oMaxTicker = oDictElement.Value.OrderBy(p => p.Price).Last();
                if (oMinTicker.Price <= 0) continue;
                decimal nPercentDiff = (oMaxTicker.Price - oMinTicker.Price) * 100.0M / oMinTicker.Price;    
                if( nPercentDiff >= 5.0M ) continue;    
                aResult.Add( new CurrencySymbols(oDictElement.Key, oDictElement.Value.Select(p=> p.Symbol).ToArray()));  

            }

            return aResult.ToArray() ;
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

            CurrencySymbols[]? aCurrencies = await CreateCurrencySymbols();
            if ( aCurrencies == null ) return false;    

            Dictionary<ExchangeType, IFundingRate[]>? aFundingRates = await LoadFundingHistory();
            if (aFundingRates == null) return false;

            CreateFundingDates(aCurrencies, aFundingRates);
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
