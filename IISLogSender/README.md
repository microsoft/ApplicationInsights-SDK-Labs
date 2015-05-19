# IISLogSender #

>The Microsoft Application Insights API SDK enables developers to instrument their .NET application to track events and metrics.
> -- <cite>[Getting started with Application Insights](http://azure.microsoft.com/en-us/documentation/articles/app-insights-start-monitoring-app-health-usage/)</cite>

This project uses the Application Insights SDK to parse IIS log files and send their contents as RequestTelemetry items to Application Insights.


## Requirements ##

[Microsoft Application Insights SDK API](http://www.nuget.org/packages/Microsoft.ApplicationInsights/)

## Usage ##

Execute IISLogSender.exe <IIS Log Directory>
E.g. IISLogSender.exe C:\Inetpub\Logs

## Limitations ##

Requests will only be processed if they are within the last 48 hours from UTC time.