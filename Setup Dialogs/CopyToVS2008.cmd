REM Copy setup dialogs to VS 2008 development environment
REM -------------------------------------------------------
cd /d %~dp0 
copy ".\0\*.*" "%VS90COMNTOOLS%Deployment\VsdDialogs\0\*.*"
copy ".\1033\*.*" "%VS90COMNTOOLS%Deployment\VsdDialogs\1033\*.*"
pause