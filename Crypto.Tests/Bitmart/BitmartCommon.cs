using Crypto.Common;
using Crypto.Exchanges.All;
using Crypto.Interface.Futures;
using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Tests.Bitmart
{
    internal class BitmartCommon
    {
        public static async Task<IFuturesExchange> CreateExchange()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            IFuturesExchange oExchange = await ExchangeFactory.CreateExchange(ExchangeType.BitmartFutures, oSetup, null);
            return oExchange;
        }
    }
}
