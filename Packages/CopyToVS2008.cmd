REM Copy packages to VS 2008 development environment
REM -------------------------------------------------
cd /d %~dp0
curl -L -o .\NETFX30\dotnetfx3.exe http://go.microsoft.com/fwlink/?LinkId=70848
curl -L -o .\NETFX30\dotnetfx3_x64.exe http://go.microsoft.com/fwlink/?LinkId=70849
xcopy /S /Y ".\ASPNETAjax\*.*" "%Program Files%\Microsoft SDKs\Windows\v6.0A\Bootstrapper\Packages\ASPNETAjax\*.*"
xcopy /S /Y ".\Memba.Update\*.*" "%Program Files%\Microsoft SDKs\Windows\v6.0A\Bootstrapper\Packages\Memba.Update\*.*"
xcopy /S /Y ".\NETFX30\*.*" "%Program Files%\Microsoft SDKs\Windows\v6.0A\Bootstrapper\Packages\NETFX30\*.*"

