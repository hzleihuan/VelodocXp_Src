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
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels; //Binding

using Memba.Common.Presentation; //Constants
using Memba.FileUpload.Providers; //FileStorage
using Memba.Files.Business; //File, FileBroker

// The following settings must be added to your configuration file in order for 
// the new Indigo item added to your project to work correctly.

// <system.serviceModel>
//    <services>
//      <!-- Before deployment, you should remove the returnFaults behavior configuration to avoid disclosing information in exception messages -->
//      <service name=".wcfTransferService" behaviorConfiguration="returnFaults">
//        <endpoint contract=".IwcfTransferService" binding="wsHttpBinding"/>
//      </service>
//    </services>
//    <behaviors>
//      <serviceBehaviors>
//        <behavior name="returnFaults" >
//          <serviceDebug includeExceptionDetailInFaults="true" />
//        </behavior>
//       </serviceBehaviors>
//    </behaviors>
// </system.serviceModel>

// A WCF Service consists of a contract (defined below), 
// a class which implements that interface, and configuration 
// entries that specify behaviors and endpoints associated with 
// that implementation (see <system.serviceModel> in your application
// configuration file).

namespace Memba.Files.WebServices
{
    /// <summary>
    /// wcfTransferService WCF Service class for integration with teh Outlook Add-In and other applications
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class wcfTransferService : ITransferService, IDisposable
    {
        #region Private Members
        //Implemented as a private variable to ensure it is disposed of properly
        private System.IO.Stream _StorageStream;
        #endregion

        #region ITransferService Members
        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="securityCode"></param>
        /// <returns></returns>
        public bool Authenticate(string email, string securityCode)
        {
            bool bRet = true;

            //Check email
            string sUserList = (string)System.Web.HttpRuntime.Cache[Constants.UserList];
            if (String.IsNullOrEmpty(sUserList))
            {
                sUserList = System.Web.Configuration.WebConfigurationManager.AppSettings[Constants.UserList];
                System.Web.HttpRuntime.Cache.Add(
                    Memba.Common.Presentation.Constants.UserList,
                    sUserList,
                    null,
                    System.Web.Caching.Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, Memba.Common.Presentation.Constants.SlidingExpiration, 0),
                    System.Web.Caching.CacheItemPriority.Default,
                    null);
            }
            if (sUserList.IndexOf(email, StringComparison.OrdinalIgnoreCase) < 0)
                bRet = false;

            //Check security code
            string sSecurityCode = (string)System.Web.HttpRuntime.Cache[Constants.SecurityCode];
            if (String.IsNullOrEmpty(sSecurityCode))
            {
                sSecurityCode = System.Web.Configuration.WebConfigurationManager.AppSettings[Constants.SecurityCode];
                System.Web.HttpRuntime.Cache.Add(
                    Memba.Common.Presentation.Constants.SecurityCode,
                    sSecurityCode,
                    null,
                    System.Web.Caching.Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, Memba.Common.Presentation.Constants.SlidingExpiration, 0),
                    System.Web.Caching.CacheItemPriority.Default,
                    null);
            }
            if (!sSecurityCode.Equals(securityCode))
                bRet = false;

            return bRet;
        }
        /// <summary>
        /// Upload a file
        /// </summary>
        /// <param name="request"></param>
        public void Upload(RemoteFileDescriptor request)
        {
            //report start
            System.Diagnostics.Trace.WriteLine("Start uploading " + request.FileName);

            //TODO: We need to report exceptions properly using FaultContractAttribute and FaultException
            //In this particular example, we want to raise a proper exception as soon as possible if the file upload is too large.
            //Note that currently the upload progresses for a while before the exception is raised.

            //Using HttpContext to access configuration data (especially httpRuntime section in web.config)
            //from WCF services requires HttpContext which is only available to WCF services in Asp.Net compatibility mode
            //See: http://blogs.msdn.com/wenlong/archive/2006/01/23/516041.aspx

            //But the good news is apparently since .NET 3.0 SP1, WCF services are not dependant upon the httpRuntime section
            //See: http://blogs.msdn.com/wenlong/archive/2008/03/10/why-changing-sendtimeout-does-not-help-for-hosted-wcf-services.aspx
            //So we essentially need to check that request.Length (unless there is a way to access the Content-Length Htpp header)
            //is lower than the value of BasicHttpBinding.MaxReceivedMessageSize configured in web.config

#if NOTUSED
            //Raise error Http 413 asap
            //Use FaultContractAttribute and FaultException to report an error
            Binding objBinding = OperationContext.Current.Host.Description.Endpoints[0].Binding;
            System.Diagnostics.Debug.Assert(objBinding.Name == "BasicHttpBinding");
            BasicHttpBinding objBasicHttpBinding = objBinding as BasicHttpBinding;
            if (objBasicHttpBinding != null)
            {
                if (request.Length > objBasicHttpBinding.MaxReceivedMessageSize)
                    throw new System.Web.HttpException(413, "Too large!");
            }
#endif

            //Authenticate user
            if (!Authenticate(request.Email, request.SecurityCode))
                throw new ApplicationException(Resources.Web.glossary.WebService_AuthenticationFailInfo);

            //Stream file
            int iBytesRead;
            byte[] arrBuffer = new byte[Constants.WcfStreamingBufferSize];

            //The file key is unknown from the client
            object objProviderFileKey = FileStorage.GetNewFileKey(request.FileName);

            //Create stream to write to
            _StorageStream = FileStorage.GetFileStream(objProviderFileKey, AccessMode.Create);
            do
            {
                // Read bytes from input stream
                iBytesRead = request.Stream.Read(arrBuffer, 0, Constants.WcfStreamingBufferSize);
                // Write bytes to output stream
                _StorageStream.Write(arrBuffer, 0, iBytesRead);

            } while (iBytesRead > 0);
            _StorageStream.Close();
            
            //Create file object
            File f = new File(
                request.FileGuid,
                request.FileName,
                request.ContentType,
                objProviderFileKey.ToString(),
                request.Length,
                request.HashCode);

            //Get path to file storage
            string sPath = FileStorage.Provider.ConnectionString;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sPath);
            
            //Add definition to file storage (Existence will be checked in FileBroker)
            FileBroker.Insert(f, di);

            // report end
            System.Diagnostics.Trace.WriteLine("Upload complete!");
        }
        /// <summary>
        /// Gets the size of a file
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <returns></returns>
        public long GetFileSize(Uri remoteFile)
        {
            //Get path to file storage
            string sPath = FileStorage.Provider.ConnectionString;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sPath);

            //Get the file guid
            string sFileGuid = System.IO.Path.GetFileNameWithoutExtension(remoteFile.LocalPath);
            Guid gFileGuid = new Guid(sFileGuid);

            //Get file definition
            File f = FileBroker.SelectByGuid(gFileGuid, di);
            System.Diagnostics.Debug.Assert(f.Guid == gFileGuid);

