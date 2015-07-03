@echo off

rem ############################################################
rem Build the main project.
call build.bat %1 %2
if not %ERRORLEVEL%==0 exit /B %ERRORLEVEL%

rem Reuse the build parameters (configuration, platform, etc.).

rem ############################################################
rem Use the correct NUnit runner.
set NUNIT_SUFFIX=
if "%BUILD_PLATFORM:"=%"=="x86" set NUNIT_SUFFIX=-x86
set "NUNIT_RUNNER=%~dp0packages\NUnit.Runners.2.6.4\tools\nunit-console%NUNIT_SUFFIX%.exe"

set OUTPUT_DIR=build\bin\%BUILD_CONFIG%
if "%BUILD_PLATFORM:"=%"=="x86" set OUTPUT_DIR=%OUTPUT_DIR%\%BUILD_PLATFORM%
if "%BUILD_PLATFORM:"=%"=="x64" set OUTPUT_DIR=%OUTPUT_DIR%\%BUILD_PLATFORM%

rem ############################################################
rem Build the unit tests.
msbuild src\test\repomonitor-core-tests.csproj %BUILD_PARAMS%
set COMPILE_EXIT=%ERRORLEVEL%
if not %COMPILE_EXIT%==0 exit /B %COMPILE_EXIT%

rem ############################################################
rem Run the unit tests.
"%NUNIT_RUNNER%" %OUTPUT_DIR%\repomonitor-core-tests.dll /nologo /noshadow /xml:%OUTPUT_DIR%\test-results.xml
set UNITTEST_EXIT=%ERRORLEVEL%
exit /B %UNITTEST_EXIT%
