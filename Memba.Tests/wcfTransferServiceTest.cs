﻿// The following code was generated by Microsoft Visual Studio 2005.
// The test owner should check each test for validity.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net; //HttpWebRequest
using System.IO; //FileStream
using System.ServiceModel; //EndpointAddress, BasicHttpBinding
using Channels = System.ServiceModel.Channels; //Binding
using Microsoft.Win32; //Registry, RegistryKey

namespace Memba.Tests
{
    /// <summary>
    ///This is a test class for TransferServiceClient and is intended
    ///to contain all TransferServiceClient Unit Tests
    ///</summary>
    [TestClass()]
    public class TransferServiceClientTest
    {
        private TestContext testContextInstance;

        private static FileInfo _FileInfo = new FileInfo(Constants.File0);
        private static Uri _RemoteUri;
        private static bool _IsUploadComplete = false;

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
        ///A test for Authenticate (string, string)
        ///</summary>
        [TestMethod()]
        public void AuthenticateTest()
        {
            using (TransferServiceClient objClientProxy = new TransferServiceClient(this.GetWcfTransferServiceBinding(), this.GetWcfTransferServiceEndpoint()))
            {
                bool bRet = objClientProxy.Authenticate(Constants.Sender, Constants.SecurityCode);
                Assert.IsTrue(bRet, "Wrong credentials to access wcf service.");
            }
        }
        /// <summary>
        ///A test for Upload (string, string, System.Guid, string, string, long, string, System.IO.Stream)
        ///</summary>
        [TestMethod()]
        public void UploadTest()
        {
            Assert.IsTrue(_FileInfo.Exists, String.Format("Test file \"{0}\" does not exist.", _FileInfo.FullName));
            //Open local input stream (We can call dispose/using because of WCFTransferServiceFix)
            using (FileStream objFileStream = _FileInfo.OpenRead())
            //Start service client
            using (TransferServiceClient objClientProxy = new TransferServiceClient(this.GetWcfTransferServiceBinding(), this.GetWcfTransferServiceEndpoint()))
            {
                Guid gFileGuid = Guid.NewGuid();
                _RemoteUri = new Uri(Constants.DevWebSite + gFileGuid.ToString("N") + ".dat");
                objClientProxy.Upload(
                    this.GetMimeType(_FileInfo.Name),
                    Constants.Sender,
                    gFileGuid,
                    _FileInfo.Name,
                    String.Empty,
                    _FileInfo.Length,
                    Constants.SecurityCode,
                    objFileStream);
            }
            Assert.IsTrue(true); //If we have reached here, we should be OK. We could consider tracking the uploaded file in the storage directory 
            _IsUploadComplete = true;
        }
        /// <summary>
        ///A test for GetFileSize (System.Uri)
        ///</summary>
        [TestMethod()]
        public void GetFileSizeTest()
        {
            int iTimeout = 0;
            while (!_IsUploadComplete && iTimeout < 10000)
            {
                System.Threading.Thread.Sleep(500);
                iTimeout += 500;
            }

            Assert.IsNotNull(_RemoteUri, "You need to run this test in sequence with upload test.");

            using (TransferServiceClient objClientProxy = new TransferServiceClient(this.GetWcfTransferServiceBinding(), this.GetWcfTransferServiceEndpoint()))
            {
                long lSize = objClientProxy.GetFileSize(_RemoteUri);
                Assert.AreEqual<long>(_FileInfo.Length, lSize);
            }
        }
        /// <summary>
        ///A test for Download (System.Uri, out string, out System.Guid, out string, out string, out long, out string, out System.IO.Stream)
        ///</summary>
        [TestMethod()]
        public void DownloadTest()
        {
            int iTimeout = 0;
            while (!_IsUploadComplete && iTimeout < 10000)
            {
                System.Threading.Thread.Sleep(500);
                iTimeout += 500;
            }

            Assert.IsNotNull(_RemoteUri, "You need to run this test in sequence with upload test.");
            
            string sLocalFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), _FileInfo.Name);
            FileInfo objLocalFileInfo = new FileInfo(sLocalFile);
            if (objLocalFileInfo.Exists)
            {
                objLocalFileInfo.Attributes &= ~FileAttributes.ReadOnly;
                objLocalFileInfo.Delete();
            }

