using Crypto.Common;
using Crypto.Exchange.Mexc;
using Crypto.Interface;

namespace Crypto.Tests
{
    [TestClass]
    public class MexcTests
    {
        [TestMethod]
        public async Task MexcSpotMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup();  

            ICryptoSpotExchange oSpot = new MexcSpotExchange(oSetup);

            ISymbol[]? aSymbols = await oSpot.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            ISymbol? oEth = aSymbols.FirstOrDefault(p => p.Base == "ETH" && p.Quote == "USDT");
            Assert.IsNotNull(oEth); 


        }

        [TestMethod]
        public async Task MexcFuturesMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup();

            ICryptoFuturesExchange oFutures = new MexcFuturesExchange(oSetup);

            IFuturesSymbol[]? aSymbols = await oFutures.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            // IFuturesSymbol[] aFirst = aSymbols.Take(60).ToArray();

            IFuturesSymbol? oToFind = aSymbols.FirstOrDefault(p => p.Base == "AKRO");
            Assert.IsNotNull(oToFind);


            IFundingRate[]? aHistorySingle = await oFutures.GetFundingRatesHistory(oToFind);
            Assert.IsNotNull(aHistorySingle);
            Assert.IsTrue(aHistorySingle.Length > 10);

            IFundingRate[]? aHistoryMulti = await oFutures.GetFundingRatesHistory(aSymbols.Take(30).ToArray());
            Assert.IsNotNull(aHistoryMulti);
            Assert.IsTrue(aHistoryMulti.Length > 100);


            IFundingRateSnapShot? oRateFound = await oFutures.GetFundingRates(oToFind);

            IFundingRateSnapShot[]? aRates = await oFutures.GetFundingRates(aSymbols.Take(60).ToArray());    
            Assert.IsNotNull(aRates);
            Assert.IsTrue(aRates.Length >= 10);

            IFundingRateSnapShot[] aOrdered = aRates.OrderByDescending(p => p.Rate).ToArray();
            Assert.IsTrue(aOrdered.Any());



        }

        [TestMethod]
        public async Task MexcFuturesBarsTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup();

            ICryptoFuturesExchange oFutures = new MexcFuturesExchange(oSetup);

            IFuturesSymbol[]? aSymbols = await oFutures.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "BTC");
            Assert.IsNotNull(oSymbol);

            IFuturesBar[]? aBars = await oFutures.GetBars(oSymbol, Timeframe.H1, DateTime.Today.AddDays(-120), DateTime.Today);
            Assert.IsNotNull(aBars);
            Assert.IsTrue(aBars.Length > 24);
            Assert.IsTrue(aBars[aBars.Length-1].DateTime.Date == DateTime.Today);   

            IFuturesBar[]? aBarsMulti = await oFutures.GetBars(aSymbols.Take(30).ToArray(), Timeframe.H1, DateTime.Today.AddDays(-2), DateTime.Today);
            Assert.IsNotNull(aBarsMulti);
            Assert.IsTrue(aBarsMulti.Length > 100);

        }





    }
}