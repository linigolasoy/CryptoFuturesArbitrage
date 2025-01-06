using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{
    public interface IFundingTestData: IFundingData
    {
        public ICommonLogger Logger { get; }

        public DateTime From { get; }
        public DateTime To { get; }
        public Task<bool> LoadSymbols();

        public Task<bool> LoadRates();
    }
}
