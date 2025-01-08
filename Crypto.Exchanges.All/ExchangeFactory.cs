using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.Bitget;
using Crypto.Exchanges.All.Bybit;
using Crypto.Exchanges.All.CoinEx;
using Crypto.Interface;
using Crypto.Interface.Futures;

namespace Crypto.Exchanges.All
{

    public class ExchangeFactory
    {

        public static async Task<ICryptoFuturesExchange> CreateExchange( ExchangeType eType, ICryptoSetup oSetup )
        {
            ICryptoFuturesExchange? oResult = null; 
            switch ( eType )
            {
                case ExchangeType.BingxFutures:
                    oResult = new BingxFutures(oSetup);
                    break;  
                case ExchangeType.CoinExFutures:
                    oResult = new CoinexFutures(oSetup);
                    break;
                case ExchangeType.BitgetFutures:
                    oResult = new BitgetFutures(oSetup);
                    break;
                // case ExchangeType.ByBitFutures:
                //     return new BybitFutures(oSetup);
                default:
                    throw new NotImplementedException();
            }
            // Start private websockets
            await oResult.Account.StartSockets();
            await Task.Delay(2000);
            return oResult;
        }

    }
}
