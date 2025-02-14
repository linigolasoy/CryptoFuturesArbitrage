using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using System.Collections.Concurrent;

namespace Crypto.Tests.Bingx
{
    [TestClass]
    public class BingxTradingTests
    {

        private ConcurrentDictionary<WebsocketQueueType, int> m_aReceived = new ConcurrentDictionary<WebsocketQueueType, int>();

        [TestMethod]
        public async Task BingxLeverageTests()
        {
            IFuturesExchange oExchange = await PoloniexCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
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
            IFuturesExchange oExchange = await PoloniexCommon.CreateExchange();

            await Task.Delay(1000);
            oExchange.Account.OnPrivateEvent += Account_OnPrivateEvent;
            IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
            Assert.IsNotNull(aSymbols);

            // Start sockets
            bool bSockets = await oExchange.Account.StartSockets();
            Assert.IsTrue(bSockets);
            await Task.Delay(1000);
            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
            Assert.IsNotNull(oSymbol);

            
            ITradingResult<bool> oResult = await oExchange.Trading.SetLeverage(oSymbol, 5);
            Assert.IsTrue(oResult.Success);

            IFuturesLeverage? oNewLeverage = await oExchange.Trading.GetLeverage(oSymbol);
            Assert.IsNotNull(oNewLeverage);
            Assert.IsTrue( oNewLeverage.LongLeverage == 5);
            Assert.IsTrue( oNewLeverage.ShortLeverage == 5);
            decimal nPrice = 0.8M;
            ITradingResult <IFuturesOrder ?>oOrder = await oExchange.Trading.CreateLimitOrder(oSymbol, true, 10, nPrice);    
            Assert.IsTrue(oOrder.Success);
            Assert.IsNotNull(oOrder.Result);
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

            ITradingResult<bool> oCanceled = await oExchange.Trading.CancelOrder(oFound);
            Assert.IsTrue(oCanceled.Success);
            Assert.IsTrue(oCanceled.Result);
            await Task.Delay(1000);
            Assert.IsTrue(oOrderWs.OrderStatus == FuturesOrderStatus.Canceled);

            decimal nQuantity = 5;
            ITradingResult<IFuturesOrder?> oMarketOpen = await oExchange.Trading.CreateMarketOrder(oSymbol, true, nQuantity);
            Assert.IsTrue(oMarketOpen.Success); 
            Assert.IsNotNull(oMarketOpen.Result);
            await Task.Delay(1000);
            IFuturesPosition[] aRest = oExchange.Account.PositionManager.GetData();
            Assert.IsNotNull(aRest);
            IFuturesPosition? oPosition = aRest.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
            Assert.IsNotNull(oPosition);
            Assert.IsTrue(oPosition.Quantity == nQuantity); 

            ITradingResult<IFuturesOrder?> oMarketOpen2 = await oExchange.Trading.CreateMarketOrder(oSymbol, true, nQuantity);
            Assert.IsTrue(oMarketOpen2.Success);
            Assert.IsNotNull(oMarketOpen2.Result);
            await Task.Delay(1000);
            Assert.IsTrue(oPosition.Quantity == nQuantity * 2M);


            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();
            Assert.IsNotNull(aPositions);
            Assert.IsTrue(aPositions.Any());

            ITradingResult<bool> oClose = await oExchange.Trading.ClosePosition(oPosition); 
            Assert.IsTrue(oClose.Success);
            Assert.IsTrue(oClose.Result);
            await Task.Delay(5000);
            Assert.IsTrue(oPosition.Closed);    

            aRest = oExchange.Account.PositionManager.GetData();
            Assert.IsNotNull(aRest);
            Assert.IsTrue(!aRest.Any(p=> p.Symbol.Symbol == oSymbol.Symbol ));
        }

        private async Task Account_OnPrivateEvent(IWebsocketQueueItem oItem)
        {
            m_aReceived.AddOrUpdate(oItem.QueueType, p => 1, (t, p) => ++p);
        }

        [TestMethod]
        public async Task BingxAccountTests()
        {
            IFuturesExchange oExchange = await PoloniexCommon.CreateExchange();

            IFuturesBalance[]? aBalances = await oExchange.Account.GetBalances();
            Assert.IsNotNull(aBalances);

            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();    
            Assert.IsNotNull(aPositions);

        }
    }
}
