# restore NuGet packages
. $PSScriptRoot\..\NuGet.exe install $PSScriptRoot\packages.config -OutputDirectory $PSScriptRoot\PSApplicationInsights\

# run tests
Import-Module Pester
$testResults = Invoke-Pester -Script $PSScriptRoot\Tests\PSApplicationInsights.Tests.ps1 -PassThru

if ($testResults.FailedCount -gt 0)
{
  throw "BUILD FAILED: There were $($testResults.FailedCount ) failed tests... stopping"
}

# deploy?