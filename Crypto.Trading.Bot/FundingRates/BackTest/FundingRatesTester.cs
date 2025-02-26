using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
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
        private const bool LOG = false;

        private const decimal MAX_MONEY = 2000;
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
        private decimal m_nMoney = 0;
        public bool Ended { get; private set; } = true;

        private IFuturesExchange[] m_aExchanges = Array.Empty<IFuturesExchange>();  

        private FundingRateDate[] m_aFundingDates = Array.Empty<FundingRateDate>();
        private FundingRateChance? m_oActiveChange = null;
        private FundingRateDate? m_oActiveDate = null;


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
                // if (eType == ExchangeType.CoinExFutures) continue;
                Logger.Info($"     {eType.ToString()}");
                IFuturesExchange? oExchange = await ExchangeFactory.CreateExchange(eType, Setup, Logger);
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
            dStart = dStart.Date.AddDays(1);    

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
                aResult.Add( new FundingRateDate(this.Setup, oDate.Key, oDate.Value, aCurrencies) ); 
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
            m_nMoney = Setup.Amount * 2.0M;
            await Task.Delay(1000);
            Ended = false;
            return true;
        }


        /// <summary>
        /// Find best chance on date next change
        /// </summary>
        /// <param name="oDate"></param>
        /// <returns></returns>
        private FundingRateChance? FindBestOnDate(FundingRateDate oDate)
        {
            FundingRateChance[] aChances = oDate.ToChances(m_nMoney);
            if( aChances == null || aChances.Length <= 0 ) return null; 
            return aChances.OrderByDescending(p=> p.ProfitPercent ).FirstOrDefault();   
        }


        private void LogChance( string strStep, FundingRateChance oChance)
        {
            if( oChance.PositionBuy == null || oChance.PositionSell == null ) return;
            if (!LOG) return;
            StringBuilder oBuild = new StringBuilder();

            oBuild.Append($"   {strStep.PadRight(8)} {oChance.DateClose!.Value.ToShortDateString()} {oChance.DateClose!.Value.ToShortTimeString()} ({oChance.ProfitPercent} %)");
            oBuild.Append( $" Buy {oChance.PositionBuy.Symbol.ToString()} ");
            oBuild.Append( $" Sell {oChance.PositionSell.Symbol.ToString()} ");
            oBuild.Append($" Profit = {oChance.ProfitRealized.ToString()} ({oChance.ProfitUnrealized.ToString()}) ");

            Logger.Info(oBuild.ToString()); 

        }

        /// <summary>
        /// Put chance data
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task<bool> PutChanceData(FundingRateChance oChance)
        {

            if (oChance.PositionBuy == null || oChance.PositionSell == null) return false;
            if (oChance.PositionBuy.FundingRate == null || oChance.PositionSell.FundingRate == null) return false; 
            DateTime dFrom = oChance.FundingData.FundingDate.DateTime.AddHours(-1);
            DateTime dTo = oChance.FundingData.FundingDate.DateTime.Date.AddDays(7);
            Timeframe eFrame = Timeframe.M15;

            IFuturesBar[]? aBarsBuy = await oChance.PositionBuy.Symbol.Exchange.History.GetBars(oChance.PositionBuy.Symbol, eFrame, dFrom, dTo);
            if( aBarsBuy == null || aBarsBuy.Length <= 0 ) return false;
            IFuturesBar[]? aBarsSell = await oChance.PositionSell.Symbol.Exchange.History.GetBars(oChance.PositionSell.Symbol, eFrame, dFrom, dTo);
            if (aBarsSell == null || aBarsSell.Length <= 0) return false;

            if( !oChance.Start(aBarsBuy, aBarsSell) ) return false;
            LogChance("Started", oChance);

            return true;
        }


        /// <summary>
        /// Step to next funding rate
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IBackTestResult?> Step()
        {
            IBackTestResult? oResult = null;
            while (true)
            {
                if( m_oActiveChange == null )
                {
                    // TODO: Find new chance
                    FundingRateDate? oSelected = m_aFundingDates[0];    
                    if( m_oActiveDate != null )
                    {
                        oSelected = m_aFundingDates.Where(p=> p.DateTime > m_oActiveDate.DateTime).FirstOrDefault();
                        if (oSelected == null) break;
                    }
                    m_oActiveDate = oSelected;  
                    FundingRateChance? oChance = FindBestOnDate(oSelected);
                    if( oChance != null )
                    {
                        bool bOk = await PutChanceData(oChance);
                        if( bOk )
                        {
                            m_oActiveChange = oChance;
                        }
                    }
                }
                else
                {
                    // TODO: Eval on next date
                    m_oActiveChange.Step();
                    LogChance((m_oActiveChange.Status == FundingRateChance.ChanceStatus.Open ? "Step":"Close"), m_oActiveChange);
                    if( m_oActiveChange.Status == FundingRateChance.ChanceStatus.Closed )
                    {
                        Logger.Info($"     {m_oActiveChange.FundingData.Currency} {m_oActiveChange.ProfitRealized}");
                        oResult = new BaseBackTestResult(this, m_oActiveChange.DateOpen!.Value, m_oActiveChange.DateClose!.Value, m_oActiveChange.ProfitRealized);
                        m_nMoney += oResult.Profit;
                        if( m_nMoney > MAX_MONEY )
                        {
                            m_nMoney = MAX_MONEY;   
                        }
                        m_oActiveChange = null; 
                        break;
                    }
                }
            }

            await Task.Delay(1000);
            return oResult;
        }

        public async Task<bool> Stop()
        {
            await Task.Delay(2000); return true;    
        }
    }
}
