/*
 * Copyright © 2005-2008 Memba SA. All rights reserved.
 * 
 * This file is part of Velodoc XP Edition.
 * 
 * Velodoc XP Edition is open-source software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 2 of the License, or (at your option) any later version.
 * 
 * Velodoc XP Edition is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with Velodoc XP Edition.
 * If not, see <http://www.gnu.org/licenses/>.
 * 
 * For more information, please contact Memba at <http://www.memba.com>.
 * You can find more information about Velodoc at <http://www.velodoc.com> and <http://www.velodoc.com/help>.
 * You can try Velodoc online at <http://www.velodoc.net>.
 *
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;

using System.Collections; //IDictionary
using System.IO; //FileInfo, DirectoryInfo
using System.Security.AccessControl; //DirectorySecurity
using System.Security.Principal; //SecurityIdentifier, NTAccount
using System.DirectoryServices; //DirectoryEntry
using System.Xml; //XmlDocument, XmlElement, XmlNode
using System.Windows.Forms; //MessageBox
using System.Diagnostics; //Debug, Process
using Microsoft.Win32; //Registry, RegistryKey

namespace Memba.Install
{
    [RunInstaller(true)]
    public partial class WebInstaller : Installer
    {       
        #region Constants
        private const string W3SVC = "IIS://localhost/W3SVC";
        private const string METABASEROOT = "/LM/W3SVC/";
        private const string BINDINGS = "ServerBindings";
        private const int DEFAULTSITEID = 1; //1 is the identifier for the default web site
        private const string DEFAULTVDIR = "VelodocXP";

        //Registry keys and names //Should match keys and names in VelodocLicenseProvider
        private const string REGVELODOCKEY = "SOFTWARE\\Memba\\Velodoc\\XP Edition";
        //private const string REGLICKEY = "LicenseKey";
        private const string REGLICUSR = "LicensedUser";
        private const string REGLICORG = "LicensedOrganization";
        private const string REGMAJVER = "MajorVersion"; //DWord
        private const string REGMINVER = "MinorVersion"; //DWord
        private const string REGSITEID = "SiteId"; //DWord
        private const string REGVDIR = "VirtualDir";

        //The following constants are defined in the configuration of the custom action in the web setup project
        //This requires a CustomActionData property valued
        // /LICUSR="[USERNAME]" /LICORG="[COMPANYNAME]" /STRDIR="[STORAGEDIR]\" /SITE="[TARGETSITE]" /VDIR="[TARGETVDIR]" /WEBSITEDIR="[TARGETDIR]\" /HOST="[SMTPHOST]" /FROM="[SMTPFROM]" /USER="[SMTPUSER]" /PWD="[SMTPPWD]" /LIST="[USERLIST]" /CODE="[USERCODE]"
        //private const string LICKEY = "LICKEY"; //License key
        private const string LICUSR = "LICUSR"; //Licensee (contact)
        private const string LICORG = "LICORG"; //Licensee (company)
        private const string STRDIR = "STRDIR"; //Storage folder where uploaded files are stored
        private const string SITE = "SITE"; //The site reported by the installer in the form /LM/W3SVC/1
        //private const string PORT = "PORT"; //Http port of web server, generally 80
        //The installer is supposed to return TARGETPORT as explained in http://msdn2.microsoft.com/en-us/library/aa289522(VS.71).aspx
        //But it does not work. TARGETSITE works.
        private const string VDIR = "VDIR"; //Virtual directory mapped to WEBSITEDIR
        private const string WEBDIR = "WEBDIR"; //Installation path of web app where web.config and 404.htm are
        private const string HOST = "HOST"; //Address of smtp server, port is 25
        private const string FROM = "FROM"; //Sender of email notifications
        private const string USER = "USER"; //Smtp user name
        private const string PWD = "PWD"; //Smtp password
        private const string LIST = "LIST"; //List of email addresses separated by semi-columns
        private const string CODE = "CODE"; //Security code
        private const string SVCDIR = "SVCDIR"; //Installation path of purge service where app.config (Memba.PurgeService.XP.exe.config) is

        //Message boxes
        private const string DEBUGINFO = "Debug Info";
        private const string INSTALLERR = "Install Error";
        private const string UNINSTALLERR = "Uninstall Error";
        #endregion

        #region Contructor
        /// <summary>
        /// Constructor
        /// </summary>
        public WebInstaller()
        {
            InitializeComponent();
        }
        #endregion

        #region Installer Members
        /// <summary>
        /// Performs the installation
        /// </summary>
        /// <param name="stateSaver"></param>
        /// <exception cref="InstallException">We throw an InstallException which will trigger a rollback if any step fails</exception>
        public override void Install(IDictionary stateSaver)
        {
            try
            {
#if DEBUG
                //To debug an installer class:
                // 1) Edit the properties of the project containing the installer class
                // 2) On the Debug tab, set the start action to start external program and designate
                //      C:\Windows\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe
                // 3) On the Debug tab too, in the start options, set teh command line arguments to
                //      <AssemblyName>.dll /LogToConsole=true
                //      Note: you can find teh assembly name on the Application tab
                // 4) Set the project as startup project and start debugging (F5) in Visual Studio
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
#endif

                this.Context.LogMessage(this.GetType().Name + ": Install started");
                base.Install(stateSaver);

                #region Custom Action Data
                string sSite = this.Context.Parameters[SITE];
                if (String.IsNullOrEmpty(sSite))
                    sSite = METABASEROOT + DEFAULTSITEID; //default web site;
#if DEBUG
                MessageBox.Show("Site: " + sSite + (sSite.Equals(METABASEROOT + DEFAULTSITEID) ? " (Default)" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                
                string sVirtualDir = this.Context.Parameters[VDIR];
                if (String.IsNullOrEmpty(sVirtualDir))
                    sVirtualDir = DEFAULTVDIR; //default data
#if DEBUG
                MessageBox.Show("VirtualDir: " + sVirtualDir + (sVirtualDir.Equals(DEFAULTVDIR) ? " (Default)" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                /*
                string sPort = this.Context.Parameters[PORT];
                if (String.IsNullOrEmpty(sPort))
                    sPort = "80"; //default data
#if DEBUG
                MessageBox.Show("Port: " + sPort + (sPort.Equals("80") ? " (Default)" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                */
                #endregion

                #region Add registry info
                try
                {
                    //string sLicenseKey = this.Context.Parameters[LICKEY];
                    string sLicensedUser = this.Context.Parameters[LICUSR];
                    string sLicensedOrganisation = this.Context.Parameters[LICORG];
                    System.Diagnostics.Debug.Assert(sSite.StartsWith(METABASEROOT));
                    string sServerNum = sSite.Substring(METABASEROOT.Length);
                    int iServerNum;
                    bool bTry = Int32.TryParse(sServerNum, out iServerNum);
                    System.Diagnostics.Debug.Assert(bTry);

#if DEBUG
                    MessageBox.Show(REGLICUSR + ": " + sLicensedUser + "\n" + REGLICORG + ": " + sLicensedOrganisation, DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                    this.Context.LogMessage(this.GetType().Name + ": Adding registry info");
                    //WebInstaller.AddValueToRegistryKey(REGLICKEY, sLicenseKey);
                    WebInstaller.AddValueToRegistryKey(REGLICUSR, sLicensedUser);
                    WebInstaller.AddValueToRegistryKey(REGLICORG, sLicensedOrganisation);
                    WebInstaller.AddValueToRegistryKey(REGMAJVER, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major);
                    WebInstaller.AddValueToRegistryKey(REGMINVER, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor);
                    WebInstaller.AddValueToRegistryKey(REGSITEID, iServerNum);
                    WebInstaller.AddValueToRegistryKey(REGVDIR, sVirtualDir);
                    this.Context.LogMessage(this.GetType().Name + ": Info added to registry");
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot add info to registry";

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Add required permissions to storage directory
                string sStorageDir = this.Context.Parameters[STRDIR];
#if DEBUG
                MessageBox.Show("StorageDir: " + sStorageDir + (String.IsNullOrEmpty(sStorageDir) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sStorageDir))
                    sStorageDir = "C:\\STORAGE"; //default data
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Adding permissions to " + sStorageDir);
                    WebInstaller.AddStoragePermissions(sStorageDir);
                    //C:\Windows\Temp is used by .NET Framework to generate serialization classes
                    //and on Windows XP and Windows Server 2003, neither ASPNET nor IIS_WPG have permissions
                    //Note that this issue is solved on Vista and Windows Server 2008
                    WebInstaller.AddStoragePermissions(Environment.ExpandEnvironmentVariables("%windir%\\Temp"));
                    this.Context.LogMessage(this.GetType().Name + ": Permissions added to " + sStorageDir);
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot add permissions to " + sStorageDir;

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Map .dat files to aspnet_isapi.dll
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Mapping .dat extension to apnet_isapi.dll on " + sVirtualDir);
                    WebInstaller.MapDatExtensionToAspNet(sVirtualDir, sSite);
                    this.Context.LogMessage(this.GetType().Name + ": .dat extensions mapped to aspnet_isapi.dll on " + sVirtualDir);
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot map .dat extension to apnet_isapi.dll on " + sVirtualDir;

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Change 404 error document
                string sWebInstallDir = this.Context.Parameters[WEBDIR];
#if DEBUG
                MessageBox.Show("Web install dir: " + sWebInstallDir + (String.IsNullOrEmpty(sWebInstallDir) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sWebInstallDir))
                    sWebInstallDir = Environment.GetEnvironmentVariable("SystemDrive") + "\\InetPub\\wwwroot\\" + sVirtualDir; //default data
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Changing Http error 404 page on " + sVirtualDir);
                    WebInstaller.Switch404Page(sVirtualDir, sSite, sWebInstallDir);
                    this.Context.LogMessage(this.GetType().Name + ": Http error 404 page changed on " + sVirtualDir);
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot change Http error 404 page on " + sVirtualDir;

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }

                #endregion

                #region Update storage in web.config
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Updating fileStorage in " + Path.Combine(sWebInstallDir, "web.config"));
                    WebInstaller.UpdateStorageConfig(sWebInstallDir, sStorageDir);
                    this.Context.LogMessage(this.GetType().Name + ": fileStorage updated in " + Path.Combine(sWebInstallDir, "web.config"));
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot update fileStorage in " + Path.Combine(sWebInstallDir, "web.config");

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }

                #endregion

                #region Update smtp config in web.config
                string sSmtpHost = this.Context.Parameters[HOST];
