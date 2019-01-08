@echo off
set CONFIG=%1
if NOT "%CONFIG%" == "PROD" (
	if not "%CONFIG%" == "STAGE" (
		set errorlevel=7
		exit 7
	)
)

cd /d %~dp0
set WORKDIR=%CD%
echo WORKDIR=%WORKDIR%
if exist %WORKDIR%\UCSF.GlobalHealth.exe.config (
	del /q %WORKDIR%\UCSF.GlobalHealth.exe.config
)
copy %WORKDIR%\UCSF.GlobalHealth.exe_%CONFIG%.config %WORKDIR%\UCSF.GlobalHealth.exe.config

for /f "tokens=3,4" %%i in ('type %WORKDIR%\UCSF.GlobalHealth.exe.config^|findstr "connectionString"') do set NN=%%i%%j
for /f "tokens=1 delims=;" %%a in ('echo %NN:"=%') do (echo %%a)

for /f "tokens=2" %%i in ('date /T') do (set USEDATE=%%i)
for /f "tokens=1,2,3 delims=/" %%i in ('echo %USEDATE%') do (set DATEDIR=%%i%%j%%k)
for /f "tokens=1,2 delims= " %%i in ('time /T') do (
							set TM=%%i
							set ATM=%%j
  						  )
for /f "tokens=1,2 delims=:" %%i in ('echo %TM%') do (
							set HH=%%i
							set MM=%%j
		  				  )
set TIMESTAMP=%DATEDIR%%HH%%MM%%ATM%

set NEWLOGFILE=ucsfGH_%CONFIG%_%TIMESTAMP%.log
if exist %NEWLOGFILE% del %NEWLOGFILE%


set LOGFILE=%WORKDIR%\ucsf_global_health_%CONFIG%.log
if exist %LOGFILE% (
	move /y %LOGFILE% %NEWLOGFILE%
)
forfiles -p . -m  ucsfGH_%CONFIG%*.log -s  -d 30 -c "cmd /c del @path"
rem pause
%WORKDIR%\UCSF.GlobalHealth.exe
for /f "tokens=1" %%b in ('type %LOGFILE%^|findstr "System.Net.WebException:"') do (
	set RESULT=%%b
	set Result1=%%b
)
for /f %%b in ('type %LOGFILE%^|findstr "SqlClient.SqlException"') do (
	set RESULT=%%b
	set RESULT2=%%b
)
if not "%RESULT%" == "" (
	echo "Found %RESULT1% %RESULT2% %RERUN%. Check %LOGFILE%"
	exit 7
	
)