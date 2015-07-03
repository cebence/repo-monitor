@echo off
rem ############################################################
rem test.bat - Builds everything and runs unit tests.
rem ############################################################

rem ############################################################
rem Build the main project.
call %~dp0build.bat %1 %2
if not %ERRORLEVEL%==0 exit /B %ERRORLEVEL%

rem ############################################################
rem Build unit tests.
msbuild src\test\repomonitor-core-tests.csproj %BUILD_PARAMS%
set COMPILE_EXIT=%ERRORLEVEL%
if not %COMPILE_EXIT%==0 exit /B %COMPILE_EXIT%

rem ############################################################
rem Run unit tests.
call %~dp0run-tests.bat %1 %2
exit /B %ERRORLEVEL%
