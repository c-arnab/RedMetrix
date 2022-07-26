using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using NRedisTimeSeries;
using NRedisTimeSeries.DataTypes;
using NRedisTimeSeries.Commands;
using StackExchange.Redis;
namespace RedMetrixProcessor
{
    public class ConfigureInitializationServices
    {  
        private  readonly IConfigurationRoot _config;

        public ConfigureInitializationServices(IConfigurationRoot config)
        {
            _config = config;
        }

        public void DeleteKeys(IDatabase db)
        {         
            
            try{
                AnsiConsole.MarkupLine("[bold yellow]Deleting Keys[/]");
                db.KeyDelete("ts_pv:t"); //TimeSeries-PageView-Total
                db.KeyDelete("ts_o:t"); //TimeSeries-Orders-Total
                db.KeyDelete("ts_o:v"); //TimeSeries-Orders-TotalValue
                db.KeyDelete("ts_o:t:cod");//TimeSeries-Orders-Total-CashOnDelivery
                db.KeyDelete("ts_o:t:dc");//TimeSeries-Orders-Total-DebitCard
                db.KeyDelete("ts_o:t:cc");//TimeSeries-Orders-Total-CreditCard
                db.KeyDelete("ts_o:t:nb");//TimeSeries-Orders-Total-NetBanking
                db.KeyDelete("ts_o:t:ap");//TimeSeries-Orders-Total-AmazonPay
                db.KeyDelete("ts_o:t:gp");//TimeSeries-Orders-Total-GooglePay
                db.KeyDelete("ts_pv:pp:h");//TimeSeries-PageView-PagePerformance-Home
                db.KeyDelete("ts_pv:pp:p");//TimeSeries-PageView-PagePerformance-Product
                db.KeyDelete("ts_fnl:pl");//TimeSeries-Funnel-ProductListings
                db.KeyDelete("ts_fnl:pd");//TimeSeries-Funnel-ProductDetailView
                db.KeyDelete("ts_fnl:ac");//TimeSeries-Funnel-AddedToCart
                db.KeyDelete("ts_fnl:vc");//TimeSeries-Funnel-ViewCart
                db.KeyDelete("ts_fnl:co");//TimeSeries-Funnel-Checkout
                db.KeyDelete("ts_fnl:sc");//TimeSeries-Funnel-Success
            }catch(Exception ex){
                AnsiConsole.WriteException(ex, new ExceptionSettings
                            {
                                Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                                Style = new ExceptionStyle
                                {
                                    Exception = new Style().Foreground(Color.Grey),
                                    Message = new Style().Foreground(Color.White),
                                    NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                                    Parenthesis = new Style().Foreground(Color.Cornsilk1),
                                    Method = new Style().Foreground(Color.Red),
                                    ParameterName = new Style().Foreground(Color.Cornsilk1),
                                    ParameterType = new Style().Foreground(Color.Red),
                                    Path = new Style().Foreground(Color.Red),
                                    LineNumber = new Style().Foreground(Color.Cornsilk1),
                                }
                            });
            }
            AnsiConsole.MarkupLine("[bold green]Finished[/]"); 
        } 

        
        public void InitializeTimeSeriesTotalPageViews(IDatabase db)
        {
            try{
                AnsiConsole.MarkupLine("[bold yellow]TSPageViews[/]");
                db.TimeSeriesCreate("ts_pv:t", retentionTime: 600000);
                
                }catch(Exception ex){
                AnsiConsole.WriteException(ex, new ExceptionSettings
                            {
                                Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                                Style = new ExceptionStyle
                                {
                                    Exception = new Style().Foreground(Color.Grey),
                                    Message = new Style().Foreground(Color.White),
                                    NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                                    Parenthesis = new Style().Foreground(Color.Cornsilk1),
                                    Method = new Style().Foreground(Color.Red),
                                    ParameterName = new Style().Foreground(Color.Cornsilk1),
                                    ParameterType = new Style().Foreground(Color.Red),
                                    Path = new Style().Foreground(Color.Red),
                                    LineNumber = new Style().Foreground(Color.Cornsilk1),
                                }
                            });
            }
            AnsiConsole.MarkupLine("[bold green]Finished[/]"); 
        }

        public void InitializeTimeSeriesTotalOrderNValue(IDatabase db)
        {
            try{
                AnsiConsole.MarkupLine("[bold yellow]TSOrders[/]");
                db.TimeSeriesCreate("ts_o:t", retentionTime: 600000);
                ulong totalorders=0;
                // TODO: Get Data from Rest Api for total orders.
                totalorders=Convert.ToUInt64(_config.GetSection("InitializationOptions:TotalOrders").Value);
                db.TimeSeriesAdd("ts_o:t", "*", Convert.ToDouble(totalorders));  
                db.TimeSeriesCreate("ts_o:v", retentionTime: 600000);
                double totalordervalue=0;
                // TODO: Get Data from Rest Api for total order value
                totalordervalue=Convert.ToDouble(_config.GetSection("InitializationOptions:TotalOrderValue").Value);
                db.TimeSeriesAdd("ts_o:v", "*", totalordervalue);
                }catch(Exception ex){
                AnsiConsole.WriteException(ex, new ExceptionSettings
                            {
                                Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                                Style = new ExceptionStyle
                                {
                                    Exception = new Style().Foreground(Color.Grey),
                                    Message = new Style().Foreground(Color.White),
                                    NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                                    Parenthesis = new Style().Foreground(Color.Cornsilk1),
                                    Method = new Style().Foreground(Color.Red),
                                    ParameterName = new Style().Foreground(Color.Cornsilk1),
                                    ParameterType = new Style().Foreground(Color.Red),
                                    Path = new Style().Foreground(Color.Red),
                                    LineNumber = new Style().Foreground(Color.Cornsilk1),
                                }
                            });
            }
            AnsiConsole.MarkupLine("[bold green]Finished[/]"); 
        }
        
