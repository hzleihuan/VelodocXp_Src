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
using System.Web;

namespace Memba.FileUpload
{
    /// <summary>
    /// FilteredHttpWorkerRequest is an HttpWorkerRequest filtered from uploaded files which is used with IIS 7 integrated pipeline mode 
    /// </summary>
    internal class FilteredHttpWorkerRequest : HttpWorkerRequest
    {
        private byte[] _requestBuffer;
        private HttpWorkerRequest _wr;

        public FilteredHttpWorkerRequest(HttpWorkerRequest wr, byte[] requestBuffer)
        {
            this._wr = wr;
            this._requestBuffer = requestBuffer;
        }

        public override void CloseConnection()
        {
            if (this._wr != null)
                this._wr.CloseConnection();
        }

        public override void EndOfRequest()
        {
            if (this._wr != null)
                this._wr.EndOfRequest();
        }

        public override bool Equals(object obj)
        {
            if (this._wr != null)
                return this._wr.Equals(obj);
            else
                return (obj == null);
        }

        public override void FlushResponse(bool finalFlush)
        {
            if (this._wr != null)
                this._wr.FlushResponse(finalFlush);
        }

        public override string GetAppPath()
        {
            return (this._wr == null) ? null : this._wr.GetAppPath();
        }

        public override string GetAppPathTranslated()
        {
            return (this._wr == null) ? null : this._wr.GetAppPathTranslated();
        }

        public override string GetAppPoolID()
        {
            return (this._wr == null) ? null : this._wr.GetAppPoolID();
        }

        public override long GetBytesRead()
        {
            return (this._wr == null) ? 0L : this._wr.GetBytesRead();
        }

        public override byte[] GetClientCertificate()
        {
            return (this._wr == null) ? null : this._wr.GetClientCertificate();
        }

        public override byte[] GetClientCertificateBinaryIssuer()
        {
            return (this._wr == null) ? null : this._wr.GetClientCertificateBinaryIssuer();
        }

        public override int GetClientCertificateEncoding()
        {
            return (this._wr == null) ? 0 : this._wr.GetClientCertificateEncoding();
        }

        public override byte[] GetClientCertificatePublicKey()
        {
            return (this._wr == null) ? null : this._wr.GetClientCertificatePublicKey();
        }

        public override DateTime GetClientCertificateValidFrom()
        {
            return (this._wr == null) ? DateTime.Now : this._wr.GetClientCertificateValidFrom();
        }

        public override DateTime GetClientCertificateValidUntil()
        {
            return (this._wr == null) ? DateTime.Now : this._wr.GetClientCertificateValidUntil();
        }

        public override long GetConnectionID()
        {
            return (this._wr == null) ? 0L : this._wr.GetConnectionID();
        }

        public override string GetFilePath()
        {
            return (this._wr == null) ? null : this._wr.GetFilePath();
        }

        public override string GetFilePathTranslated()
        {
            return (this._wr == null) ? null : this._wr.GetFilePathTranslated();
        }

        public override int GetHashCode()
        {
            return (this._wr == null) ? 0 : this._wr.GetHashCode();
        }

        public override string GetHttpVerbName()
        {
            return (this._wr == null) ? null : this._wr.GetHttpVerbName();
        }

        public override string GetHttpVersion()
        {
            return (this._wr == null) ? null : this._wr.GetHttpVersion();
        }

        public override string GetKnownRequestHeader(int index)
        {
            if (index == HttpWorkerRequest.HeaderContentLength) //11
            {
                if (this._requestBuffer == null)
                    return null;
                else
                {
                    int length = this._requestBuffer.Length;
                    return length.ToString();
                }
            }
            return (this._wr == null) ? null : this._wr.GetKnownRequestHeader(index);
        }

        public override string GetLocalAddress()
        {
            return (this._wr == null) ? null : this._wr.GetLocalAddress();
        }

        public override int GetLocalPort()
        {
            return (this._wr == null) ? 0 : this._wr.GetLocalPort();
        }

        public override string GetPathInfo()
        {
            return (this._wr == null) ? null : this._wr.GetPathInfo();
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return (this._wr == null) ? null : this._requestBuffer;
        }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            int count = 0;
            byte[] preloadedEntityBody = this.GetPreloadedEntityBody();
            if (preloadedEntityBody != null)
            {
                count = preloadedEntityBody.Length;
                Buffer.BlockCopy(preloadedEntityBody, 0, buffer, offset, count);
            }
            return count;
        }

        public override int GetPreloadedEntityBodyLength()
        {
            return (this._requestBuffer == null) ? 0 : this._requestBuffer.Length;
        }

        public override string GetProtocol()
        {
            return (this._wr == null) ? null : this._wr.GetProtocol();
        }

