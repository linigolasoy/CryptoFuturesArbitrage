using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{

    public enum Timeframe
    {
        M1 = 1,
        M5 = 5,
        M15 = 15,
        M30 = 30,
        H1 = 60,
        H4 = 240,
        D1 = 1440
    }
    public interface IBarData
    {
        public Timeframe Timeframe { get; }
        public DateTime DateTime { get; }
        public decimal Open { get; }
        public decimal Close { get; }
        public decimal Volume { get; }
        public decimal High { get; }
        public decimal Low { get; }
    }
    public interface ISpotBar: IBarData
    {
        public ISpotSymbol Symbol { get; }
    }

    public interface IFuturesBar : IBarData
    {
        public IFuturesSymbol Symbol { get; }
    }

}
