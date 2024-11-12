using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WooCommerce.NET.WooCommerce;

namespace WooCommerceNET.WooCommerce.v3
{
    public class SubscriptionMetaData : WooCommerceEntity
    {

        [DataMember(EmitDefaultValue = false)]
        public override long? id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string key { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object value { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string display_key { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string display_value { get; set; }
    }

    public class SubscriptionLineItem : WooCommerceEntity
    {
        [DataMember(EmitDefaultValue = false)]
        public override long? id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int product_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int variation_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int quantity { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string tax_class { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string subtotal { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string subtotal_tax { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string total { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string total_tax { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<SubscriptionMetaData> meta_data { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string sku { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double price { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string parent_name { get; set; }
    }

    public class Subscription : WooCommerceEntity
    {
        public static string Endpoint { get { return "subscriptions"; } }

        [DataMember(EmitDefaultValue = false)]
        public override long? id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int parent_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string currency { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string version { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool prices_include_tax { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_created { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_modified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double discount_total { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double discount_tax { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double shipping_total { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double shipping_tax { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double cart_tax { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double total { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double total_tax { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int customer_id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string order_key { get; set; }

        /// <summary>
        /// List of billing address data. See Customer - Billing properties
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public CustomerBilling billing { get; set; }

        /// <summary>
        /// List of shipping address data. See Customer - Shipping properties
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public CustomerShipping shipping { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string payment_method { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string payment_method_title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string customer_ip_address { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string customer_user_agent { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string created_via { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string customer_note { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object date_completed { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_paid { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string number { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<SubscriptionMetaData> meta_data { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<SubscriptionLineItem> line_items { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_created_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_modified_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object date_completed_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_paid_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string billing_period { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string billing_interval { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? start_date_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? trial_end_date_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? next_payment_date_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? last_payment_date_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? cancelled_date_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? end_date_gmt { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? resubscribed_from { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string resubscribed_subscription { get; set; }

    }
}
