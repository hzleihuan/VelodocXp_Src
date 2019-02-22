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
using System.Collections.Generic;
using System.Text;
using System.IO; //Stream
using System.Text.RegularExpressions; //Regex

namespace Memba.FileUpload.Licensing
{
    /// <summary>
    /// A response filter used to display a Velodoc banner
    /// </summary>
    /// <remarks>See articles below for more information about filtering http reponses</remarks>
    /// <see cref="http://www.ondotnet.com/pub/a/dotnet/2003/10/20/httpfilter.html"/>
    /// <see cref="http://www.codeproject.com/aspnet/RemovingWhiteSpacesAspNet.asp"/>
    public class VelodocLicenseFilter : Stream
    {
        #region Private Members
        private Stream _Stream;
        private Encoding _Encoding;
        private StringBuilder _StringBuilder;
        bool _IsClosed;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        public VelodocLicenseFilter(Stream stream, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (encoding == null)
                throw new ArgumentNullException("encoding");
            
            _Stream = stream;
            _Encoding = encoding;
            _StringBuilder = new StringBuilder();
            _IsClosed = false;
        }
        #endregion

        #region Stream Members
        /// <summary>
        /// Indicates whether the current stream supports writing (no async write though).
        /// </summary>
        public override bool CanWrite
        {
            get { return !_IsClosed; }
        }
        /// <summary>
        /// Writes a sequence of arrBytes to the current stream and advances the current position within this stream by the number of arrBytes written.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset + count > buffer.Length)
                throw new ArgumentException();

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (_IsClosed)
                throw new ObjectDisposedException("VelodocLicenseFilter");
            else
                _StringBuilder.Append(_Encoding.GetString(buffer, offset, count)); //Write the stream to the StringBuilder
        }
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            //We would better wait for Close to do any processing because Flush() can be called anytime before the stream is completely written
        }
        /// <summary>
        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// </summary>
        public override void Close()
        {
            const string TEXTSEARCHED = "</\\s*body>"; //matches </body> and </ body>
            const string REPLACEMENT =  "<div id=\"divPoweredBy\" class=\"cssPoweredBy\" style=\"position:fixed;z-index:99999;right:0;bottom:0;margin:0;padding:6px;background-color:Black;font-size:small\">" + 
                                            "<a href=\"http://www.velodoc.net\" style=\"color:White;text-decoration:none;\" onmouseover=\"this.style.textDecoration='underline'\" onmouseout=\"this.style.textDecoration='none'\">Try Velodoc online</a><br />" +
                                            "<a href=\"http://www.velodoc.com/download\" style=\"color:White;text-decoration:none;\" onmouseover=\"this.style.textDecoration='underline'\" onmouseout=\"this.style.textDecoration='none'\">Download Velodoc</a><br />" +
                                        "</div></body>";
            if (_StringBuilder.Length > 0)
            {
                string sHtml = _StringBuilder.ToString();
                Regex rx = new Regex(TEXTSEARCHED, RegexOptions.IgnoreCase);
                sHtml = rx.Replace(sHtml, REPLACEMENT);

                byte[] arrBytes = _Encoding.GetBytes(sHtml);
                _Stream.Write(arrBytes, 0, arrBytes.Length);
            }
            
            _Stream.Close();
            _IsClosed = true;

            base.Close();
        }
        /// <summary>
        /// Releases all resources used by the VelodocLicenseFilter object.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_Stream != null)
            {
                this._Stream.Dispose();
                this._Stream = null;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Unsupported Members
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }
        public override bool CanRead
        {
            get { return false; }
        }
        public override bool CanSeek
        {
            get { return false; }
        }
        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }
        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        public override int ReadByte()
        {
            throw new NotSupportedException();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
