using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;

namespace Crypto.Tests.Bingx
{
    [TestClass]
    public class BingxWebsocketTests
    {

        private int m_nReceived1 = 0;

        [TestMethod]
        public async Task BingxFundingAccountSocket()
        {
            IFuturesExchange oExchange = await BingxCommon.CreateExchange();

            oExchange.Account.OnPrivateEvent += MyOnPrivateEvent;
            await Task.Delay(1000);
            bool bResultStart = await oExchange.Account.StartSockets();
            Assert.IsTrue(bResultStart);

            await Task.Delay(10000);
            IFuturesBalance[] aBalances = oExchange.Account.BalanceManager.GetData();   
            Assert.IsTrue(aBalances.Length > 0);
            Assert.IsTrue(m_nReceived1 > 0);
        }

        private async Task MyOnPrivateEvent(Interface.Futures.Websockets.IWebsocketQueueItem oItem)
        {
            m_nReceived1++;
        }

        [TestMethod]
        public async Task BingxAllSymbolsSocket()
        {
            IFuturesExchange oExchange = await BingxCommon.CreateExchange();

            await Task.Delay(1000);
            IFuturesSymbol[]? aSymbols = await oExchange.Market.GetSymbols();
            Assert.IsNotNull(aSymbols); 
            bool bResultStart = await oExchange.Market.StartSockets();
            Assert.IsTrue(bResultStart);

            await Task.Delay(4000);

            Assert.IsNotNull(oExchange.Market.Websocket);
            Assert.IsTrue(oExchange.Market.Websocket.OrderbookManager.Count == aSymbols.Length);
            Assert.IsTrue(oExchange.Market.Websocket.FundingRateManager.Count == aSymbols.Length);

            IOrderbook[]? aData = oExchange.Market.Websocket.OrderbookManager.GetData();    
            Assert.IsNotNull(aData);
            Assert.IsTrue(aData.Length == aSymbols.Length);

            IFundingRate[]? aFunding = oExchange.Market.Websocket.FundingRateManager.GetData();
            Assert.IsNotNull(aFunding);
            Assert.IsTrue(aFunding.Length == aSymbols.Length);  

            bool bResultStop = await oExchange.Market.EndSockets(); 
            Assert.IsTrue(bResultStop);
        }
        

        /*
        [TestMethod]
        public async Task BingxMarketWebsocketTest()
        {
            IFuturesExchange oExchange = await BingxCommon.CreateExchange();
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
        */
    }
}
