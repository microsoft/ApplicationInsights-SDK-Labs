@echo off
setlocal

xcopy /E /Y ".\Common\bin\debug\netstandard2.0\*.*" ".\Host\bin\debug\netcoreapp2.1\publish\"
xcopy /E /Y ".\Library\bin\debug\netstandard2.0\*.*" ".\Host\bin\debug\netcoreapp2.1\publish\"
xcopy    /Y ".\Host\bin\debug\netcoreapp2.1\*.*" ".\Host\bin\debug\netcoreapp2.1\publish\"

cd .\Host\bin\debug\netcoreapp2.1\publish\

dotnet Microsoft.LocalForwarder.Host.dll