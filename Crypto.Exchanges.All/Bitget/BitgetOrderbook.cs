using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.Bitget.Websocket;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{


    internal class BitgetOrderbook : BaseOrderbook, IOrderbook
    {

        public BitgetOrderbook(IFuturesSymbol oSymbol, BitgetOrderBookUpdate oUpdate):
            base(oSymbol, oUpdate.Timestamp.ToLocalTime(), DateTime.Now)
        { 

            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oUpdate.Asks.OrderBy(p => p.Price))
            {
                aAsks.Add(new BaseOrderbookPrice(this, item.Price, item.Quantity));
            }
            // base.PutAsks(aAsks.ToArray());
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oUpdate.Bids.OrderByDescending(p => p.Price))
            {
                aBids.Add(new BaseOrderbookPrice(this, item.Price, item.Quantity));
            }
            Bids = aBids.ToArray();

        }

        public BitgetOrderbook(IFuturesSymbol oSymbol, DateTime dTimestamp, BitgetDataAction oAction ) :
            base(oSymbol, dTimestamp.ToLocalTime() , DateTime.Now)
        {
            if (oAction.Data == null || oAction.Data.Count <= 0) return;
            var oData = oAction.Data[0];
            if( oData.AskData == null || oData.BidData == null ) return;
            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice> ();
            foreach (var oItem in oData.AskData)
            {

                decimal nPrice = Decimal.Parse(oItem[0], CultureInfo.InvariantCulture);
                decimal nVolume = Decimal.Parse(oItem[1], CultureInfo.InvariantCulture);
                aAsks.Add( new BaseOrderbookPrice(this, nPrice, nVolume) );
            }
            Asks = aAsks.ToArray();
            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var oItem in oData.BidData)
            {
                decimal nPrice = Decimal.Parse(oItem[0], CultureInfo.InvariantCulture);
                decimal nVolume = Decimal.Parse(oItem[1], CultureInfo.InvariantCulture);
                aBids.Add(new BaseOrderbookPrice(this, nPrice, nVolume));
            }
            Bids = aBids.ToArray();
        }

    }
}
