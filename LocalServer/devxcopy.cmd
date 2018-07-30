@echo off
setlocal

xcopy /E /Y ".\Common\bin\release\netstandard2.0\*.*" ".\Host\bin\Release\netcoreapp2.0\publish\"
xcopy /E /Y ".\Library\bin\release\netstandard2.0\*.*" ".\Host\bin\Release\netcoreapp2.0\publish\"

cd .\Host\bin\Release\netcoreapp2.0\publish\

dotnet Microsoft.LocalForwarder.Host.dll