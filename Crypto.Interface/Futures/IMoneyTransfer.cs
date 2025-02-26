using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{

    public enum MoneyTransferStatus
    {
        Initial,
        FuturesToSpot,
        Transfer,
        WaitForArrival,
        SpotToFutures,
        Done
    }


    public interface IMoneyTransfer
    {

        public IFuturesExchange From { get; }
        public IFuturesExchange To { get; }

        public decimal Quantity { get; }    
        public MoneyTransferStatus Status { get;}


        public Task<ITradingResult<decimal>> Step();
    }
}
