# Application Insights Aggregate Metrics SDK TelemetryModule #

>The Microsoft Application Insights API SDK enables developers to instrument their .NET application to track events and metrics.
> -- <cite>[Getting started with Application Insights](http://azure.microsoft.com/en-us/documentation/articles/app-insights-start-monitoring-app-health-usage/)</cite>

This project extends the Application Insights SDK API for .NET to support in-process aggregation of metrics. This enables the developer to both track a higher volume of metrics and reduces the actual volume of data sent over the network.


## Requirements ##

[Microsoft Application Insights SDK API](http://www.nuget.org/packages/Microsoft.ApplicationInsights/)

## Installation ##

To install the latest release you can add the [SDK Labs MyGet](https://www.myget.org/gallery/applicationinsights-sdk-labs) package source https://www.myget.org/F/applicationinsights-sdk-labs/api/v2 to NuGet and install the package id Microsoft.ApplicationInsights.AggregateMetrics.

Instructions for getting started with MyGet can be found at http://docs.myget.org/docs/walkthrough/getting-started-with-nuget.

```
> Install-Package Microsoft.ApplicationInsights.AggregateMetrics -IncludePrerelease
```

## Usage ##

Once installed, add a using for the AggregateMetrics namespace then you can use the TrackAggregateMetric extension method to track metrics you want automatically aggregated using your instance of TelemetryClient.

## Limitations ##

To ensure memory usage is minimized this extension enforces several limits in its usage.

- The metric name and properties have a maximum length of 16 characters.
- Each aggregated metric has a maximum of three properties.
- Each aggregated metric has a maximum of five property values.

###Add using for the Aggregate Metrics namespace###
```C#
using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
```

###Track a simple aggregate metric###
```C#
telemetryClient.TrackAggregateMetric("MyMetricName", 123.00);
```

###Track a simple aggregate metric with unnamed properties###
```C#
telemetryClient.TrackAggregateMetric("MyMetricName", 123.00, "My property");
```

###Register a metric to set names for properties###
```C#
telemetryClient.RegisterAggregateMetric("MyMetricName", "My property name");
telemetryClient.TrackAggregateMetric("MyMetricName", 123.00, "My property val");
```

###Register a metric and enable percentile calculation###
```C#
telemetryClient.RegisterAggregateMetric("MyMetricName", percentileCalculation: PercentileCalculation.OrderByLargest);
telemetryClient.TrackAggregateMetric("MyMetricName", 123.00, "My property val");
```