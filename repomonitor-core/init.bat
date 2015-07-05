@echo off
rem ############################################################
rem init.bat - Initializes the build parameters (configuration, platform).
rem ############################################################

rem ############################################################
rem Constants, defaults, etc.
set "DOTNET_451_SDK=C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1"

rem ############################################################
rem Set configuration to debug, but allow the user to override it.
set OK_CONFIGS=(Debug Release)
set DEFAULT_CONFIG=Debug
set BUILD_CONFIG=
for %%G in %OK_CONFIGS% do (if /i "%~1"=="%%G" set BUILD_CONFIG=%%G)
if not defined BUILD_CONFIG set BUILD_CONFIG=%DEFAULT_CONFIG%
echo Build configuration: %BUILD_CONFIG%

set BUILD_PARAMS=/p:Configuration=%BUILD_CONFIG%

rem ############################################################
rem Set platform to AnyCPU, but allow the user to override it.
set OK_PLATFORMS=(AnyCPU x86 x64)
set DEFAULT_PLATFORM=AnyCPU
set BUILD_PLATFORM=
for %%G in %OK_PLATFORMS% do (if /i "%~2"=="%%G" set BUILD_PLATFORM=%%G)
if not defined BUILD_PLATFORM set BUILD_PLATFORM=%DEFAULT_PLATFORM%
if defined BUILD_PLATFORM set "BUILD_PARAMS=%BUILD_PARAMS% /p:Platform=%BUILD_PLATFORM%"

rem Use .NET Framework SDK 4.5 by default, but if 4.5.1 is available use it.
if exist "%DOTNET_451_SDK%" set "BUILD_PARAMS=%BUILD_PARAMS% /p:TargetFrameworkVersion=v4.5.1"

rem Limit the amount of information on the console.
set "BUILD_PARAMS=%BUILD_PARAMS% /v:minimal"

set OUTPUT_DIR=build\bin\%BUILD_CONFIG%
if "%BUILD_PLATFORM:"=%"=="x86" set OUTPUT_DIR=%OUTPUT_DIR%\%BUILD_PLATFORM%
if "%BUILD_PLATFORM:"=%"=="x64" set OUTPUT_DIR=%OUTPUT_DIR%\%BUILD_PLATFORM%
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

rem Direct MSBuild to save the build log to OUTPUT_DIR.
set "BUILD_REPORT=%OUTPUT_DIR%\build-log.txt"
set "BUILD_PARAMS=%BUILD_PARAMS% /fl /flp:logfile=%BUILD_REPORT%;verbosity:minimal"

rem ############################################################
rem Use the correct NUnit runner.
set NUNIT_SUFFIX=
if "%BUILD_PLATFORM:"=%"=="x86" set NUNIT_SUFFIX=-x86
set "NUNIT_RUNNER=%~dp0packages\NUnit.Runners.2.6.4\tools\nunit-console%NUNIT_SUFFIX%.exe"
set "TEST_REPORT=%OUTPUT_DIR%\test-results.xml"

rem ############################################################
rem Code coverage tool setup.
set "OPEN_COVER=%~dp0packages\OpenCover.4.5.3723\OpenCover.Console.exe"
set COVERAGE_FILTER="+[repomonitor*]* -[*-tests]*"
set "COVERAGE_XML=%OUTPUT_DIR%\coverage-results.xml"
set "REPORT_GEN=%~dp0packages\ReportGenerator.2.1.7.0\tools\ReportGenerator.exe"
set "COVERAGE_REPORT=%OUTPUT_DIR%\coverage-report"
