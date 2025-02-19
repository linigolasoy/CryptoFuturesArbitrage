using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.BackTest
{

    public enum BackTestStatus
    {
        Open,
        Closed
    }

    public interface IBackTestResult
    {
        public IBackTester BackTester { get; }
        public BackTestStatus Status { get; }   
        public DateTime ActualDate { get; }
        public DateTime StartDate { get; }

        public decimal ProfitPending { get; }   
        public decimal ProfitClosed { get; }

        public decimal ProfitTotal { get; }
    }
}
