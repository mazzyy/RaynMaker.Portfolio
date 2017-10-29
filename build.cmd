@echo off
setlocal

call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat"

set home=%~dp0

echo Updating dependencies
cd %home%\src\RaynMaker.Portfolio.Html
call npm install

echo Building Solution
cd %home%
rmdir /s /q bin\Release
call msbuild /p:Configuration=Release /p:Platform="Any CPU" %home%\RaynMaker.Portfolio.sln /t:rebuild


endlocal
