using Crypto.Exchange.Bingx.Responses;
using Crypto.Interface.Futures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx
{
    public partial class BingxFuturesExchange
    {

        private const string ENDPOINT_LEVERAGE      = "openApi/swap/v2/trade/leverage";
        private const string ENDPOINT_PLACE_ORDER   = "openApi/swap/v2/trade/order";

        private int m_nLeverageActual = 1;

        /// <summary>
        /// Sets leverage prior to orders
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bBuy"></param>
        /// <param name="nLeverage"></param>
        /// <returns></returns>
        private async Task<bool> SetLeverage( IFuturesSymbol oSymbol, bool bBuy, int nLeverage )
        {
            var oPayload = new
            {
                leverage = nLeverage,
                side = (bBuy ? "LONG": "SHORT"),
                symbol = oSymbol.Symbol
            };

            string? strResponse = await SignRequest(ENDPOINT_LEVERAGE, HttpMethod.Post, oPayload, null);
            if (strResponse == null) return false;
            ResponseFutures? oResponse = JsonConvert.DeserializeObject<ResponseFutures>(strResponse);
            if (oResponse == null || oResponse.Code != 0 || !string.IsNullOrEmpty(oResponse.Message)) return false;
            return true;
        }

        /// <summary>
        /// Create limit order
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bBuy"></param>
        /// <param name="nMargin"></param>
        /// <param name="nLeverage"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nMargin, int nLeverage, decimal nPrice)
        {
            bool bLeverage = await SetLeverage(oSymbol, bBuy, nLeverage);
            if(!bLeverage) return null;

            double nMoney = (double)((decimal)nLeverage * nMargin);
            double nQuantity = Math.Round( nMoney / (double)nPrice, 5);

            var oPayload = new
            {
                symbol = oSymbol.Symbol,
                type = "LIMIT",
                // type = "MARKET",
                side = "BUY",
                positionSide = (bBuy ? "LONG" : "SHORT"),
                quantity = nQuantity.ToString(CultureInfo.InvariantCulture),
                price = ((double)nPrice).ToString(CultureInfo.InvariantCulture)
            };

            string? strResponse = await SignRequest(ENDPOINT_PLACE_ORDER, HttpMethod.Post, oPayload, null);
            if (strResponse == null) return null;
            ResponseFutures? oResponse = JsonConvert.DeserializeObject<ResponseFutures>(strResponse);
            if (oResponse == null || oResponse.Code != 0 || !string.IsNullOrEmpty(oResponse.Message)) return null;
            if (oResponse.Data == null) return null;

            return null;    
        }
    }
}
