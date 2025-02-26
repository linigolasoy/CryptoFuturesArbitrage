using Crypto.Common;
using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Tests.BitUnix
{
    internal class BitUnixCommon
    {
        public static async Task<IFuturesExchange> CreateExchange()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            IFuturesExchange oExchange = await ExchangeFactory.CreateExchange(ExchangeType.BitUnixFutures, oSetup, null);
            return oExchange;   
        }
    }
}
