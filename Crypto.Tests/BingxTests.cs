using Crypto.Common;
using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using NuGet.Frameworks;

namespace Crypto.Tests
{
    [TestClass]
    public class BingxTests
    {
        
        [TestMethod]
        public async Task BingxOrdersTest()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);  

            ICryptoFuturesExchange oExchange = ExchangeFactory.CreateExchange( ExchangeType.BingxFutures, oSetup);  

            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "GMT" && p.Quote == "USDT");
            Assert.IsNotNull(oSymbol);

            ICryptoWebsocket? oWs = await oExchange.CreateWebsocket();
            Assert.IsNotNull(oWs);

            await oWs.Start();
            await Task.Delay(1000);
            await oWs.SubscribeToMarket(new IFuturesSymbol[] { oSymbol }); 

            await Task.Delay(20000);

            IOrderbook? oOrderbook = oWs.OrderbookManager.GetData(oSymbol.Symbol);
            Assert.IsNotNull(oOrderbook);

            decimal nMoney = 20;
            IOrderbookPrice? oPrice = oWs.OrderbookManager.GetBestAsk(oSymbol.Symbol, nMoney);  
            Assert.IsNotNull(oPrice);   

            IFuturesBalance[]? aBalances = oWs.BalanceManager.GetData();
            Assert.IsNotNull(aBalances);
            Assert.IsTrue(aBalances.Length > 0);


            IFuturesOrder? oOrder = await oExchange.CreateLimitOrder(oSymbol, true, 20, 1, oPrice.Price);
            Assert.IsNotNull(oOrder);

            await Task.Delay(5000);

            await oWs.Stop();
        }

        [TestMethod]
        public async Task BingxFuturesMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oFutures = ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup);

            IFuturesSymbol[]? aSymbols = await oFutures.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            // IFuturesSymbol[] aFirst = aSymbols.Take(60).ToArray();

            IFuturesSymbol? oToFind = aSymbols.FirstOrDefault(p => p.Base == "XRP");
            Assert.IsNotNull(oToFind);


            DateTime dFrom = DateTime.Today.AddMonths(-2);
            IFundingRate[]? aHistorySingle = await oFutures.GetFundingRatesHistory(oToFind, dFrom);
            Assert.IsNotNull(aHistorySingle);
            Assert.IsTrue(aHistorySingle.Length > 10);

            IFundingRate[]? aHistoryMulti = await oFutures.GetFundingRatesHistory(aSymbols.Take(30).ToArray(), dFrom);
            Assert.IsNotNull(aHistoryMulti);
            Assert.IsTrue(aHistoryMulti.Length > 100);


            IFundingRateSnapShot? oRateFound = await oFutures.GetFundingRates(oToFind);
            Assert.IsNotNull(oRateFound);


            IFundingRateSnapShot[]? aRates = await oFutures.GetFundingRates(aSymbols);    
            Assert.IsNotNull(aRates);
            Assert.IsTrue(aRates.Length >= 10);

            IFundingRateSnapShot[] aOrdered = aRates.OrderByDescending(p => p.Rate).ToArray();
            Assert.IsTrue(aOrdered.Any());



        }


        [TestMethod]
        public async Task BingxFuturesBarsTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oFutures = ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup);

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


        [TestMethod]
        public async Task BingxAccountTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oFutures = ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup);

            IFuturesBalance[]? aBalances = await oFutures.GetBalances();
            Assert.IsNotNull(aBalances);

        }

        [TestMethod]
        public async Task BingxFundingWebsocketTest()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oExchange = ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup);
            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);

            ICryptoWebsocket? oWebsockets = await oExchange.CreateWebsocket();
            Assert.IsNotNull(oWebsockets);

            bool bStarted = await oWebsockets.Start();
            Assert.IsTrue(bStarted);

            await Task.Delay(1000);

            bool bSubscribed = await oWebsockets.SubscribeToFundingRates(aSymbols);
            Assert.IsTrue(bSubscribed);

            await Task.Delay(10000);

            IFundingRate[]? aFundings = oWebsockets.FundingRateManager.GetData();
            Assert.IsNotNull(aFundings);
            Assert.IsTrue(aFundings.Length >= aSymbols.Length);

            await Task.Delay(2000);

            await oWebsockets.Stop();
        }

        [TestMethod]
        public async Task BingxMarketWebsocketTest()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oExchange = ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup);
            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);

            ICryptoWebsocket? oWebsockets = await oExchange.CreateWebsocket();
            Assert.IsNotNull(oWebsockets);

            bool bStarted = await oWebsockets.Start();
            Assert.IsTrue(bStarted);


            int nSymbols = aSymbols.Length;
            bool bSubscribed = await oWebsockets.SubscribeToMarket(aSymbols.Take(nSymbols).ToArray());

            Assert.IsTrue(bSubscribed);


            await Task.Delay(30000);

            IOrderbookManager oManager = oWebsockets.OrderbookManager;
            IOrderbook[] aBook = oManager.GetData();
            Assert.IsNotNull(aBook);
            Assert.IsTrue(aBook.Length == nSymbols);
            DateTime dMax = aBook.Select(p=> p.UpdateDate).Max();  
            double nDiff = (DateTime.Now - dMax).TotalSeconds;

            Assert.IsTrue(nDiff <= 1.5);

            await Task.Delay(5000);


            await oWebsockets.Stop();

        }
        

    }
}