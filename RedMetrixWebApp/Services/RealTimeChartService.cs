using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using dotnetredis.Providers;
using NRedisTimeSeries;
using NRedisTimeSeries.Commands;
using NRedisTimeSeries.Commands.Enums;
using NRedisTimeSeries.DataTypes;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;


namespace RedMetrixWebApp
{
    public class RealtimeChartService
    {
        private readonly ILogger<RealtimeChartService> _logger;
        private readonly RedisProvider _redisProvider;

        public RealtimeChartService(ILogger<RealtimeChartService> logger, RedisProvider redisProvider)
        {
            _logger = logger;
            _redisProvider = redisProvider;
        }
        
        public RealtimeChart GetChartData()
        {
            return new RealtimeChart()
            {
                PageViews=GetPageViews(),
                Orders = GetOrders(),
                OrderValue = GetOrderValue(),
                PagePerformanceHome=GetPagePerformance("Home"),
                PagePerformanceProduct=GetPagePerformance("Product"),
                Conversions=GetConversions(),
                OrdersByPaymentMethod=GetOrdersByPaymentMethod(),
                Updated= DateTime.Now.ToLocalTime()

            };
        }
        

        public PageView GetPageViews()
        {
            PageView views = new PageView(0,0);
            try{
                var db = _redisProvider.Database();
                IReadOnlyList<TimeSeriesTuple> results =  db.TimeSeriesRevRange("ts_pv:t", "-", "+", aggregation: TsAggregation.Sum, timeBucket: 60000, count:2); 
            
              views= new PageView(Convert.ToInt64(results[0].Val), Convert.ToInt64(results[1].Val));
                
            }catch(Exception ex){
                _logger.LogError(ex.Message);
            }
            return views;
        }

        public double GetOrderValue()
        {
            double orderamount = 0;
            try{
                var db = _redisProvider.Database();
                TimeSeriesTuple value = db.TimeSeriesGet("ts_o:v");
                orderamount = value.Val;
            }catch(Exception ex){
                _logger.LogError(ex.Message);
            }
            return orderamount;
        }

        public long GetOrders()
        {
            long orders = 0;
            try{
                var db = _redisProvider.Database();
                TimeSeriesTuple value = db.TimeSeriesGet("ts_o:t");
                orders=Convert.ToInt64(value.Val);
            }catch(Exception ex){
                _logger.LogError(ex.Message);
            }
            return orders;
        }
        public PagePerf GetPagePerformance(string pagetype){
            string key="";
            if (pagetype=="Home")
            {
                key="ts_pv:pp:h";
            }else if(pagetype=="Product")
            {
                key="ts_pv:pp:p";
            }else{}
            List<int> avgdata=new List<int>();
            List<string> timedata= new List<string>();
            List<List<int>> maxmindata= new List<List<int>>();
            try{
                var db = _redisProvider.Database();
               
                IReadOnlyList<TimeSeriesTuple> resultsmax =  db.TimeSeriesRange(key, "-", "+", aggregation: TsAggregation.Max, timeBucket: 60000, count:10);
                IReadOnlyList<TimeSeriesTuple> resultsmin =  db.TimeSeriesRange(key, "-", "+", aggregation: TsAggregation.Min, timeBucket: 60000, count:10);
                IReadOnlyList<TimeSeriesTuple> resultsavg =  db.TimeSeriesRange(key, "-", "+", aggregation: TsAggregation.Avg, timeBucket: 60000, count:10);
                maxmindata=GetMaxMinList(resultsmax,resultsmin);
                foreach (var result in resultsavg)
                {
                  avgdata.Add(Convert.ToInt32(result.Val));
                }
                timedata=GetStringTimeList(resultsavg);
               
            }catch(Exception ex){
                _logger.LogError(ex.Message);
            }
            return new PagePerf
            {
                time=timedata,
                maxmin=maxmindata,
                avg=avgdata
            };
        }

        private List<List<int>> GetMaxMinList(IReadOnlyList<TimeSeriesTuple> resultsmax,IReadOnlyList<TimeSeriesTuple> resultsmin)
        {
            return resultsmax.Concat(resultsmin)
                                .GroupBy(o => o.Time)
                                .Select(g => g.Select(s => (int)s.Val).ToList())
                                .ToList();
        }

