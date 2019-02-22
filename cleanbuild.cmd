cd /d %~dp0 

attrib -R -H -S *.scc /S
del *.scc /S

attrib -R -H -S *.vssscc /S
del *.vssscc /S

attrib -R -H -S *.vspscc /S
del *.vspscc /S

attrib -R -H -S *.vsmdi /S
del *.vsmdi /S

attrib -R -H -S *.suo /S
del *.suo /S

attrib -R -H -S *.csproj.user /S
del *.csproj.user /S

REM Assemblies

REM Documentation

cd Memba.Dns
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.Dns.Tests
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.FileDownload
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.Files
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.FileUpload
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.Install
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.PurgeService
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.Tests
rd /S /Q bin
rd /S /Q obj
cd..

cd Memba.WebControls
rd /S /Q bin
rd /S /Q obj
cd..

cd Packages
cd NETFX30
attrib -R dotnetfx3.exe
del dotnetfx3.exe
attrib -R dotnetfx3_x64.exe
del dotnetfx3_x64.exe
cd..
cd..

REM Setup Dialogs

cd XP
cd Bin
del /S /Q *.*
cd..
cd..

cd XP_deploy
rd /S /Q AssemblyInfo
rd /S /Q Debug
rd /S /Q Release
rd /S /Q Source
del ResolveAssemblyReference.cache
cd..

cd XP_Setup
rd /S /Q Debug
rd /S /Q Release
cd..

pause