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
using System.IO;
using System.Web;
using System.Globalization; //CultureInfo

using Memba.FileUpload.Properties;

namespace Memba.FileUpload
{
    //TODO: check whether we should use a BufferedStream on top of RequestStream
    //(see http://msdn2.microsoft.com/en-US/library/system.io.bufferedstream.aspx)
    //and/or asynchronous BeginRead/EndRead to increase performances
    //(see http://msdn2.microsoft.com/en-US/library/kztecsys.aspx)
    //Maybe we should have derived from HttpInputStream ???????????????
    /// <summary>
    /// 
    /// </summary>
    internal sealed class RequestStream : Stream
    {
        #region Private Members
        private long _Position;
        private bool _IsEntireEntityBodyIsPreloaded;
        private bool _IsOpen;
        private HttpWorkerRequest _HttpWorkerRequest;
        private byte[] _RequestBuffer;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        public RequestStream(HttpWorkerRequest httpWorkerRequest)
        {
            this._IsOpen = true;
            this._IsEntireEntityBodyIsPreloaded = true;
            this._HttpWorkerRequest = httpWorkerRequest;
            this._RequestBuffer = httpWorkerRequest.GetPreloadedEntityBody();
            if ((this._RequestBuffer == null) || (this._RequestBuffer.Length == 0))
                this._IsEntireEntityBodyIsPreloaded = false;
        }
        #endregion

        #region Overriden Members
        /// <summary>
        /// Releases the unmanaged resources used by the RquestStream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
              try
              {
                    if (disposing)
                          this._IsOpen = false;
              }
              finally
              {
                    base.Dispose(disposing);
              }
        }
        /// <summary>
        /// Overrides System.IO.Stream.Flush() so that no action is performed.
        /// </summary>
        public override void Flush()
        {
            //Leave blank
        }
        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        public override int Read(byte[] buffer, int offset, int count)
        {
#if(TRACE)
                ThreadLog.WriteLine(String.Format(CultureInfo.InvariantCulture, "Start reading {0} bytes", count));
#endif

            if (!_IsOpen)
                throw new ObjectDisposedException(null);
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (offset + count > buffer.Length)
                throw new ArgumentException(Resources.ExceptionEndOfBufferReached);

            int iBytesRead = 0;
            //bool bRetry = true;

            //LABEL_RETRY:
            if (this._IsEntireEntityBodyIsPreloaded)
            {
                iBytesRead = this.ReadPreloadedEntityBody(buffer, offset, count);
#if(TRACE)
                ThreadLog.WriteLine(String.Format(CultureInfo.InvariantCulture, "Just read {0} preloaded bytes", iBytesRead));
#endif

                if (iBytesRead < count)
                {
                    if (this._HttpWorkerRequest.IsClientConnected() && !this._HttpWorkerRequest.IsEntireEntityBodyIsPreloaded())
                    {
                        iBytesRead += this.ReadEntityBody(buffer, offset + iBytesRead, count - iBytesRead);
#if(TRACE)
                        ThreadLog.WriteLine(String.Format(CultureInfo.InvariantCulture, "Just read {0} more bytes", iBytesRead));
#endif
                    }
                    this._IsEntireEntityBodyIsPreloaded = false;
                }
            }
            else if (this._HttpWorkerRequest.IsClientConnected() && !this._HttpWorkerRequest.IsEntireEntityBodyIsPreloaded())
            {
                iBytesRead = this.ReadEntityBody(buffer, offset, count);
#if(TRACE)
                ThreadLog.WriteLine(String.Format(CultureInfo.InvariantCulture, "Just read {0} bytes", iBytesRead));
#endif
            }
            this._Position += iBytesRead;
            if (iBytesRead == 0)
            {
#if(TRACE)
                ThreadLog.WriteLine("Houston, we have a problem: no bytes to read ...................");
#endif
                //if (bRetry)
                //{
                //    bRetry = false;
                //    goto LABEL_RETRY;
                //}
                //else
                    throw new System.Net.WebException(Resources.ExceptionConnectionClosed, System.Net.WebExceptionStatus.ConnectionClosed);
            }
            return iBytesRead;
        }
        /// <summary>
        /// Reads request preloaded data from the client by using the specified buffer to read from, byte offset, and maximum bytes.
        /// </summary>
        /// <param name="buffer">The byte array to read data into.</param>
        /// <param name="offset">The byte offset at which to begin reading.</param>
        /// <param name="size">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        private int ReadPreloadedEntityBody(byte[] buffer, int offset, int count)
        {
            int iBytesRead;
            if ((this._Position + count) < this._RequestBuffer.Length)
            {
                iBytesRead = count;
            }
            else
            {
                iBytesRead = this._RequestBuffer.Length - ((int)this._Position);
            }
            //TODO: There is potentially a problem here: my understanding is we cannot
            //read data at a position after 2Gb (2^31 = int.MaxValue).
            Buffer.BlockCopy(this._RequestBuffer, (int)this._Position, buffer, offset, iBytesRead);
            return iBytesRead;
        }
        /// <summary>
        /// Reads request data from the client (when not preloaded) by using the specified buffer to read from, byte offset, and maximum bytes.
        /// </summary>
        /// <param name="buffer">The byte array to read data into.</param>
        /// <param name="offset">The byte offset at which to begin reading.</param>
        /// <param name="size">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        private int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            /*
            We have been experiencing the following issue on the Velodoc production server
            