#if DEBUG
                MessageBox.Show("Smtp host: " + sSmtpHost + (String.IsNullOrEmpty(sSmtpHost) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sSmtpHost))
                    sSmtpHost = "smtp.acme.com"; //default data
                string sSmtpPort = "25";
                string sFrom = this.Context.Parameters[FROM];
#if DEBUG
                MessageBox.Show("From: " + sFrom + (String.IsNullOrEmpty(sFrom) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sFrom))
                    sFrom = "noreply@acme.com"; //default data
                bool bDefaultCredentials = false;
                string sUserName = this.Context.Parameters[USER];
#if DEBUG
                MessageBox.Show("User name: " + sUserName + (String.IsNullOrEmpty(sUserName) ? "- Using defaultCredentials = true" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sUserName))
                    bDefaultCredentials = true; //default credentials
                string sPassword = this.Context.Parameters[PWD];
#if DEBUG
                MessageBox.Show("Password: " + sPassword + (String.IsNullOrEmpty(sPassword) ? "- Using defaultCredentials = true" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sPassword))
                    bDefaultCredentials = true; //default credentials
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Updating smtp configuration in " + Path.Combine(sWebInstallDir, "web.config"));
                    WebInstaller.UpdateSmtpConfig(sWebInstallDir, sSmtpHost, sSmtpPort, sFrom, sUserName, sPassword, bDefaultCredentials);
                    this.Context.LogMessage(this.GetType().Name + ": Smtp configuration updated in " + Path.Combine(sWebInstallDir, "web.config"));
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot update smtp configuration in " + Path.Combine(sWebInstallDir, "web.config");

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Update appSettings in web.config
                string sUserList = this.Context.Parameters[LIST];
