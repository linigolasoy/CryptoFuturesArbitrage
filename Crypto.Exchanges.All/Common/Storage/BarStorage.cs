using Crypto.Interface;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class BarStorage : IFuturesBar
    {

        public BarStorage(IFuturesSymbol oSymbol, Timeframe eFrame, BarJson oJson) 
        { 
            Symbol = oSymbol;
            Timeframe = eFrame;
            DateTime = oJson.DateTime;
            Open = oJson.Open;
            Close = oJson.Close;
            High = oJson.High;
            Low = oJson.Low;
            Volume = oJson.Volume;
        }
        public IFuturesSymbol Symbol { get; }

        public Timeframe Timeframe { get; }

        public DateTime DateTime { get; }

        public decimal Open { get; }

        public decimal Close { get; }

        public decimal Volume { get; }

        public decimal High { get; }

        public decimal Low { get; private set; }


    }
}
