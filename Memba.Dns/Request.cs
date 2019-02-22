// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

namespace Memba.Dns
{
	/// <summary>
	/// A Request logically consists of a number of questions to ask the DNS Server. Create a request and
	/// add questions to it, then pass the request to Resolver.Lookup to query the DNS Server. It is important
	/// to note that many DNS Servers DO NOT SUPPORT MORE THAN 1 QUESTION PER REQUEST, and it is advised that
	/// you only add one question to a request. If not ensure you check Response.ReturnCode to see what the
	/// server has to say about it.
	/// </summary>
	public sealed class Request
    {
        #region Private Members
        // A request is a series of questions, an 'opcode' (RFC1035 4.1.1) and a flag to denote
		// whether recursion is required (don't ask..., just assume it is)
		private ArrayList	_Questions;
		private bool		_RecursionDesired;
		private OpCode		_OpCode;
        #endregion

        #region Constructor
        /// <summary>
		/// Construct this object with the default values and create an ArrayList to hold
		/// the questions as they are added
		/// </summary>
		public Request()
		{
			// default for a request is that recursion is desired and using standard query
			_RecursionDesired = true;
			_OpCode = OpCode.StandardQuery;

			// create an expandable list of questions
			_Questions = new ArrayList();

        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// 
        /// </summary>
        public bool RecursionDesired
        {
            get { return _RecursionDesired; }
            set { _RecursionDesired = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public OpCode OpCode
        {
            get { return _OpCode; }
            set { _OpCode = value; }
        }
        #endregion

        #region Other Members
        /// <summary>
		/// Adds a question to the request to be sent to the DNS server.
		/// </summary>
		/// <param name="question">The question to add to the request</param>
		public void AddQuestion(Question question)
		{
			// abandon if null
			if (question == null) throw new ArgumentNullException("question");

            //TODO: Review
            //if (_Questions.Count > 0)
            //    throw new NotSupportedException("Oops, adding several questions does not seem to work.");
            //This may have to do with the 512 byte limit of a message which may be truncated
            //This may also have to do with a comment at http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
            //whereby there is a bug at ResourceRecord line 56, because the + operator creates a new Pointer
            //instead of incrementing the current pointer.

			// add this question to our collection
			_Questions.Add(question);
		}
        /// <summary>
		/// Convert this request into a byte array ready to send direct to the DNS server
		/// </summary>
		/// <returns></returns>
		public byte[] GetMessage()
		{
			// construct a message for this request. This will be a byte array but we're using
			// an arraylist as we don't know how big it will be
			ArrayList data = new ArrayList();
			
			// the id of this message - this will be filled in by the resolver
			data.Add((byte)0);
			data.Add((byte)0);
			
			// write the bitfields
			data.Add((byte)(((byte)_OpCode<<3)  | (_RecursionDesired?0x01:0)));
			data.Add((byte)0);

			// tell it how many questions
			unchecked
			{
				data.Add((byte)(_Questions.Count >> 8));
				data.Add((byte)_Questions.Count);
			}
			
			// the are no requests, name servers or additional records in a request
			data.Add((byte)0); data.Add((byte)0);
			data.Add((byte)0); data.Add((byte)0);
			data.Add((byte)0); data.Add((byte)0);

			// that's the header done - now add the questions
			foreach (Question question in _Questions)
			{
				AddDomain(data, question.Domain);
				unchecked
				{
					data.Add((byte)0);
					data.Add((byte)question.Type);
					data.Add((byte)0);
					data.Add((byte)question.Class);
				}
			}

			// and convert that to an array
			byte[] message = new byte[data.Count];
			data.CopyTo(message);
			return message;
		}
		/// <summary>
		/// Adds a domain name to the ArrayList of bytes. This implementation does not use
		/// the domain name compression used in the class Pointer - maybe it should.
		/// </summary>
		/// <param name="data">The ArrayList representing the byte array message</param>
		/// <param name="domainName">the domain name to encode and add to the array</param>
		private static void AddDomain(ArrayList data, string domainName)
		{
			int position = 0;
			int length = 0;

			// start from the beginning and go to the end
			while (position < domainName.Length)
			{
				// look for a period, after where we are
				length = domainName.IndexOf('.', position) - position;
				
				// if there isn't one then this labels length is to the end of the string
				if (length < 0) length = domainName.Length - position;
				
				// add the length
				data.Add((byte)length);

				// copy a char at a time to the array
				while (length-- > 0)
				{
					data.Add((byte)domainName[position++]);
				}

				// step over '.'
				position++;
			}
				
			// end of domain names
			data.Add((byte)0);
        }
        /// <summary>
        /// Prints a request
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder objStringBuilder = new StringBuilder();
            foreach (Question objQuestion in _Questions)
            {
                if (objQuestion != null)
                    objStringBuilder.Append(objQuestion);
            }
            objStringBuilder.Append("[");
            objStringBuilder.Append(_RecursionDesired);
            objStringBuilder.Append("][");
            objStringBuilder.Append(_OpCode);
            objStringBuilder.Append("]");
            return objStringBuilder.ToString();
        }
        #endregion
    }
}
