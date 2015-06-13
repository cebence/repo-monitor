@echo off

set _CONFIG_=Debug
set _PLATFORM_=x86

rem Use .NET Framework SDK 4.5 by default.
set TARGET_SDK=

rem But if there's 4.5.1 use that one instead.
if exist "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1" set TARGET_SDK=/p:TargetFrameworkVersion=v4.5.1

msbuild src\main\repomonitor-core.csproj /p:Configuration=%_CONFIG_% /p:Platform=%_PLATFORM_% %TARGET_SDK%

if %ERRORLEVEL%==0 echo Okay!
