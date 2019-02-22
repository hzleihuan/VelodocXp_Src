// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;

namespace Memba.Dns
{
	/// <summary>
	/// An SOA Resource Record (RR) (RFC1035 3.3.13)
	/// </summary>
	public sealed class SOARecord : RecordBase
    {
        #region Private Members
        // these fields constitute an SOA RR
		private string	_PrimaryNameServer;
		private string	_ResponsibleMailAddress;
		private int	_Serial;
		private int	_Refresh;
		private int	_Retry;
		private int	_Expire;
		private int	_DefaultTTL;
        #endregion

        #region Constructor
        /// <summary>
		/// Constructs an SOA record by reading bytes from a return message
		/// </summary>
		/// <param name="pointer">A logical pointer to the bytes holding the record</param>
		internal SOARecord(Pointer pointer) 
		{
			// read all fields RFC1035 3.3.13
			_PrimaryNameServer = pointer.ReadDomain();
			_ResponsibleMailAddress = pointer.ReadDomain();
			_Serial = pointer.ReadInt();
			_Refresh = pointer.ReadInt();
			_Retry = pointer.ReadInt();
			_Expire = pointer.ReadInt();
			_DefaultTTL = pointer.ReadInt();
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Gets the PrimaryNameServer value of the SOA record
        /// </summary>
        public string PrimaryNameServer
        {
            get { return _PrimaryNameServer; }
        }
        /// <summary>
        /// Gets the ResponsibleMailAddress value of the SOA record
        /// </summary>
        public string ResponsibleMailAddress
        {
            get { return _ResponsibleMailAddress; }
        }
        /// <summary>
        /// Gets the Serial value of the SOA record
        /// </summary>
        public int Serial
        {
            get { return _Serial; }
        }
        /// <summary>
        /// Gets the Refresh value of the SOA record
        /// </summary>
        public int Refresh
        {
            get { return _Refresh; }
        }
        /// <summary>
        /// Gets the Retry value of the SOA record
        /// </summary>
        public int Retry
        {
            get { return _Retry; }
        }
        /// <summary>
        /// Gets the Expire value of the SOA record
        /// </summary>
        public int Expire
        {
            get { return _Expire; }
        }
        /// <summary>
        /// Gets the default TTL of the SOA record
        /// </summary>
        public int DefaultTTL
        {
            get { return _DefaultTTL; }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// Prints an SOA record
        /// </summary>
        /// <returns></returns>
        public override string ToString()
		{
			return string.Format("SOA {0} {1} (Serial = {2}, Refresh = {3}, Retry = {4}, Expire  = {5}, Default TTL = {6})",
				new object[] { _PrimaryNameServer, _ResponsibleMailAddress,	_Serial, _Refresh, _Retry, _Expire,	_DefaultTTL });
        }
        #endregion
    }
}
