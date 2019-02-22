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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;

using System.IO;
using System.Security.AccessControl;
using System.DirectoryServices;
using System.Diagnostics;
using Microsoft.Win32;

namespace Memba.Tests
{
    /// <summary>
    ///This is a test class for Memba.Install.WebInstaller and is intended
    ///to contain all Memba.Install.WebInstaller Unit Tests
    ///</summary>
    [TestClass()]
    public class WebInstallerTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for AddNameToRegistryKey (string, string) and RemoveNameFromRegistryKey (string)
        ///</summary>
        [DeploymentItem("Memba.Install.XP.dll")]
        [TestMethod()]
        public void RegistryKeyTest()
        {
            const string KEY = "SOFTWARE\\Memba\\Velodoc\\XP Edition";
            const string NAME = "TEST";
            const string VALUE = "dummy";

            RegistryKey objRegistryKey;
            //objRegistryKey = Registry.LocalMachine.OpenSubKey(KEY);
            //Assert.IsTrue(objRegistryKey == null, "Make sure we do not mess with an existing installation of Velodoc");
            //objRegistryKey.Close();

            Memba.Tests.Memba_Install_WebInstallerAccessor.AddValueToRegistryKey(NAME, VALUE);

            objRegistryKey = Registry.LocalMachine.OpenSubKey(KEY);
            Assert.IsTrue(objRegistryKey != null);
            Assert.IsTrue(((string)objRegistryKey.GetValue(NAME)) == VALUE);
            objRegistryKey.Close();

            Memba.Tests.Memba_Install_WebInstallerAccessor.RemoveValueFromRegistryKey(NAME);

            objRegistryKey = Registry.LocalMachine.OpenSubKey(KEY);
            if (objRegistryKey == null)
                Assert.IsTrue(true); //We are good
            else
            {
                Assert.IsTrue(objRegistryKey.GetValue(NAME) == null); //At least the name does not exist anymore
                objRegistryKey.Close();
            }
        }
        /// <summary>
        ///A test for AddStoragePermissions (string, DirectoryInfo)
        ///</summary>
        [DeploymentItem("Memba.Install.dll")]
        [TestMethod()]
        public void AddStoragePermissionsTest()
        {
            string identity = Environment.UserDomainName + "\\" + Environment.UserName;
            DirectoryInfo di = new DirectoryInfo(Constants.Directory);
            Memba.Tests.Memba_Install_WebInstallerAccessor.AddStoragePermissions(identity, di);

            //Check
            DirectorySecurity ds = di.GetAccessControl();
            AuthorizationRuleCollection colRules = ds.GetAccessRules(true, false, typeof(System.Security.Principal.NTAccount));
            System.Collections.IEnumerator objEnumerator = colRules.GetEnumerator();
            bool bFound = false;
            while (objEnumerator.MoveNext())
            {
                AuthorizationRule objRule = objEnumerator.Current as AuthorizationRule;
                if (objRule.IdentityReference.Value.Equals(identity))
                    bFound = true;
            }
            Assert.IsTrue(bFound);

            //Rollback
            ds.RemoveAccessRule(new FileSystemAccessRule(identity, FileSystemRights.Modify, AccessControlType.Allow));
#if(DEBUG)
            colRules = ds.GetAccessRules(true, false, typeof(System.Security.Principal.NTAccount));
            objEnumerator = colRules.GetEnumerator();
            while (objEnumerator.MoveNext())
            {
                AuthorizationRule objRule = objEnumerator.Current as AuthorizationRule;
                if (objRule.IdentityReference.Value.Equals(identity))
                    Assert.Fail("Cannot rollback");
            }
#endif
        }
        /// <summary>
        ///A test for FindServerNum (string)
        ///</summary>
        [DeploymentItem("Memba.Install.dll")]
        [TestMethod()]
        public void FindServerNumTest()
        {
            string actual = Memba.Tests.Memba_Install_WebInstallerAccessor.FindServerNum(Constants.Port);
            Assert.AreEqual(Constants.ServerNum, actual);
        }
        /// <summary>
        ///A test for MapDatExtensionToAspNet (string, string)
        ///</summary>
        [DeploymentItem("Memba.Install.dll")]
        [TestMethod()]
        public void MapDatExtensionToAspNetTest()
        {
            const string MAPS = "ScriptMaps";

            string sAppPath = Memba.Tests.Memba_Install_WebInstallerAccessor.W3SVC
                + "/" + Constants.ServerNum + "/ROOT/" + Constants.VirtualDir;
            string sSite = Memba.Tests.Memba_Install_WebInstallerAccessor.METABASEROOT
                + Constants.ServerNum;

            WebInstallerTest.EnumerateProperties(sAppPath);
            Memba.Tests.Memba_Install_WebInstallerAccessor.MapDatExtensionToAspNet(Constants.VirtualDir, sSite);
            WebInstallerTest.EnumerateProperties(sAppPath);

            //Note: it may have been preferrable to use System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
            string sDatMapping = ".dat," + Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD";
            string sDatMapping64 = ".dat," + Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework64\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD";

            DirectoryEntry objApplication = new DirectoryEntry(sAppPath);
            objApplication.RefreshCache();
            int iPos = objApplication.Properties[MAPS].IndexOf(sDatMapping);
            if (iPos < 0)
                Assert.Fail("Mapping not found");
            else
                objApplication.Properties[MAPS].RemoveAt(iPos); //Rollback
            if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") != "x86") //i.e. AMD64 or I64. Note that 32-bit OS installed on 64-bit processor returns x86
            {
                iPos = objApplication.Properties[MAPS].IndexOf(sDatMapping64);
                if (iPos < 0)
                    Assert.Fail("Mapping 64 not found");
                else
                    objApplication.Properties[MAPS].RemoveAt(iPos); //Rollback
            }
            objApplication.CommitChanges();
            objApplication.Close();

            Assert.IsTrue(true); //Success
        }
        /// <summary>
        ///A test for Switch404Page (string, string, string)
        ///</summary>
        [DeploymentItem("Memba.Install.dll")]
        [TestMethod()]
        public void Switch404PageTest()
        {
            const string ERRORS = "HttpErrors";

            string sAppPath = Memba.Tests.Memba_Install_WebInstallerAccessor.W3SVC
                + "/" + Constants.ServerNum + "/ROOT/" + Constants.VirtualDir;
            string sSite = Memba.Tests.Memba_Install_WebInstallerAccessor.METABASEROOT
                + Constants.ServerNum;

            //Find 404 page
            string s404Page = Path.Combine(TestContext.TestDir, "..\\..\\XP\\404.htm");
            FileInfo objFileInfo = new FileInfo(s404Page);
            Assert.IsTrue(objFileInfo.Exists);

            WebInstallerTest.EnumerateProperties(sAppPath);
            Memba.Tests.Memba_Install_WebInstallerAccessor.Switch404Page(Constants.VirtualDir, sSite, objFileInfo.DirectoryName);
            WebInstallerTest.EnumerateProperties(sAppPath);

            DirectoryEntry objApplication = new DirectoryEntry(sAppPath);
            if (objApplication.Properties[ERRORS].Value is Array)
            {
                Array arrErrors = (Array)objApplication.Properties[ERRORS].Value;
                bool bFound = false;
                string sRollback = String.Empty;
                int iRollback = -1;
                string s404 = String.Empty;
                for (int iPos = 0; iPos < arrErrors.Length; iPos++)
                {
                    if ((arrErrors.GetValue(iPos) != null) && ((string)arrErrors.GetValue(iPos)).StartsWith("401"))
                    {
                        sRollback = (string)arrErrors.GetValue(iPos);
                        sRollback = sRollback.Replace("401", "404");
                    }
                    if ((arrErrors.GetValue(iPos) != null) && ((string)arrErrors.GetValue(iPos)).StartsWith("404"))
                    {
                        //Assuming there is only one
                        s404 = (string)arrErrors.GetValue(iPos);
                        iRollback = iPos;
                        bFound = true;
                        break;
                    }
                }
                if ((bFound) && (iRollback > -1) && (!String.IsNullOrEmpty(sRollback)))
                {
                    objApplication.Properties[ERRORS].RemoveAt(iRollback);
                    objApplication.Properties[ERRORS].Insert(iRollback, sRollback);
                    objApplication.CommitChanges();
                    Assert.IsTrue(s404.Contains(objFileInfo.FullName));
                }
                else
                {
                    Assert.Fail("404 Error page not found");
                }
            }
            objApplication.Close();
        }
        /// <summary>
        ///A test for RegisterASPNET2 (string, string)
        ///</summary>
        [DeploymentItem("Memba.Install.XP.dll")]
        [TestMethod()]
        public void RegisterASPNET2Test()
        {
            const string MAPS = "ScriptMaps";

            string sSite = Memba.Tests.Memba_Install_WebInstallerAccessor.METABASEROOT
                + Constants.ServerNum;
            string sAppPath = Memba.Tests.Memba_Install_WebInstallerAccessor.W3SVC
                + "/" + Constants.ServerNum + "/ROOT/" + Constants.VirtualDir;

            WebInstallerTest.EnumerateProperties(sAppPath);
            Memba.Tests.Memba_Install_WebInstallerAccessor.RegisterASPNET2(Constants.VirtualDir, sSite);
            WebInstallerTest.EnumerateProperties(sAppPath);

            DirectoryEntry objApplication = new DirectoryEntry(sAppPath);
            objApplication.RefreshCache();
            //Maybe it would be preferrable to use System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
            string sAspNetMapping = ".aspx," + Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG";
            string sAspNetMapping64 = ".aspx," + Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework64\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG";
            int iPos = objApplication.Properties[MAPS].IndexOf(sAspNetMapping);
            if (iPos < 0)
                Assert.Fail(".NET 2.0 not installed on " + sAppPath);
            if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") != "x86") //i.e. AMD64 or I64. Note that 32-bit OS installed on 64-bit processor returns x86
            {
                iPos = objApplication.Properties[MAPS].IndexOf(sAspNetMapping64);
                if (iPos < 0)
                    Assert.Fail("Mapping 64 not found");
            }
            objApplication.CommitChanges();
            objApplication.Close();

            Assert.IsTrue(true); //Success

        }
        /// <summary>
        ///A test for UpdateAppConfig (string)
        ///</summary>
        [DeploymentItem("Memba.Install.dll")]
        [TestMethod()]
        public void UpdateAppConfigTest()
        {
            //Find app.config
            string sAppConfig = Path.Combine(TestContext.TestDir, "..\\..\\Memba.PurgeService\\app.config");
            FileInfo objFileInfo = new FileInfo(sAppConfig);
            Assert.IsTrue(objFileInfo.Exists);

            //Make a working copy
            string sWorkAppConfig = Path.Combine(Constants.Directory, "Memba.PurgeService.XP.exe.config");
            objFileInfo.CopyTo(sWorkAppConfig, true);
            objFileInfo = new FileInfo(sWorkAppConfig);
            Assert.IsTrue(objFileInfo.Exists);
            if (objFileInfo.Exists)
                objFileInfo.Attributes &= ~FileAttributes.ReadOnly; //We use source control

            //Call UpdateWebConfig
            Memba.Tests.Memba_Install_WebInstallerAccessor.UpdateAppConfig(Constants.Directory, Constants.BadDirectory);

            //Check changes
            using (StreamReader objStreamReader = objFileInfo.OpenText())
            {
                string sConfig = objStreamReader.ReadToEnd();
                Assert.IsTrue(sConfig.Contains("key=\"Directory\" value=\"" + Constants.BadDirectory + "\""));
            }

            //Purge
            objFileInfo.Delete();
        }
        /// <summary>
        ///A test for UpdateSmtpConfig (string, string, string, string, string, string, bool)
        ///</summary>
        [DeploymentItem("Memba.Install.XP.dll")]
        [TestMethod()]
        public void UpdateSmtpConfigTest()
        {
            //Find web.config
            string sWebConfig = Path.Combine(TestContext.TestDir, "..\\..\\XP\\web.config");
            FileInfo objFileInfo = new FileInfo(sWebConfig);
            Assert.IsTrue(objFileInfo.Exists);

            //Make a working copy
            string sWorkWebConfig = Path.Combine(Constants.Directory, "web.config");
            objFileInfo.CopyTo(sWorkWebConfig, true);
            objFileInfo = new FileInfo(sWorkWebConfig);
            Assert.IsTrue(objFileInfo.Exists);
            if (objFileInfo.Exists)
                objFileInfo.Attributes &= ~FileAttributes.ReadOnly; //We use source control

            string host = "smtp.hotmail.com";
            string port = "25";
            string from = "noreply@acme.com";
            string userName = "COMPUTERNAME\\SmtpUser";
            string password = "1234567890";

            //Call UpdateSmtpConfig with specific credentials
            Memba.Tests.Memba_Install_WebInstallerAccessor.UpdateSmtpConfig(
                Constants.Directory,
                host,
                port,
                from,
                userName,
                password,
                false
                );

            //Check changes
            using (StreamReader objStreamReader = objFileInfo.OpenText())
            {
                string sConfig = objStreamReader.ReadToEnd();
                Assert.IsTrue(sConfig.Contains("from=\"" + from + "\""));
                Assert.IsTrue(sConfig.Contains("host=\"" + host + "\""));
                Assert.IsTrue(sConfig.Contains("port=\"" + port + "\""));
                Assert.IsTrue(sConfig.Contains("userName=\"" + userName + "\""));
                Assert.IsTrue(sConfig.Contains("password=\"" + password + "\""));
            }

            //Call UpdateSmtpConfig with default credentials
            Memba.Tests.Memba_Install_WebInstallerAccessor.UpdateSmtpConfig(
                Constants.Directory,
                host,
                port,
                from,
                null,
                null,
                true
                );

            //Check changes
            using (StreamReader objStreamReader = objFileInfo.OpenText())
            {
                string sConfig = objStreamReader.ReadToEnd();
                Assert.IsTrue(sConfig.Contains("from=\"" + from + "\""));
                Assert.IsTrue(sConfig.Contains("host=\"" + host + "\""));
                Assert.IsTrue(sConfig.Contains("port=\"" + port + "\""));
                Assert.IsTrue(sConfig.Contains("defaultCredentials=\"true\""));
            }

            //Purge
            objFileInfo.Delete();
        }
        /// <summary>
        ///A test for UpdateStorageConfig (string, string)
        ///</summary>
        [DeploymentItem("Memba.Install.XP.dll")]
        [TestMethod()]
        public void UpdateStorageConfigTest()
        {
            //Find web.config
            string sWebConfig = Path.Combine(TestContext.TestDir, "..\\..\\XP\\web.config");
            FileInfo objFileInfo = new FileInfo(sWebConfig);
            Assert.IsTrue(objFileInfo.Exists);

            //Make a working copy
            string sWorkWebConfig = Path.Combine(Constants.Directory, "web.config");
            objFileInfo.CopyTo(sWorkWebConfig, true);
            objFileInfo = new FileInfo(sWorkWebConfig);
            Assert.IsTrue(objFileInfo.Exists);
            if (objFileInfo.Exists)
                objFileInfo.Attributes &= ~FileAttributes.ReadOnly; //We use source control

            //Call UpdateStorageConfig
            Memba.Tests.Memba_Install_WebInstallerAccessor.UpdateStorageConfig(Constants.Directory, Constants.BadDirectory);

            //Check changes
            using (StreamReader objStreamReader = objFileInfo.OpenText())
            {
                string sConfig = objStreamReader.ReadToEnd();
                Assert.IsTrue(sConfig.Contains("connectionString=\"" + Constants.BadDirectory + "\""));
            }

            //Purge
            objFileInfo.Delete();
        }
        /// <summary>
        ///A test for UpdateWebConfig (string)
        ///</summary>
        [DeploymentItem("Memba.Install.dll")]
        [TestMethod()]
        public void UpdateWebConfigTest()
        {
            //Find web.config
            string sWebConfig = Path.Combine(TestContext.TestDir, "..\\..\\XP\\web.config");
            FileInfo objFileInfo = new FileInfo(sWebConfig);
            Assert.IsTrue(objFileInfo.Exists);

            //Make a working copy
            string sWorkWebConfig = Path.Combine(Constants.Directory, "web.config");
            objFileInfo.CopyTo(sWorkWebConfig, true);
            objFileInfo = new FileInfo(sWorkWebConfig);
            Assert.IsTrue(objFileInfo.Exists);
            if (objFileInfo.Exists)
                objFileInfo.Attributes &= ~FileAttributes.ReadOnly; //We use source control

            string userList = "a@acme.com;b@acme.com;c@acme.com;d@acme.com";
            string securityCode = "wxyz$%*=";

            Memba.Tests.Memba_Install_WebInstallerAccessor.UpdateWebConfig(
                Constants.Directory,
                userList,
                securityCode
                );

            //Check changes
            using (StreamReader objStreamReader = objFileInfo.OpenText())
            {
                string sConfig = objStreamReader.ReadToEnd();
                Assert.IsTrue(sConfig.Contains("key=\"UserList\" value=\"" + userList + "\""));
                Assert.IsTrue(sConfig.Contains("key=\"SecurityCode\" value=\"" + securityCode + "\""));
            }

            //Purge
            objFileInfo.Delete();
        }

