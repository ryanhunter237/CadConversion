@echo off
setlocal
set "script=C:\Users\ryanh\Git\CadConversion\bin\Debug\net8.0-windows\CadConversion.exe"
set "input=C:\Users\ryanh\Documents\cad\data\office-chair"
set "outdir=C:\Users\ryanh\Git\CadConversion\test\output"
"%script%" -input "%input%" -outdir "%outdir%"
endlocal