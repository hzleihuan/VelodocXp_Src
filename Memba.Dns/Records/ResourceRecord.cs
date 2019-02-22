// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;

namespace Memba.Dns
{
	/// <summary>
	/// Represents a Resource Record as detailed in RFC1035 4.1.3
	/// </summary>
	public class ResourceRecord
    {
        #region Private Members
		private string _Domain;
		private DnsType	_DnsType;
		private DnsClass _DnsClass;
		private int	_TTL;
		private RecordBase _Record;
        #endregion

        #region Constructor
        /// <summary>
		/// Construct a resource record from a pointer to a byte array
		/// </summary>
		/// <param name="pointer">the position in the byte array of the record</param>
		internal ResourceRecord(Pointer pointer)
		{
			// extract the domain, question type, question class and Ttl
			_Domain = pointer.ReadDomain();
			_DnsType = (DnsType)pointer.ReadShort();
			_DnsClass = (DnsClass)pointer.ReadShort();
			_TTL = pointer.ReadInt();

			// the next short is the record length, we only use it for unrecognised record types
			int recordLength = pointer.ReadShort();

			// and create the appropriate RDATA record based on the dnsType
			switch (_DnsType)
			{
				case DnsType.NS:
                    _Record = new NSRecord(pointer);
                    break;
				case DnsType.MX:
                    _Record = new MXRecord(pointer);
                    break;
				case DnsType.ANAME:
                    _Record = new ANameRecord(pointer);
                    break;
				case DnsType.SOA:
                    _Record = new SOARecord(pointer);
                    break;
				default:
					// move the pointer over this unrecognised record
					pointer += recordLength;
					break;
			}
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// 
        /// </summary>
        public string Domain
        {
            get { return _Domain; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DnsType Type
        {
            get { return _DnsType; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DnsClass Class
        {
            get { return _DnsClass; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int TTL
        {
            get { return _TTL; }
        }
        /// <summary>
        /// 
        /// </summary>
        public RecordBase Record
        {
            get { return _Record; }
        }
        #endregion
    }

	// Answers, Name Servers and Additional Records all share the same RR format
	public class Answer : ResourceRecord
	{
		internal Answer(Pointer pointer) : base(pointer) {}
	}

	public class NameServer : ResourceRecord
	{
		internal NameServer(Pointer pointer) : base(pointer) {}
	}

	public class AdditionalRecord : ResourceRecord
	{
		internal AdditionalRecord(Pointer pointer) : base(pointer) {}
	}
}