        #region Debugging Helpers
        /// <summary>
        /// Enumerate children ADSI objects
        /// </summary>
        /// <param name="metabasePath"></param>
        static void EnumerateChildren(string metabasePath)
        {
            //  metabasePath is of the form "IIS://<servername>/<path>"
            //    for example "IIS://localhost/W3SVC/1/Root/MyVDir" 
            //    or "IIS://localhost/W3SVC/AppPools/MyAppPool"
            Trace.WriteLine("\nEnumerating children for " + metabasePath);

            try
            {
                DirectoryEntry entry = new DirectoryEntry(metabasePath);
                DirectoryEntries children = entry.Children;

                System.Collections.IEnumerator enumerator = children.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DirectoryEntry de = enumerator.Current as DirectoryEntry;
                    if (de != null)
                        Trace.WriteLine("\t" + de.Name);
                    de.Close();
                }
                entry.Close();

                Trace.WriteLine("-->Done.");
            }
            catch (Exception Ex)
            {
                Trace.WriteLine("Failed in EnumerateChildren with the following exception: " + Ex.Message);
            }
        }
        /// <summary>
        /// Enumerate ADSI properties
        /// </summary>
        /// <param name="metabasePath"></param>
        static void EnumerateProperties(string metabasePath)
        {
            //  metabasePath is of the form "IIS://<servername>/<path>"
            //    for example "IIS://localhost/W3SVC/1/Root/MyVDir" 
            //    or "IIS://localhost/W3SVC/AppPools/MyAppPool"
            Trace.WriteLine("\nEnumerating properties for " + metabasePath);

            try
            {
                DirectoryEntry entry = new DirectoryEntry(metabasePath);
                PropertyCollection props = entry.Properties;

                Trace.WriteLine("Total properties = " + props.Count.ToString());

                foreach (string propName in props.PropertyNames)
                {
                    Trace.WriteLine(propName + " =");
                    foreach (object value in entry.Properties[propName])
                    {
                        Trace.WriteLine("\t" + value.ToString() + "\t" + value.GetType());
                    }
                }
                entry.Close();
                Trace.WriteLine("-->Done.");

            }
            catch (Exception Ex)
            {
                Trace.WriteLine("Failed in EnumerateProperties with the following exception: " + Ex.Message);
            }
        }
        #endregion
    }
}
