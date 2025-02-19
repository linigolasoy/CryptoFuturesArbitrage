﻿using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;

namespace Crypto.Tests.Bitmart
{
    [TestClass]
    public class BitmartWebsocketTests
    {

        private int m_nReceived1 = 0;

        [TestMethod]
        public async Task BitmartFundingAccountSocket()
        {
            IFuturesExchange oExchange = await BitmartCommon.CreateExchange();

            oExchange.Account.OnPrivateEvent += MyOnPrivateEvent;
            await Task.Delay(1000);
            bool bResultStart = await oExchange.Account.StartSockets();
            Assert.IsTrue(bResultStart);

            await Task.Delay(20000);
            IFuturesBalance[] aBalances = oExchange.Account.BalanceManager.GetData();
            Assert.IsTrue(aBalances.Length > 0);
            Assert.IsTrue(m_nReceived1 > 0);
        }
        private async Task MyOnPrivateEvent(Interface.Futures.Websockets.IWebsocketQueueItem oItem)
        {
            m_nReceived1++;
        }

        [TestMethod]
        public async Task BitmartAllSymbolsSocket()
        {
            IFuturesExchange oExchange = await BitmartCommon.CreateExchange();

            await Task.Delay(1000);
            IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
            Assert.IsNotNull(aSymbols);
            bool bResultStart = await oExchange.Market.StartSockets();
            Assert.IsTrue(bResultStart);

            await Task.Delay(10000);

            Assert.IsNotNull(oExchange.Market.Websocket);
            Assert.IsTrue(oExchange.Market.Websocket.FundingRateManager.Count == aSymbols.Length);
            Assert.IsTrue(oExchange.Market.Websocket.OrderbookManager.Count >= aSymbols.Length - 2);

            IOrderbook[]? aData = oExchange.Market.Websocket.OrderbookManager.GetData();
            Assert.IsNotNull(aData);
            Assert.IsTrue(aData.Length >= aSymbols.Length - 2);


            decimal nDelay = 0;
            decimal nMaxDelay = 0;
            decimal nMinDelay = 9E10M;
            IOrderbook? oMaxData = null;

            foreach (var oData in aData)
            {
                decimal nActDelay = (decimal)(oData.ReceiveDate - oData.UpdateDate).TotalMilliseconds;
                if (nActDelay > nMaxDelay) { nMaxDelay = nActDelay; oMaxData = oData; }
                if (nActDelay < nMinDelay) { nMinDelay = nActDelay; }
                nDelay += nActDelay;
            }
            nDelay /= (decimal)aData.Length;

            decimal nDelayGlobal = (decimal)(DateTime.Now - oExchange.Market.Websocket.OrderbookManager.LastUpdate).TotalMilliseconds;

            IOrderbook? oBtc = aData.FirstOrDefault(p => p.Symbol.Base == "BTC" && p.Symbol.Quote == "USDT");
            Assert.IsNotNull(oBtc);


            IFundingRate[]? aFunding = oExchange.Market.Websocket.FundingRateManager.GetData();
            Assert.IsNotNull(aFunding);
            Assert.IsTrue(aFunding.Length == aSymbols.Length);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "BAN" && p.Quote == "USDT");
            Assert.IsNotNull(oSymbol);
            IFundingRate? oRateFound = oExchange.Market.Websocket.FundingRateManager.GetData(oSymbol.Symbol);
            Assert.IsNotNull(oRateFound);

            bool bResultStop = await oExchange.Market.EndSockets();
            Assert.IsTrue(bResultStop);
        }

    }
}
