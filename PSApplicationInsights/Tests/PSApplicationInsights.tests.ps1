Import-Module $PSScriptRoot\..\PSApplicationInsights -Force

Describe 'New-TelemetryClient' {

  Context 'Constructor'   {
    
    It 'does not throw' {
      { New-TelemetryClient -InstrumentationKey "iKey" } | Should Not Throw
    }

    It 'returns a new telemetry client'     {
      New-TelemetryClient -InstrumentationKey "iKey" | Should BeOfType Microsoft.ApplicationInsights.TelemetryClient     
    }

    It 'sets the instrumentation key'     {
      $sut = New-TelemetryClient -InstrumentationKey "iKey" 
      $sut.InstrumentationKey | Should Be "iKey"
    }
  }

  Context 'TrackEvent'   {
    BeforeEach {$sut = New-TelemetryClient -InstrumentationKey "iKey" }

    It 'tracks event with only event name' {
      { $sut.TrackEvent('event',@{customProperty='a'}) } | Should Not Throw
    }
    It 'tracks event with custom properties' {
      { $sut.TrackEvent('event',@{customProperty='a'}) } | Should Not Throw
    }
    It 'tracks event with custom properties and metrics' {
      { $sut.TrackEvent('event',@{customProperty='a'}, @{customMetric=3.0}) } | Should Not Throw
    }
  }

  Context 'TrackMetric' {
    BeforeEach {$sut = New-TelemetryClient -InstrumentationKey "iKey" }

    It 'tracks metric with only metric name' {
      { $sut.TrackMetric('metric',1,@{customProperty='a'}) } | Should Not Throw
    }
    It 'tracks metric with custom properties' {
      { $sut.TrackMetric('metric',1,@{customProperty='a'}) } | Should Not Throw
    }
    It 'tracks metric with custom properties and metrics' {
      { $sut.TrackMetric('metric',1,@{customProperty='a'}, @{customMetric=3.0}) } | Should Not Throw
    }
  }

  Context 'TrackTrace' {
    BeforeEach {$sut = New-TelemetryClient -InstrumentationKey "iKey" }

    It 'tracks traces ' {
      { $sut.TrackTrace('trace') } | Should Not Throw
    }

    It 'tracks traces with severity level' {
      { $sut.TrackTrace('trace', 'verbose') } | Should Not Throw
    }
    It 'tracks traces with severity level, and custom properties' {
      { 
        $sut.TrackTrace('trace', 'Verbose', @{customProperty='PowerShell!'}
        ) 
      } | Should Not Throw
    }
    It 'tracks traces with severity level, custom properties and custom metrics' {
      {   
        $sut.TrackTrace('trace', 
          'verbose', 
          @{'customProperty'='PowerShell!'}, 
          @{customMetrics=3.0}
        ) 
      } | Should Not Throw
    }

    It 'does not throw if it traces with invalid severity because core sdk never throws' {
      { $sut.TrackTrace('trace', 'invalid') } | Should Not Throw
    }
  }

  Context 'TrackException' {
    BeforeEach {
      $sut = New-TelemetryClient -InstrumentationKey "iKey" 
      try{ throw 'exception'} catch { $exception = $_.Exception}
    }

    It 'tracks exceptions' {
      { $sut.TrackException($exception) } | Should Not Throw
    }

    It 'tracks exceptions with custom properties' {
      {
        $sut.TrackException($exception, 
          @{customProperty='PowerShell!'}
        ) 
      } | Should Not Throw
    }
    It 'tracks exceptions with custom properties and custom metrics' {
      {
        $sut.TrackException($exception, 
          @{'customProperty'='PowerShell!'}, 
          @{customMetrics=3.0}
        ) 
      } | Should Not Throw
    }
  }
}