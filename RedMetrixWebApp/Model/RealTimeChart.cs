using System;
using System.Collections.Generic;
namespace RedMetrixWebApp
{
    public class RealtimeChart
    {
        public PageView PageViews { get; set; }

        public long Orders { get; set; }

        public double OrderValue{ get; set; }

        public PagePerf PagePerformanceHome{ get; set; }
        public PagePerf PagePerformanceProduct{ get; set; }

        public Conversion Conversions{ get; set; }

        public List<PaymentMethodOrders> OrdersByPaymentMethod{ get; set; }

        public DateTime Updated { get; set; }
    }

    public class PageView   
    {
        public PageView(long pv, long prev_pv)
        {
            this.pv=pv;
            this.prev_pv=prev_pv;
        }
        public long pv { get; set; }
        public long prev_pv{ get; set; }
    }

    public class PagePerf
    {
        public List<string> time {get;set;}
        public List<List<int>> maxmin {get;set;}
        public List<int> avg{get;set;}
    }

    public class Conversion
    {
        public List<FunnelItem> FunnelItems{get;set;}
        public long TotalFunnelValue{get;set;}
       // public double ConversionRate{get;set;}
    }
    public class FunnelItem
    {
        public FunnelItem(int Order, string Item, long Value)
    {
        this.Order=Order;
        this.Item = Item;
        this.Value = Value;        
    }
       public int Order { get; set; }
       public string Item { get; set; }
       public long Value { get; set; }
      
    }

    public class PaymentMethodOrders
    {
        public PaymentMethodOrders(string PaymentMethod, long Orders)
        {
        this.PaymentMethod = PaymentMethod;
        this.Orders = Orders;
        }
       public string PaymentMethod { get; set; }
       public long Orders { get; set; }
    } 
}