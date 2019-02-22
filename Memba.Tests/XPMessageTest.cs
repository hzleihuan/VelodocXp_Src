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
using Memba.Files.Business;

namespace Memba.Tests
{
    /// <summary>
    ///This is a test class for Memba.Files.Business.Message and is intended
    ///to contain all Memba.Files.Business.Message Unit Tests
    ///</summary>
    [TestClass()]
    public class MessageTest
    {
        #region Private Members
        private TestContext testContextInstance;
        #endregion

        #region Property Accessors
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
        #endregion

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
        ///A test for Send ()
        ///</summary>
        [TestMethod()]
        public void SendTest()
        {
            try
            {
                string Text;
                List<File> Attachments = new List<File>();

                Text = "Hello John,\nPlease find herewith some holiday pictures.\nBest regards,\nJoe Bloggs.";
                File f1 = new File(
                    "DSC00001.jpg",
                    "image/jpeg",
                    Guid.NewGuid().ToString("N"),
                    123456789L,
                    "11bc2424e1c739b7eea12f372e0f6d07798b0eda");
                Attachments.Add(f1);
                File f2 = new File(
                    "DSC00002.jpg",
                    "image/jpeg",
                    Guid.NewGuid().ToString("N"),
                    234567891L,
                    "21bc2424e1c739b7eea12f372e0f6d07798b0edb");
                Attachments.Add(f2);
                File f3 = new File(
                    "DSC00003.jpg",
                    "image/jpeg",
                    Guid.NewGuid().ToString("N"),
                    345678912L,
                    "31bc2424e1c739b7eea12f372e0f6d07798b0edc");
                Attachments.Add(f3);
                
                Message target = new Message(
                    Constants.Sender,
                    Constants.Recipient,
                    Text,
                    Attachments,
                    Constants.DevWebSite);

                target.Send();
                
                Assert.IsTrue(true); //If no exception thrown by Send method, we are ok
            }
            catch(System.Net.Mail.SmtpException)
            {
                Assert.Inconclusive("Configure SMTP in App.config.");
            }
        }
    }
}
