@echo off
setlocal

set home=%~dp0

set project=%home%\src\RaynMaker.Portfolio.Web
echo Updating node modules of %project%

cd %project%
npm install

endlocal
