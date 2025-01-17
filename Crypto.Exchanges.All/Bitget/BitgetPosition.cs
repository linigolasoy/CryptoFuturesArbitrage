using Bitget.Net.Objects.Models.V2;
using Bitget.Net.Enums.V2;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetPositionLocal : IFuturesPosition
    {

        public BitgetPositionLocal(IFuturesSymbol oSymbol, BitgetPositionUpdate oUpdate) 
        { 
            Symbol = oSymbol;
            Id = oUpdate.PositionId;
            Direction = (oUpdate.PositionSide == PositionSide.Long ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            Leverage = (int)oUpdate.Leverage;
            Quantity = oUpdate.Total;
            AveragePrice = oUpdate.AverageOpenPrice;
            ProfitRealized = oUpdate.RealizedProfitAndLoss;
            ProfitUnRealized = oUpdate.UnrealizedProfitAndLoss;
        }

        public BitgetPositionLocal(IFuturesSymbol oSymbol, BitgetPosition oUpdate)
        {
            Symbol = oSymbol;
            Id = string.Empty;
            Direction = (oUpdate.PositionSide == PositionSide.Long ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            Leverage = (int)oUpdate.Leverage;
            Quantity = oUpdate.Total;
            AveragePrice = oUpdate.AverageOpenPrice;
            ProfitRealized = oUpdate.RealizedProfitAndLoss;
            ProfitUnRealized = oUpdate.UnrealizedProfitAndLoss;
        }

        public WebsocketQueueType QueueType { get => WebsocketQueueType.Poisition; }
        public IFuturesSymbol Symbol { get; }

        public string Id { get; }

        public FuturesPositionDirection Direction { get; }

        public int Leverage { get; }
        public bool Closed { get; set; } = false;
        public decimal Quantity { get; private set; }

        public decimal AveragePrice { get; private set; }

        public decimal ProfitRealized { get; private set; }

        public decimal ProfitUnRealized { get; private set; }

        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public void Update(IFuturesPosition oPosition)
        {
            AveragePrice = oPosition.AveragePrice;
            Quantity = oPosition.Quantity;
            ProfitRealized = oPosition.ProfitRealized;
            ProfitUnRealized = oPosition.ProfitUnRealized;
            LastUpdate = oPosition.LastUpdate;
            Closed = oPosition.Closed;  
        }
    }
}
