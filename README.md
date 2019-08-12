# ApplicationInsights-SDK-Labs
Application Insights experimental projects repository. Note that Azure Monitor only provides support when using the [supported SDKs](https://docs.microsoft.com/en-us/azure/azure-monitor/app/platforms). We’re constantly assessing opportunities to expand our support for other languages, so follow our [GitHub Announcements](https://github.com/microsoft/ApplicationInsights-Announcements/issues) page to receive the latest SDK news.

# How to use
All NuGet packages from this repository being hosted in the SDK Labs [NuGet gallery](https://www.myget.org/gallery/applicationinsights-sdk-labs). 

## Install NuGet in Visual Studio

If you’re using **Visual Studio to manage NuGet** package in your project, here’s what to do:

1. Add the Application Insights SDK Labs package source to NuGet. Source: https://www.myget.org/F/applicationinsights-sdk-labs/nuget

  ![nuget](https://cloud.githubusercontent.com/assets/9950081/17712617/aafcd162-63ab-11e6-8997-02f6cb17d697.png)

2. Check "Include prerelease" as all packages are marked as pre-release
3. Find the NuGet you want to try out:

  ![020](https://cloud.githubusercontent.com/assets/9950081/17712674/dd3fd61a-63ab-11e6-91fe-28b1c5bd9e99.png) 

## Install NuGet in command line package manager

If you are using the **command line package manager** this is all you need:

```
> Install-Package "Microsoft.ApplicationInsights.DependencyCallstacks" -Source "https://www.myget.org/F/applicationinsights-sdk-labs/" -Pre
```
