using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using NRedisTimeSeries;
using StackExchange.Redis;
namespace RedMetrixProcessor
{
    public class DataServices
    {
        private  readonly IConfigurationRoot _config;

        public DataServices(IConfigurationRoot config)
        {
            _config = config;
        }
        
        public void ProcessData(IDatabase db)
        {
            string folderPath = _config.GetSection("FolderPath").Value;
            DirectoryInfo startDir = new DirectoryInfo(folderPath);
            var files = startDir.EnumerateFiles();
            AnsiConsole.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Default)
                .Start("[yellow]Initializing ...[/]", ctx =>
                {
                    foreach (var file in files)
                    {
                        ctx.Status("[bold blue]Started Processing..[/]");
                        HandleFile(file,db);
                        AnsiConsole.MarkupLine($"[grey]LOG:[/] Done[grey]...[/]");
                    }
                });

            // Done
            AnsiConsole.MarkupLine("[bold green]Finished[/]");
        }

        private static void HandleFile(FileInfo file,IDatabase db)
        {
               
               Console.WriteLine(file.FullName);

              using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
              using var sr = new StreamReader(fs, Encoding.UTF8);
               // int count = 0;
                //int total =0;
                
                string line = String.Empty;
                
                while ((line = sr.ReadLine()) != null)
                {
                    try{
                            
                        string pagetype="";          
                        using JsonDocument doc = JsonDocument.Parse(line);
                        if(doc.RootElement.GetProperty("event_detail").GetProperty("event_type").GetString()=="Page view")
                        {
                                //total++;
                                TSPageViews(db);
                                if(doc.RootElement.TryGetProperty("contexts", out var context))
                            {                           

                                if(context.TryGetProperty("page", out var cont_page))
                                {
                                    pagetype=cont_page.GetProperty("type").GetString();
                                    
                                    if (pagetype=="Success Confirmation")
                                        {
                                                                                    
                                        double orderamount=context.GetProperty("transaction").GetProperty("amount_to_pay").GetDouble();
                                        string paymentmethod=context.GetProperty("transaction").GetProperty("payment_method").GetString();
                                        
                                        TSOrders(orderamount,db);
                                        TSOrdersbypaymenttype(paymentmethod,db);
                                        TSFunnel("Success",db);
                                        
                                    }else if(pagetype=="Checkout"){
                                        TSFunnel("Checkout",db);
                                    }else if(pagetype=="Added To Cart"){
                                        TSFunnel("AddToCart",db); 
                                    }else if(pagetype=="View Cart"){
                                        TSFunnel("ViewCart",db);
                                    }else if(pagetype=="Product"){
                                        long pageperf=Convert.ToInt64(GetEndTime(context)-GetStartTime(context));
                                        TSPagePerformance("Product",pageperf,db);
                                        TSFunnel("ProductDetail",db);
                                    }else if(pagetype=="Wishlist"){
                                        TSFunnel("ProductList",db);
                                    }else if(pagetype=="Search"){
                                        TSFunnel("ProductList",db);
                                    }else if(pagetype=="Category"){
                                        TSFunnel("ProductList",db);
                                    }else if(pagetype=="Home"){
                                        long pageperf=Convert.ToInt64(GetEndTime(context)-GetStartTime(context));
                                        TSPagePerformance("Home",pageperf,db);
                                        TSFunnel("ProductList",db);
                                    }else{}  
                                }
                            }
                            
                        }
                        

                    }catch(Exception ex){
                       AnsiConsole.WriteException(ex);
                     Console.WriteLine(line); 
                     
                    }finally{
                       
                    }
                    
                }
           // Console.WriteLine($"{total}");
        }   
        
        private static void TSPageViews( IDatabase db)
        {
            db.TimeSeriesAdd("ts_pv:t", "*", 1);
        }

        private static void TSOrders( double orderamount, IDatabase db)
        {
           db.TimeSeriesIncrBy("ts_o:t", 1, timestamp: "*");
           db.TimeSeriesIncrBy("ts_o:v", orderamount, timestamp: "*");
        }
        