#if DEBUG
            _StorageStream = FileStorage.GetFileStream(f.Key, AccessMode.Read);
            System.Diagnostics.Debug.Assert(_StorageStream.Length.Equals(f.Size));
            _StorageStream.Close();
#endif

            //Return file size
            return f.Size;
        }
        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RemoteFileDescriptor Download(RemoteFileIdentifier request)
        {
            //Get path to file storage
            string sPath = FileStorage.Provider.ConnectionString;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sPath);

            //Get the file guid
            string sFileGuid = System.IO.Path.GetFileNameWithoutExtension(request.RemoteFile.LocalPath);
            Guid gFileGuid = new Guid(sFileGuid);

            //Get file definition
            File f = FileBroker.SelectByGuid(gFileGuid, di);
            System.Diagnostics.Debug.Assert(f.Guid == gFileGuid);

            //Report start
            System.Diagnostics.Trace.WriteLine("Start downloading " + f.FileName);

            //Open stream
            _StorageStream = FileStorage.GetFileStream(f.Key, AccessMode.Read);
            System.Diagnostics.Debug.Assert(f.Size == _StorageStream.Length);

            //Note that we cannot close the stream before returning the stream to the client
            //Otherwise the client gets a closed stream.
            //So we need to monitor the use of the stream made by the client to close it on a separate thread
            //once the client has entirely read it or once the connection is closed
            //System.Threading.Thread objMonitoringThread = new System.Threading.Thread(
            //    new System.Threading.ParameterizedThreadStart(this.MonitorAndClose));
            //objMonitoringThread.Start(objStorageStream);

            RemoteFileDescriptor objRemoteFileInfoRet = new RemoteFileDescriptor();
            objRemoteFileInfoRet.ContentType = f.ContentType;
            objRemoteFileInfoRet.Email = null;
            objRemoteFileInfoRet.FileGuid = f.Guid;
            objRemoteFileInfoRet.FileName = f.FileName;
            objRemoteFileInfoRet.HashCode = f.HashValue;
            objRemoteFileInfoRet.Length = f.Size;
            objRemoteFileInfoRet.SecurityCode = null;
            objRemoteFileInfoRet.Stream = _StorageStream;

            // report end
            System.Diagnostics.Trace.WriteLine("Download complete!");

            // return result
            return objRemoteFileInfoRet;
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// This method accepts a stream and closes it if it is not consumed for more than a specific period of time (see line with Sleep statement).
        /// </summary>
        private void MonitorAndClose(object stream)
        {
            System.IO.Stream objStream = stream as System.IO.Stream;
            System.Diagnostics.Debug.Assert(stream != null);

            do
            {
                long lPosition = objStream.Position;
                System.Threading.Thread.Sleep(Constants.WcfStreamingTimeOut);
                //If no new bytes have been read in the last Constants.WcfStreamingTimeOut millisecs
                //Then close teh stream
                if (lPosition == objStream.Position)
                {
                    objStream.Dispose();
                    objStream = null;
                    break;
                }
            } while (true);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes of resources (do not use the using pattern with teh service)
        /// </summary>
        public void Dispose()
        {
            if (_StorageStream != null)
            {
                _StorageStream.Dispose();
                _StorageStream = null;
            }
        }
        #endregion
    }
}