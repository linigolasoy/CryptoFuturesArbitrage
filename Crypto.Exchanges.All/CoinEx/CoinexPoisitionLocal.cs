using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using CoinEx.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexPoisitionLocal : IFuturesPosition
    {

        public CoinexPoisitionLocal( IFuturesSymbol oSymbol, CoinExPositionUpdate oUpdate) 
        { 
            Symbol = oSymbol;   
            Id = oUpdate.Position.Id.ToString();
            if( oUpdate.Position.Side == PositionSide.Short ) Direction = FuturesPositionDirection.Short;
            Leverage = (int)oUpdate.Position.Leverage;
            Quantity = oUpdate.Position.CloseAvailable;
            AveragePrice = oUpdate.Position.AverageEntryPrice;
            ProfitRealized = oUpdate.Position.RealizedPnl;
            ProfitUnRealized = oUpdate.Position.UnrealizedPnl;  
        }

        public CoinexPoisitionLocal(IFuturesSymbol oSymbol, CoinExPosition oPos ) 
        { 
            Symbol = oSymbol;
            Id = oPos.Id.ToString();
            if (oPos.Side == PositionSide.Short) Direction = FuturesPositionDirection.Short;
            Leverage = (int)oPos.Leverage;
            Quantity = oPos.CloseAvailable;
            AveragePrice = oPos.AverageEntryPrice;
            ProfitRealized = oPos.RealizedPnl;
            ProfitUnRealized = oPos.UnrealizedPnl;

        }
        public IFuturesSymbol Symbol { get; }

        public string Id { get; }

        public FuturesPositionDirection Direction { get; private set; } = FuturesPositionDirection.Long;

        public int Leverage { get; }

        public decimal Quantity { get; private set; }

        public decimal AveragePrice { get; private set; }

        public decimal ProfitRealized { get; private set; }

        public decimal ProfitUnRealized { get; private set; }

        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public void Update(IFuturesPosition oPosition)
        {
            Quantity = oPosition.Quantity;
            AveragePrice = oPosition.AveragePrice;
            ProfitRealized = oPosition.ProfitRealized;
            ProfitUnRealized = oPosition.ProfitUnRealized;
            LastUpdate = oPosition.LastUpdate;
        }
    }
}
