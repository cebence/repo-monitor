@echo off

rem Restore NuGet packages before the build.
nuget restore repomonitor-core.sln

rem Set build parameters.
set _CONFIG_=/p:Configuration=Debug
set _PLATFORM_=/p:Platform=x86

rem Use .NET Framework SDK 4.5 by default.
set TARGET_SDK=

rem But if there's 4.5.1 use that one instead.
if exist "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1" set TARGET_SDK=/p:TargetFrameworkVersion=v4.5.1

msbuild src\main\repomonitor-core.csproj %_CONFIG_% %_PLATFORM_% %TARGET_SDK%

rem if %ERRORLEVEL%==0 echo Okay!
