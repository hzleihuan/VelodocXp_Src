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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Net; //WebException
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using Memba.FileUpload.Providers;
using Memba.FileUpload.Properties;

namespace Memba.FileUpload
{
    internal sealed class RequestFilter : IDisposable
    {
        #region Private Members
        //We use Stream instead of RequestStream to allow unit testing from a dumped file
        private Stream _RequestStream;
        private CryptoStream _CryptoStream;
        private HashAlgorithm _HashAlgorithm;

        private Encoding _Encoding;
        private byte[] _MultiPartBoundary;
        private byte[] _MultiPartBoundary2;
        private Regex _MultiPartRegex;
        private StringBuilder _FilteredRequestStringBuilder;
        private bool _IsProcessingUploadedFile;

        private Stream _CurrentStream;
        private UploadData _UploadData;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>The request stream is passed as a Stream instead of a RequestStream to allow unit testing from dump files.</remarks>
        /// <param name="requestStream">A Stream representing the request stream</param>
        /// <param name="hashAlgorithm">The hash algorithm used to hash file upload data</param>
        /// <param name="contentLength"></param>
        /// <param name="encoding"></param>
        /// <param name="multipartBoundary"></param>
        /// <param name="uploadData"></param>
        public RequestFilter(Stream requestStream, HashAlgorithm hashAlgorithm, long contentLength, Encoding encoding, string multipartBoundary, UploadData uploadData)
        {
            if (requestStream == null)
                throw new ArgumentNullException("requestStream");
            if (hashAlgorithm == null)
                throw new ArgumentNullException("hashAlgorithm");
            if (contentLength < 0)
                throw new ArgumentOutOfRangeException("contentLength");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (multipartBoundary == null)
                throw new ArgumentNullException("multipartBoundary");
            //Note: Even for unit testing we should have an uploadData object
            if (uploadData == null)
                throw new ArgumentNullException("uploadData");

            _RequestStream = requestStream;
            //_CryptoStream = null; //CA1805
            _HashAlgorithm = hashAlgorithm;
            _Encoding = encoding;
            _MultiPartBoundary = _Encoding.GetBytes(multipartBoundary);
            _MultiPartBoundary2 = _Encoding.GetBytes(Constants.LineSeparator + multipartBoundary);
            _MultiPartRegex = new Regex(@".*name=\""(?<name>.*)\"".*filename=\""(?<filename>.*)\"".*\r\nContent-Type:\s(?<contenttype>.*)",
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            _FilteredRequestStringBuilder = new StringBuilder(Constants.StringBuilderCapacity);
            _FilteredRequestStringBuilder.Append(multipartBoundary);
            //_IsProcessingUploadedFile = false; //CA1805

            _UploadData = uploadData;
            _UploadData.ReportStart(contentLength);
        }
        #endregion

        #region Property Accessors
        public Stream RequestStream
        {
            get { return _RequestStream; }
        }
        //public UploadData UploadData
        //{
        //    get { return _UploadData; }
        //}
        public Encoding Encoding
        {
            get { return _Encoding; }
        }
        public byte[] MultiPartBoundary
        {
            get { return _MultiPartBoundary; }
        }
        public byte[] MultiPartBoundary2
        {
            get { return _MultiPartBoundary2; }
        }
        //public StringBuilder FilteredRequestStringBuilder
        //{
        //    get { return _FilteredRequestStringBuilder; }
        //}
        public string FilteredRequest
        {
            get { return _FilteredRequestStringBuilder.ToString(); }
        }
        #endregion

        #region IDisposable Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Not sure which type of stream this is considering provider model")]
        public void Dispose()
        {
            if (_CurrentStream != null)
            {
                try { _CurrentStream.Dispose(); }
                catch { }
            }
            if (_CryptoStream != null)
            {
                try { _CryptoStream.Dispose(); }
                catch { }
            }
        }

        #endregion

        #region Other Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="?"></param>
        public void ProcessPartHeaders(string buffer)
        {
            if (_UploadData.ProgressStatus == UploadProgressStatus.Canceled)
                throw new WebException(Resources.ExceptionRequestCanceled, WebExceptionStatus.RequestCanceled);
           
            //use regular expressions to find name, filename and content-type
            Match objMatch = _MultiPartRegex.Match(buffer);
            if ((objMatch != null) && (objMatch.Success)) //part headers define an uploaded file
            {
                _IsProcessingUploadedFile = true;
                string sName = objMatch.Groups[Constants.MultiPartName].Value;
                string sFileName = objMatch.Groups[Constants.MultiPartFilename].Value;
                string sContentType = objMatch.Groups[Constants.MultiPartContentType].Value;
                object objFileKey = null;
                if (!String.IsNullOrEmpty(sFileName)) //input fields have a browsed file
                {
                    objFileKey = FileStorage.GetNewFileKey(sFileName);

                    //The following has been moved to ProcessPartData to avoid creating empty files for 
                    //files to upload which do not exist on the client.
                    //_CurrentStream = FileStorage.GetFileStream(objFileKey, AccessMode.Create);
                    //_HashAlgorithm.Initialize(); //Important before reusing;
                    //_CryptoStream = new CryptoStream(Stream.Null, _HashAlgorithm, CryptoStreamMode.WriteLine);
                    //_UploadData.ReportFileCreated(_RequestStream.Position, sName, sFileName, sContentType, objFileKey);
                }
                //Maybe we should have the following line within the if
                //In this case, empty input file controls will not be reported in the upload monitor. Useful?
                _UploadData.ReportFileCreated(_RequestStream.Position, sName, sFileName, sContentType, objFileKey);
            }
            else //part headers do not define an uploaded file
            {
                _IsProcessingUploadedFile = false;
                _FilteredRequestStringBuilder.Append(Constants.LineSeparator);
                _FilteredRequestStringBuilder.Append(buffer);
                _FilteredRequestStringBuilder.Append(Constants.HeadersFromDataSeparator);
                _UploadData.ReportPosition(_RequestStream.Position);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public void ProcessPartData(ref byte[] buffer, int startIndex, int count)
        {
            if (_UploadData.ProgressStatus == UploadProgressStatus.Canceled)
                throw new WebException(Resources.ExceptionRequestCanceled, WebExceptionStatus.RequestCanceled);

            if (count > 0)
            {
                if (_IsProcessingUploadedFile)
                {
                    if (_CurrentStream == null)
                    {
                        System.Diagnostics.Debug.Assert(_CryptoStream == null); //If _CurrentStream is null, _CryptoStream should be null
                        object objFileKey = _UploadData.UploadFileInProgress.ProviderFileKey;

                        _CurrentStream = FileStorage.GetFileStream(objFileKey, AccessMode.Create);
                        _HashAlgorithm.Initialize(); //Important before reusing;
                        _CryptoStream = new CryptoStream(Stream.Null, _HashAlgorithm, CryptoStreamMode.Write);
                    }
                    if (_CurrentStream.CanWrite)
                    {
                        //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
                        _CurrentStream.Write(buffer, startIndex, count);
                        _CryptoStream.Write(buffer, startIndex, count);
                        _UploadData.ReportFilePosition(_RequestStream.Position, count);
                        //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;
                        
                        //Careful with Thread.Sleep(0):
                        // (1) it is costly: 4000 cycles
                        // (2) it does not always have the expected effect
                        // see http://blogs.msdn.com/khen1234/archive/2006/02/02/523623.aspx
                        // see also http://blogs.msdn.com/oldnewthing/archive/2005/10/04/476847.aspx
                        // see especially http://www.bluebytesoftware.com/blog/PermaLink,guid,1c013d42-c983-4102-9233-ca54b8f3d1a1.aspx

                        //We have just written data to disk, yield some time to other upload threads
                        System.Threading.Thread.Sleep(0);
                    }
                }
                else
                {
                    string sBuffer = _Encoding.GetString(buffer, startIndex, count);
                    _FilteredRequestStringBuilder.Append(sBuffer);
                    _UploadData.ReportPosition(_RequestStream.Position);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ProcessEndOfPart()
        {
            if (_UploadData.ProgressStatus == UploadProgressStatus.Canceled)
                throw new WebException(Resources.ExceptionRequestCanceled, WebExceptionStatus.RequestCanceled);

            if (_IsProcessingUploadedFile) //File upload
            {
                //Clean provider stream (output)
                if (_CurrentStream != null)
                {
                    _CurrentStream.Close();
                    //_CurrentStream.Dispose(); //CA2202
                    _CurrentStream = null;
                }
                //Clean crypto stream
                if (_CryptoStream != null)
                {
                    _CryptoStream.Close();
                    //_CryptoStream.Dispose(); //CA2202
                    _CryptoStream = null;
                }
                //Report completion
                if (!String.IsNullOrEmpty(_UploadData.UploadFileInProgress.OriginalPath)
                    && (_UploadData.UploadFileInProgress.ContentPosition > 0))
                    _UploadData.ReportFileCompleted(_RequestStream.Position, _HashAlgorithm.Hash);
            }
            else //Not a file upload
            {
                _FilteredRequestStringBuilder.Append(_Encoding.GetString(_MultiPartBoundary2));
                _UploadData.ReportPosition(_RequestStream.Position);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ProcessEndOfRequest()
        {
            _FilteredRequestStringBuilder.Append(Constants.EndOfRequest);
            _FilteredRequestStringBuilder.Append(Constants.LineSeparator);
            _UploadData.ReportCompleted(_RequestStream.Position);
        }
        #endregion
    }
}

