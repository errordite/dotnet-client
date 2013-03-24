@setlocal enableextensions
@cd /d "%~dp0"
call "%VS110COMNTOOLS%..\..\VC\vcvarsall.bat"

"%FrameworkDir%\%FrameworkVersion%\msbuild.exe" "%CD%\client\Errordite.Client.Build\Projects\BuildClient.proj" /logger:FileLogger,Microsoft.Build.Engine;BuildClient.log /p:Configuration=Debug /p:Targets="Package" /p:Increment=none /p:SourcePath="%CD%" /p:Platform="Any Cpu" /p:Branch=trunk /m 

if NOT %ERRORLEVEL% == 0 pause