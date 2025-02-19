﻿using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxPositionLocal : IFuturesPosition
    {



        public BingxPositionLocal( IFuturesSymbol oSymbol, BingXPosition oParsed) 
        {
            Symbol = oSymbol;
            Id = oParsed.PositionId;    
            FuturesPositionDirection eDirection = FuturesPositionDirection.Long;
            if( oParsed.Side == BingX.Net.Enums.TradeSide.Short ) eDirection = FuturesPositionDirection.Short;  
            Direction = eDirection;
            Leverage = (int)oParsed.Leverage;
            Quantity = oParsed.Size;
            AveragePrice = oParsed.AveragePrice;
            ProfitRealized = oParsed.RealizedProfit;
            ProfitUnRealized = oParsed.UnrealizedProfit;    
        }

        public BingxPositionLocal( IFuturesSymbol oSymbol, BingXFuturesPositionChange oParsed )
        {
            Symbol = oSymbol;
            Id = string.Empty;
            FuturesPositionDirection eDirection = FuturesPositionDirection.Long;
            if (oParsed.Side == BingX.Net.Enums.TradeSide.Short) eDirection = FuturesPositionDirection.Short;
            Direction = eDirection;
            Leverage = 0;
            Quantity = oParsed.Size;
            AveragePrice = oParsed.EntryPrice;
            ProfitRealized = oParsed.RealizedPnl;
            ProfitUnRealized = oParsed.UnrealizedPnl;

        }

        public WebsocketQueueType QueueType { get => WebsocketQueueType.Poisition; }
        public IFuturesSymbol Symbol { get; }
        public string Id { get; }   
        public FuturesPositionDirection Direction { get; }

        public int Leverage { get; }

        public decimal Quantity { get; private set; }

        public decimal AveragePrice { get; private set; }

        public decimal ProfitRealized { get; private set; }

        public decimal ProfitUnRealized { get; private set; }
        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public bool Closed { get; set; } = false;
        public void Update( IFuturesPosition oPos )
        {
            Quantity = oPos.Quantity;
            AveragePrice = oPos.AveragePrice;
            ProfitRealized = oPos.ProfitRealized;
            ProfitUnRealized = oPos.ProfitUnRealized;
            LastUpdate = oPos.LastUpdate;
            Closed = oPos.Closed;
        }
    }
}
