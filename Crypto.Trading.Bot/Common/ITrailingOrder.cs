using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    /// <summary>
    /// Trailing stop
    /// </summary>
    public interface ITrailingOrder
    {
        public IFuturesExchange Exchange { get; }   
        public IFuturesPosition Position { get; }

        public decimal Distance { get; }

        public decimal? PriceTakeProfit { get; }
        public decimal? PriceStopLoss { get; }


        public Task<ITradingResult<bool>> Start();

        public Task<ITradingResult<bool>> Trail();
    }
}