        private static void TSOrdersbypaymenttype( string paymentmethod, IDatabase db)
        {
           switch (paymentmethod)
                {
                    case "Cash On Delivery":
                       
                       db.TimeSeriesAdd("ts_o:t:cod", "*", 1);
                        break;
                    case "Debit Card":
                        
                        db.TimeSeriesAdd("ts_o:t:dc", "*", 1);
                        break;
                    case "Credit Card":
                        
                        db.TimeSeriesAdd("ts_o:t:cc", "*", 1);
                        break;
                    case "Netbanking":
                        
                        db.TimeSeriesAdd("ts_o:t:nb", "*", 1);
                        break;
                    case "Amazon Pay":
                        
                        db.TimeSeriesAdd("ts_o:t:ap", "*", 1);
                        break;
                    case "Google Pay":
                        
                        db.TimeSeriesAdd("ts_o:t:gp", "*", 1);
                        break;
                    default:
                         AnsiConsole.MarkupLine($"[red]{paymentmethod}[/]");
                        break;
                }
        }
        /*
        private static ulong GetStartTime( JsonElement context)
        {
            ulong starttime=context.GetProperty("performance_timing").GetProperty("redirectStart").GetUInt64();
            if (starttime==0){
                starttime=context.GetProperty("performance_timing").GetProperty("fetchStart").GetUInt64();    
            }
            if (starttime==0){
                starttime=context.GetProperty("performance_timing").GetProperty("requestStart").GetUInt64();    
            }
            return starttime;                           
        }
        private static ulong GetEndTime( JsonElement context)
        {
            ulong endtime=context.GetProperty("performance_timing").GetProperty("domContentLoadedEventEnd").GetUInt64();
            if (endtime==0){
                endtime=context.GetProperty("performance_timing").GetProperty("domInteractive").GetUInt64();    
            }
            if (endtime==0){
                endtime=context.GetProperty("performance_timing").GetProperty("domComplete").GetUInt64();    
            }
            if (endtime==0){
                endtime=context.GetProperty("performance_timing").GetProperty("loadEventEnd").GetUInt64();    
            }
            return endtime;    
        }
        */
        private static ulong GetStartTime( JsonElement context)
        {
            string s_starttime=context.GetProperty("performance_timing").GetProperty("requestStart").GetString();
            ulong starttime=Convert.ToUInt64(s_starttime);
            return starttime;                           
        }
        private static ulong GetEndTime( JsonElement context)
        {
            string s_endtime=context.GetProperty("performance_timing").GetProperty("domContentLoadedEventEnd").GetString();
            ulong endtime=Convert.ToUInt64(s_endtime);
            return endtime;    
        }

        private static void TSPagePerformance( string pagetype,long pageperf, IDatabase db)
        {
            if (pagetype=="Home"){
                db.TimeSeriesAdd("ts_pv:pp:h", "*", pageperf);
            }else if(pagetype=="Product"){
                db.TimeSeriesAdd("ts_pv:pp:p", "*", pageperf);
            }else{}
        }

        private static void TSFunnel( string funneltype,IDatabase db)
        {
            switch (funneltype)
                {
                    case "Success":
                       
                       db.TimeSeriesAdd("ts_fnl:sc", "*", 1);
                        break;
                    case "Checkout":
                        
                        db.TimeSeriesAdd("ts_fnl:co", "*", 1);
                        break;
                    case "ViewCart":
                        
                        db.TimeSeriesAdd("ts_fnl:vc", "*", 1);
                        break;
                    case "AddToCart":
                        
                        db.TimeSeriesAdd("ts_fnl:ac", "*", 1);
                        break;
                    case "ProductDetail":
                        
                        db.TimeSeriesAdd("ts_fnl:pd", "*", 1);
                        break;
                    case "ProductList":
                        
                        db.TimeSeriesAdd("ts_fnl:pl", "*", 1);
                        break;
                    default:
                         AnsiConsole.MarkupLine($"[red]{funneltype}[/]");
                        break;
                }
        }
    }
}