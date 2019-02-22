using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Configuration.Provider;
using System.Security.AccessControl; //DirectorySecurity
using System.Security.Principal; //WindowsIdentity, SecurityIdentifier
using Memba.FileUpload.Properties;

namespace Memba.FileUpload.Providers
{
    //Using a UNC path raises two issues as explained in http://support.microsoft.com/kb/207671
    // 1) Security issues can be resolved as detailed at http://west-wind.com/weblog/posts/1572.aspx especially if teh two computers are not in the same domain
    // 2) Performance issues are detailed at http://www.microsoft.com/technet/prodtechnol/windowsserver2003/technologies/webapp/iis/remstorg.mspx
    // and http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/fd959ed7-01e5-4cfb-923d-ad10dcfe0eda.mspx

    public class FSFileStorageProvider : FileStorageProviderBase
    {
        private const string NAME = "FSFileStorageProvider";
        private const string DESCRIPTION = "Secure file system storage provider";
        private const string APPLICATIONNAME = "/";

        #region Private Members
        private string _ApplicationName;
        private string _ConnectionString;
        #endregion

        #region Property Accessors
        public override string ApplicationName
        {
            get { return _ApplicationName; }
            set { _ApplicationName = value; }
        }

        public override string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objSiteConfiguration"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Verify that objSiteConfiguration isn't null
            if (config == null)
                throw new ArgumentNullException (FileStorage.Config);

            // Assign the provider a default name if it doesn't have one
            if (String.IsNullOrEmpty (name))
                name = NAME;

            // Add a default "description" attribute to objSiteConfiguration if the
            // attribute doesn't exist or is empty
            if (string.IsNullOrEmpty (config[FileStorage.DescriptionAttribute])) {
                config.Remove(FileStorage.DescriptionAttribute);
                config.Add(FileStorage.DescriptionAttribute, DESCRIPTION);
            }

            // Call the base class's Initialize method
            base.Initialize(name, config);

            // Initialize _applicationName
            _ApplicationName = config[FileStorage.ApplicationNameAttribute];

            if (string.IsNullOrEmpty(_ApplicationName))
                _ApplicationName = APPLICATIONNAME;

            config.Remove(FileStorage.ApplicationNameAttribute);

            // Initialize _ConnectionString
            _ConnectionString = config[FileStorage.ConnectionStringAttribute];

            if (String.IsNullOrEmpty (_ConnectionString))
                throw new ProviderException (Resources.ExceptionStorageProviderMissingConnectionString);

            //DirectoryInfo objDirectoryInfo = new DirectoryInfo(_ConnectionString); //CA1804
            if (!Directory.Exists(_ConnectionString))
            {
                try
                {
                    Directory.CreateDirectory(_ConnectionString);
                }
                catch (Exception Ex)
                {
                    throw new ProviderException(Resources.ExceptionStorageProviderCannotCreateDirectory, Ex); 
                }
            }

            config.Remove(FileStorage.ConnectionStringAttribute);

            //if (WebConfigurationManager.ConnectionStrings[_ConnectionString] != null)
            //    _ConnectionString = WebConfigurationManager.ConnectionStrings[_ConnectionString].ConnectionStringAttribute;
            //if (String.IsNullOrEmpty (_ConnectionString))
            //    throw new ProviderException ("Empty database connection string");

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                string attr = config.GetKey (0);
                if (!String.IsNullOrEmpty (attr))
                    throw new ProviderException(String.Format(Resources.Culture, Resources.ExceptionStorageProviderUnrecognizedAttribute, attr));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public override object GetNewFileKey(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return null;

            //return DateTime.UtcNow.Year.ToString() + DateTime.UtcNow.Month.ToString() + "\\" + Guid.NewGuid().ToString("N");
            return Guid.NewGuid().ToString("N");           
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerFileKey"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override Stream GetFileStream(object providerFileKey, AccessMode mode)
        {
            if (providerFileKey == null)
                return null;
           
            FileStream objFileStreamRet;
            string sFilePath = Path.Combine(_ConnectionString, providerFileKey.ToString());

            if (mode == AccessMode.Create)
                objFileStreamRet = new FileStream(sFilePath, FileMode.Create, FileAccess.Write);
            else
                objFileStreamRet = new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return objFileStreamRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerFileKey"></param>
        /// <returns></returns>
        public override FileInfo GetFileInfo(object providerFileKey)
        {
            if (providerFileKey == null)
                return null;

            return new FileInfo(Path.Combine(_ConnectionString, providerFileKey.ToString()));
        }
        /// <summary>
        /// 
        /// </summary>
        public override void DeleteFile(object providerFileKey)
        {
            if (providerFileKey == null)
                return;

            FileInfo objFileInfo = new FileInfo(Path.Combine(_ConnectionString, providerFileKey.ToString()));
            if (objFileInfo.Exists)
            {
                //if ((objFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                objFileInfo.Attributes &= ~FileAttributes.ReadOnly;
                objFileInfo.Delete();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerFileKey"></param>
        /// <returns></returns>
        public override bool FileExists(object providerFileKey)
        {
            if (providerFileKey == null)
                return false;

            return File.Exists(Path.Combine(_ConnectionString, providerFileKey.ToString()));
        }
        #endregion
    }
}