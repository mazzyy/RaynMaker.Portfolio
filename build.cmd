@echo off
setlocal

set home=%~dp0

set project=%home%\src\RaynMaker.Portfolio.Service
echo Updating node modules of %project%

cd %project%
npm install

set project=%home%\src\RaynMaker.Portfolio.Html
echo Updating node modules of %project%

cd %project%
npm install

cd %home%\src\RaynMaker.Portfolio.Html
npm run build


endlocal
