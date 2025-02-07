using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{
    public class ArbitrageFactory
    {

        public static IOppositeOrder CreateOppositeOrder( IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort, int nLeverage, DateTime dLimitDate, ICryptoSetup oSetup )
        {
            return new OppositeOrder( oSymbolLong, oSymbolShort , nLeverage, dLimitDate, oSetup);  
        }
        public static async Task<IOppositeOrder[]?> CreateOppositeOrderFromExchanges(IFuturesExchange[] aExchanges, ICryptoSetup oSetup)
        {
            return await OppositeOrder.CreateFromExchanges(aExchanges, oSetup);
        }
    }
}
