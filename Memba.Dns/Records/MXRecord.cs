// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;

namespace Memba.Dns
{
	/// <summary>
	/// An MX (Mail Exchanger) Resource Record (RR) (RFC1035 3.3.9)
	/// </summary>
	public sealed class MXRecord : RecordBase, IComparable
    {
        #region Private Members
        // an MX record is a domain name and an integer preference
		private string _DomainName;
		private int	_Preference;
        #endregion

        #region Constructor
        /// <summary>
		/// Constructs an MX record by reading bytes from a return message
		/// </summary>
		/// <param name="pointer">A logical pointer to the bytes holding the record</param>
		internal MXRecord(Pointer pointer)
		{
			_Preference = pointer.ReadShort();
			_DomainName = pointer.ReadDomain();
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Gets the domain name of an MX record (e.g. mail.memba.com)
        /// </summary>
		public string DomainName
        {
            get { return _DomainName; }
        }
        /// <summary>
        /// Gets the preference of an MX Record (e.g. 10)
        /// </summary>
		public int Preference
        {
            get { return _Preference; }
        }
        #endregion

        #region IComparable Members
        /// <summary>
		/// Implements the IComparable interface so that we can sort the MX records by their
		/// lowest preference
		/// </summary>
		/// <param name="other">the other MxRecord to compare against</param>
		/// <returns>1, 0, -1</returns>
		public int CompareTo(object value)
		{
			MXRecord objMXRecord = value as MXRecord;
            if (objMXRecord == null)
                return 1;
			// we want to be able to sort MX Records by preference
			else if (objMXRecord._Preference < _Preference)
                return 1;
			else if (objMXRecord._Preference > _Preference)
                return -1;
			else // order mail servers of same preference by name
			    return -objMXRecord._DomainName.CompareTo(_DomainName);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            MXRecord objMXRecord = value as MXRecord;
            if (objMXRecord == null)
                return false;
            else if (objMXRecord._Preference != _Preference) // preference must match
                return false;
            else if (objMXRecord._DomainName != _DomainName) // and so must the domain name
                return false;
            else // its a match
                return true;
        }
        /// <summary>
        /// == operator
        /// </summary>
        /// <param name="record1"></param>
        /// <param name="record2"></param>
        /// <returns></returns>
		public static bool operator == (MXRecord record1, MXRecord record2)
		{
			if (record1 == null) throw new ArgumentNullException("record1");
			return record1.Equals(record2);
		}
	    /// <summary>
	    /// != operator
	    /// </summary>
	    /// <param name="record1"></param>
	    /// <param name="record2"></param>
	    /// <returns></returns>
		public static bool operator != (MXRecord record1, MXRecord record2)
		{
			return !(record1 == record2);
		}
        /*
		public static bool operator<(MXRecord record1, MXRecord record2)
		{
			if (record1._preference > record2._preference) return false;
			if (record1._domainName > record2._domainName) return false;
			return false;
		}

		public static bool operator>(MXRecord record1, MXRecord record2)
		{
			if (record1._preference < record2._preference) return false;
			if (record1._domainName < record2._domainName) return false;
			return false;
		}
        */
		#endregion

        #region Other Members
        /// <summary>
        /// Gets the hash code of an MX record
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
		{
			return _Preference;
		}
        /// <summary>
        /// Prints an MX record
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("MX {0} {1}", _Preference, _DomainName);
        }
        #endregion
	}
}
