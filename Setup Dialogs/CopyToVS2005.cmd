REM Copy setup dialogs to VS 2005 development environment
REM -------------------------------------------------------
cd /d %~dp0 
copy /Y ".\0\*.*" "%VS80COMNTOOLS%Deployment\VsdDialogs\0\*.*"
copy /Y ".\1033\*.*" "%VS80COMNTOOLS%Deployment\VsdDialogs\1033\*.*"
pause