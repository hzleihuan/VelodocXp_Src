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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Memba.Dns;

namespace Memba.Dns.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ResolverTest
    {
        public ResolverTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        /// <summary>
        ///A test for an A Lookup (Request)
        ///</summary>
        [TestMethod()]
        public void DNS_RS01_ALookupTest()
        {
            Request objRequest1 = new Request();
            objRequest1.AddQuestion(new Question("memba.com", DnsType.ANAME, DnsClass.IN));
            Response objResponse1 = Resolver.Lookup(objRequest1);
            Assert.IsNotNull(objResponse1);
            Assert.AreEqual(objResponse1.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse1.Answers.Length, 1);
            Assert.AreEqual(objResponse1.Answers[0].Record.GetType(), typeof(ANameRecord));
            Assert.AreEqual(((ANameRecord)objResponse1.Answers[0].Record).IPAddress, IPAddress.Parse("69.65.102.245"));

            //This one should use the cache
            Response objResponse1b = Resolver.Lookup(objRequest1);
            Assert.IsNotNull(objResponse1b);
            Assert.AreEqual(objResponse1b.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse1b.Answers.Length, 1);
            Assert.AreEqual(objResponse1b.Answers[0].Record.GetType(), typeof(ANameRecord));
            Assert.AreEqual(((ANameRecord)objResponse1b.Answers[0].Record).IPAddress, IPAddress.Parse("69.65.102.245"));
            
            IPAddress[] arrIPAddresses2 = new IPAddress[] { IPAddress.Parse("64.233.167.99"), IPAddress.Parse("64.233.187.99"), IPAddress.Parse("72.14.207.99")};
            Request objRequest2 = new Request();
            objRequest2.AddQuestion(new Question("google.com", DnsType.ANAME, DnsClass.IN));
            Response objResponse2 = Resolver.Lookup(objRequest2);
            Assert.IsNotNull(objResponse2);
            Assert.AreEqual(objResponse2.ReturnCode, ReturnCode.Success);
            Assert.IsTrue(objResponse2.Answers.Length > 0);
            foreach (Answer objAnswer in objResponse2.Answers)
            {
                Assert.AreEqual(objAnswer.Record.GetType(), typeof(ANameRecord));
                Assert.IsTrue(Array.IndexOf<IPAddress>(arrIPAddresses2, ((ANameRecord)objAnswer.Record).IPAddress) > -1);
            }

            IPAddress[] arrIPAddresses3 = new IPAddress[] { IPAddress.Parse("207.46.197.32"), IPAddress.Parse("207.46.232.182") };
            Request objRequest3 = new Request();
            objRequest3.AddQuestion(new Question("microsoft.com", DnsType.ANAME, DnsClass.IN));
            Response objResponse3 = Resolver.Lookup(objRequest3);
            Assert.IsNotNull(objResponse3);
            Assert.AreEqual(objResponse3.ReturnCode, ReturnCode.Success);
            Assert.IsTrue(objResponse3.Answers.Length > 0);
            foreach (Answer objAnswer in objResponse3.Answers)
            {
                Assert.AreEqual(objAnswer.Record.GetType(), typeof(ANameRecord));
                Assert.IsTrue(Array.IndexOf<IPAddress>(arrIPAddresses3, ((ANameRecord)objAnswer.Record).IPAddress) > -1);
            }

            IPAddress[] arrIPAddresses4 = new IPAddress[] { IPAddress.Parse("68.180.206.184"), IPAddress.Parse("206.190.60.37") };
            Request objRequest4 = new Request();
            objRequest4.AddQuestion(new Question("yahoo.com", DnsType.ANAME, DnsClass.IN));
            Response objResponse4 = Resolver.Lookup(objRequest4);
            Assert.IsNotNull(objResponse4);
            Assert.AreEqual(objResponse4.ReturnCode, ReturnCode.Success);
            Assert.IsTrue(objResponse4.Answers.Length > 0);
            foreach (Answer objAnswer in objResponse4.Answers)
            {
                Assert.AreEqual(objAnswer.Record.GetType(), typeof(ANameRecord));
                Assert.IsTrue(Array.IndexOf<IPAddress>(arrIPAddresses4, ((ANameRecord)objAnswer.Record).IPAddress) > -1);
            }
        }
        /// <summary>
        ///A test for a MX Lookup (Request)
        ///</summary>
        [TestMethod()]
        public void DNS_RS02_MXLookupTest()
        {
            Request objRequest1 = new Request();
            objRequest1.AddQuestion(new Question("memba.com", DnsType.MX, DnsClass.IN));
            Response objResponse1 = Resolver.Lookup(objRequest1);
            Assert.IsNotNull(objResponse1);
            Assert.AreEqual(objResponse1.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse1.Answers.Length, 1);
            Assert.AreEqual(objResponse1.Answers[0].Record.GetType(), typeof(MXRecord));
            Assert.AreEqual(((MXRecord)objResponse1.Answers[0].Record).Preference, 10);
            Assert.AreEqual(((MXRecord)objResponse1.Answers[0].Record).DomainName, "mail.memba.com");

            //This one should use the cache
            Response objResponse1b = Resolver.Lookup(objRequest1);
            Assert.IsNotNull(objResponse1b);
            Assert.AreEqual(objResponse1b.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse1b.Answers.Length, 1);
            Assert.AreEqual(objResponse1b.Answers[0].Record.GetType(), typeof(MXRecord));
            Assert.AreEqual(((MXRecord)objResponse1b.Answers[0].Record).Preference, 10);
            Assert.AreEqual(((MXRecord)objResponse1b.Answers[0].Record).DomainName, "mail.memba.com");
            
            string[] arrDomainNames2 = new string[] { "smtp1.google.com", "smtp2.google.com", "smtp3.google.com", "smtp4.google.com" };
            Request objRequest2 = new Request();
            objRequest2.AddQuestion(new Question("google.com", DnsType.MX, DnsClass.IN));
            Response objResponse2 = Resolver.Lookup(objRequest2);
            Assert.IsNotNull(objResponse2);
            Assert.AreEqual(objResponse2.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse2.Answers.Length, arrDomainNames2.Length);
            foreach (Answer objAnswer in objResponse2.Answers)
            {
                Assert.AreEqual(objAnswer.Record.GetType(), typeof(MXRecord));
                Assert.IsTrue(Array.IndexOf<string>(arrDomainNames2, ((MXRecord)objAnswer.Record).DomainName) > -1);
            }

            string[] arrDomainNames3 = new string[] { "mail.global.frontbridge.com" };
            Request objRequest3 = new Request();
            objRequest3.AddQuestion(new Question("microsoft.com", DnsType.MX, DnsClass.IN));
            Response objResponse3 = Resolver.Lookup(objRequest3);
            Assert.IsNotNull(objResponse3);
            Assert.AreEqual(objResponse3.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse3.Answers.Length, arrDomainNames3.Length);
            foreach (Answer objAnswer in objResponse3.Answers)
            {
                Assert.AreEqual(objAnswer.Record.GetType(), typeof(MXRecord));
                Assert.IsTrue(Array.IndexOf<string>(arrDomainNames3, ((MXRecord)objAnswer.Record).DomainName) > -1);
            }

            string[] arrDomainNames4 = new string[] { "a.mx.mail.yahoo.com", "b.mx.mail.yahoo.com", "c.mx.mail.yahoo.com", "d.mx.mail.yahoo.com", "e.mx.mail.yahoo.com", "f.mx.mail.yahoo.com", "g.mx.mail.yahoo.com" };
            Request objRequest4 = new Request();
            objRequest4.AddQuestion(new Question("yahoo.com", DnsType.MX, DnsClass.IN));
            Response objResponse4 = Resolver.Lookup(objRequest4);
            Assert.IsNotNull(objResponse4);
            Assert.AreEqual(objResponse4.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse4.Answers.Length, arrDomainNames4.Length);
            foreach (Answer objAnswer in objResponse4.Answers)
            {
                Assert.AreEqual(objAnswer.Record.GetType(), typeof(MXRecord));
                Assert.IsTrue(Array.IndexOf<string>(arrDomainNames4, ((MXRecord)objAnswer.Record).DomainName) > -1);
            }
        }
        /// <summary>
        ///A test for an NS Lookup (Request)
        ///</summary>
        [TestMethod()]
        public void DNS_RS03_NSLookupTest()
        {
            Request objRequest = new Request();
            objRequest.AddQuestion(new Question("memba.com", DnsType.NS, DnsClass.IN));
            Response objResponse = Resolver.Lookup(objRequest);
            Assert.IsNotNull(objResponse);
            Assert.AreEqual(objResponse.ReturnCode, ReturnCode.Success);
            Assert.IsTrue(objResponse.Answers.Length > 1); //At least 2 NS records
            foreach(Answer objAnswer in objResponse.Answers)
            {
                Assert.AreEqual(objAnswer.Record.GetType(), typeof(NSRecord));
                Assert.IsTrue(((NSRecord)objAnswer.Record).DomainName.EndsWith("dnsmadeeasy.com"));
            }
        }
        /// <summary>
        ///A test for an SOA Lookup (Request)
        ///</summary>
        [TestMethod()]
        public void DNS_RS04_SOALookupTest()
        {
            Request objRequest = new Request();
            objRequest.AddQuestion(new Question("memba.com", DnsType.SOA, DnsClass.IN));
            Response objResponse = Resolver.Lookup(objRequest);
            Assert.IsNotNull(objResponse);
            Assert.AreEqual(objResponse.ReturnCode, ReturnCode.Success);
            Assert.AreEqual(objResponse.Answers.Length, 1);
            Assert.AreEqual(objResponse.Answers[0].Record.GetType(), typeof(SOARecord));
            Assert.IsTrue(((SOARecord)objResponse.Answers[0].Record).PrimaryNameServer.EndsWith("dnsmadeeasy.com"));
            Assert.IsTrue(((SOARecord)objResponse.Answers[0].Record).ResponsibleMailAddress.EndsWith("dnsmadeeasy.com"));
        }
        /// <summary>
        ///A test for a Request with multiple questions (does not seem to work - see comment in Request.AddQuestion)
        ///</summary>
        [TestMethod()]
        public void DNS_RS05_ANYLookupTest()
        {
            Request objRequest = new Request();
            objRequest.AddQuestion(new Question("memba.com", DnsType.ANAME, DnsClass.IN));
            objRequest.AddQuestion(new Question("memba.com", DnsType.NS, DnsClass.IN));
            objRequest.AddQuestion(new Question("memba.com", DnsType.MX, DnsClass.IN));
            objRequest.AddQuestion(new Question("memba.com", DnsType.SOA, DnsClass.IN));
            Response objResponse = Resolver.Lookup(objRequest);
            Assert.IsNotNull(objResponse);
            Assert.AreEqual(objResponse.ReturnCode, ReturnCode.ServerFailure);
        }
    }
}
