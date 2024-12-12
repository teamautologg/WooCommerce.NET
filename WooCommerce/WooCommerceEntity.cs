using Newtonsoft.Json;
using System.Collections.Generic;
using WooCommerceNET.Base;

namespace WooCommerce.NET.WooCommerce
{
    public class HrefObject
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public class Links
    {
        [JsonProperty("self")] 
        public List<HrefObject> Self { get; set; } = new ();

        [JsonProperty("collection")]
        public List<HrefObject> Collection { get; set; } = new();

        [JsonProperty("customer")]
        public List<HrefObject> Customer { get; set; } = new();

        [JsonProperty("up")]
        public List<HrefObject> Up { get; set; } = new();
    }
    public class WooCommerceEntity : JsonObject
    {
        public virtual long? id { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }
    }
}
