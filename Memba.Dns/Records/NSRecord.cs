// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;

namespace Memba.Dns
{
	/// <summary>
	/// A Name Server Resource Record (RR) (RFC1035 3.3.11)
	/// </summary>
	public sealed class NSRecord : RecordBase
    {
        #region Private Members
        // Domain Name of the Name Server
		private string _DomainName;
        #endregion

        #region Constructor
        /// <summary>
		/// Constructs a NS record by reading bytes from a return message
		/// </summary>
		/// <param name="pointer">A logical pointer to the bytes holding the record</param>
		internal NSRecord(Pointer pointer)
		{
			_DomainName = pointer.ReadDomain();
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Gets the domain name of the NS record
        /// </summary>
        public string DomainName
        {
            get { return _DomainName; }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// Prints an NS Record
        /// </summary>
        /// <returns></returns>
        public override string ToString()
		{
			return String.Format("NS {0}", _DomainName);
        }
        #endregion
    }
}