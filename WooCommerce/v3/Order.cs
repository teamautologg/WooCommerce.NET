using System.Runtime.Serialization;
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
    }
}
