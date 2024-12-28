using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.CoinEx;
using Crypto.Interface;
using Crypto.Interface.Futures;

namespace Crypto.Exchanges.All
{

    public class ExchangeFactory
    {

        public static ICryptoFuturesExchange CreateExchange( ExchangeType eType, ICryptoSetup oSetup )
        {
            switch( eType )
            {
                case ExchangeType.BingxFutures:
                    return new BingxFutures(oSetup);
                case ExchangeType.CoinExFutures:
                    return new CoinexFutures(oSetup);
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
