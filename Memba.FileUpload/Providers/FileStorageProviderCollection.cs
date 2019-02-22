using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;
using Memba.FileUpload.Properties;

namespace Memba.FileUpload.Providers
{
    //For more information on custom providers
    //see: http://msdn.microsoft.com/library/en-us/dnaspp/html/ASPNETProvMod_Prt8.asp
    
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Justification = "See: http://msdn.microsoft.com/library/en-us/dnaspp/html/ASPNETProvMod_Prt8.asp")]
    public class FileStorageProviderCollection : ProviderCollection
    {
        public new FileStorageProviderBase this[string name]
        {
            get { return (FileStorageProviderBase)base[name]; }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is FileStorageProviderBase))
                throw new ArgumentException(Resources.ExceptionStorageProviderInvalidType, "provider");

            base.Add(provider);
        }
    }
}