        public override string GetQueryString()
        {
            return (this._wr == null) ? null : this._wr.GetQueryString();
        }

        public override byte[] GetQueryStringRawBytes()
        {
            return (this._wr == null) ? null : this._wr.GetQueryStringRawBytes();
        }

        public override string GetRawUrl()
        {
            return (this._wr == null) ? null : this._wr.GetRawUrl();
        }

        public override string GetRemoteAddress()
        {
            return (this._wr == null) ? null : this._wr.GetRemoteAddress();
        }

        public override string GetRemoteName()
        {
            return (this._wr == null) ? null : this._wr.GetRemoteName();
        }

        public override int GetRemotePort()
        {
            return (this._wr == null) ? 0 : this._wr.GetRemotePort();
        }

        public override int GetRequestReason()
        {
            return (this._wr == null) ? 0 : this._wr.GetRequestReason();
        }

        public override string GetServerName()
        {
            return (this._wr == null) ? null : this._wr.GetServerName();
        }

        public override string GetServerVariable(string name)
        {
            return (this._wr == null) ? null : this._wr.GetServerVariable(name);
        }

        public override int GetTotalEntityBodyLength()
        {
            return (this._requestBuffer == null) ? 0 : this._requestBuffer.Length;
        }

        public override string GetUnknownRequestHeader(string name)
        {
            return (this._wr == null) ? null : this._wr.GetUnknownRequestHeader(name);
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            return (this._wr == null) ? null : this._wr.GetUnknownRequestHeaders();
        }

        public override string GetUriPath()
        {
            return (this._wr == null) ? null : this._wr.GetUriPath();
        }

        public override long GetUrlContextID()
        {
            return (this._wr == null) ? 0 : this._wr.GetUrlContextID();
        }

        public override IntPtr GetUserToken()
        {
            return (this._wr == null) ? IntPtr.Zero : this._wr.GetUserToken();
        }

        public override IntPtr GetVirtualPathToken()
        {
            return (this._wr == null) ? IntPtr.Zero : this._wr.GetVirtualPathToken();
        }

        public override bool HeadersSent()
        {
            return (this._wr == null) ? false : this._wr.HeadersSent();
        }

        public override bool IsClientConnected()
        {
            return (this._wr == null) ? false : this._wr.IsClientConnected();
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return (this._wr == null) ? false : this._wr.IsEntireEntityBodyIsPreloaded();
        }

        public override bool IsSecure()
        {
            return (this._wr == null) ? false : this._wr.IsSecure();
        }

        public override string MapPath(string virtualPath)
        {
            return (this._wr == null) ? null : this._wr.MapPath(virtualPath);
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            return 0;
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            //byte[] buffer2 = new byte[size];
            //int count = this.ReadEntityBody(buffer2, size);
            //if (count > 0)
            //{
            //    Buffer.BlockCopy(buffer2, 0, buffer, offset, count);
            //}
            return 0; //count;
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            if (this._wr != null)
                this._wr.SendCalculatedContentLength(contentLength);
        }

        public override void SendCalculatedContentLength(long contentLength)
        {
            if (this._wr != null)
                this._wr.SendCalculatedContentLength(contentLength);
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            if (this._wr != null)
                this._wr.SendKnownResponseHeader(index, value);
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (this._wr != null)
                this._wr.SendResponseFromFile(handle, offset, length);
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            if (this._wr != null)
                this._wr.SendResponseFromFile(filename, offset, length);
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (this._wr != null)
                this._wr.SendResponseFromMemory(data, length);
        }

        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            if (this._wr != null)
                this._wr.SendResponseFromMemory(data, length);
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            if (this._wr != null)
                this._wr.SendStatus(statusCode, statusDescription);
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (this._wr != null)
                this._wr.SendUnknownResponseHeader(name, value);
        }

        public override void SetEndOfSendNotification(HttpWorkerRequest.EndOfSendNotification callback, object extraData)
        {
            if (this._wr != null)
                this._wr.SetEndOfSendNotification(callback, extraData);
        }

        public override string ToString()
        {
            return (this._wr == null) ? null : this._wr.ToString();
        }

        protected HttpWorkerRequest WorkerRequest
        {
            get { return (this._wr == null) ? null : this._wr; }
        }

        public override string MachineConfigPath
        {
            get { return (this._wr == null) ? null : this._wr.MachineConfigPath; }
        }

        public override string MachineInstallDirectory
        {
            get { return (this._wr == null) ? null : this._wr.MachineInstallDirectory; }
        }

        public override Guid RequestTraceIdentifier
        {
            get { return (this._wr == null) ? Guid.Empty : this._wr.RequestTraceIdentifier; }
        }

        public override string RootWebConfigPath
        {
            get { return (this._wr == null) ? null : this._wr.RootWebConfigPath; }
        }
    }
}

