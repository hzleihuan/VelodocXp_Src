REM Copy packages to VS 2005 development environment
REM -------------------------------------------------
cd /d %~dp0
curl -L -o .\NETFX30\dotnetfx3.exe http://go.microsoft.com/fwlink/?LinkId=70848
curl -L -o .\NETFX30\dotnetfx3_x64.exe.exe http://go.microsoft.com/fwlink/?LinkId=70849
xcopy /S /Y ".\ASPNETAjax\*.*" "%Program Files%\Microsoft Visual Studio 8\SDK\v2.0\Bootstrapper\Packages\ASPNETAjax\*.*"
xcopy /S /Y ".\Memba.Update\*.*" "%Program Files%\Microsoft Visual Studio 8\SDK\v2.0\Bootstrapper\Packages\Memba.Update\*.*"
xcopy /S /Y ".\NETFX30\*.*" "%Program Files%\Microsoft Visual Studio 8\SDK\v2.0\Bootstrapper\Packages\NETFX30\*.*"