        public void InitializeTimeSeriesOrderByPaymentMethod(IDatabase db)
        {
            try{
                AnsiConsole.MarkupLine("[bold yellow]TSOrdersByPaymentMethod[/]");
                var label = new TimeSeriesLabel("chart", "Ordersbypaymenttype");
                var labels = new List<TimeSeriesLabel> { label };
                db.TimeSeriesCreate("ts_o:t:cod", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_o:t:cod", "*", 0);
                db.TimeSeriesCreate("ts_o:t:dc", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_o:t:dc", "*", 0);
                db.TimeSeriesCreate("ts_o:t:cc", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_o:t:cc", "*", 0);
                db.TimeSeriesCreate("ts_o:t:nb", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_o:t:nb", "*", 0);
                db.TimeSeriesCreate("ts_o:t:ap", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_o:t:ap", "*", 0);
                db.TimeSeriesCreate("ts_o:t:gp", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_o:t:gp", "*", 0);
                }catch(Exception ex){
                AnsiConsole.WriteException(ex, new ExceptionSettings
                            {
                                Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                                Style = new ExceptionStyle
                                {
                                    Exception = new Style().Foreground(Color.Grey),
                                    Message = new Style().Foreground(Color.White),
                                    NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                                    Parenthesis = new Style().Foreground(Color.Cornsilk1),
                                    Method = new Style().Foreground(Color.Red),
                                    ParameterName = new Style().Foreground(Color.Cornsilk1),
                                    ParameterType = new Style().Foreground(Color.Red),
                                    Path = new Style().Foreground(Color.Red),
                                    LineNumber = new Style().Foreground(Color.Cornsilk1),
                                }
                            });
            }
            AnsiConsole.MarkupLine("[bold green]Finished[/]"); 
        }
        public void InitializeTimeSeriesPagePerformance(IDatabase db)
        {
            try{
                AnsiConsole.MarkupLine("[bold yellow]TSPagePerformance[/]");
                db.TimeSeriesCreate("ts_pv:pp:h", retentionTime: 600000);
                db.TimeSeriesCreate("ts_pv:pp:p", retentionTime: 600000);
                
                }catch(Exception ex){
                AnsiConsole.WriteException(ex, new ExceptionSettings
                            {
                                Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                                Style = new ExceptionStyle
                                {
                                    Exception = new Style().Foreground(Color.Grey),
                                    Message = new Style().Foreground(Color.White),
                                    NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                                    Parenthesis = new Style().Foreground(Color.Cornsilk1),
                                    Method = new Style().Foreground(Color.Red),
                                    ParameterName = new Style().Foreground(Color.Cornsilk1),
                                    ParameterType = new Style().Foreground(Color.Red),
                                    Path = new Style().Foreground(Color.Red),
                                    LineNumber = new Style().Foreground(Color.Cornsilk1),
                                }
                            });
            }
            AnsiConsole.MarkupLine("[bold green]Finished[/]"); 
        }

        public void InitializeTimeSeriesFunnel(IDatabase db)
        {
            try{
                AnsiConsole.MarkupLine("[bold yellow]TSFunnel[/]");
                var label = new TimeSeriesLabel("chart", "Funnel");
                var labels = new List<TimeSeriesLabel> { label };
                db.TimeSeriesCreate("ts_fnl:pl", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_fnl:pl", "*", 0);
                db.TimeSeriesCreate("ts_fnl:pd", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_fnl:pd", "*", 0);
                db.TimeSeriesCreate("ts_fnl:ac", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_fnl:ac", "*", 0);
                db.TimeSeriesCreate("ts_fnl:vc", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_fnl:vc", "*", 0);
                db.TimeSeriesCreate("ts_fnl:co", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_fnl:co", "*", 0);
                db.TimeSeriesCreate("ts_fnl:sc", retentionTime: 600000, labels: labels);
                db.TimeSeriesAdd("ts_fnl:sc", "*", 0);
                }catch(Exception ex){
                AnsiConsole.WriteException(ex, new ExceptionSettings
                            {
                                Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                                Style = new ExceptionStyle
                                {
                                    Exception = new Style().Foreground(Color.Grey),
                                    Message = new Style().Foreground(Color.White),
                                    NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                                    Parenthesis = new Style().Foreground(Color.Cornsilk1),
                                    Method = new Style().Foreground(Color.Red),
                                    ParameterName = new Style().Foreground(Color.Cornsilk1),
                                    ParameterType = new Style().Foreground(Color.Red),
                                    Path = new Style().Foreground(Color.Red),
                                    LineNumber = new Style().Foreground(Color.Cornsilk1),
                                }
                            });
            }
            AnsiConsole.MarkupLine("[bold green]Finished[/]"); 
        }

    }
}