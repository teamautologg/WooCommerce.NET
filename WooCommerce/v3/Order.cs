using System.Runtime.Serialization;
using Newtonsoft.Json;
using WooCommerceNET.Base;

namespace WooCommerceNET.WooCommerce.v3
{
    public class OrderBatch : BatchObject<Order> { }

    [DataContract]
    public class Order : v2.Order
    {
        public byte[] Invoice
        {
            get; set;
        }

        public string InvoiceId { get; set; }

        [JsonProperty("needs_payment")]
        public bool NeedsPayment { get; set; }

        [JsonProperty("payment_url")]
        public string PaymentUrl { get; set; }
    }
}
