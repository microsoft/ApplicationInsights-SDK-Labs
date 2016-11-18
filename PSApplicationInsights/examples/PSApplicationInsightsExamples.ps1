<#  
# old way
Add-Type -Path "microsoft.applicationinsights.2.0.0-rc1\lib\net45\Microsoft.ApplicationInsights.dll" 
$tc = New-Object -TypeName Microsoft.ApplicationInsights.TelemetryClient
$tc.InstrumentationKey = "iKey"
#>  

# PSApplicationInsights
$tc = New-TelemetryClient -InstrumentationKey "iKey"

# Events

$tc.TrackEvent('event')
$tc.TrackEvent('event', @{customProperty='PowerShell!'})
$tc.TrackEvent('event', @{customProperty='PowerShell!'}, @{customMetrics=3.0})
  
# Metrics

$tc.TrackMetric('metric', 1)
$tc.TrackMetric('metric', 1, @{customProperty='PowerShell!'})
$tc.TrackMetric('metric', 1, @{customProperty='PowerShell!'}, @{customMetrics=3.0})
  
# Traces
  
$tc.TrackTrace('trace')
$tc.TrackTrace('trace', 'Verbose')
$tc.TrackTrace('trace', 'Verbose', @{customProperty='PowerShell!'})
$tc.TrackTrace('trace', 'Verbose', @{customProperty='PowerShell!'}, @{customMetrics=3.0})
  
# Exceptions  

try{ 
  throw 'exception'
} 
catch { 
  $exception = $_.Exception

  $tc.TrackException($exception)
  $tc.TrackException($exception, @{customProperty='PowerShell!'})
  $tc.TrackException($exception, @{customProperty='PowerShell!'}, @{customMetrics=3.0})   
}

# Availability 

function Get-Availability
{
  $true # or custom logic / Invoke-RestMethod
}

$duration = Measure-Command {
  [bool] $isSuccess = Get-Availability
}

if ($isSuccess) {
  $tc.TrackAvailability('test',(Get-Date), $duration, 'run from powershell!', $isSuccess)
}
else {  
  $tc.TrackAvailability('test',(Get-Date), $duration, 'run from powershell!', $false, 'more detailed error message')
}

# Dependency

function Get-Dependency
{
  $true # or custom logic / Invoke-RestMethod
}

$duration = Measure-Command {
  [bool] $result = Get-Dependency
}

$tc.TrackDependency('dependency name', 'command name', (Get-Date), $duration, $result) 
$tc.TrackDependency('dependency type name', 'target', 'dependency name', 'data', (Get-Date), $duration, '200', $result)

# Page View
$tc.TrackPageView('page name')

# Request
$tc.TrackRequest('name', (Get-Date), $duration, '200', $true)

# Flush
$tc.Flush()