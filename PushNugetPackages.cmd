setlocal

set BINROOT=%TF_BUILD_BINARIESDIRECTORY%\Release
if not exist %BINROOT% echo "Error: '%BINROOT%' does not exist."&goto :eof

set NUGET=%TF_BUILD_SOURCESDIRECTORY%\NuGet.exe
if not exist %NUGET% echo "Error: '%NUGET%' does not exist."&goto :eof

set NUGET_GALLERY=https://www.myget.org/F/applicationinsights-sdk-labs/api/v2/package

for /r "%BINROOT%" %%P in (*.nupkg) do call :push %%P
goto :eof

:push 
set PACKAGE=%1
if %PACKAGE:.symbols.=% == %PACKAGE% (
    %NUGET% push "%PACKAGE%" -source %NUGET_GALLERY%
)
goto :eof

endlocal
