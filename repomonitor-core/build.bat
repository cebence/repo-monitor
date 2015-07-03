@echo off
rem ############################################################
rem build.bat - Builds the main project and its unit tests.
rem ############################################################

call %~dp0init.bat %1 %2

rem ############################################################
rem Build the project.
msbuild repomonitor-core.sln %BUILD_PARAMS%
set COMPILE_EXIT=%ERRORLEVEL%

exit /B %COMPILE_EXIT%
