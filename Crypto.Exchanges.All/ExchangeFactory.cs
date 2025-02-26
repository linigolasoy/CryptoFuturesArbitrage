using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.Bitget;
using Crypto.Exchanges.All.Bitmart;
using Crypto.Exchanges.All.BitUnix;
using Crypto.Exchanges.All.CoinEx;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;

namespace Crypto.Exchanges.All
{

    public class ExchangeFactory
    {

        public static async Task<IFuturesExchange> CreateExchange( ExchangeType eType, ICryptoSetup oSetup, ICommonLogger? oLogger )
        {
            IFuturesExchange? oResult = null; 
            switch ( eType )
            {
                case ExchangeType.BingxFutures:
                    oResult = new BingxFutures(oSetup, oLogger);
                    break;  
                case ExchangeType.CoinExFutures:
                    oResult = new CoinexFutures(oSetup, oLogger);
                    break;
                case ExchangeType.BitgetFutures:
                    oResult = new BitgetFutures(oSetup, oLogger);
                    break;
                case ExchangeType.BitmartFutures:
                    oResult = new BitmartFutures(oSetup, oLogger);
                    break;
                case ExchangeType.BitUnixFutures:
                    oResult = new BitunixFutures(oSetup);
                    break;
                // case ExchangeType.ByBitFutures:
                //     return new BybitFutures(oSetup);
                default:
                    throw new NotImplementedException();
            }
            // Start private websockets
            // await oResult.Account.StartSockets();
            // await Task.Delay(2000);
            return oResult;
        }


        public static IMoneyTransfer CreateMoneyTransfer( IFuturesExchange oFrom, IFuturesExchange oTo, decimal nQuantity ) 
        { 
            return new MoneyTransfer(oFrom, oTo, nQuantity);
        }
    }
}
