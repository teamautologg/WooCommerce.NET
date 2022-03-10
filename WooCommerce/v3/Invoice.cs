using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using WooCommerceNET.Base;

namespace WooCommerceNET.WooCommerce.v3
{
    public class InvoiceBatch : BatchObject<Invoice> { }

    [DataContract]
    public class Invoice : JsonObject
    {
        public static string Endpoint { get { return "orders"; } }

        /// <summary>
        /// Unique identifier for the resource. 
        /// read-only
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long? id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_created { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_modified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_sent { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_due { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_paid { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_of_service { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_of_service_end { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string created_via { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string version { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int parent_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string payment_status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string payment_method_title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string payment_method_name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string payment_transaction_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int reference_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string reference_type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string reference_number { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int customer_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int author_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string number { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string formatted_number { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string journal_type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string vat_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string discount_notice { get; set; }


    }

    public class InvoicePdf : JsonObject
    {
        [DataMember(EmitDefaultValue = false)]
        public string file { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool is_preview { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string path { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string filename { get; set; }
    }

    public class WcInvoiceItem : WCItem<Invoice>
    {
        public WcInvoiceItem(RestAPI api) : base(api)
        {
        }

        public async Task<Invoice> GetInvoice(long orderId, Dictionary<string, string> parms = null)
        {
            var json = await API.GetRestful(APIEndpoint + "/" + orderId.ToString() + "/invoices", parms).ConfigureAwait(false);
            var invoiceList = JsonConvert.DeserializeObject<IList<Invoice>>(json);

            return invoiceList.FirstOrDefault();
        }

        public async Task<InvoicePdf> GetInvoicePdf(long invoiceId, Dictionary<string, string> parms = null)
        {
            var json = await API.GetRestful($"sab/v1/invoices/{invoiceId}/pdf", parms, true).ConfigureAwait(false);
            var invoicePdf = JsonConvert.DeserializeObject<InvoicePdf>(json);
            return invoicePdf;
        }
    }
}
