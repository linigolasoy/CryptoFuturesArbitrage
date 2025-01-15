using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoFuturesArbitrage.Console
{
    internal class LagTester
    {
        public LagTester( ICryptoSetup oSetup, ICommonLogger oLogger )
        {
            Setup = oSetup;
            Logger = oLogger;
        }

        public ICryptoSetup Setup { get; }
        public ICommonLogger Logger { get; }



    }
}
