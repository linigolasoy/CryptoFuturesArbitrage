using Crypto.Common;
using Crypto.Exchange.Mexc;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;

namespace Crypto.Tests
{
    [TestClass]
    public class MexcTests
    {

        /// <summary>
        /// Base spot get symbols
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MexcSpotMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoSpotExchange oSpot = new MexcSpotExchange(oSetup);

            ISymbol[]? aSymbols = await oSpot.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            ISymbol? oEth = aSymbols.FirstOrDefault(p => p.Base == "ETH" && p.Quote == "USDT");
            Assert.IsNotNull(oEth);


        }

        /// <summary>
        /// Futures get symbols and futures funding
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MexcFuturesMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

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


        /// <summary>
        /// Futures get H1 bars
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MexcFuturesBarsTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oFutures = new MexcFuturesExchange(oSetup);

            IFuturesSymbol[]? aSymbols = await oFutures.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "BTC");
            Assert.IsNotNull(oSymbol);

            IFuturesBar[]? aBars = await oFutures.BarFeeder.GetBars(oSymbol, Timeframe.H1, DateTime.Today.AddDays(-120), DateTime.Today);
            Assert.IsNotNull(aBars);
            Assert.IsTrue(aBars.Length > 24);
            Assert.IsTrue(aBars[aBars.Length - 1].DateTime.Date == DateTime.Today);

            IFuturesBar[]? aBarsMulti = await oFutures.BarFeeder.GetBars(aSymbols.Take(30).ToArray(), Timeframe.H1, DateTime.Today.AddDays(-2), DateTime.Today);
            Assert.IsNotNull(aBarsMulti);
            Assert.IsTrue(aBarsMulti.Length > 100);

        }


        /*
        [TestMethod]
        public async Task MexcMarketWebsocketTest()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oExchange = new MexcFuturesExchange(oSetup);
            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);

            ICryptoWebsocket? oWebsockets = await oExchange.CreateWebsocket();
            Assert.IsNotNull(oWebsockets);

            bool bStarted = await oWebsockets.Start();
            Assert.IsTrue(bStarted);


            int nSymbols = 30;
            await oWebsockets.SubscribeToMarket(aSymbols.Take(nSymbols).ToArray());


            await Task.Delay(20000);

            IWebsocketManager<IOrderbook> oManager = oWebsockets.OrderbookManager;
            IOrderbook[] aOrderbooks = oManager.GetData();
            Assert.IsNotNull(aOrderbooks); 
            Assert.IsTrue(aOrderbooks.Length == nSymbols);


            await Task.Delay(40000);


            await oWebsockets.Stop();   

        }
        */
    }
}