using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;
using System.IO;

namespace Memba.FileUpload.Providers
{
    //For more information on custom providers
    //see: http://msdn.microsoft.com/library/en-us/dnaspp/html/ASPNETProvMod_Prt8.asp

    public abstract class FileStorageProviderBase : ProviderBase
    {
        #region Property Accessors
        public abstract string ApplicationName { get; set; }
        public abstract string ConnectionString { get; set; }
        #endregion

        #region Other Members
        public abstract object GetNewFileKey(string fileName);
        public abstract Stream GetFileStream(object providerFileKey, AccessMode mode);
        public abstract FileInfo GetFileInfo(object providerFileKey);
        public abstract void DeleteFile(object providerFileKey);
        public abstract bool FileExists(object providerFileKey);
        #endregion
    }

    public enum AccessMode
    {
        Read,
        Create
    }
}
