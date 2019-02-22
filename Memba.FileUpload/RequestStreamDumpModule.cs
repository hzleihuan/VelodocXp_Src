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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Security.Permissions; //SecurityAction

namespace Memba.FileUpload
{
    /// <summary>
    /// This module is only used for debugging. It dumps the upload request to a file on disk.
    /// To use, just add the module to the httpModules section of web.objSiteConfiguration.
    /// You can optionally specify the dump file location (dumpFileLocation) in the appSettings
    /// section of web.objSiteConfiguration.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    //Class is sealed [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class RequestStreamDumpModule : IHttpModule
    {
        #region Private Members
        private static string _DumpFileLocation;
        private static object _SyncLock = new object(); 
        #endregion

        #region IHttpModule Members
        /// <summary>
        /// 
        /// </summary>
        void IHttpModule.Dispose()
        {
            //Leave blank becuase there is nothing to do here
        }
        /// <summary>
        /// Init is used to specifiy the application events the httpModule will subscribe to.
        /// </summary>
        /// <param name="context"></param>
        void IHttpModule.Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += new EventHandler(this.OnPreRequestHandlerExecute);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Occurs just before ASP.NET begins executing an http handler (for example, a page or an XML Web service). 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication objHttpApplication = sender as HttpApplication;
            System.Diagnostics.Debug.Assert(objHttpApplication.Context != null);

            if (RequestStreamDumpModule.IsUploadRequest(objHttpApplication.Context))
            {
                HttpWorkerRequest objWorkerRequest = GetWorkerRequest(objHttpApplication.Context);
                if (objWorkerRequest != null)
                {
                    using (RequestStream objRequestStream = new RequestStream(objWorkerRequest))
                    using (FileStream objFileStream = File.Create(RequestStreamDumpModule.GetDumpFilePath()))
                    {
                        byte[] arrBuffer = new byte[Constants.BufferSize];
                        int iBytesReadToWrite = 0;
                        long lTotalBytesReadToWrite = 0;
                        long lContentLength = long.Parse(objWorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength), CultureInfo.InvariantCulture); //11
                        while ((lTotalBytesReadToWrite < lContentLength) && ((iBytesReadToWrite = objRequestStream.Read(arrBuffer, 0, Constants.BufferSize)) > 0))
                        {
                            objFileStream.Write(arrBuffer, 0, iBytesReadToWrite);
                            lTotalBytesReadToWrite += iBytesReadToWrite;
                        }
                        //objFileStream.Close();
                    }
                }
            }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetDumpFilePath()
        {
            string sRet;
            string sFileBaseName;
            lock (_SyncLock)
            {
                if (_DumpFileLocation == null)
                {

                    _DumpFileLocation = ConfigurationManager.AppSettings["dumpFileLocation"];
                    if ((_DumpFileLocation != null) && !Path.IsPathRooted(_DumpFileLocation))
                    {
                        _DumpFileLocation = HttpContext.Current.Server.MapPath(_DumpFileLocation);
                    }
                    if ((_DumpFileLocation == null) || !Directory.Exists(_DumpFileLocation))
                    {
                        _DumpFileLocation = HttpContext.Current.Server.MapPath("~/");
                    }
                }
                sFileBaseName = Path.Combine(_DumpFileLocation, "requestDump");
            }
            string sFileExtention = ".bak";
            int index = 0;
            do
            {
                index++;
                sRet = string.Concat(new string[] { sFileBaseName, "(", index.ToString(CultureInfo.InvariantCulture), ")", sFileExtention });
            }
            while (File.Exists(sRet));
            return sRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static HttpWorkerRequest GetWorkerRequest(HttpContext httpContext)
        {
            IServiceProvider objServiceProvider = (IServiceProvider)httpContext;
            HttpWorkerRequest objWorkerRequest = (HttpWorkerRequest)objServiceProvider.GetService(typeof(HttpWorkerRequest));
            return objWorkerRequest;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static bool IsUploadRequest(HttpContext httpContext)
        {
            string sContentType = httpContext.Request.ContentType;
            bool bRet = (sContentType != null && sContentType.StartsWith(Constants.MultiPartFormData, StringComparison.InvariantCultureIgnoreCase));
            return bRet;
        }
        #endregion
    }
}
