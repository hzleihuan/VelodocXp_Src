All required bootstrapper packages need to be copied to:
- C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\Bootstrapper\Packages (VS2005)
- C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bootstrapper\Packages (VS2008)

1) To create the NETFX30 Bootstrapper Package
------------------------------------------

Option A

Installing Visual Studio 2005 extensions for .NET Framework 3.0 (WCF & WPF), November 2006 CTP
http://www.microsoft.com/downloads/details.aspx?FamilyId=F54F5537-CC86-4BF5-AE44-F5A1E805680D&displaylang=en
creates a new bootstrapper package for .NET Framework 3.0 in C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\BootStrapper\Packages\NETFX30

As explained in http://msmvps.com/blogs/haarongonzalez/archive/2007/04/09/772757.aspx, you need to copy the following two files to the NETFX30 folder:
http://go.microsoft.com/fwlink/?LinkId=70848
http://go.microsoft.com/fwlink/?LinkId=70849

Option B

Copy the .NET 3.0 bootstrapper package from Visual Studio 2008 and Windows SDK 6.0A
No x64 support. 

2) To create the ASP.NET Ajax Extensions Bootstrapper Package
----------------------------------------------------------
Use the Bootstrapper manifest generator available at http://www.codeplex.com/bmg

3) To create the Memba.Update Bootstrapper Package
----------------------------------------------------------
Use the Bootstrapper manifest generator available at http://www.codeplex.com/bmg
