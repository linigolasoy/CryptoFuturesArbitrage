using Crypto.Exchange.Bingx.Responses;
using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Futures
{
    internal class FuturesBar : IFuturesBar
    {
        public FuturesBar( IFuturesSymbol oSymbol, Timeframe eFrame, FuturesBarParsed oParsed ) 
        { 
            Symbol = oSymbol;
            Timeframe = eFrame;
            Open = (decimal)oParsed.Open;
            High = (decimal)oParsed.High;
            Low = (decimal)oParsed.Low;
            Close = (decimal)oParsed.Close;
            Volume = (decimal)oParsed.Volume;   
            DateTime = BingxCommon.ParseUnixTimestamp(oParsed.UnixTime);
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
