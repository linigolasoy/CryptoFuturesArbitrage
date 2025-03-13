using Crypto.Common;
using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Crypto.Trading.Bot;
using Crypto.Trading.Bot.Arbitrage;
using Crypto.Trading.Bot.Common;
using Crypto.Trading.Bot.FundingRates.Model;
using HTX.Net.Enums;

namespace Crypto.Tests
{
    [TestClass]
    public class BotTests
    {

        private IFuturesPosition?   m_oTrailPosition = null;
        private ITrailingOrder?     m_oTrailOrder = null;

        [TestMethod]
        public async Task TrailingOrderTest()
        {
            CancellationTokenSource oSource = new CancellationTokenSource();
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);


            IFuturesExchange oExchange = await ExchangeFactory.CreateExchange(ExchangeType.CoinExFutures, oSetup, null);


            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
            Assert.IsNotNull(oSymbol);  


            bool bOk = await oExchange.Market.StartSockets();    

            Assert.IsTrue(bOk);
            bOk = await oExchange.Account.StartSockets();
            Assert.IsTrue(bOk);



            oExchange.Account.OnPrivateEvent += OnPrivateEvent;

            await Task.Delay(1000);


            var oResult = await oExchange.Trading.CreateMarketOrder(oSymbol, true, 5);
            Assert.IsNotNull(oResult);
            Assert.IsTrue(oResult.Success);

            int nRetries = 0;
            while( m_oTrailPosition == null && nRetries < 20 )
            {
                await Task.Delay(200);
                nRetries++;
            }

            Assert.IsNotNull(m_oTrailPosition);

            ITrailingOrder oTrail = BotFactory.CreateTrailingOrder(m_oTrailPosition, 0.2M);


            var oTrailStart = await oTrail.Start();
            Assert.IsNotNull(oTrailStart);
            Assert.IsTrue(oTrailStart.Success); 


            for( int i = 0; i < 20000; i++ )
            {
                var oTrailStep = await oTrail.Trail();

                Assert.IsNotNull(oTrailStep);
                Assert.IsTrue(oTrailStep.Success);
                await Task.Delay(100);
            }

            await Task.Delay(2000);


            await Task.Delay(200000);
            Assert.IsNotNull(oExchange.Market.Websocket);
            await oExchange.Market.Websocket!.Stop();

        }

        private async Task OnPrivateEvent(IWebsocketQueueItem oItem)
        {
            switch( oItem.QueueType )
            {
                case WebsocketQueueType.Poisition:
                    m_oTrailPosition = m_oTrailPosition = (IFuturesPosition)oItem;
                    break;
                default:
                    break;
            }
        }

        [TestMethod]
        public async Task FundingSocketTests()
        {

            CancellationTokenSource oSource = new CancellationTokenSource();
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "FundingSocketTests", oSource.Token);


            IFundingSocketData oSocketData = BotFactory.CreateFundingSocket(oSetup, oLogger);

            bool bStarted = await oSocketData.Start();
            Assert.IsTrue(bStarted);

            await Task.Delay(5000);
            Assert.IsNotNull(oSocketData.Websockets);

            IFundingDate[]? aDates = await oSocketData.GetFundingDates();   
            Assert.IsNotNull(aDates);
            Assert.IsTrue(aDates.Any());

            IFundingDate? oNext = await oSocketData.GetNext(null);
            Assert.IsNotNull(oNext);

            IFundingPair? oPair = oNext.GetBest();
            Assert.IsNotNull(oPair);    

            foreach( var oDate in aDates )
            {
                IFundingPair? oBest = oDate.GetBest();  
                Assert.IsNotNull(oBest);    
            }

            await oSocketData.Stop();