            Event Type:	Warning
            Event Source:	ASP.NET 2.0.50727.0
            Event Category:	Web Event 
            Event ID:	1309
            Date:		1/6/2007
            Time:		5:35:27 PM
            User:		N/A
            Computer:	MBP001
            Description:
            Event code: 153005 
            Event message: An unhandled exception occured in the upload module. 
            Event time: 06/01/2007 17:35:27 
            Event time (UTC): 06/01/2007 16:35:27 
            Event ID: 1b1d4a11b766439c8393edf55b3840e6 
            Event sequence: 154 
            Event occurrence: 1 
            Event detail code: 0 
             
            Application information: 
                Application domain: /LM/W3SVC/1/ROOT-2-128125740722407500 
                Trust level: Full 
                Application Virtual Path: / 
                Application Path: C:\Inetpub\Velodoc\ 
                Machine name: MBP001 
             
            Process information: 
                Process ID: 716 
                Process name: w3wp.exe 
                Account name: MBP001\IUSR_VELODOC 
             
            Exception information: 
                Exception type: ThreadAbortException 
                Exception message: Thread was being aborted. 
             
            Request information: 
                Request URL: https://www.velodoc.net/Anonymous/uploadControl.aspx?muid=6ef5d6ccf5674440840439f98cfa8150 
                Request path: /Anonymous/uploadControl.aspx 
                User host address: 86.137.95.23 
                User: jlchereau 
                Is authenticated: True 
                Authentication Type: Forms 
                Thread account name: MBP001\IUSR_VELODOC 
             
            Thread information: 
                Thread ID: 1 
                Thread account name: MBP001\IUSR_VELODOC 
                Is impersonating: False 
                Stack trace:    at System.Web.UnsafeNativeMethods.EcbGetAdditionalPostedContent(IntPtr pECB, Byte[] bytes, Int32 offset, Int32 bufferSize)
                at System.Web.Hosting.ISAPIWorkerRequestInProc.GetAdditionalPostedContentCore(Byte[] bytes, Int32 offset, Int32 bufferSize)
                at System.Web.Hosting.ISAPIWorkerRequest.ReadEntityBody(Byte[] buffer, Int32 offset, Int32 size)
                at System.Web.Hosting.ISAPIWorkerRequest.ReadEntityBody(Byte[] buffer, Int32 size)
                at Memba.FileUpload.RequestStream.ReadEntityBody(Byte[] buffer, Int32 offset, Int32 size)
                at Memba.FileUpload.RequestStream.Read(Byte[] buffer, Int32 offset, Int32 count)
                at Memba.FileUpload.MimeParser.Parse()
                at Memba.FileUpload.UploadHttpModule.OnPreRequestHandlerExecute(Object sender, EventArgs e)
 
            In the following article they mention the use of a constrained region 
            http://msdn.microsoft.com/msdnmag/issues/05/10/Reliability/default.aspx
            
            */ 
            
            int iBytesRead = 0;

            System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                if ((this._Position + size) > this.Length) //We are at the end of the uploaded file
                {
                    size = (int)(this.Length - this._Position);
                }

                if (offset <= 0) //This is the standard course of action
                {
                    iBytesRead = this._HttpWorkerRequest.ReadEntityBody(buffer, size);
                }
                else
                {
                    if ((this._RequestBuffer == null) || (size > this._RequestBuffer.Length))
                    {
                        this._RequestBuffer = new byte[size];
                    }
                    iBytesRead = this._HttpWorkerRequest.ReadEntityBody(this._RequestBuffer, size);
                    Buffer.BlockCopy(this._RequestBuffer, 0, buffer, offset, iBytesRead);
                }
            }
            
            /*
            //Removed as per recommendation of Doug Stewart from Microsoft
            try
            {
                System.Threading.Thread.Sleep(0);   //Give an opportunity for the thread to temptatively abort here
                                                    //considering we have differed abort above
            }
            catch (System.Threading.ThreadAbortException)
            {
                System.Threading.Thread.ResetAbort();
            }
            */
 
            return iBytesRead;
        }
        /// <summary>
        /// Throws a NotSupportedException. The stream does not support both writing and seeking.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Throws a NotSupportedException. The stream does not support both writing and seeking.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Throws a NotSupportedException. The stream does not support both writing and seeking.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return this._IsOpen; }
        }
        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }
        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (!_IsOpen)
                    throw new ObjectDisposedException("RequestStream");
                return long.Parse(this._HttpWorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength), CultureInfo.InvariantCulture);
            }
        }
        /// <summary>
        ///  Gets or sets the position within the current stream. Setting throws a NotSupportedException.
        /// </summary>
        public override long Position
        {
            get
            {
                if (!_IsOpen)
                    throw new ObjectDisposedException("RequestStream");
                return this._Position;
            }
            set { throw new NotSupportedException(); }
        }
        #endregion
    }
}
