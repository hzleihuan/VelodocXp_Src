// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;
using System.Text.RegularExpressions;

namespace Memba.Dns
{
	/// <summary>
	/// Represents a DNS Question, comprising of a domain to query, the type of query (QTYPE) and the class
	/// of query (QCLASS). This class is an encapsulation of these three things, and extensive argument checking
	/// in the constructor as this may well be created outside the assembly (public protection)
	/// </summary>
    public sealed class Question
    {
        #region Private Members
        // A question is these three things combined
        private string _Domain;
        private DnsType _DnsType;
        private DnsClass _DnsClass;
        #endregion

        #region Constructors
        /// <summary>
        /// Construct the question from parameters, checking for safety
        /// </summary>
        /// <param name="domain">the domain name to query eg. bigdevelopments.co.uk</param>
        /// <param name="dnsType">the QTYPE of query eg. DnsType.MX</param>
        /// <param name="dnsClass">the CLASS of query, invariably DnsClass.IN</param>
        public Question(string domain, DnsType dnsType, DnsClass dnsClass)
        {
            // check the input parameters
            if (domain == null) throw new ArgumentNullException("domain");

            // do a sanity check on the domain name to make sure its legal
            if (domain.Length == 0 || domain.Length > 255 || !Regex.IsMatch(domain, @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6}$"))
            //There are several issues with thw following Regex: code|project.com, -codeproject.com and code_project.com would be validated
            //if (domain.Length == 0 || domain.Length > 255 || !Regex.IsMatch(domain, @"^[a-z|A-Z|0-9|-|_]{1,63}(\.[a-z|A-Z|0-9|-|_]{1,63})+$"))
            {
                // domain names can't be bigger tan 255 chars, and individal labels can't be bigger than 63 chars
                throw new ArgumentException(String.Format(Properties.Resources.QuestionExceptionInvalidDomainName, domain), "domain");
            }

            // sanity check the DnsType parameter
            if (!Enum.IsDefined(typeof(DnsType), dnsType) || dnsType == DnsType.None)
            {
                throw new ArgumentOutOfRangeException("dnsType", String.Format(Properties.Resources.QuestionExceptionInvalidDnsType, dnsType));
            }

            // sanity check the DnsClass parameter
            if (!Enum.IsDefined(typeof(DnsClass), dnsClass) || dnsClass == DnsClass.None)
            {
                throw new ArgumentOutOfRangeException("dnsClass", String.Format(Properties.Resources.QuestionExceptionInvalidDnsClass, dnsClass));
            }

            // just remember the values
            _Domain = domain;
            _DnsType = dnsType;
            _DnsClass = dnsClass;
        }
        /// <summary>
        /// Construct the question reading from a DNS Server response. Consult RFC1035 4.1.2
        /// for byte-wise details of this structure in byte array form
        /// </summary>
        /// <param name="pointer">a logical pointer to the Question in byte array form</param>
        internal Question(Pointer pointer)
        {
            // extract from the message
            _Domain = pointer.ReadDomain();
            _DnsType = (DnsType)pointer.ReadShort();
            _DnsClass = (DnsClass)pointer.ReadShort();
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Gets the domain of the Question
        /// </summary>
        public string Domain
        {
            get { return _Domain; }
        }
        /// <summary>
        /// Gets the Dns type of the Question (A, NS, MX, SOA, ...)
        /// </summary>
        public DnsType Type
        {
            get { return _DnsType; }
        }
        /// <summary>
        /// Gets the Dns class of the question (Generally IN for Internet)
        /// </summary>
        public DnsClass Class
        {
            get { return _DnsClass; }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// Prints a question
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0} {1} {2}]", _Domain, _DnsType, _DnsClass);
        }
        #endregion
    }
}
