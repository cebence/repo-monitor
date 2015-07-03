@echo off
rem ############################################################
rem code-coverage.bat - Performs code coverage analysis.
rem ############################################################

call %~dp0init.bat %1 %2

rem ############################################################
rem Run the code coverage tool.
"%OPEN_COVER%" -target:run-tests.bat -register:user -filter:%COVERAGE_FILTER% -output:%COVERAGE_XML%
set COVERAGE_EXIT=%ERRORLEVEL%
if not %COVERAGE_EXIT%==0 exit /B %COVERAGE_EXIT%

rem ############################################################
rem Generate user-friendly coverage reports.
"%REPORT_GEN%" -reports:%COVERAGE_XML% -targetdir:%COVERAGE_REPORT%
