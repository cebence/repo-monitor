@echo off
rem ############################################################
rem run-tests.bat - Unit tests runner for OpenCover.
rem ############################################################

rem If not initialized do it, otherwise reuse the variables.
if not defined BUILD_PARAMS call %~dp0init.bat %1 %2

rem ############################################################
rem Run unit tests.
"%NUNIT_RUNNER%" %OUTPUT_DIR%\repomonitor-core-tests.dll /nologo /noshadow /xml:%TEST_REPORT%
set UNITTEST_EXIT=%ERRORLEVEL%
exit /B %UNITTEST_EXIT%
