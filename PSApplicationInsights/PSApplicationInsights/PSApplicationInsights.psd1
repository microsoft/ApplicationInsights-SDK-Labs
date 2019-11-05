@{
  # Root Module
  RootModule = 'PSApplicationInsights.psm1'

  # Version Number
  ModuleVersion = '1.0'

  # Unique Module ID
  GUID = '3ea2fc69-50e2-4c25-95ae-89164ef9b388'

  # Module Author
  Author = 'Antonio Blanco'

  # Company
  CompanyName = ''

  # Copyright
  Copyright = ''

  # Module Description
  Description = 'PowerShell wrapper for Microsoft Application Insights'

  # Minimum PowerShell Version Required
  PowerShellVersion = '2.0'

  # Name of Required PowerShell Host
  #PowerShellHostName = ''

  # Minimum Host Version Required
  #PowerShellHostVersion = ''

  # Minimum .NET Framework-Version
  DotNetFrameworkVersion = '4.5'

  # Minimum CLR (Common Language Runtime) Version
  #CLRVersion = ''

  # Processor Architecture Required (X86, Amd64, IA64)
  #ProcessorArchitecture = ''

  # Required Modules (will load before this module loads)
  RequiredModules = @()

  # Required Assemblies
  RequiredAssemblies = @()

  # PowerShell Scripts (.ps1) that need to be executed before this module loads
  ScriptsToProcess = @()

  # Type files (.ps1xml) that need to be loaded when this module loads
  TypesToProcess = @()

  # Format files (.ps1xml) that need to be loaded when this module loads
  FormatsToProcess = @()

  # 
  NestedModules = @()

  # List of exportable functions
  FunctionsToExport = @('New-TelemetryClient')

  # List of exportable cmdlets
  #CmdletsToExport = '*'

  # List of exportable variables
  #VariablesToExport = '*'

  # List of exportable aliases
  #AliasesToExport = '*'

  # List of all modules contained in this module
  #ModuleList = @()

  # List of all files contained in this module
  #FileList = @()

  # Private data that needs to be passed to this module
  #PrivateData = ''
}