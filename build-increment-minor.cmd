@setlocal enableextensions
@cd /d "%~dp0"
call "%VS110COMNTOOLS%..\..\VC\vcvarsall.bat"

"%FrameworkDir%\%FrameworkVersion%\msbuild.exe" "%CD%\client\Errordite.Client.Build\Projects\BuildClient.proj" /logger:FileLogger,Microsoft.Build.Engine;BuildClient.log /p:Configuration=Release /p:Targets="Package" /p:SourcePath="%CD%" /p:Increment="minor" /p:Platform="Any Cpu" /p:Branch=trunk /m 
pause
if NOT %ERRORLEVEL% == 0 pause