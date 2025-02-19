using CoinEx.Net.Enums;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Common;
using Crypto.Exchanges.All.Common;
using Crypto.Exchanges.All.Common.Storage;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexHistory : IFuturesHistory
    {
        private CoinexFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;
        private IFuturesBarFeeder m_oBarFeeder;
        private IFundingRateFeeder m_oFundingFeeder;

        public CoinexHistory(CoinexFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oGlobalClient = new ExchangeRestClient();
            m_oBarFeeder = new BaseBarFeeder(m_oExchange);
            m_oBarFeeder.OnGetBarsDay += OnGetBarsDay;
            m_oFundingFeeder = new BaseFundingFeeder(m_oExchange);
            m_oFundingFeeder.OnGetFunding += OnGetFunding;
        }

        /// <summary>
        /// Get funding rates from web
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="dFrom"></param>
        /// <returns></returns>
        private async Task<IFundingRate[]?> OnGetFunding(IFuturesSymbol oSymbol, DateTime dFrom)
        {
            DateTime dFromActual = dFrom.Date;
            DateTime dToActual = DateTime.Now;

            int nLimit = 1000;
            int nPage = 1;
            List<IFundingRate> aResult = new List<IFundingRate>();
            while (true)
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetFundingRateHistoryAsync(oSymbol.Symbol, dFromActual, dToActual, nPage, nLimit);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null) break;

                if (oResult.Data.Items != null && oResult.Data.Items.Count() > 0)
                {
                    foreach (CoinExFundingRateHistory oData in oResult.Data.Items)
                    {
                        if (oData.FundingTime == null) continue;
                        aResult.Add(new CoinexFundingRate(oSymbol, oData));
                    }
                }

                if (!oResult.Data.HasNext) break;
                nPage++;

            }
            return aResult.ToArray();
        }

        private KlineInterval? TimeframeToCoinex( Timeframe eFrame )
        {
            switch (eFrame)
            {
                case Timeframe.M1:
                    return KlineInterval.OneMinute;
                case Timeframe.M5:
                    return KlineInterval.FiveMinutes;
                case Timeframe.M15:
                    return KlineInterval.FifteenMinutes;
                case Timeframe.M30:
                    return KlineInterval.ThirtyMinutes;
                case Timeframe.H1:
                    return KlineInterval.OneHour;
                case Timeframe.H4:
                    return KlineInterval.FourHours;
                case Timeframe.D1:
                    return KlineInterval.OneDay;
                default:
                    return null;
            }

        }
        private async Task<IFuturesBar[]?> OnGetBarsDay(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dDate)
        {
            KlineInterval? eInterval = TimeframeToCoinex(eTimeframe);
            if (eInterval == null) return null;

            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetKlinesAsync(
                    oSymbol.Symbol,
                    eInterval.Value,
                    1000);
            //. .GetKlinesAsync(oSymbol.Symbol, eInterval.Value, dFrom, dToActual, 1000);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.Count() <= 0) return null;

            List<IFuturesBar> aResult = new List<IFuturesBar>();
               
            foreach (var oData in oResult.Data)
            {
                    IFuturesBar oBar = new CoinexBar(oSymbol, eTimeframe, oData);
                    if (!aResult.Any(p => p.DateTime == oBar.DateTime))
                    {
                        aResult.Add(oBar);
                    }
            }

            return aResult.Where(p => p.DateTime.Date == dDate.Date).ToArray();
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            return await GetBars( new IFuturesSymbol[] {oSymbol}, eTimeframe, dFrom, dTo);  
        }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            return await m_oBarFeeder.GetBars(aSymbols, eTimeframe, dFrom, dTo);
        }

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom)
        {
            return await GetFundingRatesHistory( new IFuturesSymbol[] { oSymbol }, dFrom);
        }

        /// <summary>
        /// Multi symbol history
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            
            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                IFundingRate[]? aPartial = await m_oFundingFeeder.GetFundingRatesHistory(oSymbol, dFrom);
                if( aPartial != null) aResult.AddRange(aPartial);   
            }

            return aResult.ToArray();
        }

    }
}
