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

        public static IOppositeOrder CreateOppositeOrder( IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort, int nLeverage, DateTime dLimitDate )
        {
            return new OppositeOrder( oSymbolLong, oSymbolShort , nLeverage, dLimitDate);  
        }
        public static async Task<IOppositeOrder[]?> CreateOppositeOrderFromExchanges(IFuturesExchange[] aExchanges)
        {
            return await OppositeOrder.CreateFromExchanges(aExchanges);
        }
    }
}
