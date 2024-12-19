using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx
{
    internal class BingxCommon
    {
        public const string URL_FUTURES_BASE = "https://open-api.bingx.com/";


        public static HttpClient GetHttpClient()
        {
            return new HttpClient();
        }
        public static DateTime ParseUnixTimestamp(long nTimestamp)
        {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(nTimestamp);
            return offset.LocalDateTime;
        }
    }
}
