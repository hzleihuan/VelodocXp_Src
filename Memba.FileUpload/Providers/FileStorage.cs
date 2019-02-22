using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.IO;

using Memba.FileUpload.Properties;

namespace Memba.FileUpload.Providers
{
    public static class FileStorage
    {
        internal const string Config = "config";
        internal const string ApplicationNameAttribute = "applicationName";
        internal const string DescriptionAttribute = "description";
        internal const string ConnectionStringAttribute = "connectionString";

        private const string SECTION = "system.web/fileStorage";

        private static FileStorageProviderBase _Provider; // = null; //CA1805
        private static FileStorageProviderCollection _Providers; // = null; //CA1805
        private static object _lock = new object();
        /// <summary>
        /// Returns the default file storage provider
        /// </summary>
        public static FileStorageProviderBase Provider
        {
            get
            {
                // Make sure a provider is loaded
                LoadProviders();

                //Return the default provider
                return _Provider;
            }
        }
        /// <summary>
        /// Returns the collection of file storage providers defined in config file
        /// </summary>
        public static FileStorageProviderCollection Providers
        {
            get
            {
                // Make sure a provider is loaded
                LoadProviders();

                //Return the provider collection
                return _Providers;
            }
        }
        /// <summary>
        /// Returns a new file key for a file designated by its name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static object GetNewFileKey(string fileName)
        {
            // Make sure a provider is loaded
            LoadProviders();

            // Delegate to the provider
            return _Provider.GetNewFileKey(fileName);
        }
        /// <summary>
        /// Gets a stream to the file located in storage and designated by its key
        /// </summary>
        /// <param name="providerFileKey"></param>
        /// <param name="mode"></param>
        public static Stream GetFileStream(object providerFileKey, AccessMode mode)
        {
            // Make sure a provider is loaded
            LoadProviders();

            // Delegate to the provider
            return _Provider.GetFileStream(providerFileKey, mode);
        }
        /// <summary>
        /// Gets a FileInfo to a file located in the file system
        /// </summary>
        /// <param name="providerFileKey"></param>
        public static FileInfo GetFileInfo(object providerFileKey)
        {
            // Make sure a provider is loaded
            LoadProviders();

            // Delegate to the provider
            return _Provider.GetFileInfo(providerFileKey);
        }
        /// <summary>
        /// Delete a file designated by its key from storage
        /// </summary>
        /// <param name="providerFileKey"></param>
        public static void DeleteFile(object providerFileKey)
        {
            // Make sure a provider is loaded
            LoadProviders();

            // Delegate to the provider
            _Provider.DeleteFile(providerFileKey);
        }
        /// <summary>
        /// Checks whether a file designated by its key exists in storage
        /// </summary>
        /// <param name="providerFileKey"></param>
        public static bool FileExists(object providerFileKey)
        {
            // Make sure a provider is loaded
            LoadProviders();

            // Delegate to the provider
            return _Provider.FileExists(providerFileKey);
        }
        /// <summary>
        /// Loads all file storage providers defined in config file
        /// </summary>
        private static void LoadProviders()
        {
            // Avoid claiming lock if providers are already loaded
            if (_Provider == null)
            {
                lock (_lock)
                {
                    // Do this again to make sure _Provider is still null
                    if (_Provider == null)
                    {
                        // Get a reference to the <fileStorage> section
                        FileStorageConfigurationSection section = (FileStorageConfigurationSection)
                            WebConfigurationManager.GetSection(SECTION);

                        // Load registered providers and point _Provider
                        // to the default provider
                        _Providers = new FileStorageProviderCollection();
                        ProvidersHelper.InstantiateProviders(section.Providers, _Providers, typeof(FileStorageProviderBase));
                        _Provider = _Providers[section.DefaultProvider];

                        if (_Provider == null)
                            throw new ProviderException(Resources.ExceptionStorageProviderCannotLoadDefault);
                    }
                }
            }
        }
    }
}
