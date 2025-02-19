using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.BackTest
{
    public interface IBackTester
    {
        public ICryptoSetup Setup { get; }  

        public ICommonLogger Logger { get; }
        public DateTime From { get; }
        public DateTime To { get; }
        public Task<bool> Start();
        public Task<bool> Stop();

        public bool Ended { get; }  

        public Task<IBackTestResult?> Step();


    }
}