            TransferServiceClient objClientProxy = null;
            Stream objRemoteStream = null;
            Stream objLocalStream = null;
            try
            {
                //We cannot use the using pattern here because it is incompatible with out ref parameters
                objClientProxy = new TransferServiceClient(this.GetWcfTransferServiceBinding(), this.GetWcfTransferServiceEndpoint());
                string sEmail;
                Guid gFileGuid;
                string sFileName;
                string sHashCode;
                long lLength;
                string sSecurityCode;

                objClientProxy.Download(
                    _RemoteUri,
                    out sEmail, //returns null
                    out gFileGuid,
                    out sFileName,
                    out sHashCode,
                    out lLength,
                    out sSecurityCode, //returns null
                    out objRemoteStream);

                Assert.IsNull(sEmail);
                Assert.IsNull(sSecurityCode);

                objLocalStream = objLocalFileInfo.Create();
                byte[] arrBuffer = new byte[4096];
                int iBytesRead;
                do
                {
                    iBytesRead = objRemoteStream.Read(arrBuffer, 0, arrBuffer.Length);
                    objLocalStream.Write(arrBuffer, 0, iBytesRead);
                } while (iBytesRead > 0);

                Assert.IsTrue(objLocalFileInfo.Exists);
                Assert.AreEqual<long>(_FileInfo.Length, objLocalFileInfo.Length);
            }
            finally
            {
                if (objClientProxy != null)
                {
                    objClientProxy.Dispose(); //<-- We can call dispose because of WCFTransferServiceFix
                    objClientProxy = null;
                }
                if (objRemoteStream != null)
                {
                    objRemoteStream.Close();
                    objRemoteStream = null;
                }
                if (objLocalStream != null)
                {
                    objLocalStream.Close();
                    objLocalStream = null;
                }
            }
        }

        #region Private Helpers
        /// <summary>
        /// Gets the WCF service endpoint
        /// </summary>
        /// <returns></returns>
        private EndpointAddress GetWcfTransferServiceEndpoint()
        {
            #region Test for IIS
            string sEndpointAddress = Constants.IISWebSite + "wcfTransferService.svc";
            HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(sEndpointAddress + "?wsdl");
            //objHttpWebRequest.Method = "HEAD"; //<-- Fail
            using (HttpWebResponse objHttpWebResponse = (HttpWebResponse)objHttpWebRequest.GetResponse())
            {
                string sServer = objHttpWebResponse.Headers[HttpResponseHeader.Server];
                //The server header for Cassini is "ASP.NET Development Server/8.0.0.0"
                Assert.IsTrue(sServer.StartsWith("Microsoft-IIS"), "WCF streaming services only work in IIS, not in Cassini/Visual Studio Web Server");
            }
            #endregion

            return new EndpointAddress(sEndpointAddress);
        }
        /// <summary>
        /// Gets the WCF Service Binding
        /// </summary>
        /// <returns></returns>
        private Channels.Binding GetWcfTransferServiceBinding()
        {
            BasicHttpBinding objBindingRet = new BasicHttpBinding();
            objBindingRet.MessageEncoding = WSMessageEncoding.Mtom;
            objBindingRet.TransferMode = TransferMode.Streamed;
            objBindingRet.MaxBufferSize = 65536;
            objBindingRet.MaxReceivedMessageSize = 67108864;
            objBindingRet.Security.Mode = BasicHttpSecurityMode.None; //No SSL
            objBindingRet.ProxyAddress = null;
            objBindingRet.UseDefaultWebProxy = true; //Automatic Proxy
#if DEBUG
            //Increase timeouts to allow step-by-step debugging
            objBindingRet.ReceiveTimeout = new TimeSpan(0, 5, 0);
            objBindingRet.SendTimeout = new TimeSpan(0, 5, 0);
#endif           
            return objBindingRet;
        }
        /// <summary>
        /// Gets the MIME content type of a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetMimeType(string fileName)
        {
            const string DefaultMimeType = "application/octetstream";
            const string MimeTypeKey = "Content Type";
            string sMimeTypeRet = DefaultMimeType;
            string sExtension = System.IO.Path.GetExtension(fileName).ToLower();
            RegistryKey objRegistryKey = Registry.ClassesRoot.OpenSubKey(sExtension);
            if (objRegistryKey != null)
            {
                string temp = (string)objRegistryKey.GetValue(MimeTypeKey);
                if (temp != null)
                    sMimeTypeRet = temp;
            }
            return sMimeTypeRet;
        }
        #endregion
    }
}
