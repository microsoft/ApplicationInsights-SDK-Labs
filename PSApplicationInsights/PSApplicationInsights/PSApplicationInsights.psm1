function ConvertTo-Dictionary
{
  #requires -Version 2.0

  param (
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [hashtable]
    $InputObject,
    [Parameter(Mandatory = $true)]
    [Type]$ValueType 
  )

  process
  {
    $outputObject = New-Object -TypeName "System.Collections.Generic.Dictionary[[string],[$($ValueType.FullName)]]]"

    foreach ($entry in $InputObject.GetEnumerator())
    {
      $newKey = $entry.Key -as [string]
            
      if ($null -eq $newKey)
      {
        $message = 'Could not convert key "{0}" of type "{1}" to type "{2}"'-f
        $entry.Key,
        $entry.Key.GetType().FullName,
        [string].FullName

        Write-Warning -Message $message
      }
      elseif ($outputObject.ContainsKey($newKey))
      {
        Write-Warning -Message "Duplicate key `"$newKey`" detected in input object."
      }

      $outputObject.Add($newKey, $entry.Value)
    }

    Write-Output -InputObject $outputObject
  }
}

function New-TrackEventOverload
{
  param(
    [Parameter(Mandatory=$true)]
    $TrackEventDelegate
  )
 
  # When we add a script method, the scope becomes the current user scope instead of the module scope
  # Because of this, our private function ConvertTo-Dictionary is not available in the user scope. 
  
  # To avoid this, we can create a closure over the local variable which has the function definition  
  $convertToDictionaryFunc = ${function:ConvertTo-Dictionary}  
  return {
    param(
      [Parameter(Mandatory=$true)]
      [string] $EventName,
      [AllowNull()]
      [hashtable] $Properties, 
      [AllowNull()]
      [hashtable] $Metrics
    )

    $propertiesDictionary = $Properties | & $convertToDictionaryFunc -ValueType string
    $metricsDictionary = $Metrics | & $convertToDictionaryFunc -ValueType double
 
    $TrackEventDelegate.Invoke($EventName, $propertiesDictionary, $metricsDictionary)
  }.GetNewClosure()
}

function New-TrackMetricOverload
{
  param(
    [Parameter(Mandatory=$true)]
    $TrackMetricDelegate
  )
 
  # When we add a script method, the scope becomes the current user scope instead of the module scope
  # Because of this, our private function ConvertTo-Dictionary is not available in the user scope. 
  
  # To avoid this, we can create a closure over the local variable which has the function definition  
  $convertToDictionaryFunc = ${function:ConvertTo-Dictionary}  
  return {
    param(
      [Parameter(Mandatory=$true)]
      [string] $MetricName,
      [Parameter(Mandatory=$true)]
      [double] $Value,
      [AllowNull()]
      [hashtable] $Properties
    )
    $propertiesDictionary = $Properties | & $convertToDictionaryFunc -ValueType string

    $TrackMetricDelegate.Invoke($MetricName, $Value, $propertiesDictionary)
  }.GetNewClosure()
}

function New-TrackTraceOverload
{
  param(
    [Parameter(Mandatory=$true)]
    $TrackTraceDelegate
  )
 
  # When we add a script method, the scope becomes the current user scope instead of the module scope
  # Because of this, our private function ConvertTo-Dictionary is not available in the user scope. 
  
  # To avoid this, we can create a closure over the local variable which has the function definition  
  $convertToDictionaryFunc = ${function:ConvertTo-Dictionary}  
  return {
    param(
      [Parameter(Mandatory=$true)]
      [string] $Message,
      [AllowNull()]
      [string] $SeverityLevel, 
      [AllowNull()]
      [hashtable] $Properties
    )
    $propertiesDictionary = $Properties | & $convertToDictionaryFunc -ValueType string
      
    try {
      $level = [Microsoft.ApplicationInsights.DataContracts.SeverityLevel]($SeverityLevel)
      $TrackTraceDelegate.Invoke($Message, $level, $propertiesDictionary)
    } 
    catch {
      Write-Warning -Message "Application Insights: No valid Severity Level was specified, using the default instead. Valid severity levels are 'Verbose','Information','Warning','Error','Critical'"
     
      $TrackTraceDelegate.Invoke($Message, $propertiesDictionary)
    }
  }.GetNewClosure()
}

function New-TrackExceptionOverload
{
  param(
    [Parameter(Mandatory=$true)]
    $TrackExceptionDelegate
  )
 
  # When we add a script method, the scope becomes the current user scope instead of the module scope
  # Because of this, our private function ConvertTo-Dictionary is not available in the user scope. 
  
  # To avoid this, we can create a closure over the local variable which has the function definition  
  $convertToDictionaryFunc = ${function:ConvertTo-Dictionary}  
  return {
    param(
      [Parameter(Mandatory=$true)]
      [Exception] $Exception,
      [AllowNull()]
      [hashtable] $Properties, 
      [AllowNull()]
      [hashtable] $Metrics
    )
    $propertiesDictionary = $Properties | & $convertToDictionaryFunc -ValueType string
    $metricsDictionary = $Metrics | & $convertToDictionaryFunc -ValueType double
 
    $TrackExceptionDelegate.Invoke($Exception, $propertiesDictionary, $metricsDictionary)
  }.GetNewClosure()
}

function New-TelemetryClient
{
  <#
      .SYNOPSIS
      Creates a new instance of an Application Insights TelemetryClient

      .DESCRIPTION
      Creates a new instance of an Application Insights TelemetryClient that can be used in your PowerShell scripts.
      Includes PowerShell overloads for using Hashtables for adding custom properties and custom metrics. 
      Because this instance uses the InMemory channel, be sure to call the Flush() method to flush and send telemetry to App Insights.

      .PARAMETER InstrumentationKey
      The Application Insights Instrumentation Key

      .EXAMPLE
      Create a new telemetry client instance
      $ai = New-TelemetryClient -InstrumentationKey "iKey"

      .EXAMPLE
      Track a event with custom properties
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackEvent('event')

      .EXAMPLE
      Track a event with custom properties
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackEvent('event', @{customProperty='a'})

      .EXAMPLE
      Track a event with custom properties and custom metrics
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackEvent('event', @{customProperty='a'}, @{customMetric=3.0})

      .EXAMPLE
      Track a metric with custom properties
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackMetric('metric', 1)

      .EXAMPLE
      Track a metric with custom properties
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackMetric('metric', 1, @{customProperty='a'})

      .EXAMPLE
      Track a metric with custom properties and custom metrics
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackMetric('metric', 1, @{customProperty='a'}, @{customMetric=3.0})

      .EXAMPLE
      Track a trace 
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackTrace('trace')

      .EXAMPLE
      Track a trace with a severity level. Valid severity levels are 'Verbose','Information','Warning','Error','Critical'
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackTrace('trace', 'verbose')

      .EXAMPLE
      Track a trace with a severity level and custom properties
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      $ai.TrackTrace('trace', 'verbose', @{customProperty='a'})

      .EXAMPLE
      Track a exception 
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      try{ 
      throw 'exception'
      } 
      catch { 
      $exception = $_.Exception
      $tc.TrackException($exception)
      }

      .EXAMPLE
      Track a exception with custom properties
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      try{ 
      throw 'exception'
      } 
      catch { 
      $exception = $_.Exception
      $tc.TrackException($exception, @{customProperty='PowerShell!'})
      }

      .EXAMPLE
      Track a exception with custom properties and custom metrics
      $ai = New-TelemetryClient -InstrumentationKey "iKey"
      try{ 
      throw 'exception'
      } 
      catch { 
      $exception = $_.Exception
      $tc.TrackException($exception, @{'customProperty'='PowerShell!'}, @{customMetrics=3.0})   
      }

      .NOTES    
      Author: Antonio Blanco (anblanco@microsoft.com)

      .LINK
      https://docs.microsoft.com/en-us/azure/application-insights/

      .INPUTS
      [System.String]

      .OUTPUTS
      [Microsoft.ApplicationInsights.TelemetryClient]
  #>

  param(
    [Parameter(Mandatory=$true)]
    [string] $InstrumentationKey
  )

  Add-Type -Path "$PSScriptRoot\microsoft.applicationinsights.2.2.0-beta4\lib\net45\Microsoft.ApplicationInsights.dll" 
  $appInsights = New-Object -TypeName Microsoft.ApplicationInsights.TelemetryClient
  $appInsights.InstrumentationKey = $InstrumentationKey 

  #TrackEvent overload
  $trackEvent = New-TrackEventOverload -TrackEventDelegate $appInsights.TrackEvent
  $appInsights | Add-Member -MemberType ScriptMethod -Name TrackEvent -Value $trackEvent -Force

  #TrackMetric overload
  $trackMetric = New-TrackMetricOverload -TrackMetricDelegate $appInsights.TrackMetric
  $appInsights | Add-Member -MemberType ScriptMethod -Name TrackMetric -Value $trackMetric -Force

  #TrackTrace overload
  $trackTrace = New-TrackTraceOverload -TrackTraceDelegate $appInsights.TrackTrace
  $appInsights | Add-Member -MemberType ScriptMethod -Name TrackTrace -Value $trackTrace -Force

  #TrackException overload
  $trackException = New-TrackExceptionOverload -TrackExceptionDelegate $appInsights.TrackException
  $appInsights | Add-Member -MemberType ScriptMethod -Name TrackException -Value $trackException -Force

  Write-Output -InputObject $appInsights
}