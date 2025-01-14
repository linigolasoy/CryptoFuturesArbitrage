using Crypto.Interface;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{

    /// <summary>
    /// Funding dates to use on websockets
    /// </summary>
    public interface IFundingSocketData: IFundingData
    {

        public ICommonLogger Logger { get; }

        public IFuturesWebsocketPublic[]? Websockets { get; }

        public Task<bool> Start();
        public Task Stop();

        public Task<IFundingDate[]?> GetFundingDates();

    }
}