            await Task.Delay(2000);
        }

        [TestMethod]
        public async Task SocketManagerTests()
        {
            CancellationTokenSource oSource = new CancellationTokenSource();
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "SocketManagerTests", oSource.Token);


            ISocketManager oManager = BotFactory.CreateSocketManager(oSetup, oLogger);  

            bool bStarted = await oManager.Start();
            Assert.IsTrue(bStarted);

            if (!bStarted) return;
            await Task.Delay(10000);
            await oManager.Stop();  
        }


        [TestMethod]
        public async Task EthOptionsTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            IFuturesExchange oExchangeBingx = await ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup, null);

            DateTime dFrom = DateTime.Today.AddMonths(-3);

            IFuturesSymbol? oSymbol = oExchangeBingx.SymbolManager.GetAllValues().FirstOrDefault(p=> p.Base == "ETH" && p.Quote == "USDT");
            Assert.IsNotNull(oSymbol);
            IFuturesBar[]? aBars = await oExchangeBingx.History.GetBars(oSymbol, Timeframe.M15, dFrom, DateTime.Today);
            Assert.IsNotNull(aBars);
            aBars = aBars.OrderBy(p=> p.DateTime).ToArray();

            DateTime? dActual = null;
            int nHour = 7;

            decimal nDelta = 0.5M; // Value decay each point
            decimal nPutPrice = 93M;
            decimal nTheta = 5M;

            decimal nTotalProfit = 0;

            decimal nWin = 400;
            decimal nLoss = 1200;
            decimal nRangeUp = 10;
            decimal nRangeDn = 10;
            decimal nFeesDay = 137;
            decimal nOptionCount = 6;

            while( true)
            {
                IFuturesBar? oMonday = aBars.FirstOrDefault(p => (dActual == null ? true : p.DateTime.Date > dActual.Value.Date) &&
                                                                p.DateTime.DayOfWeek == DayOfWeek.Monday &&
                                                                p.DateTime.Hour == 7 &&
                                                                p.DateTime.Minute == 0);
                if (oMonday == null) break;


                IFuturesBar? oFriday = aBars.FirstOrDefault(p => p.DateTime > oMonday.DateTime &&
                                                                p.DateTime.DayOfWeek == DayOfWeek.Friday &&
                                                                p.DateTime.Hour == 7 &&
                                                                p.DateTime.Minute == 0);

                if (oFriday == null) break;
                IFuturesBar[] aWeekBars = aBars.Where(p=> p.DateTime >= oMonday.DateTime && p.DateTime < oFriday.DateTime).OrderBy(p=> p.DateTime).ToArray();
                decimal nPriceStart = oMonday.Open;
                int nPriceInt = (int)nPriceStart;
                int nModulus = nPriceInt % 100;
                decimal nPriceOption = (decimal)nPriceInt - (decimal)nModulus;
                if( nModulus >= 50 )
                {
                    nPriceOption = (decimal)nPriceInt + (decimal)( 100 - nModulus);
                }


                decimal nPricePool = oMonday.Open;
                decimal nPriceLow = (100M - nRangeDn) * oMonday.Open / 100M;
                decimal nPriceUp  = (100M + nRangeDn) * oMonday.Open / 100M;
                decimal nActualValueOption = nPutPrice;


                decimal nTotalDays = 0;
                decimal nFees = 0;
                decimal nProfit = 0;
                bool bClosed = false;
                decimal nLastPrice = 0;
                foreach ( IFuturesBar oBar in aWeekBars)
                {
                    decimal nDays = (decimal)(oBar.DateTime - oMonday.DateTime).TotalDays;
                    decimal nDeprecation = nTheta * nDays * nOptionCount;

                    // if (oBar.High <= nPriceOption) continue;
                    // decimal nDiff = oBar.High - nPriceOption;
                    // decimal nValueOption = nDiff * nDelta + nPutPrice;
                    nActualValueOption -= nDeprecation;
                    nTotalDays = (decimal)(oBar.DateTime - oMonday.DateTime).TotalDays;
                    nFees = nFeesDay * nTotalDays;
                    nProfit = nFees;
                    nLastPrice = oBar.Close;
                    if( oBar.Low <= nPriceLow )
                    {
                        nProfit -= nLoss;
                        decimal nWinOption = (nPriceOption - nPriceLow) * nDelta * nOptionCount;
                        nWinOption -= nDeprecation;
                        nProfit += nWinOption;
                        nTotalProfit += nProfit;
                        bClosed = true;
                        break;
                    }
                    else if( oBar.High >= nPriceUp ) 
                    {
                        nProfit += nWin - nPutPrice * nOptionCount;
                        nTotalProfit += nProfit;
                        bClosed = true;
                        break;
                    }

                }
                // nTotalProfit += nProfit;
                if( !bClosed  ) 
                { 
                    if( nLastPrice >= nPriceOption)
                    {
                        nProfit -= nPutPrice * nOptionCount;    
                    }
                    else
                    {
                        nProfit += (nPriceOption - nLastPrice - nPutPrice) * nOptionCount;
                    }
                    nTotalProfit += nProfit;
                }



                dActual = oMonday.DateTime.Date.AddDays(1);
            }


        }

        /*
        /// <summary>
        /// Evaluates funding rates using websockets
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MatchFundingWebsocketTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oExchangeMexc = new MexcFuturesExchange(oSetup);
            ICryptoFuturesExchange oExchangeBingx = new BingxFuturesExchange(oSetup);


            IFuturesSymbol[]? aSymbolsMexc = await oExchangeMexc.GetSymbols();
            Assert.IsNotNull(aSymbolsMexc);

            IFuturesSymbol[]? aSymbolsBingx = await oExchangeBingx.GetSymbols();
            Assert.IsNotNull(aSymbolsBingx);


            ICryptoWebsocket? oWsMexc = await oExchangeMexc.CreateWebsocket();   
            Assert.IsNotNull(oWsMexc);
            ICryptoWebsocket? oWsBingx = await oExchangeBingx.CreateWebsocket();
            Assert.IsNotNull(oWsBingx);

            aSymbolsBingx = aSymbolsBingx.Where(p => aSymbolsMexc.Any(q => p.Base == q.Base && p.Quote == q.Quote)).ToArray();

            IFundingRateSnapShot[]? aFundingsBingx = await oExchangeBingx.GetFundingRates(aSymbolsBingx);
            Assert.IsNotNull (aFundingsBingx);

            
            aSymbolsMexc = aSymbolsMexc.Where(p => aSymbolsBingx.Any(q => p.Base == q.Base && p.Quote == q.Quote)).ToArray();

            aSymbolsBingx = aFundingsBingx.OrderByDescending(p => Math.Abs(p.Rate)).Take(200).Select(p => p.Symbol).ToArray();


            await oWsMexc.Start();
            await oWsBingx.Start();


            await oWsMexc.SubscribeToMarket(aSymbolsMexc);
            await oWsBingx.SubscribeToMarket(aSymbolsBingx);


            await Task.Delay(20000);

            IWebsocketManager<IOrderbook> oMexcManager = oWsMexc.OrderbookManager;
            IWebsocketManager<IOrderbook> oBingxManager = oWsBingx.OrderbookManager;


            SortedDictionary<decimal, IOrderbook[]> aSorted = new SortedDictionary<decimal, IOrderbook[]>();
            decimal nBestFound = 0;

            ITicker[] aTickersBingx = oBingxManager.GetData();
            ITicker[] aTickersMexc  = oMexcManager.GetData();
            foreach (ITicker oTickerBing in aTickersBingx)
            {
                string strBase = oTickerBing.Symbol.Base;
                string strQuote = oTickerBing.Symbol.Quote;

                ITicker? oTickerMexc = aTickersMexc.Where(p => p.Symbol.Base == strBase && p.Symbol.Quote == strQuote).FirstOrDefault();
                if (oTickerMexc == null) continue;


                decimal nRate = Math.Abs(oTickerBing.FundingRate - oTickerMexc.FundingRate) * 100M;

                if (nRate > nBestFound)
                {
                    nBestFound = nRate;
                }

                
                if (nRate > 0.10M)
                {
                    aSorted[nRate] = new ITicker[] { oTickerBing, oTickerMexc };
                }
                

            }

            Assert.IsTrue(aSorted.Count > 0);

        }
        */

    }
}