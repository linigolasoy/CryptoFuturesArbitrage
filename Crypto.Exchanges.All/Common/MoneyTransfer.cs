using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class MoneyTransfer : IMoneyTransfer
    {
        public MoneyTransfer(IFuturesExchange oFrom, IFuturesExchange oTo, decimal nQuantity) 
        { 
            From = oFrom;
            To = oTo;
            Quantity = nQuantity;
        }
        public IFuturesExchange From { get; }

        public IFuturesExchange To { get; }

        public decimal Quantity { get; }

        public MoneyTransferStatus Status { get; private set; } = MoneyTransferStatus.Initial;

        public async Task<ITradingResult<decimal>> Step()
        {
            throw new NotImplementedException();
        }
    }
}
