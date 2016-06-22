# Application Insights Aggregate Metrics SDK TelemetryModule #

>The Microsoft Application Insights API SDK enables developers to instrument their .NET application to track events and metrics.
> -- <cite>[Getting started with Application Insights](http://azure.microsoft.com/en-us/documentation/articles/app-insights-start-monitoring-app-health-usage/)</cite>

This project extends the Application Insights SDK API for .NET to support in-process aggregation of metrics. This enables the developer to both track a higher volume of metrics and reduces the actual volume of data sent over the network.

There are two different implementation of the metrics API. Those APIs optimized for the different usage patterns. The reason to keep both is to choose which one to integrate into Application Insights core.

## Requirements ##

[Microsoft Application Insights SDK API](http://www.nuget.org/packages/Microsoft.ApplicationInsights/)

## Installation ##

To install the latest release you can add the [SDK Labs MyGet](https://www.myget.org/gallery/applicationinsights-sdk-labs) package source https://www.myget.org/F/applicationinsights-sdk-labs/api/v2 to NuGet and install the package id Microsoft.ApplicationInsights.AggregateMetrics.

Instructions for getting started with MyGet can be found at http://docs.myget.org/docs/walkthrough/getting-started-with-nuget.

```
> Install-Package Microsoft.ApplicationInsights.AggregateMetrics -IncludePrerelease
```

## Usage - first API ##

Once installed, add a using for the `AggregateMetrics.One` namespace then you can use the `TrackAggregateMetric` extension method to track metrics you want automatically aggregated using your instance of `TelemetryClient`.

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

## Usage - second API ##

This API implements the number of metric types with the names consistent with the metric types from [Metrics.NET](https://github.com/etishor/Metrics.NET/wiki/Available-Metrics) library.

Once installed, add a using for the `AggregateMetrics.Two` namespace. You can than use different types of metrics like.

### Gauges (IGauge interface)

Gauges is a simple metric type that takes the value from the delegate. It can be used to track the value of performance counter or queue size. Use it like this:

``` csharp
TelemetryClient client = new TelemetryClient();

client.Gauge("Queue length", () => { return queue.Length; });
```

### Counters (ICounter interface)

Counter represents an integer value that can be incremented or decremented. You can use this metric type to count the number of worker threads or some business metric.

``` csharp
TelemetryClient client = new TelemetryClient();

var simpleCounter = client.Counter("# of active worker threads");

simpleCounter.Increment();
simpleCounter.Decrement();
simpleCounter.Increment(2);
simpleCounter.Decrement(2);
```

### Meters (IMeter interface)

Meter represents the metric that measurese the rate at which an event occurs. You can use meter to count failed requests per second metric.

``` charp
TelemetryClient client = new TelemetryClient();

var simpleMeter = client.Meter("Failed requests");

simpleMeter.Mark();
```


### Metrics custom properties and context

Metrics API allows to create an automatically-aggregated metrics. Metrics will be sent as a regular `MetricTelemetry` by the end of the interval by `AggregateMetricsTelemetryModule`. Telemetry module `AggregateMetricsTelemetryModule` sends metrics using the standard `TrackMetric` call to `TelemetryClient`. So all telemetry initializers will run for the resulting metric telemetry object.

You can also assign metric-specific custom properties to the metric by generating it from the dedicated telemetry client.

``` charp
TelemetryClient client = new TelemetryClient();
client.Context.Properties["metric property"] = "metric value";

var simpleMeter = client.Meter("Failed requests");
simpleMeter.Mark();
```

`MetricTelemetry` object generated by this code will have a custom property "metric property" set to "metric value".

### Implementation details

All metric types implements `ICounterValue` interface. Metrics stored in `TelemetryConfiguration` so you may have separate sets of metrics.