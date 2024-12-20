using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common.Websockets
{
    internal class BaseStatistics : ICommonWebsocketStatistics
    {
        public int PingCount { get; internal set; } = 0;

        public int ReceivedCount { get; internal set; } = 0;

        public int SentCount { get; internal set; } = 0;
    }
}
