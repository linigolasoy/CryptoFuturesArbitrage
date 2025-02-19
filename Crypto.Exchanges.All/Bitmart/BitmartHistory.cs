using BitMart.Net.Enums;
using BitMart.Net.Objects.Models;
using Crypto.Common;
using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.Common;
using Crypto.Exchanges.All.Common.Storage;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using CryptoClients.Net.Interfaces;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartHistory : IFuturesHistory
    {
        private BitmartFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;
        private IFuturesBarFeeder m_oBarFeeder;
        private IFundingRateFeeder m_oFundingFeeder;

        public BitmartHistory(BitmartFutures oExchange) 
        {
            m_oExchange = oExchange;
            m_oGlobalClient = oExchange.GlobalClient;
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
            int nLimit = 100;

            List<IFundingRate> aResult = new List<IFundingRate>();
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.ExchangeData.GetFundingRateHistoryAsync(oSymbol.Symbol, nLimit);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;

            foreach (BitMartFundingRateHistory oData in oResult.Data)
            {
                aResult.Add(new BitmartFundingRateLocal(oSymbol, oData));
            }
            if (aResult.Count <= 0) return null;

            return aResult.ToArray();
        }


        private FuturesKlineInterval? TimeframeToBitmart(Timeframe eFrame)
        {
            switch (eFrame)
            {
                case Timeframe.M1:
                    return FuturesKlineInterval.OneMinute;
                case Timeframe.M5:
                    return FuturesKlineInterval.FiveMinutes;
                case Timeframe.M15:
                    return FuturesKlineInterval.FifteenMinutes;
                case Timeframe.M30:
                    return FuturesKlineInterval.ThirtyMinutes;
                case Timeframe.H1:
                    return FuturesKlineInterval.OneHour;
                case Timeframe.H4:
                    return FuturesKlineInterval.FourHours;
                case Timeframe.D1:
                    return FuturesKlineInterval.OneDay;
                default:
                    return null;
            }

        }
        private async Task<IFuturesBar[]?> OnGetBarsDay(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dDate)
        {
            FuturesKlineInterval? eInterval = TimeframeToBitmart(eTimeframe);
            if (eInterval == null) return null;

            DateTime dFrom = dDate.Date.AddHours(-5);
            DateTime dTo = dDate.Date.AddDays(1).AddHours(5);
            List<IFuturesBar> aResult = new List<IFuturesBar>();
            while (dFrom <= dTo)
            {

                var oResult = await m_oExchange.GlobalClient.BitMart.UsdFuturesApi.ExchangeData.GetKlinesAsync(
                    oSymbol.Symbol,
                    eInterval.Value,
                    dFrom,
                    dTo);

                //.BingX.PerpetualFuturesApi.ExchangeData.GetKlinesAsync(oSymbol.Symbol, eInterval.Value, dFrom, dToActual, 1000);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null || oResult.Data.Count() <= 0) break;

                foreach (var oData in oResult.Data)
                {
                    IFuturesBar oBar = new BitmartBar(oSymbol, eTimeframe, oData);
                    if (!aResult.Any(p => p.DateTime == oBar.DateTime))
                    {
                        aResult.Add(oBar);
                    }
                }

                dFrom = aResult.Select(p=> p.DateTime).Max().AddSeconds(1); 
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

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                IFundingRate[]? aPartial = await m_oFundingFeeder.GetFundingRatesHistory(oSymbol, dFrom);
                if (aPartial != null) aResult.AddRange(aPartial);
            }

            return aResult.ToArray();
        }
    }
}
