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
using System.Text;
using System.ServiceModel; //EndPointAddress
using Channels = System.ServiceModel.Channels; //Binding
using System.Net.NetworkInformation;
using System.ComponentModel; //LicenceProvider
using Microsoft.Win32; //Registry, RegistryKey
using System.Web; //HttpContext
using System.Web.Caching; //Cache

namespace Memba.FileUpload.Licensing
{
    /// <summary>
    /// The VelodocLicenseProvider class which returns a valid VelodocLicense object after reading the registry
    /// </summary>
    /// <remarks>See articles below for more information about licensing .NET applications</remarks>
    /// <see cref="http://www.developer.com/net/net/article.php/11087_3074001_1"/>
    /// <see cref="http://windowsclient.net/articles/licensing.aspx"/>
    public class VelodocLicenseProvider : LicenseProvider
    {
        private const string APPROOTKEY = "SOFTWARE\\Memba\\Velodoc\\XP Edition";
#if DEBUG
        internal const string LICENSINGSVC = "http://10.0.0.12/Licensing/LicensingService.svc";
#else
        internal const string LICENSINGSVC = "http://www.velodoc.com/Licensing/LicensingService.svc";
#endif
        private const string PRODUCTCODE = "VXP08";
        private const string LICENSEKEY = "LicenseKey";
        private const string ACTIVATIONKEY = "ActivationKey";
        private const int CHECKKEYSRANDOMMAX = 50; //To randomly check keys on velodoc.com. The higher, the less often it is checked

        /// <summary>
        /// Gets the license from wherever it is stored
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <param name="allowExceptions"></param>
        /// <returns></returns>
        public override License GetLicense(LicenseContext context, Type type, object instance, bool allowExceptions)
        {
            const string CACHEDLIC = "VelodocLicense";
          
            //Try to get the VelodocLicense from the http cache
            VelodocLicense objLicenseRet = HttpRuntime.Cache[CACHEDLIC] as VelodocLicense;

            //No need to check for context.UsageMode because users get at least a GPL License

            if (objLicenseRet == null)
            {
                string sLicenseKey;
                string sActivationKey;
                RegistryKey objRegistryKey = null;
                try
                {
                    //All instances on the same machine share the same license key: should not be an issue.
                    //Should match keys and names in WebInstaller.
                    objRegistryKey = Registry.LocalMachine.OpenSubKey(APPROOTKEY);
                    sLicenseKey = (string)objRegistryKey.GetValue(LICENSEKEY);
                    sActivationKey = (string)objRegistryKey.GetValue(ACTIVATIONKEY);

                    //Validate the license key (the following lines will raise an exception
                    //if the license key is not a Guid)
                    Guid gLicenseKey = new Guid(sLicenseKey);
                    Guid gActivationKey = new Guid(sActivationKey);
                }
                catch
                {
                    //Should anything occur, user at least gets a GPL license
                    sLicenseKey = null;
                    sActivationKey = null;
                }
                finally
                {
                    if (objRegistryKey != null)
                        objRegistryKey.Close();
                }

                //Randomly check the activation key
                if (!String.IsNullOrEmpty(sLicenseKey))
                {
                    Random objRandom = new Random();
                    if (objRandom.Next(0, CHECKKEYSRANDOMMAX).Equals(0))
                    {
                        LicensingServiceClient objLicensingServiceClient = null;

                        try
                        {
                            //Then configure the service
                            BasicHttpBinding objHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None);
                            objHttpBinding.ProxyAddress = null;
                            objHttpBinding.UseDefaultWebProxy = true;
                            EndpointAddress objRemoteAddress = new EndpointAddress(LICENSINGSVC);

                            //Finally call the service
                            objLicensingServiceClient = new LicensingServiceClient(objHttpBinding, objRemoteAddress);
                            int iResult = objLicensingServiceClient.Check(sLicenseKey, sActivationKey);
                            if (iResult == 1) //Activation not found
                            {
                                this.Clear();
                                sLicenseKey = null;
                                sActivationKey = null;
                            }
                        }
                        catch
                        {
                            //If there is an exception (for example if there is no Internet connection),
                            //do nothing. Note that our servers may also not be available for any reason.
                        }
                        finally
                        {
                            if (objLicensingServiceClient != null)
                            {
                                objLicensingServiceClient.Dispose(); //We can call Dispose thanks to LicensingServiceFix
                                objLicensingServiceClient = null;
                            }
                        }
                    }
                }

                //Create Velodoc License and add to cache
                objLicenseRet = new VelodocLicense(sLicenseKey, sActivationKey);
                HttpRuntime.Cache.Add(
                    CACHEDLIC,
                    objLicenseRet,
                    null,
                    Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, Constants.SlidingExpiration, 0),
                    CacheItemPriority.Default,
                    null);
            }

