using Crypto.Exchanges.All;
using Crypto.Interface.Futures;
using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{
    internal class FundingTestData : IFundingTestData
    {
        private Dictionary<string, IFuturesSymbol[]>? m_aPairs = null;
        private Dictionary<DateTime, IFundingDate> m_aDates = new Dictionary<DateTime, IFundingDate>();

        private string CURRENCY_QUOTE = "USDT";

        public FundingTestData(ICryptoSetup oSetup, ICommonLogger oLogger, DateTime dFrom, DateTime dTo)
        {
            Logger = oLogger;
            From = dFrom;
            To = dTo;
            List<ICryptoFuturesExchange> aExchanges = new List<ICryptoFuturesExchange>();
            foreach (var eType in oSetup.ExchangeTypes)
            {
                switch (eType)
                {
                    case ExchangeType.CoinExFutures:
                    case ExchangeType.BingxFutures:
                    case ExchangeType.BitgetFutures:
                        aExchanges.Add(ExchangeFactory.CreateExchange(eType, oSetup));
                        break;
                    default:
                        break;
                }
            }
            Exchanges = aExchanges.ToArray();

        }

        public ICryptoFuturesExchange[] Exchanges { get; }

        public ICommonLogger Logger { get; }

        public DateTime From { get; }

        public DateTime To { get; }

        /// <summary>
        /// Get next date
        /// </summary>
        /// <param name="dActual"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IFundingDate? GetNext(DateTime? dActual)
        {
            if (dActual == null)
            {
                DateTime? dFirst = m_aDates.Keys.OrderBy(p => p).FirstOrDefault();
                if (dFirst == null) return null;
                return m_aDates[dFirst.Value];
            }
            DateTime? dKey = m_aDates.Keys.OrderBy(p => p).FirstOrDefault(p => p > dActual);
            if (dKey == null) return null;
            if (!m_aDates.ContainsKey(dKey.Value)) return null;
            IFundingDate oResult = m_aDates[dKey.Value];
            return oResult;
        }


        /// <summary>
        /// Get 
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <param name="aTasks"></param>
        private void CreateDates(IFuturesSymbol[] aSymbols, List<Task<IFundingRate[]?>> aTasks)
        {
            if (aTasks.Any(p => p.Result == null || p.Result.Length <= 0)) return;

            // Date dictionary
            SortedDictionary<DateTime, List<IFundingRate>> aDates = new SortedDictionary<DateTime, List<IFundingRate>>();
            foreach (var oTask in aTasks)
            {
                if (oTask.Result == null) continue;
                IFundingRate[] aFunding = oTask.Result;
                foreach (var oFunding in aFunding)
                {
                    DateTime dFunding = oFunding.SettleDate;
                    DateTime dDate = new DateTime(dFunding.Year, dFunding.Month, dFunding.Day, dFunding.Hour, dFunding.Minute, 0);
                    if (dDate < From || dDate > To) continue;
                    if (!aDates.ContainsKey(dDate)) aDates[dDate] = new List<IFundingRate>();
                    aDates[dDate].Add(oFunding);
                }
            }

            // Dictionary loop to create pairs
            foreach (var oDate in aDates)
            {
                FundingDate? oFound = null;
                if (m_aDates.ContainsKey(oDate.Key))
                {
                    oFound = (FundingDate)m_aDates[oDate.Key];
                }
                else
                {
                    oFound = new FundingDate(oDate.Key);
                    m_aDates[oDate.Key] = oFound;
                }

                // Put pairs
                oFound.Put(aSymbols, oDate.Value.ToArray());
            }
            return;
        }

        /// <summary>
        /// Get funding rates
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoadRates()
        {
            if (m_aPairs == null) return false;
            int nTotal = m_aPairs.Keys.Count;
            int nActual = 0;
            int nPercent = 0;
            // Create Dictionary to match
            m_aDates.Clear();

            Logger.Info("Funding history load started...");
            foreach (var oPair in m_aPairs)
            {
                List<Task<IFundingRate[]?>> aTasks = new List<Task<IFundingRate[]?>>();
                foreach (var oSymbol in oPair.Value)
                {
                    aTasks.Add(oSymbol.Exchange.GetFundingRatesHistory(oSymbol, From));
                }

                await Task.WhenAll(aTasks);
                nActual++;
                int nPercentActual = (nActual * 100) / nTotal;
                if (nPercentActual - nPercent >= 5)
                {
                    nPercent = nPercentActual;
                    Logger.Info($"   {nPercent} %");
                }
                CreateDates(oPair.Value, aTasks);

            }
            Logger.Info("Funding history load ended...");
            return true;
        }


        /// <summary>
        /// Load and match symbols 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoadSymbols()
        {

            Dictionary<string, List<IFuturesSymbol>> aPairs = new Dictionary<string, List<IFuturesSymbol>>();
            Logger.Info("Symbol load started...");
            foreach (var oExchange in Exchanges)
            {
                Logger.Info($"   {oExchange.ExchangeType.ToString()}...");
                IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
                if (aSymbols == null) continue;
                foreach (var oSymbol in aSymbols)
                {
                    if (oSymbol.Quote != CURRENCY_QUOTE) continue;
                    if (!aPairs.ContainsKey(oSymbol.Base))
                    {
                        aPairs.Add(oSymbol.Base, new List<IFuturesSymbol>());
                    }
                    aPairs[oSymbol.Base].Add(oSymbol);
                }
            }

            // Add only matches
            m_aPairs = new Dictionary<string, IFuturesSymbol[]>();
            foreach (string strKey in aPairs.Keys)
            {
                List<IFuturesSymbol> aSymbols = aPairs[strKey];
                if (aSymbols.Count < 2) continue;
                m_aPairs.Add(strKey, aSymbols.ToArray());
            }
            Logger.Info("Symbol load ended...");
            await Task.Delay(1000);
            return (m_aPairs.Keys.Count > 0);
        }
    }
}
