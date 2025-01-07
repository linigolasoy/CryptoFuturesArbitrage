using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Tests.Bingx
{
    [TestClass]
    public class BingxWebsocketTests
    {
        [TestMethod]
        public async Task BingxFundingWebsocketTest()
        {
            ICryptoFuturesExchange oExchange = BingxCommon.CreateExchange();
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
            ICryptoFuturesExchange oExchange = BingxCommon.CreateExchange();
            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);

            ICryptoWebsocket? oWebsockets = await oExchange.CreateWebsocket();
            Assert.IsNotNull(oWebsockets);

            bool bStarted = await oWebsockets.Start();
            Assert.IsTrue(bStarted);


            int nSymbols = 30;
            bool bSubscribed = await oWebsockets.SubscribeToMarket(aSymbols.Take(nSymbols).ToArray());

            Assert.IsTrue(bSubscribed);


            await Task.Delay(5000);

            IOrderbookManager oManager = oWebsockets.OrderbookManager;
            IOrderbook[] aBook = oManager.GetData();
            Assert.IsNotNull(aBook);
            Assert.IsTrue(aBook.Length == nSymbols);
            DateTime dMax = aBook.Select(p => p.UpdateDate).Max();
            double nDiff = (DateTime.Now - dMax).TotalSeconds;

            Assert.IsTrue(nDiff <= 1.5);

            await Task.Delay(5000);


            await oWebsockets.Stop();

        }

    }
}
