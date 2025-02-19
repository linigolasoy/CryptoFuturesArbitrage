﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;

namespace Crypto.Interface.Futures.Account
{

    /// <summary>
    /// Position data
    /// </summary>
    public interface IFuturesPosition: IWebsocketQueueItem
    {
        public IFuturesSymbol Symbol { get; }   
        public string Id { get; }   

        public FuturesPositionDirection Direction { get; }

        public int Leverage { get; }    
        public decimal Quantity { get; }    
        public decimal AveragePrice { get; }        

        public decimal ProfitRealized { get; }

        public decimal ProfitUnRealized { get; }

        public DateTime LastUpdate { get; }
        public bool Closed { get; set; }
        public void Update(IFuturesPosition oPosition);
    }
}
