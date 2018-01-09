using System;
using Newtonsoft.Json;

namespace Lykke.Job.ExchangePolling.Contract
{
    public sealed class VolumePrice
    {
        public VolumePrice(decimal price, decimal volume)
        {
            Price =  price;
            Volume = Math.Abs(volume);
        }

        [JsonProperty("price")]
        public decimal Price { get; }

        [JsonProperty("volume")]
        public decimal Volume { get; }

    }
}
