using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;

namespace Crypto.Tests.Bingx
{
    [TestClass]
    public class BingxTradingTests
    {
        /*
        [TestMethod]
        public async Task BingxOrdersTest()
        {

            IFuturesExchange oExchange = await BingxCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = await oExchange.Market.GetSymbols();
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

            IFuturesBalance[]? aBalances = oExchange.Account.BalanceManager.GetData();
            Assert.IsNotNull(aBalances);
            Assert.IsTrue(aBalances.Length > 0);

            bool bLeverage = await oExchange.Trading.SetLeverage(oSymbol, 10);
            Assert.IsTrue(bLeverage);
            await Task.Delay(5000);

            await oWs.Stop();
        }

        */

        [TestMethod]
        public async Task BingxLeverageTests()
        {
            IFuturesExchange oExchange = await BingxCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = await oExchange.Market.GetSymbols();
            Assert.IsNotNull(aSymbols);

            IFuturesSymbol? oBtc = aSymbols.FirstOrDefault(p => p.Base == "BTC" && p.Quote == "USDT");
            Assert.IsNotNull(oBtc);

            IFuturesOrder[]? aOrders = await oExchange.Trading.GetOrders();
            Assert.IsNotNull(aOrders);


            IFuturesLeverage? oLeverageBtc = await oExchange.Trading.GetLeverage(oBtc);
            Assert.IsNotNull(oLeverageBtc); 

            IFuturesLeverage[]? aLeverages = await oExchange.Trading.GetLeverages(aSymbols.Take(30).ToArray());
            Assert.IsNotNull(aLeverages);




        }

        [TestMethod]
        public async Task BingxBasicOrderTests()
        {
            IFuturesExchange oExchange = await BingxCommon.CreateExchange();

            await Task.Delay(1000);
            IFuturesSymbol[]? aSymbols = await oExchange.Market.GetSymbols();
            Assert.IsNotNull(aSymbols);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
            Assert.IsNotNull(oSymbol);

            
            bool bResult = await oExchange.Trading.SetLeverage(oSymbol, 5);
            Assert.IsTrue(bResult);

            IFuturesLeverage? oNewLeverage = await oExchange.Trading.GetLeverage(oSymbol);
            Assert.IsNotNull(oNewLeverage);
            Assert.IsTrue( oNewLeverage.LongLeverage == 5);
            Assert.IsTrue( oNewLeverage.ShortLeverage == 5);
            decimal nPrice = 0.5M;
            IFuturesOrder? oOrder = await oExchange.Trading.CreateLimitOrder(oSymbol, true, 10, nPrice);    
            Assert.IsNotNull(oOrder);
            await Task.Delay(3000);
            IFuturesOrder[] aOrdersWs = oExchange.Account.OrderManager.GetData();
            Assert.IsNotNull(aOrdersWs);
            IFuturesOrder? oOrderWs = aOrdersWs.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
            Assert.IsNotNull(oOrderWs);
            
            await Task.Delay(1000);
            IFuturesOrder[]? aOrders = await oExchange.Trading.GetOrders();
            Assert.IsNotNull(aOrders);


            Assert.IsTrue(aOrders.Any());

            IFuturesOrder? oFound = aOrders.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
            Assert.IsNotNull(oFound);

            bool bCanceled = await oExchange.Trading.CancelOrder(oFound);
            Assert.IsTrue(bCanceled);

            Assert.IsTrue(oOrderWs.OrderStatus == FuturesOrderStatus.Canceled);

            decimal nQuantity = 5;
            IFuturesOrder? oMarketOpen = await oExchange.Trading.CreateMarketOrder(oSymbol, true, nQuantity);
            Assert.IsNotNull(oMarketOpen);
            await Task.Delay(1000);
            IFuturesPosition[] aRest = oExchange.Account.PositionManager.GetData();
            Assert.IsNotNull(aRest);
            IFuturesPosition? oPosition = aRest.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
            Assert.IsNotNull(oPosition);
            Assert.IsTrue(oPosition.Quantity == nQuantity); 

            IFuturesOrder? oMarketOpen2 = await oExchange.Trading.CreateMarketOrder(oSymbol, true, nQuantity);
            Assert.IsNotNull(oMarketOpen2);
            await Task.Delay(1000);
            Assert.IsTrue(oPosition.Quantity == nQuantity * 2M);


            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();
            Assert.IsNotNull(aPositions);
            Assert.IsTrue(aPositions.Any());

            bool bClose = await oExchange.Trading.ClosePosition(oPosition); 
            Assert.IsTrue(bClose);
            await Task.Delay(2000);
            aRest = oExchange.Account.PositionManager.GetData();
            Assert.IsNotNull(aRest);
            Assert.IsTrue(!aRest.Any(p=> p.Symbol.Symbol == oSymbol.Symbol ));
        }

        [TestMethod]
        public async Task BingxAccountTests()
        {
            IFuturesExchange oExchange = await BingxCommon.CreateExchange();

            IFuturesBalance[]? aBalances = await oExchange.Account.GetBalances();
            Assert.IsNotNull(aBalances);

            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();    
            Assert.IsNotNull(aPositions);

        }
    }
}
