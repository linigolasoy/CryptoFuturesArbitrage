using BitMart.Net.Objects.Models;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartPositionLocal : IFuturesPosition
    {
        public BitmartPositionLocal(IFuturesSymbol oSymbol, BitMartPositionUpdate oUpdate) 
        { 
            Symbol = oSymbol;   
            if( oUpdate.PositionSide == BitMart.Net.Enums.PositionSide.Short ) Direction = FuturesPositionDirection.Short;
            Quantity = CalculateQuantity(oUpdate.PositionSize);

            if (oUpdate.AverageOpenPrice != null) AveragePrice = oUpdate.AverageOpenPrice.Value;
            LastUpdate = (oUpdate.UpdateTime == null ? oUpdate.CreateTime.ToLocalTime() : oUpdate.UpdateTime!.Value.ToLocalTime());
            Closed = (Quantity <= 0);
        }

        public BitmartPositionLocal(IFuturesSymbol oSymbol, BitMartPosition oPosition )
        {
            Symbol = oSymbol;
            if (oPosition.PositionSide == BitMart.Net.Enums.PositionSide.Short) Direction = FuturesPositionDirection.Short;
            Quantity = CalculateQuantity(oPosition.CurrentQuantity!.Value);

            if (oPosition.OpenAveragePrice != null) AveragePrice = oPosition.OpenAveragePrice.Value;
            LastUpdate = oPosition.Timestamp.ToLocalTime();
            Closed = (Quantity <= 0);
        }
        public IFuturesSymbol Symbol { get; }
        public string Id { get => string.Empty; }
        public FuturesPositionDirection Direction { get; private set; } = FuturesPositionDirection.Long;

        public int Leverage { get => 1; }

        public decimal Quantity { get; private set; }

        public decimal AveragePrice { get; internal set; } = 0;

        public decimal ProfitRealized { get; }

        public decimal ProfitUnRealized { get; }

        public DateTime LastUpdate { get; private set; }

        public bool Closed { get; set; } = false;

        public WebsocketQueueType QueueType { get => WebsocketQueueType.Poisition; }

        private decimal CalculateQuantity(decimal nQuantity)
        {
            decimal nContractSize = ((BitmartSymbol)Symbol).ContractSize;
            return nQuantity * nContractSize;
        }

        public void Update(IFuturesPosition oPosition)
        {
            Quantity = oPosition.Quantity;
            LastUpdate = oPosition.LastUpdate;
            AveragePrice = oPosition.AveragePrice;  
            Closed = oPosition.Closed;
        }
    }
}
