Installation with the MSI Installer in a user environment
===========================================================
Unzip VelodocXP.1.0.0.yymmdd.zip into a directory and run setup.exe.
Note that yymmdd is the year, month and day of the build.
Follow the detailed instructions in http://velodocxp.googlecode.com/files/Installation%20Guide.v1.1.docx

Build 090116
=========================
- Fix VLXP-618: Code regression with resuming downloads 
- Fix VLXP-619: Precondition error triggered in download handler
http://www.codeplex.com/velodocaddin/Thread/View.aspx?ThreadId=40975

Build 081107
=========================
- Enhancement VLXP-603: Optionally add the sender to the list of recipients in blind copy (configured in appSettings) 
- Enhancement VLXP-598: Add a custom "X-Forward-DSN-To" header to forward delivery status notifications (DSNs) using an SMTP event sink
Note: This enhancement requires that you manually deploy the SMTP Event Sink provided with Velodoc XP Edition
(assuming you use IIS SMTP Server or Exchange) or that you configure a rule to forward DSNs to the email address in X-Forward-DSN-To
- Enhancement VLXP-597: Add an optional read receipt to email notifications (configured in appSettings)
- Enhancement VLXP-580: Allow SMTP authentication using TLS (configured in appSettings)
http://www.codeplex.com/VelodocXP/WorkItem/View.aspx?WorkItemId=2991

Build 080926
=========================
- Enhancement VLXP-596: Send email notifications from an unattended system mailbox configured in web.config
- Enhancement VLXP-592: Display better error messages especially when mail settings are not properly configured
- Enhancement VLXP-575: Check DNS MX records to validate email addresses
http://www.codeplex.com/VelodocXP/WorkItem/View.aspx?WorkItemId=2691
- Fix VLXP-594: Issue with parsing of mime parts where 1 byte is missing if the last byte of the buffer is the last byte of a multipart boundary 
- Fix VLXP-590: Error accessing a .cs file in C:\Windows\Temp
http://www.codeplex.com/VelodocXP/WorkItem/View.aspx?WorkItemId=2248
- Fix VLXP-576: Issue with terms page which is displayed with very small characters
- Fix VLXP-572: Setup fails on Windows Server 2003 domain controllers
http://www.codeplex.com/VelodocXP/WorkItem/View.aspx?WorkItemId=2692

Initial public build of Version 1.1
======================================
Not Applicable


Installation of the source code in the development environment
================================================================
Assuming you have already installed Visual Studio 2005 SP1 (possibly with the update for Vista), you will also need to install:

- Visual Studio 2005 extensions for .NET Framework 3.0 (WCF & WPF), November 2006 CTP
http://www.download.com/The-Visual-Studio-2005-extensions-for-NET-Framework-3-0-WCF-WPF-November-2006-CTP/3000-10253_4-10727672.html
If you have already installed .NET framework 3.0 SP1, you may want to read http://jlchereau.blogspot.com/2008/03/installing-visual-studio-extensions-for.html

- Visual Studio 2005 Web Deployment Project
http://msdn.microsoft.com/en-us/asp.net/aa336619.aspx

- ASP.NET 2.0 Ajax Extensions 1.0
http://www.microsoft.com/downloads/details.aspx?FamilyID=ca9d90fa-e8c9-42e3-aa19-08e2c027f5d6&displaylang=en

Finally you will need to deploy
- setup dialogs, and
- packages,
using the batch files included in the solution and according to the corresponding readme.txt files.
Do not forget to download and copy the .NET framework installers to NETFX30
since we have removed them from the packaged delivery due to their size.

Once this is completed, you should be able to compile the application and setup program without error.
For more information, see the installation guide and developer tutorial.