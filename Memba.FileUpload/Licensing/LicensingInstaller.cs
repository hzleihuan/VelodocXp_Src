using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;

namespace Memba.FileUpload.Licensing
{
    [RunInstaller(true)]
    public partial class LicensingInstaller : Installer
    {
        #region Constants
        //The following constants are defined in the configuration of the custom action in the web setup project
        //This requires a CustomActionData property valued
        // /LICKEY="[PIDKEY]" /LICUSR="[USERNAME]" /LICORG="[COMPANYNAME]"
        private const string LICKEY = "LICKEY"; //License Key
        //private const string LICUSR = "LICUSR"; //Licensee (contact)
        //private const string LICORG = "LICORG"; //Licensee (company)
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public LicensingInstaller()
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
            string sLicenseKey = this.Context.Parameters[LICKEY];
            //string sLicensedUser = this.Context.Parameters[LICUSR];
            //string sLicensedOrganisation = this.Context.Parameters[LICORG];
#if (DEBUG)
            //MessageBox.Show(REGLICKEY + ": " + sLicenseKey + "\n" + REGLICUSR + ": " + sLicensedUser + "\n" + REGLICORG + ": " + sLicensedOrganisation, "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            MessageBox.Show("LicenseKey: " + sLicenseKey, "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
            if (String.IsNullOrEmpty(sLicenseKey))
                this.Context.LogMessage(this.GetType().Name + ": Installing with GPL License");
            else
                this.Context.LogMessage(this.GetType().Name + ": License key for Memba EULA is " + sLicenseKey);
            #endregion

            #region Licensing
            try
            {
                if (!String.IsNullOrEmpty(sLicenseKey))
                {
                    this.Context.LogMessage(this.GetType().Name + ": Setting license information");
#if (DEBUG)
                    MessageBox.Show("Calling: " + VelodocLicenseProvider.LICENSINGSVC, "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
#endif
                    this.Context.LogMessage(this.GetType().Name + ": Calling " + VelodocLicenseProvider.LICENSINGSVC);
                    VelodocLicenseProvider objVelodocLicenseProvider = new VelodocLicenseProvider();
                    objVelodocLicenseProvider.SetLicense(sLicenseKey);

                    this.Context.LogMessage(this.GetType().Name + ": License information set");
                }
            }
            catch (Exception Ex)
            {
                string sMessage = "Cannot add license information to registry";

                this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);
#if(DEBUG)
                MessageBox.Show(Ex.Message, "Install Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                //We do not want to fail setup in this case
                //throw new InstallException(sMessage, Ex);
            }
            #endregion

            //THE END
            this.Context.LogMessage(this.GetType().Name + ": Install completed");
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

                this.Context.LogMessage(this.GetType().Name + ": Removing License key from registry");
                VelodocLicenseProvider objVelodocLicenseProvider = new VelodocLicenseProvider();
                objVelodocLicenseProvider.Clear();
                this.Context.LogMessage(this.GetType().Name + ": License key removed from registry");
            }
            catch (Exception Ex)
            {
                string sMessage = "Cannot remove license key from registry.";
#if(DEBUG)
                MessageBox.Show(Ex.Message, "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#else
                MessageBox.Show(sMessage, "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                this.Context.LogMessage(this.GetType().Name + ": " + sMessage + "\n" + Ex.Message);

                //We really do not want uninstall to fail, even if any step has failed
                //throw new InstallException("Uninstall Error. Please check the msi log.", Ex);
            }
            finally
            {
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
    }
}