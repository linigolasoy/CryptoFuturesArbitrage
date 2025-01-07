using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{
    public interface IFuturesAccount
    {

        public ICryptoFuturesExchange Exchange { get; }

        public Task<IFuturesBalance[]?> GetBalances();
        public Task<IFuturesPosition[]?> GetPositions();
    }
}
