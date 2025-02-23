using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.BackTest
{
    internal class BaseBackTestResult : IBackTestResult
    {
        public BaseBackTestResult(IBackTester oTester, DateTime dStart, DateTime dEnd, decimal nProfit) 
        { 
            BackTester = oTester;
            StartDate = dStart;
            EndDate = dEnd;
            Profit = nProfit;   
        }
        public IBackTester BackTester { get; }

        // public BackTestStatus Status => throw new NotImplementedException();

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

        public decimal Profit { get; }
    }
}
