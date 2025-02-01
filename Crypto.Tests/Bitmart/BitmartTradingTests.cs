using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using System.Collections.Concurrent;

namespace Crypto.Tests.Bitmart
{
    [TestClass]
    public class BitmartTradingTests
    {

        private ConcurrentDictionary<WebsocketQueueType, int> m_aReceived = new ConcurrentDictionary<WebsocketQueueType, int>();

        [TestMethod]
        public async Task BitmartLeverageTests()
        {
            IFuturesExchange oExchange = await BitmartCommon.CreateExchange();

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


            bool bResult = await oExchange.Trading.SetLeverage(oBtc, 5);
            Assert.IsTrue(bResult); 

        }

        [TestMethod]
        public async Task BitmartBasicOrderTests()
        {
            IFuturesExchange oExchange = await BitmartCommon.CreateExchange();

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

            
            bool bResult = await oExchange.Trading.SetLeverage(oSymbol, 5);
            Assert.IsTrue(bResult);

            IFuturesLeverage? oNewLeverage = await oExchange.Trading.GetLeverage(oSymbol);
            Assert.IsNotNull(oNewLeverage);
            Assert.IsTrue( oNewLeverage.LongLeverage == 5);
            Assert.IsTrue( oNewLeverage.ShortLeverage == 5);
            decimal nPrice = 2.5M;
            IFuturesOrder? oOrder = await oExchange.Trading.CreateLimitOrder(oSymbol, true, 10, nPrice);    
            Assert.IsNotNull(oOrder);
            await Task.Delay(2000);
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
            await Task.Delay(1000);
            Assert.IsTrue(oOrderWs.OrderStatus == FuturesOrderStatus.Canceled);

            decimal nQuantity = 5;
            IFuturesOrder? oMarketOpen = await oExchange.Trading.CreateMarketOrder(oSymbol, true, nQuantity);
            Assert.IsNotNull(oMarketOpen);
            await Task.Delay(3000);
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
            Assert.IsTrue(oPosition.Closed);    

            aRest = oExchange.Account.PositionManager.GetData();
            Assert.IsNotNull(aRest);
            Assert.IsTrue(!aRest.Any(p=> p.Symbol.Symbol == oSymbol.Symbol && !p.Closed ));
        }

        private async Task Account_OnPrivateEvent(IWebsocketQueueItem oItem)
        {
            m_aReceived.AddOrUpdate(oItem.QueueType, p => 1, (t, p) => ++p);
        }

        [TestMethod]
        public async Task BitmartAccountTests()
        {
            IFuturesExchange oExchange = await BitmartCommon.CreateExchange();

            IFuturesBalance[]? aBalances = await oExchange.Account.GetBalances();
            Assert.IsNotNull(aBalances);

            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();    
            Assert.IsNotNull(aPositions);

        }
    }
}
