Copy setup dialogs
to %ProgramFiles%\Microsoft Visual Studio 8\Common7\Tools\Deployment\VsdDialogs (VS 2005)
or %ProgramFiles%\Microsoft Visual Studio 9.0\Common7\Tools\Deployment\VsdDialogs (VS 2008)

To understand how setup dialogs work, go to:
- http://msdn2.microsoft.com/en-us/library/aa289522(VS.71).aspx
- http://www.codeproject.com/install/vsSetupCustomDialogs.asp
- http://www.codeproject.com/install/setupprjpwd.asp

The MSI/Windows Installer documentation is available at
- http://msdn2.microsoft.com/en-us/library/aa372392.aspx

To download Orca and edit custom dialogs,go to:
- http://support.microsoft.com/kb/255905

If you get the following message:
---------------------------------
No 'PublicKey' or 'Hash' attribute specified for file 'NETFX30\Dotnetfx3_x64.exe' in item '.NET Framework 3.0'.
you need to download .NET framework 3.0 from http://www.microsoft.com/downloads/details.aspx?FamilyId=10CC340B-F857-4A14-83F5-25634C3BF043
and copy redistributable packages for both x86 (Dotnetfx3.exe) and x64 (Dotnetfx3_x64.exe)
to %ProgramFiles%\Microsoft Visual Studio 8\SDK\v2.0\BootStrapper\Packages\NETFX30 (VS 2005)
or %programFiles%\Microosft SDKs\Windows\v6.0A\Bootstrapper\Packages\NETFX30 (VS 2008)
