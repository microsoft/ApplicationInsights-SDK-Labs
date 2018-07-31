@echo off
setlocal

xcopy /E /Y ".\Common\bin\release\netstandard2.0\*.*" ".\Host\bin\Release\netcoreapp2.1\publish\"
xcopy /E /Y ".\Library\bin\release\netstandard2.0\*.*" ".\Host\bin\Release\netcoreapp2.1\publish\"
xcopy    /Y ".\Host\bin\release\netcoreapp2.1\*.*" ".\Host\bin\release\netcoreapp2.1\publish\"

cd .\Host\bin\Release\netcoreapp2.1\publish\

dotnet Microsoft.LocalForwarder.Host.dll