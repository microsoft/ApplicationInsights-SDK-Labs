@echo off
setlocal

xcopy /E /Y ".\Common\bin\release\netstandard2.0\*.*" ".\ConsoleHost\bin\Release\netcoreapp2.1\publish\"
xcopy /E /Y ".\Library\bin\release\netstandard2.0\*.*" ".\ConsoleHost\bin\Release\netcoreapp2.1\publish\"
xcopy    /Y ".\ConsoleHost\bin\release\netcoreapp2.1\*.*" ".\ConsoleHost\bin\release\netcoreapp2.1\publish\"

cd .\ConsoleHost\bin\Release\netcoreapp2.1\publish\

dotnet Microsoft.LocalForwarder.ConsoleHost.dll