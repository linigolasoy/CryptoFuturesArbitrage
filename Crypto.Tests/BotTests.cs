using Crypto.Common;
using Crypto.Exchange.Bingx;
using Crypto.Exchange.Mexc;
using Crypto.Interface;

namespace Crypto.Tests
{
    [TestClass]
    public class BotTests
    {

        [TestMethod]
        public async Task MatchFundingTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup();

            ICryptoFuturesExchange oExchangeMexc  = new MexcFuturesExchange(oSetup);
            ICryptoFuturesExchange oExchangeBingx = new BingxFuturesExchange(oSetup);


            IFuturesSymbol[]? aSymbolsMexc = await oExchangeMexc.GetSymbols();  
            Assert.IsNotNull(aSymbolsMexc);

            IFuturesSymbol[]? aSymbolsBingx = await oExchangeBingx.GetSymbols();
            Assert.IsNotNull(aSymbolsBingx);

            string strBaseFind  = "LUNC";
            string strQuoteFind = "USDT";

            IFuturesSymbol? oSymbolMexc = aSymbolsMexc.FirstOrDefault(p => p.Base == strBaseFind && p.Quote == strQuoteFind);
            Assert.IsNotNull(oSymbolMexc);

            IFuturesSymbol? oSymbolBingx = aSymbolsBingx.FirstOrDefault(p => p.Base == strBaseFind && p.Quote == strQuoteFind);
            Assert.IsNotNull(oSymbolBingx);

            IFundingRateSnapShot? oShotMex = await oExchangeMexc.GetFundingRates(oSymbolMexc);
            Assert.IsNotNull(oShotMex);

            IFundingRateSnapShot? oShotBing = await oExchangeBingx.GetFundingRates(oSymbolBingx);
            Assert.IsNotNull(oShotBing);

            IFundingRateSnapShot[]? aFundingMexc = await oExchangeMexc.GetFundingRates(aSymbolsMexc);
            Assert.IsNotNull(aFundingMexc);

            IFundingRateSnapShot[]? aFundingBingx = await oExchangeBingx.GetFundingRates(aSymbolsBingx);
            Assert.IsNotNull(aFundingBingx);

            SortedDictionary<decimal, IFundingRate[]> aSorted = new SortedDictionary<decimal, IFundingRate[]>();
            decimal nBestFound = 0;
            foreach( IFundingRateSnapShot oFundingBing in aFundingBingx )
            {
                string strBase = oFundingBing.Symbol.Base;
                string strQuote = oFundingBing.Symbol.Quote;

                IFundingRateSnapShot? oFundingMexc = aFundingMexc.Where( p=> p.Symbol.Base == strBase && p.Symbol.Quote == strQuote ).FirstOrDefault();
                if (oFundingMexc == null) continue;

                decimal nRate = Math.Abs(oFundingBing.Rate - oFundingMexc.Rate) * 100M;

                if (nRate > nBestFound)
                {
                    nBestFound = nRate;
                }

                if ( nRate > 0.10M )
                {
                    aSorted[nRate] = new IFundingRate[] { oFundingBing, oFundingMexc };
                }

                /*
                if ( oFundingBing.Rate > 0 && oFundingMexc.Rate < 0 )
                {
                    decimal nRate = oFundingBing.Rate - oFundingMexc.Rate;
                    aSorted[nRate] = new IFundingRate[] { oFundingBing, oFundingMexc };
                    if( nRate > nBestFound )
                    {
                        nBestFound = nRate;
                    }
                }
                else if( oFundingBing.Rate < 0 && oFundingMexc.Rate > 0 )
                {
                    decimal nRate = oFundingMexc.Rate - oFundingBing.Rate;
                    aSorted[nRate] = new IFundingRate[] { oFundingBing, oFundingMexc };
                    if (nRate > nBestFound)
                    {
                        nBestFound = nRate;
                    }

                }
                */
            }
            
            Assert.IsTrue( aSorted.Count > 0 ); 

        }




    }
}