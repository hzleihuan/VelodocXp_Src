// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;
using System.Net; //IPAdress

namespace Memba.Dns
{
	/// <summary>
	/// ANAME Resource Record (RR) (RFC1035 3.4.1)
	/// </summary>
	public sealed class ANameRecord : RecordBase
    {
        #region Private Members
        // An ANAME records consists simply of an IP address
		private IPAddress _IPAddress;
        #endregion

        #region Constructor
        /// <summary>
		/// Constructs an ANAME record by reading bytes from a return message
		/// </summary>
		/// <param name="pointer">A logical pointer to the bytes holding the record</param>
		internal ANameRecord(Pointer pointer)
		{
			byte b1 = pointer.ReadByte();
			byte b2 = pointer.ReadByte();
			byte b3 = pointer.ReadByte();
			byte b4 = pointer.ReadByte();

			// this next line's not brilliant - couldn't find a better way though
			_IPAddress = IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", b1, b2, b3, b4));
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Gets the IP Address in the A Record
        /// </summary>
        public IPAddress IPAddress
        {
            get { return _IPAddress; }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// Prints an A record
        /// </summary>
        /// <returns></returns>
        public override string ToString()
		{
			return String.Format("A {0}", _IPAddress);
		}
        #endregion
	}
}
