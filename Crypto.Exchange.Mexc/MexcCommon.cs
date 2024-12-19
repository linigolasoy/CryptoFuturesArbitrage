using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc
{
    internal class MexcCommon
    {
        public const string URL_SPOT_BASE = "https://api.mexc.com";
        public const string URL_FUTURES_BASE = "https://contract.mexc.com/";


        public static HttpClient GetHttpClient()
        {
            return new HttpClient();
        }

        public static DateTime ParseUnixTimestamp(long nTimestamp, bool bSeconds = false)
        {
            var offset = ( bSeconds? 
                            DateTimeOffset.FromUnixTimeSeconds(nTimestamp):
                            DateTimeOffset.FromUnixTimeMilliseconds(nTimestamp) );
            return offset.LocalDateTime;
        }

    }
}
