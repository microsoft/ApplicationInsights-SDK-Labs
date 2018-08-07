@echo off
setlocal

xcopy /E /Y ".\Common\bin\debug\netstandard2.0\*.*" ".\ConsoleHost\bin\debug\netcoreapp2.1\publish\"
xcopy /E /Y ".\Library\bin\debug\netstandard2.0\*.*" ".\ConsoleHost\bin\debug\netcoreapp2.1\publish\"
xcopy    /Y ".\ConsoleHost\bin\debug\netcoreapp2.1\*.*" ".\ConsoleHost\bin\debug\netcoreapp2.1\publish\"

cd .\ConsoleHost\bin\debug\netcoreapp2.1\publish\

dotnet Microsoft.LocalForwarder.ConsoleHost.dll