using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Futures
{
    internal class FuturesBar : IFuturesBar
    {
        public FuturesBar( IFuturesSymbol oSymbol,
                            Timeframe eFrame,
                            long nTime,
                            double nOpen,
                            double nHigh,
                            double nLow,
                            double nClose,
                            double nVolume) 
        { 
            Symbol = oSymbol;
            Timeframe = eFrame;
            DateTime = MexcCommon.ParseUnixTimestamp(nTime, true);
            Open = (decimal)nOpen;
            High = (decimal)nHigh;
            Low = (decimal)nLow;
            Close = (decimal)nClose;
            Volume = (decimal)nVolume;
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
