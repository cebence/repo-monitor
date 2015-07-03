@echo off
rem ############################################################
rem build.bat - Builds the main project.
rem ############################################################

call %~dp0init.bat %1 %2

rem ############################################################
rem Build the project.
msbuild src\main\repomonitor-core.csproj %BUILD_PARAMS%
set COMPILE_EXIT=%ERRORLEVEL%

exit /B %COMPILE_EXIT%
