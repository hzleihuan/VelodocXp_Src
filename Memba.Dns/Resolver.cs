// Original code by Rob Philpott of Big Developments Ltd.
// Source: http://www.codeproject.com/KB/IP/dnslookupdotnet.aspx
// This file and the code contained within is freeware and may be distributed and edited without restriction.
// Modifications by Jacques L. Chereau of Memba SA.

using System;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>, Dictionary<T, K>
using System.Net; //IPEndPoint
using System.Net.NetworkInformation; //NetworkInterface
using System.Net.Sockets; //Socket

namespace Memba.Dns
{
	/// <summary>
	/// Summary description for Dns.
	/// </summary>
	public static class Resolver
	{
		private const int DNSPORT = 53;
		private const int MAXRETRYATTEMPTS = 2; //Means we try 3 trimes
        private const int TIMEOUT = 1000;
        private const int MAXCACHESIZE = 100;
		
        private static int _UniqueId;
        private static IPAddress[] _DnsServers = null;
        private static Queue<string> _RequestQueue = new Queue<string>(); 
        private static Dictionary<string, Response> _ResponseCache = new Dictionary<string, Response>();
        private static object _Lock = new object();

        #region Other Members
        /// <summary>
		/// The principal look up function, which sends a request message to the given
		/// DNS server and collects a response. This implementation re-sends the message
		/// via UDP up to two times in the event of no response/packet loss
		/// </summary>
		/// <param name="request">The logical request to send to the server</param>
		/// <param name="dnsServer">The IP address of the DNS server we are querying</param>
		/// <returns>The logical response from the DNS server or null if no response</returns>
		public static Response Lookup(Request request, IPAddress dnsServer)
		{
			// check the inputs
			if (request == null)
                throw new ArgumentNullException("request");
			if (dnsServer == null)
                throw new ArgumentNullException("dnsServer");
			
            //Search the cache
            string sRequestKey = request.ToString();
            if (_ResponseCache.ContainsKey(sRequestKey))
                return _ResponseCache[sRequestKey];

			// create an end point to communicate with
			IPEndPoint objDnsEndPoint = new IPEndPoint(dnsServer, DNSPORT);
		
			// get the message
			byte[] arrRequestMessage = request.GetMessage();

			// send the request and get the response
			byte[] arrResponseMessage = Resolver.UdpTransfer(objDnsEndPoint, arrRequestMessage);

			// and populate a response object from that and return it
			Response objResponseRet = new Response(arrResponseMessage);

            //Add successful responses to cache
            if (objResponseRet.ReturnCode == ReturnCode.Success)
            {
                lock (_Lock)
                {
                    //Limit the size of cache
                    if (_RequestQueue.Count > MAXCACHESIZE)
                    {
                        string sRemovedKey = _RequestQueue.Dequeue();
                        _ResponseCache.Remove(sRemovedKey);
                    }
                    _ResponseCache.Add(sRequestKey, objResponseRet);
                    _RequestQueue.Enqueue(sRequestKey);
                }
            }

            return objResponseRet;
        }
        /// <summary>
        /// Lookup function using the first DNS server found on the network interface cards  
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Response Lookup(Request request)
        {
            // get the Dns servers configured on the network interface cards
            IPAddress[] arrDnsServers = Resolver.GetDnsServers();

            //Assume the array returned is not null and if it is empty, raise an exception
            System.Diagnostics.Debug.Assert(arrDnsServers != null);
            if (arrDnsServers.Length < 1)
                throw new DnsException(Properties.Resources.ResolverExceptionDnsServersNotFound,
                    WebExceptionStatus.NameResolutionFailure);

            //Call the main lookup function
            return Lookup(request, arrDnsServers[0]);
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Uses UDP to send the DNS request
        /// </summary>
        /// <remarks>For a TCP implementation, check http://www.codeproject.com/KB/IP/DNS_NET_Resolver.aspx
        /// or http://www.codeplex.com/dndns</remarks>
        /// <param name="server"></param>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
		private static byte[] UdpTransfer(IPEndPoint server, byte[] requestMessage)
		{
			// UDP can fail - if it does try again keeping track of how many attempts we've made
			int iRetryAttempts = 0;

			// try repeatedly in case of failure
			while (iRetryAttempts <= MAXRETRYATTEMPTS)
			{
				// firstly, uniquely mark this request with an id
				unchecked
				{
					// substitute in an id unique to this lookup, the request has no idea about this
					requestMessage[0] = (byte)(_UniqueId >> 8);
					requestMessage[1] = (byte)_UniqueId;
				}

				// we'll be send and receiving a UDP packet
				Socket objSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			
				// we will wait at most 1 second for a dns reply
				objSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, TIMEOUT);

				// send it off to the server
				objSocket.SendTo(requestMessage, requestMessage.Length, SocketFlags.None, server);
		
				// RFC1035 states that the maximum size of a UDP datagram is 512 octets (bytes)
				byte[] arrResponseMessage = new byte[512];

				try
				{
					// wait for a response upto 1 second
					objSocket.Receive(arrResponseMessage);

					// make sure the message returned is ours
					if (arrResponseMessage[0] == requestMessage[0] && arrResponseMessage[1] == requestMessage[1])
					{
						// its a valid response - return it, this is our successful exit point
						return arrResponseMessage;
					}
				}
				catch (SocketException)
				{
					// failure - we better try again, but remember how many attempts
					iRetryAttempts++;
				}
				finally
				{
					// increase the unique id
					_UniqueId++;

					// close the socket
					objSocket.Close();
				}
			}
		
			// the operation has failed, this is our unsuccessful exit point
			throw new DnsException(Properties.Resources.ResolverExceptionResponseFailure,
                WebExceptionStatus.ReceiveFailure);
		}
        /// <summary>
        /// Gets a list of default DNS servers used on the Windows machine.
        /// </summary>
        /// <returns></returns>
        private static IPAddress[] GetDnsServers()
        {
            if (_DnsServers != null)
                return _DnsServers;

            List<IPAddress> lstDnsServers = new List<IPAddress>();

            NetworkInterface[] arrNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface objNetworkInterface in arrNetworkInterfaces)
            {
                if ((objNetworkInterface.OperationalStatus == OperationalStatus.Up)
                    && (objNetworkInterface.Speed > 0)
                    && (objNetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    && (objNetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
                {
                    IPInterfaceProperties colIPInterfaceProperties = objNetworkInterface.GetIPProperties();
                    foreach (IPAddress objDnsServer in colIPInterfaceProperties.DnsAddresses)
                    {
                        if (!lstDnsServers.Contains(objDnsServer))
                            lstDnsServers.Add(objDnsServer);
                    }
                }
            }

            lock (_Lock)
            {
                _DnsServers = lstDnsServers.ToArray();
            }

            return _DnsServers;
        }
        #endregion
    }
}
