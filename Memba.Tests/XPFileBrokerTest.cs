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
using MF = Memba.Files.Business;
using System.IO;

namespace Memba.Tests
{
    /// <summary>
    ///This is a test class for Memba.Files.Business.FileBroker and is intended
    ///to contain all Memba.Files.Business.FileBroker Unit Tests
    ///</summary>
    [TestClass()]
    public class FileBrokerTest
    {
        #region Private Members
        private TestContext testContextInstance;
        
        private static Guid Guid = Guid.Empty;
        private static string FileName;
        private static string ContentType;
        private static string Key;
        private static long Size;
        private static string HashValue;
        private static DateTime CreatedOn;
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            FileBrokerTest.FileName = "my great large file.ext";
            FileBrokerTest.ContentType = "application/octet-_Input";
            FileBrokerTest.Key = Guid.NewGuid().ToString();
            FileBrokerTest.Size = 1234567890L;
            FileBrokerTest.HashValue = "91bc2424e1c739b7eea12f372e0f6d07798b0edf";
        }
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
        ///A test for Insert (File, DirectoryInfo)
        ///</summary>
        [TestMethod()]
        public void InsertTest()
        {
            MF.File f = null;
            DirectoryInfo di = null;

            #region Null values
            try
            {
                Memba.Files.Business.FileBroker.Insert(f, di);
            }
            catch(Exception Ex)
            {
                Assert.IsInstanceOfType(Ex, typeof(ArgumentNullException));
            }
            #endregion

            f = new MF.File(
                FileBrokerTest.FileName,
                FileBrokerTest.ContentType,
                FileBrokerTest.Key,
                FileBrokerTest.Size,
                FileBrokerTest.HashValue);
            di = new DirectoryInfo(Constants.BadDirectory);
            
            #region Directory does not exist
            try
            {
                Memba.Files.Business.FileBroker.Insert(f, di);
            }
            catch (Exception Ex)
            {
                Assert.IsInstanceOfType(Ex, typeof(ArgumentException));
            }
            #endregion

            di = new DirectoryInfo(Constants.Directory);
            
            #region Correct values
            Memba.Files.Business.FileBroker.Insert(f, di);
            FileBrokerTest.Guid = f.Guid;
            FileBrokerTest.CreatedOn = f.CreatedOn;
            string sDefinitionFile = FileBrokerTest.Guid.ToString("N") + ".def";
            FileInfo objFileInfo = new FileInfo(Path.Combine(Constants.Directory, sDefinitionFile));
            Assert.IsTrue(objFileInfo.Exists);
            #endregion
        }

        /// <summary>
        ///A test for SelectByGuid (Guid, DirectoryInfo)
        ///</summary>
        [TestMethod()]
        public void SelectByGuidTest()
        {
            
            Guid g = Guid.Empty;
            DirectoryInfo di = null;
            MF.File actual;

            #region Null/Empty values
            try
            {
                actual = Memba.Files.Business.FileBroker.SelectByGuid(g, di);
            }
            catch (Exception Ex)
            {
                Assert.IsInstanceOfType(Ex, typeof(ArgumentException));
            }
            #endregion

            Assert.IsFalse(FileBrokerTest.Guid.Equals(Guid.Empty), "You need to execute InsertTest first");
            g = FileBrokerTest.Guid;
            di = new DirectoryInfo(Constants.BadDirectory);
            
            #region Directory does not exist
            try
            {
                actual = Memba.Files.Business.FileBroker.SelectByGuid(g, di);
            }
            catch (Exception Ex)
            {
                Assert.IsInstanceOfType(Ex, typeof(ArgumentException));
            }
            #endregion

            di = new DirectoryInfo(Constants.Directory);

            #region Correct values
            actual = Memba.Files.Business.FileBroker.SelectByGuid(g, di);
            Assert.AreEqual(FileBrokerTest.FileName, actual.FileName);
            Assert.AreEqual(FileBrokerTest.ContentType, actual.ContentType);
            Assert.AreEqual(FileBrokerTest.Key, actual.Key);
            Assert.AreEqual(FileBrokerTest.Size, actual.Size);
            Assert.AreEqual(FileBrokerTest.HashValue, actual.HashValue);
            Assert.AreEqual(FileBrokerTest.CreatedOn, actual.CreatedOn);
            #endregion
        }
    }
}
