using Bitget.Net.Objects.Models.V2;
using Crypto.Interface;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetBar : IFuturesBar
    {
        public BitgetBar(IFuturesSymbol oSymbol, Timeframe eFrame, BitgetFuturesKline oKLine)
        {
            Symbol = oSymbol;
            Timeframe = eFrame;
            DateTime = oKLine.OpenTime.ToLocalTime();
            Open = oKLine.OpenPrice;
            Close = oKLine.ClosePrice;
            High = oKLine.HighPrice;
            Low = oKLine.LowPrice;
            Volume = oKLine.Volume;
        }
        public IFuturesSymbol Symbol { get; }

        public Timeframe Timeframe { get; }

        public DateTime DateTime { get; }

        public decimal Open { get; }

        public decimal Close { get; }

        public decimal Volume { get; }

        public decimal High { get; }

        public decimal Low { get; }
    }
}
