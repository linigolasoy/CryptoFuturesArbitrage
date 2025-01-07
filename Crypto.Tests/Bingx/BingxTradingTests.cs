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
    public class BingxTradingTests
    {
        [TestMethod]
        public async Task BingxOrdersTest()
        {

            ICryptoFuturesExchange oExchange = BingxCommon.CreateExchange();

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

            bool bLeverage = await oExchange.Trading.SetLeverage(oSymbol, 10);
            Assert.IsTrue(bLeverage);
            IFuturesOrder? oOrder = await oExchange.Trading.CreateLimitOrder(oSymbol, true, true, 5, oPrice.Price);
            Assert.IsNotNull(oOrder);

            await Task.Delay(5000);

            await oWs.Stop();
        }



        [TestMethod]
        public async Task BingxLeverageTests()
        {
            ICryptoFuturesExchange oExchange = BingxCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
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
            ICryptoFuturesExchange oExchange = BingxCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
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
            IFuturesOrder? oOrder = await oExchange.Trading.CreateLimitOrder(oSymbol, true,true, 10, nPrice);    
            Assert.IsNotNull(oOrder);

            IFuturesOrder[]? aOrders = await oExchange.Trading.GetOrders();
            Assert.IsNotNull(aOrders);
            Assert.IsTrue(aOrders.Any());


        }

        [TestMethod]
        public async Task BingxAccountTests()
        {
            ICryptoFuturesExchange oExchange = BingxCommon.CreateExchange();

            IFuturesBalance[]? aBalances = await oExchange.Account.GetBalances();
            Assert.IsNotNull(aBalances);

            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();    
            Assert.IsNotNull(aPositions);

        }
    }
}
