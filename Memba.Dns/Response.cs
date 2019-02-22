// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;
using System.Net;

namespace Memba.Dns
{
	/// <summary>
	/// A Response is a logical representation of the byte data returned from a DNS query
	/// </summary>
	public sealed class Response
    {
        #region Private Members
        // these are fields we're interested in from the message
		private ReturnCode _ReturnCode;
		private bool _AuthoritativeAnswer;
		private bool _RecursionAvailable;
		private bool _MessageTruncated;
		private Question[] _Questions;
		private Answer[] _Answers;
		private NameServer[] _NameServers;
		private AdditionalRecord[] _AdditionalRecords;
        #endregion

        #region Constructor
        /// <summary>
		/// Construct a Response object from the supplied byte array
		/// </summary>
		/// <param name="message">a byte array returned from a DNS server query</param>
		internal Response(byte[] message)
		{
			// the bit flags are in bytes 2 and 3
			byte byFlags1 = message[2];
			byte byFlags2 = message[3];

			// get return code from lowest 4 bits of byte 3
			int iReturnCode = byFlags2 & 15;
				
			// if its in the reserved section, set to other
			if (iReturnCode > 6) iReturnCode = 6;
			_ReturnCode = (ReturnCode)iReturnCode;

			// other bit flags
			_AuthoritativeAnswer = ((byFlags1 & 4) != 0);
			_RecursionAvailable = ((byFlags2 & 128) != 0);
			_MessageTruncated = ((byFlags1 & 2) != 0);

			// create the arrays of response objects
			_Questions = new Question[GetShort(message, 4)];
			_Answers = new Answer[GetShort(message, 6)];
			_NameServers = new NameServer[GetShort(message, 8)];
			_AdditionalRecords = new AdditionalRecord[GetShort(message, 10)];

			// need a pointer to do this, position just after the header
			Pointer objPointer = new Pointer(message, 12);

			// and now populate them, they always follow this order
			for (int index = 0; index < _Questions.Length; index++)
			{
				try
				{
					// try to build a quesion from the response
					_Questions[index] = new Question(objPointer);
				}
				catch (Exception ex)
				{
					// something grim has happened, we can't continue
					throw new DnsException(ex.Message, ex);
				}
			}
			for (int index = 0; index < _Answers.Length; index++)
			{
				_Answers[index] = new Answer(objPointer);
			}
			for (int index = 0; index < _NameServers.Length; index++)
			{
				_NameServers[index] = new NameServer(objPointer);
			}
			for (int index = 0; index < _AdditionalRecords.Length; index++)
			{
				_AdditionalRecords[index] = new AdditionalRecord(objPointer);
			}
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// 
        /// </summary>
        public ReturnCode ReturnCode
        {
            get { return _ReturnCode; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool AuthoritativeAnswer
        {
            get { return _AuthoritativeAnswer; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool RecursionAvailable
        {
            get { return _RecursionAvailable; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool MessageTruncated
        {
            get { return _MessageTruncated; }
        }
        /// <summary>
        /// 
        /// </summary>
        public Question[] Questions
        {
            get { return _Questions; }
        }
        /// <summary>
        /// 
        /// </summary>
        public Answer[] Answers
        {
            get { return _Answers; }
        }
        /// <summary>
        /// 
        /// </summary>
        public NameServer[] NameServers
        {
            get { return _NameServers; }
        }
        /// <summary>
        /// 
        /// </summary>
        public AdditionalRecord[] AdditionalRecords
        {
            get { return _AdditionalRecords; }
        }
        #endregion

        #region Private Helpers
        /// <summary>
		/// Convert 2 bytes to a short. It would have been nice to use BitConverter for this,
		/// it however reads the bytes in the wrong order (at least on Windows)
		/// </summary>
		/// <param name="message">byte array to look in</param>
		/// <param name="position">position to look at</param>
		/// <returns>short representation of the two bytes</returns>
		private static short GetShort(byte[] message, int position)
		{
			return (short)(message[position]<<8 | message[position+1]);
        }
        #endregion
    }
}
