using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Tests.Bingx
{
    [TestClass]
    public class BingxBasicTests
    {


        [TestMethod]
        public async Task BingxBarsTests()
        {
            IFuturesExchange oExchange = await PoloniexCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            DateTime dFrom = DateTime.Today.AddMonths(-2);
            DateTime dTo = DateTime.Today.AddDays(-1);
            IFuturesBar[]? aBars = await oExchange.History.GetBars(aSymbols.Take(10).ToArray(), Timeframe.M15, dFrom, dTo);

            Assert.IsNotNull(aBars);
            Assert.IsTrue(aBars.Length > 30000);
        }

        [TestMethod]
        public async Task BingxFundingRatesTests()
        {
            IFuturesExchange oExchange = await PoloniexCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
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


    }
}
