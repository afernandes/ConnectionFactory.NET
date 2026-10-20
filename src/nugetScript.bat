@echo off
setlocal
pushd "%~dp0"
msbuild ConnectionFactory.sln /t:Restore /p:RestorePackagesConfig=true /p:Configuration=Release /nologo
if errorlevel 1 goto failure
msbuild ConnectionFactory.csproj /t:Rebuild /p:Configuration=Release /nologo
if errorlevel 1 goto failure
nuget pack ConnectionFactory.nuspec -Properties Configuration=Release
if errorlevel 1 goto failure
popd
exit /b 0
:failure
popd
exit /b 1
