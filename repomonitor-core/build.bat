@echo off

rem ############################################################
rem Constants, defaults, etc.
set "DOTNET_451_SDK=C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1"

rem ############################################################
rem Restore NuGet packages before the build, if it fails skip the build.
nuget restore repomonitor-core.sln
set RESOLVE_EXIT=%ERRORLEVEL%
if not %RESOLVE_EXIT%==0 exit /B %RESOLVE_EXIT%

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

rem ############################################################
rem Build the project.
msbuild src\main\repomonitor-core.csproj %BUILD_PARAMS%
set COMPILE_EXIT=%ERRORLEVEL%

exit /B %COMPILE_EXIT%
