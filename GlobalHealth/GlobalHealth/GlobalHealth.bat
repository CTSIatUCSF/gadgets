@echo off
set RERUN=%1
set WORKDIR=%~dp0
echo WORKDIR=%WORKDIR%
if "%RERUN%" == "" (
	del /q %WORKDIR%\rerun.txt
)
echo RERUN=%RERUN%
set LOGFILE=%WORKDIR%ucsf_global_health.log
for /f "tokens=*" %%i in ('date /t') do set CURDATE=%%i
echo "CURDATE=" %CURDATE%
rem goto :EOF 
if not exist %LOGFILE%.Archive (
	echo "This is Archive started %CURDATE%"
	move /y %LOGFILE% %LOGFILE%.Archive
)
if exist %LOGFILE% (
	type %LOGFILE% >> %LOGFILE%.Archive
	del %LOGFILE%
)
%WORKDIR%\UCSF.GlobalHealth.exe
%WORKDIR%\grabErrors.bat