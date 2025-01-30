using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;

namespace Crypto.Tests.Coinex
{
    [TestClass]
    public class CoinexBasicTests
    {
        [TestMethod]
        public async Task CoinexFundingRatesTests()
        {
            IFuturesExchange oExchange = await CoinexCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = await oExchange.Market.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            // IFuturesSymbol[] aFirst = aSymbols.Take(60).ToArray();

            IFuturesSymbol? oToFind = aSymbols.FirstOrDefault(p => p.Base == "XRP");
            Assert.IsNotNull(oToFind);


            DateTime dFrom = DateTime.Today.AddMonths(-2);
            IFundingRate[]? aHistorySingle = await oExchange.History.GetFundingRatesHistory(oToFind, dFrom);
            Assert.IsNotNull(aHistorySingle);
            Assert.IsTrue(aHistorySingle.Length > 10);

            IFundingRate[]? aHistoryMulti = await oExchange.History.GetFundingRatesHistory(aSymbols.ToArray(), dFrom);
            Assert.IsNotNull(aHistoryMulti);
            Assert.IsTrue(aHistoryMulti.Length > 100);

            IFundingRate[] aFound = aHistoryMulti.Where(p => p.Rate * 100M > 0.7M).OrderByDescending(p => p.SettleDate).ToArray();
            Assert.IsTrue(aFound.Any());

            IFundingRateSnapShot? oRateFound = await oExchange.Market.GetFundingRates(oToFind);
            Assert.IsNotNull(oRateFound);


            IFundingRateSnapShot[]? aRates = await oExchange.Market.GetFundingRates(aSymbols);
            Assert.IsNotNull(aRates);
            Assert.IsTrue(aRates.Length >= 10);

            IFundingRateSnapShot[] aOrdered = aRates.OrderByDescending(p => p.Rate).ToArray();
            Assert.IsTrue(aOrdered.Any());



        }

        [TestMethod]
        public async Task CoinexBarsTests()
        {
            IFuturesExchange oExchange = await CoinexCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = await oExchange.Market.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "BTC");
            Assert.IsNotNull(oSymbol);

            IFuturesBar[]? aBars = await oExchange.History.GetBars(oSymbol, Timeframe.H1, DateTime.Today.AddDays(-120), DateTime.Today);
            Assert.IsNotNull(aBars);
            Assert.IsTrue(aBars.Length > 24);
            Assert.IsTrue(aBars[aBars.Length - 1].DateTime.Date == DateTime.Today);

            IFuturesBar[]? aBarsMulti = await oExchange.History.GetBars(aSymbols.Take(30).ToArray(), Timeframe.H1, DateTime.Today.AddDays(-2), DateTime.Today);
            Assert.IsNotNull(aBarsMulti);
            Assert.IsTrue(aBarsMulti.Length > 100);

        }

    }
}
