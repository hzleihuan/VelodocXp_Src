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
using System.Web; //AspNetHostingPermission
using System.Security.Permissions; //SecurityAction
using System.Web.UI; //Pair
using System.Globalization;
using System.Runtime.Remoting.Messaging; //OneWayAttribute;
using System.Web.Caching; //HttpRuntime.Cache

using Memba.FileUpload.Properties;

namespace Memba.FileUpload
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2112:SecuredTypesShouldNotExposeFields", Justification = "UploadIdParam field is a constant, there is no point applying the property accessor pattern.")]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    //Class is sealed [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public static class UploadMonitor
    {
        public const string UploadIdParam = "muid"; //mu is for memba upload
        public const string ForceHttpMaxRequestLengthParam = "mumax"; //=0 if uploadRuntime applies (default), otherwise 1
        internal const string UPLOAD_DATA_PREFIX = "MU_";
        private const int CACHE_SLIDINGEXPIRATION = 1; //in Days
       
        #region Other Members

        #region SetUploadData
        /// <summary>
        /// Sets upload data in context cache
        /// </summary>
        /// <param name="context"></param>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        public static UploadData SetUploadData(HttpContext context, string uploadId)
        {
#if(TRACE)
            ThreadLog.WriteLine("Setting upload data with " + uploadId);
#endif

            //Note:
            //Adding UploadData to HttpContext.Items is not a possible option considering HttpContext.Items are only available during the lifetime and scope of a single request
            //Adding UploadData to HttpSessionState is a reasonable choice to share data between requests and prevent other users to access uploaded files but the server hangs the connection 
            //So we are left with two options:
            // 1) storing UploadData in HttpApplicationState.
            // 2) storing UploadData in HttpRuntime.Cache
            //Note that HttpApplicationState is a legacy of ASP, and the use of HttpRuntime.Cache is recommended in ASP.NET
            
            //Also note:
            //Because HttpRuntime.Cache is global, we need to prevent unauthorized access and check the context user each time UploadData is requested.
            
#if(DEBUG)
            //DEBUG mode should support unit tests
            if ((context == null) || (String.IsNullOrEmpty(uploadId)))
                return new UploadData((String.IsNullOrEmpty(uploadId) ? String.Empty : uploadId), String.Empty, false);
#endif
            
            if (context == null)
                throw new ArgumentNullException("context", Resources.ExceptionNullContext);            

            if (String.IsNullOrEmpty(uploadId))
                throw new ArgumentNullException("uploadId", Resources.ExceptionNullOrEmptyUploadId);

            //Needs to be removed before it can be added
            if (context.Items.Contains(UploadMonitor.UploadIdParam))
                context.Items.Remove(UploadMonitor.UploadIdParam);
            context.Items.Add(UploadMonitor.UploadIdParam, uploadId);

            UploadData objUploadDataRet = new UploadData(uploadId, context.User.Identity.Name, context.User.Identity.IsAuthenticated);

            if (HttpRuntime.Cache.Get(UPLOAD_DATA_PREFIX + uploadId) != null)
                HttpRuntime.Cache.Remove(UPLOAD_DATA_PREFIX + uploadId);

            //TODO: Not sure we could create an event callback because we need teh data after the module has completed its part of the request
            //See: http://blogs.msdn.com/tess/archive/2006/08/11/asp-net-quiz-answers-does-page-cache-leak-memory.aspx
            HttpRuntime.Cache.Add(
                UPLOAD_DATA_PREFIX + uploadId,
                objUploadDataRet,
                null,
                Cache.NoAbsoluteExpiration,
                new TimeSpan(CACHE_SLIDINGEXPIRATION, 0, 0, 0),
                CacheItemPriority.AboveNormal, //Make it above normal just in case scavenging is triggered because some caching space is needed
                null
                );

            return objUploadDataRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        public static UploadData SetUploadData(string uploadId)
        {
            return SetUploadData(HttpContext.Current, uploadId);
        }
#endregion

        #region GetUploadData
        /// <summary>
        /// Gets upload data from context cache
        /// </summary>
        /// <param name="context"></param>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        public static UploadData GetUploadData(HttpContext context, string uploadId)
        {
#if(TRACE)
            ThreadLog.WriteLine("Getting upload data for " + uploadId);
#endif

#if(DEBUG)
            //DEBUG mode should support unit tests
            if ((context == null) || (String.IsNullOrEmpty(uploadId)))
                return new UploadData(String.Empty, String.Empty, false);
#endif

            if (context == null)
                throw new ArgumentNullException("context", Resources.ExceptionNullContext);
            
            if (String.IsNullOrEmpty(uploadId))
                throw new ArgumentNullException("uploadId", Resources.ExceptionNullOrEmptyUploadId);
            
            //Try first to retrieve upload context data from context cache
            UploadData objUploadDataRet = HttpRuntime.Cache.Get(UPLOAD_DATA_PREFIX + uploadId) as UploadData;

            //If upload context data is available in application state/context cache, check user provided download has started
            if (objUploadDataRet != null)
            {
#if(TRACE)
                ThreadLog.WriteLine("Upload status is " + objUploadDataRet.ProgressStatus.ToString());
#endif
                //We need to test for Flash applets, because the Flash applet works in anonymous mode
                //but passes cookies which may contain the authentication from previous sessions on the web site
                //implying a failure of the test below
                if (!context.Request.UrlReferrer.AbsolutePath.ToLowerInvariant().EndsWith(".swf"))
                {
                    if ((objUploadDataRet.IsAuthenticated != context.User.Identity.IsAuthenticated)
                        || (objUploadDataRet.UserName != context.User.Identity.Name))
                        throw new System.Security.SecurityException(Resources.ExceptionUnauthorizedAccessToUploadData);
                }
            }
            else
            {
                //Upload context data could be null, for example when the uploadId requested does not exist 
#if(TRACE)
                ThreadLog.WriteLine("Upload data is null");
#endif
            }

            return objUploadDataRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        public static UploadData GetUploadData(string uploadId)
        {
            return GetUploadData(HttpContext.Current, uploadId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UploadData GetUploadData()
        {
            HttpContext objHttpContext = HttpContext.Current;
            string sUploadId = null;

            if (objHttpContext != null)
                sUploadId = objHttpContext.Items[UploadMonitor.UploadIdParam] as String;

            return GetUploadData(objHttpContext, sUploadId);
        }
        #endregion

        #region Release
        /// <summary>
        /// Releases entry in HttpRuntime.Cache after an upload has been completed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="uploadId"></param>
        public static void Release(HttpContext context, string uploadId)
        {
            if (context == null)
                throw new ArgumentNullException("context", Resources.ExceptionNullContext);

            if (String.IsNullOrEmpty(uploadId))
                throw new ArgumentNullException("uploadId", Resources.ExceptionNullOrEmptyUploadId);
            
            if ((context != null) && (!String.IsNullOrEmpty(uploadId)))
            {
                HttpRuntime.Cache.Remove(UPLOAD_DATA_PREFIX + uploadId);
                context.Items.Remove(UploadMonitor.UploadIdParam);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadId"></param>
        public static void Release(string uploadId)
        {
            Release(HttpContext.Current, uploadId);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Release()
        {
            HttpContext objHttpContext = HttpContext.Current;
            string sUploadId = null;
            
            if (objHttpContext != null)
                sUploadId = objHttpContext.Items[UploadMonitor.UploadIdParam] as String;

            Release(objHttpContext, sUploadId);
        }
        #endregion

        #region CancelUpload
        /// <summary>
        /// Called by user from page to cancel an upload
        /// </summary>
        public static void CancelUpload(HttpContext context, string uploadId)
        {
            if (context == null)
                throw new ArgumentNullException("context", Resources.ExceptionNullContext);

            if (String.IsNullOrEmpty(uploadId))
                throw new ArgumentNullException("uploadId", Resources.ExceptionNullOrEmptyUploadId);
                       
            UploadData objUploadData = GetUploadData(context, uploadId);
            
            if ((objUploadData != null)
                && (objUploadData.ProgressStatus != UploadProgressStatus.Canceled)
                && (objUploadData.ProgressStatus != UploadProgressStatus.Completed))
                objUploadData.ReportCanceled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadId"></param>
        public static void CancelUpload(string uploadId)
        {
            CancelUpload(HttpContext.Current, uploadId);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void CancelUpload()
        {
            HttpContext objHttpContext = HttpContext.Current;
            string sUploadId = null;

            if (objHttpContext != null)
                sUploadId = objHttpContext.Items[UploadMonitor.UploadIdParam] as String;

            CancelUpload(objHttpContext, sUploadId);
        }
        #endregion

        #endregion
    }
}
