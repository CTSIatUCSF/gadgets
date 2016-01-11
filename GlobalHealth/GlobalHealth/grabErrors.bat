@echo off
set /a RERUN=0
set TXTRERUN="0"
cd %~dp0
set WORKDIR=%CD%
echo WORKDIR=%WORKDIR%
if exist %WORKDIR%\rerun.txt (
	FOR /f "tokens=1" %%a IN ('type %WORKDIR%\rerun.txt') DO ( SET TXTRERUN=%%a)
)
set /a RERUN=%TXTRERUN%
echo RERUN=%RERUN%
echo TXTRERUN=%TXTRERUN%
set RESULT=
set RESULT1=
set RESULT2=
set ERRBODY=
set RET=
set HAVEBODY=
set HAVEIDS=
set HAVENOIDS=
set ERRHEAD="No EmployeeID in following projects:"
echo "" > %WORKDIR%\test.txt
set LOGFILE=%WORKDIR%\ucsf_global_health.log
for /f "tokens=1" %%b in ('type %LOGFILE%^|findstr "System.Net.WebException:"') do (
	set RESULT=%%b
	set Result1=%%b
)
for /f %%b in ('type %LOGFILE%^|findstr "SqlClient.SqlException"') do (
	set RESULT=%%b
	set RESULT2=%%b
)
for /f "tokens=8-13" %%a in ('type %LOGFILE%^|findstr "Person"^|findstr "was"^|findstr "not"^|findstr "found"') do (
	echo %%a %%b %%c %%d %%e %%f  >> %WORKDIR%\test.txt
)

rem echo on
if not "%RESULT%" == "" (
	echo "Running one more
	echo "Found %RESULT1% %RESULT2% RERUNNING %RERUN%. Check %LOGFILE%"
	if %RERUN%  LSS 3 (
		call :ONEMORE %RERUN%
		goto :EOF
	)
	exit 7
	
)
if exist %WORKDIR%\rerun.txt (
	del /q %WORKDIR%\rerun.txt
)
for /f "tokens=2" %%a in ('type %LOGFILE%^|findstr "ERROR"') do set ERRBODY=" %%a No EmployeeID in following Global Health projects:"
set /A SYMALL=0
set /A LINESIZE=0
set /A LINESET=10
set /A PARTNUM=10
set ERRMESS=""
for /f "tokens=12" %%a in ('type ucsf_global_health.log^|findstr "ERROR"') do call :ADDBODY "%%a"
if NOT "%ERRMESS%" == "" set ERRMESS_%PARTNUM%=%ERRMESS%



echo %ERRHEAD:"=%
for /L %%i in (10,1,%PARTNUM%) do call :SHOWPART %%i
echo symbols=%SYMALL%
call :WRONGIDs
echo "MSGBODY=%ERRBODY%
call :PREPARESQL
goto :EOF

:SHOWPART
set CURPART=%1
for /F "delims== tokens=2" %%a in ('set ERRMESS_%CURPART%') do ( 
	set RET="%%a,"
)
echo "ERRBODY=%ERRBODY%"
echo "RET=%RET%"
if not "$%HAVEBODY%$" == "$$" ( 
	set ERRBODY="%ERRBODY:"=% '+CHAR(13)+CHAR(10)+'%RET:"=%" 
) else ( 
	set ERRBODY='%RET:"=%"
	set HAVEBODY=YES
)
echo "there is ERRBODY==============="
echo "%ERRBODY%"
echo "===end ERRBODY"
goto :EOF

:WRONGIDs
for /F "tokens=*" %%a in ('type %WORKDIR%\test.txt') do (
echo "%%a"
call :ADDIDs "%%a"
)
goto :EOF

:ADDIDs
rem echo on
set NEWLINE="%1"
rem echo NEWLINE="%NEWLINE:"=%"
rem echo ERRBODY="$%ERRBODY:"=%$"
IF "%HAVEIDS%" =="" (
	set ERRBODY="%ERRBODY:"=%' +CHAR(13)+CHAR(10)+'Bad Employee IDs in following projects:"
	set HAVEIDS=YES
)
IF "%HAVEBODY%" == "" (
	set ERRBODY="+CHAR(13)+CHAR(10)+%NEWLINE:"=%"
	set HAVEBODY=YES 
) ELSE ( 
	set ERRBODY="%ERRBODY:"=% '+CHAR(13)+CHAR(10)+'%NEWLINE:"=%"
	set /a SYMALL=%SYMALL%+5 
)
set ERRBODY="%ERRBODY%"
rem echo "on EXIT HAVEBODY=%HAVEBODY%"
rem echo "on EXIT ERRBODY=%ERRBODY%"
rem echo off
goto :EOF

:PREPARESQL
set WARNMES='%ERRHEAD:"=%'+CHAR(13)+CHAR(10)+%ERRBODY:,,=%
FOR %%? IN (%WORKDIR%\test.txt) DO ( SET /A SYMALL=%%~z? - 2 +%SYMALL%+50)
echo Declare @Subject varchar(255)=@@SERVERNAME+' Global Health Warning!' >sendGHWarning.sql 
echo Declare @WarnMsg varchar(%SYMALL%) = %WARNMES:"=%'>>sendGHWarning.sql
echo EXEC msdb.dbo.sp_send_dbmail    @profile_name = 'moisey',>>sendGHWarning.sql
echo                                 @recipients = 'moisey.gruzman@ucsf.edu;gruzmanm@pacbell.net',>>sendGHWarning.sql
echo                                 @subject = @Subject,>>sendGHWarning.sql
echo                                 @body = @WarnMsg>>sendGHWarning.sql
rem sqlcmd -S STAGE-SQL-CTSI -i %WORKDIR%\sendGHWarning.sql 
rem -o %WORKDIR%\sendGHWarning.log
for /f %%a in ('hostname') do set HOSTNAME=%%a
sqlcmd -S %HOSTNAME% -i %WORKDIR%\sendGHWarning.sql 
rem -o %WORKDIR%\sendGHWarning.log
goto :EOF
 :ADDBODY
if %LINESIZE% EQU %LINESET% call :NEWPART
set TMP=%1
set ID=%TMP:~4%,
set ERRMESS=%ERRMESS%%ID:"=%
rem echo %ERRMESS%
set /A LINESIZE=%LINESIZE%+1
set /A SYMALL=%SYMALL%+10
goto :EOF
:NEWPART
set ERRMESS_%PARTNUM%=%ERRMESS%
set /A LINESIZE=0
set ERRMESS=
set /A PARTNUM=%PARTNUM%+1
goto :EOF

:ONEMORE
echo PARMA=%1
set /a RERUN=%RERUN%+1
echo RERUN=%RERUN%
echo "%RERUN%">%WORKDIR%\rerun.txt
start /separate %WORKDIR%\globalhealth.bat RERUN
goto :EOF