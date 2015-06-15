@echo off

call build.bat

set _CONFIG_=/p:Configuration=Debug
set _RUN_NUNIT_=%~dp0packages\NUnit.Runners.2.6.4\tools\nunit-console-x86.exe

rem Use .NET Framework SDK 4.5 by default.
set TARGET_SDK=

rem But if there's 4.5.1 use that one instead.
if exist "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1" set TARGET_SDK=/p:TargetFrameworkVersion=v4.5.1

rem Build the unit tests.
msbuild src\test\repomonitor-core-tests.csproj %_CONFIG_% %_PLATFORM_% %TARGET_SDK%

if not %ERRORLEVEL%==0 goto SkipTests


rem Run the unit tests.
%_RUN_NUNIT_% build\bin\Debug\x86\repomonitor-core-tests.dll -noresult

:SkipTests
