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
using System.Security.Cryptography;
using Memba.FileUpload;

namespace Memba.Tests
{
    /// <summary>
    ///This is a test class for Memba.FileUpload.MimeParser and is intended
    ///to contain all Memba.FileUpload.MimeParser Unit Tests
    ///</summary>
    [TestClass()]
    public class MimeParserTest
    {
        #region Private Members
        private TestContext testContextInstance;
        #endregion

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
        ///A test for Parse ()
        ///</summary>
        [DeploymentItem("Memba.FileUpload.XP.dll")]
        [TestMethod()]
        public void ParseTest()
        {
            //--------------------------------------------------------------------------------------------------------------------------
            // ie-utf8.bak has been created by dumping an upload post using the RequestSTreamDumpModule in place fo the UploadHttpModule
            // You can generate as many dumps and corresponding tests as required. We just provide one sample here.
            // The following values can be red when opening the dumped request with a text editor
            //--------------------------------------------------------------------------------------------------------------------------

            string REQUESTSTREAM = "..\\..\\Memba.Tests\\TestData\\ie-utf8.bak";
            long CONTENTLENGTH = 127535L;
            Encoding ENCODING = Encoding.UTF8;
            string MULTIPARTBOUNDARY = "-----------------------------7d68919104a6";

            StringBuilder objStringBuilder = new StringBuilder();
            objStringBuilder.Append("-----------------------------7d68919104a6\r\n");
            objStringBuilder.Append("Content-Disposition: form-data; name=\"__VIEWSTATE\"\r\n\r\n");
            objStringBuilder.Append("/wEPDwUKMTQ2OTkzNDMyMWRk/QGEIG6aoQo7wQztaRW4VtvYCBw=\r\n");
            objStringBuilder.Append("-----------------------------7d68919104a6\r\n");
            objStringBuilder.Append("Content-Disposition: form-data; name=\"UploadButton\"\r\n\r\n");
            objStringBuilder.Append("Upload\r\n");
            objStringBuilder.Append("-----------------------------7d68919104a6\r\n");
            objStringBuilder.Append("Content-Disposition: form-data; name=\"__EVENTVALIDATION\"\r\n\r\n");
            objStringBuilder.Append("/wEWAgKoz5qmBQLeyN+CDNaH4hynC6YqRKcear2EXPS7WTFO\r\n");
            objStringBuilder.Append("-----------------------------7d68919104a6--\r\n");
            
            string FILTEREDREQUEST = objStringBuilder.ToString();

            string sRequestDump = Path.Combine(TestContext.TestDir, REQUESTSTREAM);
            FileInfo objFileInfo = new FileInfo(sRequestDump);
            Assert.IsTrue(objFileInfo.Exists);

            FileStream objRequestStream = new FileStream(sRequestDump, FileMode.Open, FileAccess.Read);
            HashAlgorithm objHashAlgorithm = CryptoConfig.CreateFromName("SHA1") as HashAlgorithm;
            UploadData objUploadData = UploadMonitor.SetUploadData(null, "dummy");

            object requestFilterObject = Memba_FileUpload_RequestFilterAccessor.CreatePrivate(
                objRequestStream,
                objHashAlgorithm,
                CONTENTLENGTH,
                ENCODING,
                MULTIPARTBOUNDARY,
                objUploadData
                );

            Memba_FileUpload_RequestFilterAccessor requestFilterAccessor = new Memba_FileUpload_RequestFilterAccessor(requestFilterObject);

            object mimeParserObject = Memba_FileUpload_MimeParserAccessor.CreatePrivate(
                requestFilterAccessor
                );

            Memba_FileUpload_MimeParserAccessor mimeParserAccessor = new Memba_FileUpload_MimeParserAccessor(mimeParserObject);

            mimeParserAccessor.Parse();

            Assert.AreEqual(FILTEREDREQUEST, requestFilterAccessor._FilteredRequestStringBuilder.ToString());
        }
    }
}