        private List<string> GetStringTimeList(IReadOnlyList<TimeSeriesTuple> resultsavg)
        {
             List<string> timedata= new List<string>();
             foreach (var result in resultsavg)
                {
                   TimeStamp ts = result.Time;
                   System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                   dtDateTime = dtDateTime.AddMilliseconds((long)ts);
                   String hourMinute = dtDateTime.ToString("HH:mm");
                   timedata.Add(hourMinute);
                }
                return timedata;
        }

        public Conversion GetConversions(){
            List<FunnelItem> funnelItems = new List<FunnelItem>();
            long totalFunnelValue =0;
            try{
                var db = _redisProvider.Database();
                var filter = new List<string> { "chart=Funnel" };
               
               var results= db.TimeSeriesMRevRange("-", "+", filter, aggregation:TsAggregation.Sum, timeBucket:600000, count: 1);
              
                foreach (var result in results)
                {
                    string key = result.key;
                    IReadOnlyList<TimeSeriesTuple> values = result.values;
                    funnelItems.Add(new FunnelItem(GetFunnelOrder(key),PrettyFunnelItem(key),Convert.ToInt64(values[0].Val)));
                    totalFunnelValue=totalFunnelValue+Convert.ToInt64(values[0].Val);
                }
            }catch(Exception ex){
                _logger.LogError(ex.Message);
            }
            return new Conversion
            {
                FunnelItems=funnelItems,
                TotalFunnelValue=totalFunnelValue
            };
        }

        private int GetFunnelOrder(string key){
           switch (key)
                {
                    case "ts_fnl:sc":
                        return 6;
                    case "ts_fnl:co":
                        return 5;
                    case "ts_fnl:vc":
                        return 4;
                    case "ts_fnl:ac":
                        return 3;
                    case "ts_fnl:pd":
                        return 2;
                    case "ts_fnl:pl":
                        return 1;
                    default:
                        _logger.LogInformation(key);
                    break;
                } 
            return 0;
        }
        private string PrettyFunnelItem(string key){
           switch (key)
                {
                    case "ts_fnl:sc":
                        return "Transaction Success";
                    case "ts_fnl:co":
                        return "Checkout";
                    case "ts_fnl:vc":
                        return "View Cart";
                    case "ts_fnl:ac":
                        return "Add To Cart";
                    case "ts_fnl:pd":
                        return "Product Detail";
                    case "ts_fnl:pl":
                        return "Product Listings";
                    default:
                        _logger.LogInformation(key);
                    break;
                } 
            return "";
        }

        public List<PaymentMethodOrders> GetOrdersByPaymentMethod(){
            List<PaymentMethodOrders> OrdersByPaymentMethod = new List<PaymentMethodOrders>();
            try{
                var db = _redisProvider.Database();
                var filter = new List<string> { "chart=Ordersbypaymenttype" };
               
               var results= db.TimeSeriesMRevRange("-", "+", filter, aggregation:TsAggregation.Sum, timeBucket:600000, count: 1);
            //      string jsonString = JsonSerializer.Serialize(results);
            //   _logger.LogInformation(jsonString);
                foreach (var result in results)
                {
                    string key = result.key;
                   IReadOnlyList<TimeSeriesTuple> values = result.values;
                    OrdersByPaymentMethod.Add(new PaymentMethodOrders(PrettyPaymentMethod(key),Convert.ToInt64(values[0].Val)));
                }
            }catch(Exception ex){
                _logger.LogError(ex.Message);
            }

            return OrdersByPaymentMethod;
        }

        private string PrettyPaymentMethod(string key){
           switch (key)
                {
                    case "ts_o:t:cod":
                        return "Cash On Delivery";
                    case "ts_o:t:dc":
                        return "Debit Card";
                    case "ts_o:t:cc":
                        return "Credit Card";
                    case "ts_o:t:nb":
                        return "Net Banking";
                    case "ts_o:t:ap":
                        return "Amazon Pay";
                    case "ts_o:t:gp":
                        return "Google Pay";
                    default:
                        _logger.LogInformation(key);
                    break;
                } 
            return "";
        }
    }    

}