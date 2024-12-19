using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Responses
{
    internal class FuturesBarParsed
    {
        [JsonProperty("open")]
        public List<double>? Open { get; set; } = null;
        [JsonProperty("high")]
        public List<double>? High { get; set; } = null;
        [JsonProperty("low")]
        public List<double>? Low { get; set; } = null;
        [JsonProperty("close")]
        public List<double>? Close { get; set; } = null;
        [JsonProperty("vol")]
        public List<double>? Volume { get; set; } = null;
        [JsonProperty("time")]
        public List<long>? Times { get; set; } = null;

        /*
            open	double	the opening price
            close	double	the closing price
            high	double	the highest price
            low	double	the lowest price
            vol	double	volume
            time	long	time window
        */
    }
}