#if DEBUG
                MessageBox.Show("User list: " + sUserList + (String.IsNullOrEmpty(sUserList) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sUserList))
                    sUserList = "me@acme.com;you@acme.com"; //default data
                string sSecurityCode = this.Context.Parameters[CODE];
#if DEBUG
                MessageBox.Show("Security code: " + sSecurityCode + (String.IsNullOrEmpty(sSecurityCode) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sSecurityCode))
                    sSecurityCode = "1234"; //default data
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Updating user list and security code in " + Path.Combine(sWebInstallDir, "web.config"));
                    WebInstaller.UpdateWebConfig(sWebInstallDir, sUserList, sSecurityCode);
                    this.Context.LogMessage(this.GetType().Name + ": User list and security code updated in " + Path.Combine(sWebInstallDir, "web.config"));
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot update user list and security code in " + Path.Combine(sWebInstallDir, "web.config");

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Update appSettings in app.config (Memba.PurgeService.XP.exe.config)
                string sSvcInstallDir = this.Context.Parameters[SVCDIR];
#if DEBUG
                MessageBox.Show("Service install dir: " + sSvcInstallDir + (String.IsNullOrEmpty(sSvcInstallDir) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sSvcInstallDir))
                    sSvcInstallDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Memba\\Velodoc\\XP Edition"); //default data
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Updating storage directory in " + Path.Combine(sSvcInstallDir, "Memba.PurgeService.XP.exe.config"));
                    WebInstaller.UpdateAppConfig(sSvcInstallDir, sStorageDir);
                    this.Context.LogMessage(this.GetType().Name + ": Storage directory updated in " + Path.Combine(sSvcInstallDir, "Memba.PurgeService.XP.exe.config"));
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot update storage directory in " + Path.Combine(sSvcInstallDir, "Memba.PurgeService.XP.exe.config");

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Add shortcuts
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Adding shortcuts");
                    WebInstaller.AddShortcuts(sSite, sVirtualDir, Path.Combine(sWebInstallDir, "favicon.ico"));
                    this.Context.LogMessage(this.GetType().Name + ": Shortcuts added");
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot add shortcuts";

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    //throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Register ASP.NET 2.0
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Registering ASP.NET 2.0 on virtual directory");
                    WebInstaller.RegisterASPNET2(sVirtualDir, sSite);
                    this.Context.LogMessage(this.GetType().Name + ": ASP.NET 2.0 registered on virtual directory");
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot register ASP.NET 2.0 on virtual directory. Please launch aspnet_regiis.exe manually.";

                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                    //throw new InstallException(sMessage, Ex); //Do not fail
                }
                #endregion
            }
            finally
            {
                #region Install Feedback
                try
                {
#if !DEBUG
                    Process.Start("IExplore.exe", "http://www.velodoc.com/install.aspx?p=xp");
#endif
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot open install feedback page";
#if DEBUG
                    MessageBox.Show(Ex.Message, INSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);

                    //throw new InstallException(sMessage, Ex);
                }
                #endregion

                //THE END
                this.Context.LogMessage(this.GetType().Name + ": Install completed");
            }
        }
        /// <summary>
        /// Removes an installation
        /// </summary>
        /// <param name="savedState"></param>
        public override void Uninstall(IDictionary savedState)
        {
            try
            {
                this.Context.LogMessage(this.GetType().Name + ": Uninstall started");

                #region Add registry info
                //No default data
                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Removing registry info");
                    //WebInstaller.RemoveValueFromRegistryKey(REGLICKEY);
                    WebInstaller.RemoveValueFromRegistryKey(REGLICUSR);
                    WebInstaller.RemoveValueFromRegistryKey(REGLICORG);
                    WebInstaller.RemoveValueFromRegistryKey(REGMAJVER);
                    WebInstaller.RemoveValueFromRegistryKey(REGMINVER);
                    WebInstaller.RemoveValueFromRegistryKey(REGSITEID);
                    WebInstaller.RemoveValueFromRegistryKey(REGVDIR);
                    this.Context.LogMessage(this.GetType().Name + ": Registry info removed");
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot remove registry info";
#if DEBUG
                    MessageBox.Show(Ex.Message, UNINSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
                    
                    //throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Add required permissions to storage directory
                //Storage directory will be removed anyway (provided there are no files within)
                #endregion

                #region Map .dat files to aspnet_isapi.dll
                //Virtual dir will be removed anyway
                #endregion

                #region Change 404 error document
                //Virtual dir will be removed anyway
                #endregion

                #region Update storage in web.config
                //web.config will be removed anyway
                #endregion

                #region Update smtp config in web.config
                //web.config will be removed anyway
                #endregion

                #region Update appSettings in web.config
                //web.config will be removed anyway
                #endregion

                #region Update appSettings in app.config (Memba.PurgeService.XP.exe.config)
                //app.config will be removed anyway
                #endregion

                #region Add shortcuts
                const string FILENAME = "Memba Velodoc XP Edition.url";
                try
                {
                    //Remove desktop shortcut
                    string sDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    FileInfo objFileInfo = new FileInfo(Path.Combine(sDesktop, FILENAME));
                    if (objFileInfo.Exists)
                    {
                        //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        objFileInfo.Attributes &= ~FileAttributes.ReadOnly;
                        objFileInfo.Delete();
                    }

                    //Remove favorites shortcut
                    string sFavorites = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
                    objFileInfo = new FileInfo(Path.Combine(sFavorites, FILENAME));
                    if (objFileInfo.Exists)
                    {
                        //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        objFileInfo.Attributes &= ~FileAttributes.ReadOnly;
                        objFileInfo.Delete();
                    }
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot remove shortcuts";
#if DEBUG
                    MessageBox.Show(Ex.Message, UNINSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);

                    //throw new InstallException(sMessage, Ex);
                }
                #endregion

                #region Register ASP.NET 2.0
                string sSite = this.Context.Parameters[SITE];
#if DEBUG
                MessageBox.Show("Site: " + sSite + (String.IsNullOrEmpty(sSite) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sSite))
                    sSite = METABASEROOT + DEFAULTSITEID; //default web site
                string sVirtualDir = this.Context.Parameters[VDIR];
#if DEBUG
                MessageBox.Show("VirtualDir: " + sVirtualDir + (String.IsNullOrEmpty(sVirtualDir) ? "- Using default data" : String.Empty), DEBUGINFO, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                if (String.IsNullOrEmpty(sVirtualDir))
                    sVirtualDir = DEFAULTVDIR; //default data

                try
                {
                    this.Context.LogMessage(this.GetType().Name + ": Removing virtual directory");
                    WebInstaller.UnregisterASPNET2(sVirtualDir, sSite);
                    this.Context.LogMessage(this.GetType().Name + ": Virtual directory removed");
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot remove virtual directory";
#if DEBUG
                    MessageBox.Show(Ex.Message, UNINSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);

                    //throw new InstallException(sMessage, Ex);
                }
                #endregion
            }
            catch (Exception Ex)
            {
                string sMessage = "Unhandled installer failure.";
#if DEBUG
                MessageBox.Show(Ex.Message, UNINSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);

                //We really do not want uninstall to fail, even if any step has failed
                //throw new InstallException("Uninstall error. Please check the msi log.", Ex);
            }
            finally
            {
                #region Uninstall Feedback
                try
                {
#if !DEBUG
                    Process.Start("IExplore.exe", "http://www.velodoc.com/uninstall.aspx?p=xp");
#endif
                }
                catch (Exception Ex)
                {
                    string sMessage = "Cannot open uninstall feedback page";
#if DEBUG
                    MessageBox.Show(Ex.Message, UNINSTALLERR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                    this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);

                    //throw new InstallException(sMessage, Ex);
                }
                #endregion

                base.Uninstall(savedState);
                this.Context.LogMessage(this.GetType().Name + ": Uninstall completed");
            }
        }
        /// <summary>
        /// Restores the pre-installation state of the computer
        /// </summary>
        /// <param name="savedState"></param>
        public override void Rollback(IDictionary savedState)
        {
            try
            {
                this.Context.LogMessage(this.GetType().Name + ": Rollback started");
                Uninstall(savedState);
            }
            finally
            {
                base.Rollback(savedState);
                this.Context.LogMessage(this.GetType().Name + ": Rollback completed");
            }
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Adds a value to the Velodoc registry key
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void AddValueToRegistryKey(string name, string value)
        {
            using(RegistryKey objRegistryKey = Registry.LocalMachine.CreateSubKey(REGVELODOCKEY))
            {       
                if (objRegistryKey != null)
                    objRegistryKey.SetValue(name, value, RegistryValueKind.String);
            }
        }
        /// <summary>
        /// Adds a value to the Velodoc registry key
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void AddValueToRegistryKey(string name, int value)
        {
            using (RegistryKey objRegistryKey = Registry.LocalMachine.CreateSubKey(REGVELODOCKEY))
            {
                if (objRegistryKey != null)
                    objRegistryKey.SetValue(name, value, RegistryValueKind.DWord);
            }
        }
        /// <summary>
        /// Removes a value from the Velodoc registry key
        /// </summary>
        /// <param name="name"></param>
        private static void RemoveValueFromRegistryKey(string name)
        {
            RegistryKey objRegistryKey = null;
            try
            {
                objRegistryKey = Registry.LocalMachine.CreateSubKey(REGVELODOCKEY);
                if (objRegistryKey != null)
                {
                    objRegistryKey.DeleteValue(name);

                    //If this is the last value in REGVELODOCKEY, remove parents as long as they are empty
                    string sKeyName = REGVELODOCKEY;
                    while ((objRegistryKey.SubKeyCount + objRegistryKey.ValueCount).Equals(0))
                    {
                        string sSubKeyName = sKeyName.Substring(sKeyName.LastIndexOf('\\') + 1);
                        sKeyName = sKeyName.Substring(0, sKeyName.LastIndexOf('\\'));
                        objRegistryKey.Close();
                        objRegistryKey = Registry.LocalMachine.CreateSubKey(sKeyName);
                        objRegistryKey.DeleteSubKey(sSubKeyName);
                    }
                }
            }
            finally
            {
                if(objRegistryKey != null)
                    objRegistryKey.Close();
            }
        }
        /// <summary>
        /// Add modifiy permission to storage directory for ASPNET user (Windows XP) or IIS_WPG group (Windows 2003) or IIS_IUSRS group (Vista)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="storageDir"></param>
        private static void AddStoragePermissions(string storageDir)
        {
            DirectoryInfo di = new DirectoryInfo(storageDir);
            if (di.Exists)
            {
                if ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 1))
                {
                    //Windows XP is supported
                    NTAccount objNTAccount = new NTAccount(Environment.MachineName, "ASPNET");
                    AddStoragePermissions(objNTAccount.Value, di);
                }
                else if ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 2))
                {
                    //Windows 2003 is supported
                    NTAccount objNTAccount = new NTAccount(IsDomainController() ? Environment.UserDomainName : Environment.MachineName, "IIS_WPG");
                    AddStoragePermissions(objNTAccount.Value, di);
                }
                else if (Environment.OSVersion.Version.Major >= 6)
                {
                    //Windows Vista and Windows Server 2008 are supported
                    //AddStoragePermissions("BUILTIN\\IIS_IUSRS", di);
                    SecurityIdentifier objSID = new SecurityIdentifier("S-1-5-32-568");
                    NTAccount objNTAccount = (NTAccount)objSID.Translate(typeof(NTAccount));
                    AddStoragePermissions(objNTAccount.Value, di);
                }
                else
                {
                    throw new InstallException("Windows 2000 or any previous versions are unsupported");
                }                       
            }
            else
            {
                throw new DirectoryNotFoundException("Cannot find " + storageDir);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="di"></param>
        private static void AddStoragePermissions(string identity, DirectoryInfo di)
        {
#if DEBUG
            MessageBox.Show(String.Format("Giving {0} modify permissions to {1}", identity, di.FullName));
#endif
            DirectorySecurity ds = di.GetAccessControl();
            ds.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.Modify, AccessControlType.Allow));
            di.SetAccessControl(ds);
        }
        /// <summary>
        /// Maps .dat files to aspnet_isapi.dll for GET and HEAD requests
        /// </summary>
        /// <param name="virtualDir"></param>
        /// <param name="site"></param>
        /// <see cref="http://msdn2.microsoft.com/en-us/library/aa289522(VS.71).aspx"/>
        /// <see cref="http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/45e717d0-20f6-459c-9183-cf6019f2edab.mspx?mfr=true" />
        private static void MapDatExtensionToAspNet(string virtualDir, string site) //string port)
        {
            //NOTE: This could have been done in
            //File System Editor -> Web Application Folder -> Properties Window -> AppMappings

            const string MAPS = "ScriptMaps";
            
            //string sServerNum = WebInstaller.FindServerNum(port);
            System.Diagnostics.Debug.Assert(site.StartsWith(METABASEROOT));
            string sServerNum = site.Substring(METABASEROOT.Length);
            int iServerNum;
            bool bTry = Int32.TryParse(sServerNum, out iServerNum);
            System.Diagnostics.Debug.Assert(bTry);
            string sAppPath = W3SVC + "/" + sServerNum + "/ROOT/" + virtualDir;
            
            //Note: it may have been preferrable to use System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
            string sDatMapping = ".dat," + Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD";
            string sDatMapping64 = ".dat," + Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework64\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD";

            DirectoryEntry objApplication = new DirectoryEntry(sAppPath);
            objApplication.RefreshCache();
            //TODO: What if there is already a mapping for *.dat files which does not target aspnet_isapi.dll?
            int iPos = objApplication.Properties[MAPS].IndexOf(sDatMapping);
            if (iPos < 0)
                objApplication.Properties[MAPS].Add(sDatMapping);
            if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") != "x86") //i.e. AMD64 or I64. Note that 32-bit OS installed on 64-bit processor returns x86
            {
                //TODO: What if there is already a mapping for *.dat files which does not target aspnet_isapi.dll?
                iPos = objApplication.Properties[MAPS].IndexOf(sDatMapping64);
                if (iPos < 0)
                    objApplication.Properties[MAPS].Add(sDatMapping64);
            }
            objApplication.CommitChanges();
            objApplication.Close();
        }
        /// <summary>
        /// Switches the 404 page for a page which redirects to defaultErr.aspx?e=404
        /// </summary>
        /// <param name="virtualDir"></param>
        /// <param name="site"></param>
        /// <param name="webInstallDir"></param>
        private static void Switch404Page(string virtualDir, string site, string webInstallDir) //string port, string webInstallDir)
        {
            const string ERRORS = "HttpErrors";

            //string sServerNum = WebInstaller.FindServerNum(port);
            System.Diagnostics.Debug.Assert(site.StartsWith(METABASEROOT));
            string sServerNum = site.Substring(METABASEROOT.Length);
            int iServerNum;
            bool bTry = Int32.TryParse(sServerNum, out iServerNum);
            System.Diagnostics.Debug.Assert(bTry);
            string sAppPath = W3SVC + "/" + sServerNum + "/ROOT/" + virtualDir;

            string s404Page = Path.Combine(webInstallDir, "404.htm");
            FileInfo obj404FileInfo = new FileInfo(s404Page);

            if (obj404FileInfo.Exists)
            {
                DirectoryEntry objApplication = new DirectoryEntry(sAppPath);
                objApplication.RefreshCache();
                if (objApplication.Properties[ERRORS].Value is Array)
                {
                    Array arrErrors = (Array)objApplication.Properties[ERRORS].Value;
                    bool bFound = false;
                    for (int iPos = 0; iPos < arrErrors.Length; iPos++)
                    {
                        if ((arrErrors.GetValue(iPos) != null) && ((string)arrErrors.GetValue(iPos)).StartsWith("404"))
                        {
                            //Assuming there is only one
                            objApplication.Properties[ERRORS].RemoveAt(iPos);
                            objApplication.Properties[ERRORS].Insert(iPos, "404,*,FILE," + s404Page);
                            bFound = true;
                            break;
                        }
                    }
                    if (!bFound)
                        objApplication.Properties[ERRORS].Add("404,*,FILE," + s404Page);
                }
                objApplication.CommitChanges();
                objApplication.Close();
            }
            else
            {
                throw new FileNotFoundException("Cannot find " + s404Page);
            }
        }
        /// <summary>
        /// Returns the .NET Framework installation directory
        /// </summary>
        /// <returns></returns>
        public static string GetNetFrameworkDirectory()
        {
            const string FMK32 = "Framework";
            const string FMK64 = "Framework64";
            const string NETFMKVERSION = "v2.0.50727";

            string sNetDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            
            //Make sure we have .NET Framework 2.0
            if (sNetDirectory.IndexOf(NETFMKVERSION, StringComparison.OrdinalIgnoreCase) < 0)
                throw new NotSupportedException("Cannot find .NET 2.0 framework. Please check that .NET 2.0 is installed");
            
            //Use of Framework64 on x64 platforms. See: http://support.microsoft.com/kb/894435
            if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") != "x86") //i.e. AMD64 or I64. Note that 32-bit OS installed on 64-bit processor returns x86
            {
                if (sNetDirectory.IndexOf(FMK64, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    //Funtion Replace does not take StringComparison as parameter
                    int iPos = sNetDirectory.IndexOf(FMK32, StringComparison.OrdinalIgnoreCase);
                    System.Diagnostics.Debug.Assert(iPos > 0);
                    sNetDirectory = sNetDirectory.Substring(0, iPos) + FMK64 + sNetDirectory.Substring(iPos + FMK32.Length);
                }
            }
            
            return sNetDirectory;
        }
        /// <summary>
        /// Register ASP.NET 2.0 on the new web site
        /// </summary>
        /// <param name="virtualDir"></param>
        /// <param name="site"></param>
        private static void RegisterASPNET2(string virtualDir, string site)
        {
            System.Diagnostics.Debug.Assert(site.StartsWith(METABASEROOT));
            string sServerNum = site.Substring(METABASEROOT.Length);
            int iServerNum;
            bool bTry = Int32.TryParse(sServerNum, out iServerNum);
            System.Diagnostics.Debug.Assert(bTry);
            //string sAppPath = W3SVC + "/" + sServerNum + "/ROOT/" + virtualDir;
            string sAppPath = "W3SVC/" + sServerNum + "/ROOT/" + virtualDir;

            //Find Aspnet_regiis.exe
            string sAspnetRegExe = Path.Combine(WebInstaller.GetNetFrameworkDirectory(), "aspnet_regiis.exe");
            if (!File.Exists(sAspnetRegExe))
                throw new FileNotFoundException("Cannot find " + sAspnetRegExe);
            
            //Execute Aspnet_regiis.exe -s W3SVC/[sServerNum]/ROOT/VelodocXP
            Process objProcess = new Process();
            objProcess.StartInfo.UseShellExecute = false;
            objProcess.StartInfo.RedirectStandardOutput = false;
            objProcess.StartInfo.RedirectStandardError = false;
            objProcess.StartInfo.FileName = sAspnetRegExe;
            objProcess.StartInfo.Arguments = "-s " + sAppPath;
            objProcess.Start();
            objProcess.WaitForExit();
        }
        /// <summary>
        /// Fixes an issue whereby the virtual directory is not removed by uninstaller after registering ASP.NET 2.0
        /// </summary>
        /// <param name="virtualDir"></param>
        /// <param name="site"></param>
        private static void UnregisterASPNET2(string virtualDir, string site)
        {
            //There is no real need for unregistering ASP.NET 2.0 from the Velodoc app
            //when uninstalling since uninstalling should normally remove the virtual directory.
            //But the thing is since the virtual directory has been modified to register ASP.NET 2.0
            //the setup does not always remove it. We need to fix that. Maybe we could have foudna better method name.

            //string sServerNum = WebInstaller.FindServerNum(port);
            System.Diagnostics.Debug.Assert(site.StartsWith(METABASEROOT));
            string sServerNum = site.Substring(METABASEROOT.Length);
            int iServerNum;
            bool bTry = Int32.TryParse(sServerNum, out iServerNum);
            System.Diagnostics.Debug.Assert(bTry);
            string sAppPath = W3SVC + "/" + sServerNum + "/ROOT/" + virtualDir;

            DirectoryEntry objApplication = new DirectoryEntry(sAppPath);
            try
            {
                //The following line will raise an exception if the path does not exist 
                objApplication.DeleteTree();
            }
            catch
            {
                //Do nothing, virtual dir has already been removed
            }
            finally
            {
                objApplication.Close();
            }
        }
        /// <summary>
        /// Returns the metabase server number from its port
        /// </summary>
        /// <remarks>In a typical configuration, FindServerNum("80") returns "1"</remarks>
        /// <param name="port">The http port to which the server is bound</param>
        /// <returns></returns>
        private static string FindServerNum(string port)
        {
            //Enumerate the existing server nums
            List<string> lstServerNums = new List<string>();
            DirectoryEntry objRoot = new DirectoryEntry(W3SVC);
            System.Collections.IEnumerator objEnumerator = objRoot.Children.GetEnumerator();
            while (objEnumerator.MoveNext())
            {
                DirectoryEntry objChild = objEnumerator.Current as DirectoryEntry;
                if (objChild != null)
                {
                    int num;
                    bool bTry = Int32.TryParse(objChild.Name, out num);
                    if (bTry)
                        lstServerNums.Add(objChild.Name);
                }
                objChild.Close();
            }
            objRoot.Close();

            //Iterate through each server to find the one with the designated port
            for (int i = lstServerNums.Count - 1; i >= 0; i--)
            {
                DirectoryEntry objServer = new DirectoryEntry(W3SVC + "/" + lstServerNums[i]);
                string sBindings = (string)objServer.Properties[BINDINGS].Value;
                string sServerPort = sBindings.Trim(new char[] { ':' });
                if (!sServerPort.Equals(port))
                    lstServerNums.RemoveAt(i);
                objServer.Close();
            }

            //Raise exceptions
            if (lstServerNums.Count < 1)
                throw new ArgumentException("No server found for port " + port, "port");
            else if (lstServerNums.Count > 1)
                throw new ArgumentException("Several servers found for port " + port, "port");

            System.Diagnostics.Debug.Assert(lstServerNums.Count == 1);
            return lstServerNums[0];
        }
        /// <summary>
        /// Updates the storage directory in web.config
        /// </summary>
        /// <param name="webInstallDir"></param>
        /// <param name="storageDir"></param>
        private static void UpdateStorageConfig(string webInstallDir, string storageDir)
        {
            string sConfigFile = Path.Combine(webInstallDir, "web.config");

            FileInfo objFileInfo = new FileInfo(sConfigFile);
            if (objFileInfo.Exists)
            {
                //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    objFileInfo.Attributes &= ~FileAttributes.ReadOnly;

                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument.Load(sConfigFile);

                XmlElement objRootElement = objXmlDocument.DocumentElement;

                XmlNode objProviders = objRootElement.SelectSingleNode("system.web/fileStorage/providers");
                XmlNodeList lstProviders = objProviders.ChildNodes;

                for (int i = 0; i < lstProviders.Count; i++)
                {
                    if ((lstProviders.Item(i).Attributes != null) && (lstProviders.Item(i).Attributes["name"] != null)
                        && (lstProviders.Item(i).Attributes["name"].Value.Equals("FSFileStorageProvider")))
                    {
                        lstProviders.Item(i).Attributes["connectionString"].Value = storageDir;
                    }
                }

                //save the output to a file
                objXmlDocument.Save(sConfigFile);
            }
            else
            {
                throw new FileNotFoundException("Cannot find " + sConfigFile);
            }
        }
        /// <summary>
        /// Updates the Smtp configuration in web.config
        /// </summary>
        /// <param name="webInstallDir"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="from"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="defaultCredentials"></param>
        private static void UpdateSmtpConfig(string webInstallDir, string host, string port, string from, string userName, string password, bool defaultCredentials)
        {
            string sConfigFile = Path.Combine(webInstallDir, "web.config");

            FileInfo objFileInfo = new FileInfo(sConfigFile);
            if (objFileInfo.Exists)
            {
                //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    objFileInfo.Attributes &= ~FileAttributes.ReadOnly;

                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument.Load(sConfigFile);
                XmlElement objRootElement = objXmlDocument.DocumentElement;
                
                //Load smtp element
                XmlNode objSmtp = objRootElement.SelectSingleNode("system.net/mailSettings/smtp");
                objSmtp.Attributes["from"].Value = from;

                //Load and replace network element
                XmlNode objOldNetwork = objSmtp.SelectSingleNode("network");
                XmlElement objNewNetwork = objXmlDocument.CreateElement("network");
                objNewNetwork.SetAttribute("host", host);
                objNewNetwork.SetAttribute("port", port);
                if (defaultCredentials)
                    objNewNetwork.SetAttribute("defaultCredentials", "true");
                else
                {
                    objNewNetwork.SetAttribute("userName", userName);
                    objNewNetwork.SetAttribute("password", password);
                }
                objSmtp.ReplaceChild(objNewNetwork, objOldNetwork);

                //save the output to file
                objXmlDocument.Save(sConfigFile);
            }
            else
            {
                throw new FileNotFoundException("Cannot find " + sConfigFile);
            }
        }
        /// <summary>
        /// Updates appSettings in web.config
        /// </summary>
        /// <param name="webInstallDir"></param>
        /// <param name="userList"></param>
        /// <param name="securityCode"></param>
        private static void UpdateWebConfig(string webInstallDir, string userList, string securityCode)
        {
            string sConfigFile = Path.Combine(webInstallDir, "web.config");

            FileInfo objFileInfo = new FileInfo(sConfigFile);
            if (objFileInfo.Exists)
            {
                //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    objFileInfo.Attributes &= ~FileAttributes.ReadOnly;

                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument.Load(sConfigFile);
                XmlElement objRootElement = objXmlDocument.DocumentElement;

                //Load app settings
                XmlNode objAppSettings = objRootElement.SelectSingleNode("appSettings");
                XmlNodeList lstAppSettings = objAppSettings.ChildNodes;
                
                //Iterate through app settings
                for(int i = 0; i < lstAppSettings.Count; i++)
                {
                    if (lstAppSettings.Item(i).Attributes["key"].Value.Equals("UserList"))
                        lstAppSettings.Item(i).Attributes["value"].Value = userList;
                    else if (lstAppSettings.Item(i).Attributes["key"].Value.Equals("SecurityCode"))
                        lstAppSettings.Item(i).Attributes["value"].Value = securityCode;
                }

                //save the output to a file
                objXmlDocument.Save(sConfigFile);
            }
            else
            {
                throw new FileNotFoundException("Cannot find " + sConfigFile);
            }
        }
        /// <summary>
        /// Updates appSettings in app.config (Memba.PurgeService.XP.exe.config) for the purge service
        /// </summary>
        /// <param name="svcInstallDir"></param>
        /// <param name="storageDir"></param>
        private static void UpdateAppConfig(string svcInstallDir, string storageDir)
        {
            string sConfigFile = Path.Combine(svcInstallDir, "Memba.PurgeService.XP.exe.config");

            FileInfo objFileInfo = new FileInfo(sConfigFile);
            if (objFileInfo.Exists)
            {
                //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    objFileInfo.Attributes &= ~FileAttributes.ReadOnly;

                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument.Load(sConfigFile);
                XmlElement objRootElement = objXmlDocument.DocumentElement;

                //Load app settings
                XmlNode objAppSettings = objRootElement.SelectSingleNode("appSettings");
                XmlNodeList lstAppSettings = objAppSettings.ChildNodes;

                //Iterate through app settings
                for (int i = 0; i < lstAppSettings.Count; i++)
                {
                    if (lstAppSettings.Item(i).Attributes["key"].Value.Equals("Directory"))
                        lstAppSettings.Item(i).Attributes["value"].Value = storageDir;
                }

                //save the output to a file
                objXmlDocument.Save(sConfigFile);
            }
            else
            {
                throw new FileNotFoundException("Cannot find " + sConfigFile);
            }
        }
        /// <summary>
        /// Adds a shortcut to the web site
        /// </summary>
        /// <param name="site"></param>
        /// <param name="virtualDir"></param>
        /// <param name="iconFile"></param>
        private static void AddShortcuts(string site, string virtualDir, string iconFile)
        {
            /*
            Shortcuts are .url files containing:           
            [DEFAULT]
            BASEURL=http://localhost:port/virtualdir
            [InternetShortcut]
            URL=http://localhost:port/virtualdir
            IconFile=C:\Inetpub\wwwroot\VelodocXP\favicon.ico
            HotKey=0
            IconIndex=0
            IDList=
            */
            const string SHORTCUT = "[DEFAULT]\nBASEURL=http://localhost:{0}/{1}\n[InternetShortcut]\nURL=http://localhost:{0}/{1}\nIconFile={2}\nHotKey=0\nIconIndex=0\nIDList=";
            const string FILENAME = "Memba Velodoc XP Edition.url";
            
            //Find the server port
            string sSitePath = W3SVC + "/" + site.Substring(METABASEROOT.Length);
            DirectoryEntry objSite = new DirectoryEntry(sSitePath);
            string sPort = (string)objSite.Properties[BINDINGS].Value;
            sPort = sPort.Trim(new char[] { ':' });

            //Build shortcut content
            string sShortcut = String.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                SHORTCUT,
                sPort,
                virtualDir,
                iconFile);

            //Add to %ProgramFiles%\Memba\Velodoc\XP Edition
            string sProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string sXPEdition = Path.Combine(sProgramFiles, "Memba\\Velodoc\\XP Edition");
            WebInstaller.AddTextFile(sShortcut, Path.Combine(sXPEdition, FILENAME));

            //Add to favorites
            string sFavorites = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            WebInstaller.AddTextFile(sShortcut, Path.Combine(sFavorites, FILENAME));
        }
        /// <summary>
        /// Adds a text file at filePath with content text
        /// </summary>
        /// <param name="text">content of the text file</param>
        /// <param name="filePath">path to the text file</param>
        private static void AddTextFile(string text, string filePath)
        {
            FileInfo objFileInfo = new FileInfo(filePath);
            if (objFileInfo.Exists)
            {
                //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                objFileInfo.Attributes &= ~FileAttributes.ReadOnly;
                objFileInfo.Delete();
            }
            using (StreamWriter objStreamWriter = objFileInfo.CreateText())
            {
                objStreamWriter.Write(text);
            }
        }
        /// <summary>
        /// Checks whether a computer is a domain controller
        /// </summary>
        /// <returns></returns>
        private static bool IsDomainController()
        {
            //There are three ways to check whether a computer is a domain controller:

            //1) Checking HKLM\SYSTEM\CurrentControlSet\Control\ProductOptions[ProductType]
            // SERVERNT = Stand Alone Server, LANMANNT = Primary Domain Controller, LANSECNT = Backup Domain Controller
            // see http://techsupt.winbatch.com/TS/T000001036004F7.html

            //2) Using Wmi to look at the Win32_ComputerSystem.DomainRole property
            // 0 = Standalone Workstation, 1 = Member Workstation, 2  =Standalone Server, 3 = Member Server, 4 = Backup Domain Controller, 5 = Primary Domain Controller
            //See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/wmisdk/wmi/win32_computersystem.asp
            //Sample code (requires using System.Management)
            //  ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2",
            //      "SELECT * FROM Win32_ComputerSystem");
            //
            //  foreach (ManagementObject queryObj in searcher.Get())
            //  {
            //      Console.WriteLine("DomainRole: {0}", queryObj["DomainRole"]);
            //  }

            //3) Using System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().DomainControllers
            // but this requires that the computer (potentially a laptop) be connected to the domain.

            bool bIsDomainControllerRet = false;
            using (RegistryKey objRegistryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\ProductOptions"))
            {
                if (objRegistryKey != null)
                {
                    string sProductType = (string)objRegistryKey.GetValue("ProductType");
                    if (!String.IsNullOrEmpty(sProductType))
                        bIsDomainControllerRet = ((sProductType.Equals("LANMANNT", StringComparison.InvariantCultureIgnoreCase))
                            || (sProductType.Equals("LANSECNT", StringComparison.InvariantCultureIgnoreCase)));
                }
            }
            return bIsDomainControllerRet;
        }
        #endregion
    }
}