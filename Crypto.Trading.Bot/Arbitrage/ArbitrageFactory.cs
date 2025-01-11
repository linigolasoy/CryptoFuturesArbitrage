using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{
    public class ArbitrageFactory
    {

        public static IOppositeOrder CreateOppositeOrder( IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort )
        {
            return new OppositeOrder( oSymbolLong, oSymbolShort );  
        }
        public static async Task<IOppositeOrder[]?> CreateOppositeOrderFromExchanges(ICryptoFuturesExchange[] aExchanges)
        {
            return await OppositeOrder.CreateFromExchanges(aExchanges);
        }
    }
}