            return objLicenseRet;
        }
        /// <summary>
        /// Set the license key, check it and persist it
        /// </summary>
        /// <param name="licenseKey"></param>
        /// <remarks>This function is our stuff and is not really part of the LicenseProvider design but there is no better place for it.</remarks>
        public void SetLicense(string licenseKey)
        {
            string sIPAddress = null;
            string sMacAddress = null;
            NetworkInterface[] arrNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface objNetworkInterface in arrNetworkInterfaces)
            {
                if ((objNetworkInterface.OperationalStatus == OperationalStatus.Up)
                    && (objNetworkInterface.Speed > 0)
                    && (objNetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    && (objNetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
                {
                    foreach (UnicastIPAddressInformation objIPAddressInfomation in objNetworkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (!objIPAddressInfomation.IPv4Mask.Equals(System.Net.IPAddress.Any))
                        {
                            sIPAddress = objIPAddressInfomation.Address.ToString();
                            sMacAddress = objNetworkInterface.GetPhysicalAddress().ToString();
                            break;
                        }
                    }
                    if (!String.IsNullOrEmpty(sIPAddress))
                        break;
                }
            }

            //Activate the license key using a web service, first building the contract data
            ActivationData objActivationData = new ActivationData();
            objActivationData.ActivationKey = Guid.NewGuid();
            try
            {
                objActivationData.LicenseKey = new Guid(licenseKey); //Raises a FormatException if not a Guid
            }
            catch (FormatException Ex)
            {
                throw new FormatException(String.Format(
                    Properties.Resources.Culture,
                    Properties.Resources.ExceptionLicenseProviderInvalidKey,
                    licenseKey),
                    Ex);
            }
            objActivationData.MachineName = Environment.MachineName;
            objActivationData.DomainName = Environment.UserDomainName;
            objActivationData.UserName = Environment.UserName;
            objActivationData.OSVersion = Environment.OSVersion.VersionString;
            objActivationData.ProcessorCount = Environment.ProcessorCount;
            objActivationData.FmkVersion = Environment.Version.ToString();
            objActivationData.IPAddress = sIPAddress; //<-- See above
            objActivationData.MACAddress = sMacAddress;
            objActivationData.ProductCode = PRODUCTCODE;
            objActivationData.CultureName = System.Globalization.CultureInfo.CurrentUICulture.Name;

            //Then configure the service
            BasicHttpBinding objHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            objHttpBinding.ProxyAddress = null;
            objHttpBinding.UseDefaultWebProxy = true;
            EndpointAddress objRemoteAddress = new EndpointAddress(LICENSINGSVC);

            //Finally call the service
            LicensingServiceClient objLicensingServiceClient = null;
            int iResult = -1;
            try
            {
                objLicensingServiceClient = new LicensingServiceClient(objHttpBinding, objRemoteAddress);
                iResult = objLicensingServiceClient.Activate(objActivationData);
            }
            finally
            {
                if (objLicensingServiceClient != null)
                {
                    objLicensingServiceClient.Dispose(); //We can call Dispose thanks to LicensingServiceFix
                    objLicensingServiceClient = null;
                }
            }

            if (iResult == 0)
            {
                //All instances on the same machine share the same license key: should not be an issue.
                RegistryKey objRegistryKey = Registry.LocalMachine.CreateSubKey(APPROOTKEY);
                objRegistryKey.SetValue(LICENSEKEY, objActivationData.LicenseKey.ToString());
                objRegistryKey.SetValue(ACTIVATIONKEY, objActivationData.ActivationKey.ToString());
                objRegistryKey.Close();
            }
            else if (iResult == 1)
            {
                throw new LicenseException(this.GetType(), this,
                    String.Format(Properties.Resources.Culture,
                        Properties.Resources.ExceptionLicenseProviderInvalidKey,
                        licenseKey));
            }
            else
            {
                throw new LicenseException(this.GetType(), this,
                    Properties.Resources.ExceptionLicenseProviderServiceError);
            }
        }
        /// <summary>
        /// Clears license info from registry
        /// </summary>
        public void Clear()
        {
            RegistryKey objRegistryKey = null;
            try
            {
                objRegistryKey = Registry.LocalMachine.CreateSubKey(APPROOTKEY);
                objRegistryKey.DeleteValue(LICENSEKEY);
                objRegistryKey.DeleteValue(ACTIVATIONKEY);
                string sKeyName = APPROOTKEY;
                while ((objRegistryKey.SubKeyCount + objRegistryKey.ValueCount).Equals(0))
                {
                    string sSubKeyName = sKeyName.Substring(sKeyName.LastIndexOf('\\') + 1);
                    sKeyName = sKeyName.Substring(0, sKeyName.LastIndexOf('\\'));
                    objRegistryKey.Close();
                    objRegistryKey = Registry.LocalMachine.CreateSubKey(sKeyName);
                    objRegistryKey.DeleteSubKey(sSubKeyName);
                }
            }
            catch
            {
            }
            finally
            {
                if (objRegistryKey != null)
                    objRegistryKey.Close();
            }
        }

        /*
         * The following code returns the number of system processors (kept for future use)
		 *		SYSTEM_INFO si;
		 *		GetSystemInfo(out si);
		 *		uint procCount = si.dwNumberOfProcessors;
         * 
        [DllImport("KERNEL32", CharSet = CharSet.Auto)]
        public static extern void GetSystemInfo(out SYSTEM_INFO si);

        public class ProcCountLicense : License
        {
            private ProcCountLicenseProvider owner;
            private string key;
            private int validProcCount;

            public ProcCountLicense(ProcCountLicenseProvider owner, string key)
            {
                this.owner = owner;
                this.key = key;
                this.validProcCount = Int32.Parse(key.Substring(key.IndexOf(',') + 1));
            }
            public override string LicenseKey
            {
                get
                {
                    return key;
                }
            }

            public int ValidProcCount
            {
                get
                {
                    return validProcCount;
                }
            }

            public override void Dispose()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };
        */
    }
}
