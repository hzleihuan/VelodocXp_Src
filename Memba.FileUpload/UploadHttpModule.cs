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
using System.Web; //IHttpModule, HttpException
using System.Reflection; //BindingFlags
using System.Configuration; //Configuration
using System.Web.Configuration; //HttpRuntimeSection
using System.Threading; //ReaderWriterLock;
using System.Runtime.Remoting.Messaging; //OneWayAttribute;
using System.Security.Cryptography; //HashAlgorithm
using System.Security.Permissions; //SecurityAction
using System.Globalization; //CultureInfo
using System.ComponentModel; //LicenseProviderAttribute
using System.Xml; //XmlTextReader
using Memba.FileUpload.Properties;
using Memba.FileUpload.Providers;

namespace Memba.FileUpload
{
    /// <summary>
    /// 
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    //Class is sealed [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [LicenseProviderAttribute(typeof(Licensing.VelodocLicenseProvider))]
    public sealed class UploadHttpModule : IHttpModule, IDisposable
    {
        // The HttpApplication has these events that fire in the order shown: 
        //  1 - BeginRequest 
        //  2 - AuthenticateRequest 
        //  3 - AuthorizeRequest 
        //  4 - ResolveRequestCache 
        //     [A handler (a page corresponding to the request URL) is created at this point.] 
        //  5 - AcquireRequestState 
        //  6 - PreRequestHandlerExecute 
        //     [The handler is executed. In our case the Page] 
        //  7 - PostRequestHandlerExecute 
        //  8 - ReleaseRequestState 
        //     [Response filters, if any, filter the output.] 
        //  9 - UpdateRequestCache 
        // 10 - EndRequest 
        // It looks like the time to run the UploadHttpModule is during the PreRequestHandlerExecute event handler,
        // that is after authentication and authorization but just before the page itself.

        #region Private Members
        Licensing.VelodocLicense _License = null;

        //Cached values
        private static Nullable<long> _MaxAllowedContentLengthBytes = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public UploadHttpModule()
        {
            _License = (Licensing.VelodocLicense)LicenseManager.Validate(typeof(UploadHttpModule), this);
        }
        #endregion

        #region IHttpModule Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.PreRequestHandlerExecute += new EventHandler(this.OnPreRequestHandlerExecute);
            context.PostRequestHandlerExecute += new EventHandler(this.OnPostRequestHandlerExecute);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Dispose of resources 
        /// </summary>
        public void Dispose()
        {
            if (_License != null)
            {
                _License.Dispose();
                _License = null;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Occurs just before ASP.NET begins executing an http handler (for example, a page or an XML Web service). 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Too many exception types to catch to consider duplicating the code")]
        private void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication objHttpApplication = sender as HttpApplication;
            System.Diagnostics.Debug.Assert(objHttpApplication != null);
            HttpRequest objHttpRequest = objHttpApplication.Request;
            System.Diagnostics.Debug.Assert(objHttpRequest != null);

            if (objHttpRequest != null && UploadHttpModule.IsUploadRequest(objHttpRequest))
            {
#if(TRACE)
                ThreadLog.WriteLine("Entered OnPreRequestHandlerExecute for an upload request");
#endif

                HttpContext objHttpContext = objHttpApplication.Context;
                System.Diagnostics.Debug.Assert(objHttpContext != null);

                HttpWorkerRequest objHttpWorkerRequest = UploadHttpModule.GetWorkerRequest(objHttpContext);
                System.Diagnostics.Debug.Assert(objHttpWorkerRequest != null);

                if (objHttpWorkerRequest != null)
                {
                    //long lContentLength = objHttpRequest.ContentLength;
                    long lContentLength = long.Parse(objHttpWorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength), CultureInfo.InvariantCulture);

                    if (lContentLength <= 0) //This is for Flash 8 FileReference which tests an empty post before sending a large upload
                    {
#if(TRACE)
                        ThreadLog.WriteLine("No content, maybe Flash");
#endif

                        HealthMonitoringManager.LogSucessEvent(
                            Resources.ExceptionZeroContentLength,
                            null, //<- no upload data yet
                            UploadRequestSuccessEvent.RuntimeUploadSuccessZeroContentLength);

                        objHttpApplication.Response.StatusCode = 200; //OK
                        objHttpApplication.CompleteRequest();
                        //See: http://support.microsoft.com/kb/312629
                        //objHttpApplication.Response.End();
                        return;
                    }

                    //Initialize an upload monitor
                    string sUploadID = UploadHttpModule.GetUploadID(objHttpRequest);
                    if (String.IsNullOrEmpty(sUploadID))
                    {
#if(TRACE)
                        ThreadLog.WriteLine("No upload ID");
#endif

                        HttpException objHttpException = new HttpException(400, Resources.ExceptionNullOrEmptyUploadId);

                        HealthMonitoringManager.LogErrorEvent(
                            Resources.ExceptionNullOrEmptyUploadId,
                            null, //<- no upload data yet
                            UploadRequestErrorEvent.RuntimeErrorMissingUploadID,
                            objHttpException);

                        UploadHttpModule.CloseConnectionAfterError(objHttpApplication.Response);
                        throw objHttpException;

                        //See comment in relation with MaxRequestLength here below
                        //objHttpApplication.Response.StatusCode = 400; //Bad request
                        //objHttpApplication.Response.StatusDescription = Resources.ExceptionNullOrEmptyUploadId;
                        //objHttpApplication.Response.Write(String.Format(Resources.Culture, Resources.ErrorPage, 400, Resources.ExceptionNullOrEmptyUploadId));
                        //objHttpApplication.CompleteRequest();
                        //See: http://support.microsoft.com/kb/312629
                        //objHttpApplication.Response.End();
                    }

                    UploadData objUploadData = UploadMonitor.SetUploadData(objHttpContext, sUploadID);
                    System.Diagnostics.Debug.Assert(objUploadData != null);

                    //Check whether we should read MaxRequestLength from httpRuntime section of web.config (true)
                    bool bMaximizeRequestLength = UploadHttpModule.GetForceHttpMaxRequestLength(objHttpRequest);
                    //Check the upload size and end request if file is too big
                    long lMaxRequestLength = UploadHttpModule.GetMaxRequestLengthBytes(objHttpContext, bMaximizeRequestLength);
                    if ((lMaxRequestLength >= 0) && (lContentLength > lMaxRequestLength))
                    {
#if(TRACE)
                        ThreadLog.WriteLine("Post request is too large");
#endif

                        HttpException objHttpException = new HttpException(413, Resources.ExceptionPostTooLarge);

                        HealthMonitoringManager.LogErrorEvent(
                            Resources.ExceptionPostTooLarge,
                            objUploadData,
                            UploadRequestErrorEvent.RuntimeErrorPostTooLarge,
                            objHttpException);

                        objUploadData.ReportFailed(objHttpException);

                        UploadHttpModule.CloseConnectionAfterError(objHttpApplication.Response);
                        throw objHttpException;

                        //There are 3 possible options 
                        //1) Do nothing and let httpRuntime/maxRequestlength do its job
                        //2) Do something like 
                        //      objHttpApplication.Response.StatusCode = 413;
                        //      objHttpApplication.Response.StatusDescription = Resources.ExceptionPostTooLarge;
                        //      objHttpApplication.Response.Write(String.Format(Resources.Culture, Resources.ErrorPage, 413, Resources.ExceptionPostTooLarge));
                        //      objHttpApplication.CompleteRequest();
                        //      See: http://support.microsoft.com/kb/312629
                        //      //objHttpApplication.Response.End();
                        //      return;
                        //3) Raise an HttpException

                        //Option 1 is no more an option since we have implemented uploadRuntime

                        //Option 2 sometimes aborts and closes the connection with an IE error page,
                        //sometimes displays a blank page. When the IE page appears, we get 
                        //an ERROR_INTERNET_CONNECTION_ABORTED, when the blank page is displayed
                        //the post returns a 413 status code. To get some content we would need 
                        //to write to the response _Input using something like objHttpApplication.Response.Write
                        
                        //HttpRequest.GetEntireRawContent implements option 3). Actually it triggers
                        //throw new HttpException(SR.GetString("Max_request_length_exceeded"), null, 0xbbc);
                        //after calling HttpResponse.CloseConnectionAfterError(). In this case an unhdandled
                        //exception is thrown abd we can rely on Application_Error and Custom Errors which
                        //is the best option.
                    }

#if(TRACE)
                    ThreadLog.WriteLine("Start parsing upload _Input");
#endif

                    Encoding objEncoding = objHttpRequest.ContentEncoding;

                    string sContentType = objHttpRequest.ContentType;
                    int iPos = sContentType.ToLowerInvariant().IndexOf(Constants.MultiPartBoundary);
                    if (iPos < 0)
                    {
#if(TRACE)
                        ThreadLog.WriteLine("Bad request");
#endif

                        HttpException objHttpException = new HttpException(400, Resources.ExceptionMalformedContentType);

                        HealthMonitoringManager.LogErrorEvent(
                            Resources.ExceptionMalformedContentType,
                            objUploadData,
                            UploadRequestErrorEvent.RuntimeErrorMalformedContentType,
                            objHttpException
                            );

                        objUploadData.ReportFailed(objHttpException);

                        UploadHttpModule.CloseConnectionAfterError(objHttpApplication.Response);
                        throw objHttpException;

                        //See comment in relation with MaxRequestLength here above
                        //objHttpApplication.Response.StatusCode = 400; //Bad request
                        //objHttpApplication.Response.StatusDescription = Resources.ExceptionMultipartBoundaryNotFound;
                        //objHttpApplication.Response.Write(String.Format(Resources.Culture, Resources.ErrorPage, 400, Resources.ExceptionMultipartBoundaryNotFound));
                        //objHttpApplication.CompleteRequest();
                        //See: http://support.microsoft.com/kb/312629
                        //objHttpApplication.Response.End();
                        //return;
                    }
                    string sMultiPartBoundary = Constants.BoundaryPrefix + sContentType.Substring(iPos + Constants.MultiPartBoundary.Length);
#if(TRACE)
                    ThreadLog.WriteLine("Identified boundary = " + sMultiPartBoundary);
#endif

                    RequestStream objRequestStream = null;
                    RequestFilter objRequestFilter = null;

                    try
                    {
                        HashAlgorithm objHashAlgorithm = CryptoConfig.CreateFromName(Constants.HashAlgorithmName) as HashAlgorithm;
                        //objHashAlgorithm.Initialize(); Done in RequestFilter
                        objRequestStream = new RequestStream(objHttpWorkerRequest);
                        objRequestFilter = new RequestFilter(objRequestStream, objHashAlgorithm, lContentLength, objEncoding, sMultiPartBoundary, objUploadData);

#if(TRACE)
                        ThreadLog.WriteLine("Started parsing");
#endif

                        //Parse the request to filter input files
                        MimeParser objMimeParser = new MimeParser(objRequestFilter);
                        objMimeParser.Parse();
                        //Get the filtered request
                        byte[] arrFilteredRequest = objRequestFilter.Encoding.GetBytes(objRequestFilter.FilteredRequest);
                        //Redirect the filtered request
                        RedirectFilteredRequest(objHttpApplication, objHttpWorkerRequest, arrFilteredRequest);

#if(TRACE)
                        ThreadLog.WriteLine("Filtered request redirected");
#endif

                        HealthMonitoringManager.LogSucessEvent(
                            Resources.MessageUploadCompleted,
                            objUploadData,
                            UploadRequestSuccessEvent.RuntimeUploadSuccessCompleted
                            );
                    }
                    catch (Exception Ex)
                    {
#if(TRACE)
                        ThreadLog.WriteLine("Parsing error");
#endif
                        HealthMonitoringManager.LogErrorEvent(
                            Resources.ExceptionUnhandled + "\r\n" + objHttpWorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderUserAgent),
                            objUploadData,
                            UploadRequestErrorEvent.RuntimeErrorExceptionUnhandled,
                            Ex);

                        objUploadData.ReportFailed(Ex);

                        UploadHttpModule.CloseConnectionAfterError(objHttpApplication.Response);
                        if ((Ex is HttpException) || (Ex is System.Net.WebException))
                            throw;
                        else
                            throw new HttpException(500, Resources.ExceptionUnhandled, Ex);

                        //objHttpApplication.Response.StatusCode = 500; //Error
                        //objHttpApplication.Response.StatusDescription = Resources.ExceptionUnhandled;
                        //objHttpApplication.Response.Write(String.Format(Resources.Culture, Resources.ErrorPage, 500, Ex.Message));
                        //objHttpApplication.CompleteRequest();
                        //See: http://support.microsoft.com/kb/312629
                        //objHttpApplication.Response.End();
                    }
                    finally
                    {
#if(TRACE)
                        ThreadLog.WriteLine("Disposing of resources");
#endif

                        if (objRequestFilter != null)
                            objRequestFilter.Dispose();
                        if (objRequestStream != null)
                            objRequestStream.Dispose();

                        if (objUploadData != null)
                        {
                            if (objUploadData.ProgressStatus != UploadProgressStatus.Completed)
                                this.DeleteUploadFiles(objUploadData.UploadFiles);

                            //Too soon to release here: let sliding expiration work for us
                            //UploadMonitor.Release(objHttpContext, objUploadData.UploadId);
                        }
                    }
                }
#if(TRACE)
                ThreadLog.WriteLine("Exit OnPreRequestHandlerExecute");
#endif
            }

        }
        /// <summary>
        /// Occurs when the ASP.NET event handler (for example, a page or an XML Web service) finishes execution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="http://www.ondotnet.com/pub/a/dotnet/2003/10/20/httpfilter.html"/>
        /// <see cref="http://www.codeproject.com/aspnet/RemovingWhiteSpacesAspNet.asp"/>
        void OnPostRequestHandlerExecute(object sender, EventArgs e)
        {
            //Filtering should only apply if no valid license key
            if (_License.LicenseType != Licensing.LicenseType.EULA)
            {
                HttpContext objCurrentContext = HttpContext.Current;
                if (objCurrentContext != null)
                {
                    //HttpRequest objHttpRequest = objCurrentContext.Request;
                    HttpResponse objHttpResponse = objCurrentContext.Response;
                    //if((objHttpRequest.Url.CurrentExecutionFilePath.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                    //    || (objHttpRequest.Url.CurrentExecutionFilePath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
                    //    || (objHttpRequest.Url.CurrentExecutionFilePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)))
                    //{
                        //Checking content text/html is not sufficient, because when getting upload.gif
                        //IIS 7 returns a reponse with status code 304 and content type text/html
                        if ((objHttpResponse.ContentType == "text/html") && (objHttpResponse.StatusCode == 200))
                            objHttpResponse.Filter = new Licensing.VelodocLicenseFilter(objHttpResponse.Filter, objHttpResponse.ContentEncoding);
                    //}
                }
            }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// Gets the maxRequestlength applicable to the requested URL.
        /// MaxRequestLength is set in the httpRuntime section of web.config.
        /// Note that this takes into account specialization in a specific location.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="forceHttpMaxRequestLength">If true, the MaxRequestLength is defined by httpRuntime, otherwise it is defined by uploadRuntime if it exists</param>
        /// <returns></returns>
        public static long GetMaxRequestLengthBytes(HttpContext httpContext, bool forceHttpMaxRequestLength)
        {
#if DEBUG
            System.Collections.IDictionaryEnumerator objEnumerator = HttpRuntime.Cache.GetEnumerator();
            System.Diagnostics.Trace.WriteLine("--- BEGIN: Listing Runtime Cache ---");
            while (objEnumerator.MoveNext())
            {
                System.Diagnostics.Trace.WriteLine(String.Format("{0} -> {1}", objEnumerator.Key, objEnumerator.Value));
            }
            System.Diagnostics.Trace.WriteLine("--- END: Listing Runtime Cache ---");
#endif
            string sCacheKey = (forceHttpMaxRequestLength ? "T" : "F") + System.Web.HttpContext.Current.Request.CurrentExecutionFilePath.ToLowerInvariant();
            object objCachedValueRet = HttpRuntime.Cache.Get(sCacheKey);
            if (objCachedValueRet != null)
                return (long)objCachedValueRet;

            //Check if we are in IIS 7 to read requestFiltering settings
            string sIisVersion = httpContext.Request.ServerVariables["SERVER_SOFTWARE"];
            if (sIisVersion.Equals("Microsoft-IIS/7.0") && (!_MaxAllowedContentLengthBytes.HasValue))
            {
                /*
                In web.config, you may have
                <security>
                  <requestFiltering>
                    <!-- Change this value to increase file upload size on IIS 7 -->
                    <requestLimits maxAllowedContentLength="104865792"/>
                  </requestFiltering>
                </security>
                */

                //We have tried using Microsoft.Web.Administration classes to access this section but the line following
                //ServerManager objServerManager = new ServerManager();
                //raises an UnauthorizedAccessException on accessing "redirection.config"

                //The following code raises an InvalidOperationException : This operation does not apply at runtime.
                //ConfigurationSection objConfigurationSection = System.Configuration.ConfigurationManager.GetSection("loggingConfiguration") as ConfigurationSection;
                //string sXml = objConfigurationSection.SectionInformation.GetRawXml();

                //Read web.config
                try
                {
                    Configuration objConfiguration = WebConfigurationManager.OpenWebConfiguration(System.Web.HttpContext.Current.Request.ApplicationPath);
                    foreach (ConfigurationSection objSection in objConfiguration.Sections)
                    {
                        //Get "system.webServer" section
                        if (objSection.SectionInformation.Name == "system.webServer")
                        {
                            string sXmlFragment = objSection.SectionInformation.GetRawXml();
                            using (XmlTextReader objXmlSectionReader = new XmlTextReader(sXmlFragment, XmlNodeType.Element, null))
                            {
                                //Find the "security" element
                                while (objXmlSectionReader.Read())
                                {
                                    if ((objXmlSectionReader.NodeType == XmlNodeType.Element)
                                        && (objXmlSectionReader.LocalName.Equals("security")))
                                    {
                                        using (XmlReader objXmlSecurityReader = objXmlSectionReader.ReadSubtree())
                                        {
                                            //Find the "requestLimits" element 
                                            while (objXmlSecurityReader.Read())
                                            {
                                                if ((objXmlSecurityReader.NodeType == XmlNodeType.Element)
                                                    && (objXmlSecurityReader.LocalName.Equals("requestFiltering")))
                                                {
                                                    using (XmlReader objXmlRequestFilteringReader = objXmlSecurityReader.ReadSubtree())
                                                    {
                                                        //Find the "add" element 
                                                        while (objXmlRequestFilteringReader.Read())
                                                        {
                                                            if ((objXmlRequestFilteringReader.NodeType == XmlNodeType.Element)
                                                                && (objXmlRequestFilteringReader.LocalName.Equals("requestLimits")))
                                                            {
                                                                objXmlRequestFilteringReader.MoveToAttribute("maxAllowedContentLength");
                                                                if (objXmlRequestFilteringReader.ReadAttributeValue())
                                                                    _MaxAllowedContentLengthBytes = long.Parse(objXmlRequestFilteringReader.Value);
                                                            }
                                                            if (_MaxAllowedContentLengthBytes.HasValue)
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (_MaxAllowedContentLengthBytes.HasValue)
                                                    break;
                                            }
                                        }
                                    }
                                    if (_MaxAllowedContentLengthBytes.HasValue)
                                        break;
                                }
                            }

                            //"system.webServer" section processed, no need to iterate further
                            break;
                        }
                    }
                }
                catch { }
                finally
                {
                    if (!_MaxAllowedContentLengthBytes.HasValue)
                        _MaxAllowedContentLengthBytes = Constants.DefaultMaxAllowedContentLengthBytes;

                    //TODO: Note this is far from being perfect as the default value may have been overriden in applicationHost.config
                }
            }

            /*
            See content of HttpRequest.GetEntireRawContent in Reflector
            HttpRuntimeSection httpRuntime = RuntimeConfig.GetConfig(this._context).HttpRuntime;
            int maxRequestLengthBytes = httpRuntime.MaxRequestLengthBytes;
            if (this.ContentLength > maxRequestLengthBytes)
            {
                this.Response.CloseConnectionAfterError();
                throw new HttpException(SR.GetString("Max_request_length_exceeded"), null, 0xbbc);
            }
            */
            HttpRuntimeSection objHttpRuntimeSection = httpContext.GetSection(Constants.HttpRuntimeSection) as HttpRuntimeSection;
            long lMaxRequestLengthBytes = Constants.DefaultMaxRequestLengthBytes;
            if (objHttpRuntimeSection != null && objHttpRuntimeSection.MaxRequestLength >= 0)
            {
                lMaxRequestLengthBytes = (long)objHttpRuntimeSection.MaxRequestLength * Constants.Kilo;
                if (!forceHttpMaxRequestLength)
                {
                    UploadRuntimeSection objUploadRuntimeSection = httpContext.GetSection(Constants.UploadRuntimeSection) as UploadRuntimeSection;
                    if ((objUploadRuntimeSection != null) && (objUploadRuntimeSection.MaxRequestLength >= 0))
                        lMaxRequestLengthBytes = (long)objUploadRuntimeSection.MaxRequestLength * Constants.Kilo;
                }
                if (lMaxRequestLengthBytes > Constants.TopMaxRequestLengthBytes)
                    lMaxRequestLengthBytes = Constants.TopMaxRequestLengthBytes;
            }

            //If we are under IIS7 and maxAllowedContentLengthBytes has been set to a lower value, this value prevails
            if ((_MaxAllowedContentLengthBytes.HasValue) && (lMaxRequestLengthBytes > _MaxAllowedContentLengthBytes.Value))
                lMaxRequestLengthBytes = _MaxAllowedContentLengthBytes.Value;

            HttpRuntime.Cache.Add(
                sCacheKey,
                lMaxRequestLengthBytes,
                null, //TODO: ideally we should have a dependency on the web.config file to refresh the cache when modified
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                new TimeSpan(0, Constants.SlidingExpiration, 0),
                System.Web.Caching.CacheItemPriority.Normal,
                null);

            return lMaxRequestLengthBytes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        internal static HttpWorkerRequest GetWorkerRequest(HttpContext httpContext)
        {
            IServiceProvider objServiceProvider = (IServiceProvider)httpContext;
            HttpWorkerRequest objWorkerRequest = (HttpWorkerRequest)objServiceProvider.GetService(typeof(HttpWorkerRequest));
            return objWorkerRequest;
        }
        /// <summary>
        /// Checks whether the request is an upload request
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        internal static bool IsUploadRequest(HttpRequest httpRequest)
        {
            string sContentType = httpRequest.ContentType;
            bool bRet = (sContentType != null && sContentType.StartsWith(Constants.MultiPartFormData, StringComparison.InvariantCultureIgnoreCase));
            return bRet;
        }
        /// <summary>
        /// Gets the uploadId defined by a parameter called "muid" from the request
        /// Use UploadMonitor.UploadId to get the name of the parameter, e.g. "muid".
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        internal static string GetUploadID(HttpRequest httpRequest)
        {           
            //Try getting the value for UploadIdParam from querystring
            return httpRequest.QueryString[UploadMonitor.UploadIdParam];
           
            //Form fields: form fields do not work because if you try to access
            //them, you partially read the entity body and there is nothing left to process
            //for the module which hangs
            
            //Cookies: Cookies are not an option because many browser security settings
            //reject them, and they are more complex to implement than the query string
            
            //Http headers: Headers are set by the calling browser, so there is no flexibility here.
        }
        /// <summary>
        /// Gets the ForceHttpMaxRequestLengthParam defined by a parameter called "mumax" from the request
        /// Use UploadMonitor.ForceHttpMaxRequestLengthParam to get the name of the parameter, e.g. "mumax".
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        internal static bool GetForceHttpMaxRequestLength(HttpRequest httpRequest)
        {
            //Try getting the value for ForceHttpMaxRequestLengthParam from querystring
            bool bRet = false; //Default value: apply MaxRequestLength from <system.web/uploadRuntime> if it exists
            Boolean.TryParse(httpRequest.QueryString[UploadMonitor.ForceHttpMaxRequestLengthParam], out bRet);
            return bRet;

            //Form fields: form fields do not work because if you try to access
            //them, you partially read the entity body and there is nothing left to process
            //for the module which hangs

            //Cookies: Cookies are not an option because many browser security settings
            //reject them, and they are more complex to implement than the query string

            //Http headers: Headers are set by the calling browser, so there is no flexibility here.
        }
        /// <summary>
        /// The following code has been posted on http://forums.asp.net/10/55127/ShowThread.aspx
        /// by David Redman Chief Technical Officer of www.floridavue.com.
        /// It is necessary to avoid the browser hanging at the end of the request.
        /// </summary>
        /// <param name="httpWorkerRequest"></param>
        /// <param name="filteredRequest"></param>
        private static void RedirectFilteredRequest(HttpApplication httpApplication, HttpWorkerRequest httpWorkerRequest, byte[] filteredRequest)
        {
            const string ISAPI_REQUEST = "System.Web.Hosting.ISAPIWorkerRequest";
            //const string IIS6_REQUEST = "System.Web.Hosting.ISAPIWorkerRequestInProcForIIS6";
            //const string IIS7_REQUEST_CLASSIC = "System.Web.Hosting.ISAPIWorkerRequestInProcForIIS7"; Classic
            const string IIS7_REQUEST_INTEGRATED = "System.Web.Hosting.IIS7WorkerRequest";
            const string VS2005_REQUEST = "Microsoft.VisualStudio.WebHost.Request";
            const string CASSINI_REQUEST = "Cassini.Request";
            
            BindingFlags enuBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            Type objWorkerRequestType = httpWorkerRequest.GetType();
            //On Windows Server 2003 objWorkerRequestType.FullName = "System.Web.Hosting.ISAPIWorkerRequestInProcForIIS6"
            //and objWorkerRequestType.BaseType.FullName = "System.Web.Hosting.ISAPIWorkerRequest"
            //So the idea is to refer to a known type among the hierarchy of base classes if the type is unknown
            //On IIS 7 CLassic pipeline, "System.Web.Hosting.ISAPIWorkerRequestInProcForIIS7" also inherits from
            //"System.Web.Hosting.ISAPIWorkerRequest"
            while ((objWorkerRequestType != null)
                && (objWorkerRequestType.FullName != ISAPI_REQUEST)
                && (objWorkerRequestType.FullName != IIS7_REQUEST_INTEGRATED)
                && (objWorkerRequestType.FullName != VS2005_REQUEST)
                && (objWorkerRequestType.FullName != CASSINI_REQUEST)
                )
                objWorkerRequestType = objWorkerRequestType.BaseType; //This is for IIS 6 and IIS 7 Classic
            
            if (objWorkerRequestType != null)
            {
                string sTypeFullName = objWorkerRequestType.FullName;
                Type objRequestType = httpApplication.Context.Request.GetType();

                if (sTypeFullName == ISAPI_REQUEST) //which is also the base type for IIS 6 and IIS 7 Classic 
                {
                    objWorkerRequestType.GetField("_contentAvailLength", enuBindingFlags).SetValue(httpWorkerRequest, filteredRequest.Length);
                    objWorkerRequestType.GetField("_contentTotalLength", enuBindingFlags).SetValue(httpWorkerRequest, filteredRequest.Length);
                    objWorkerRequestType.GetField("_preloadedContent", enuBindingFlags).SetValue(httpWorkerRequest, filteredRequest);
                    objWorkerRequestType.GetField("_preloadedContentRead", enuBindingFlags).SetValue(httpWorkerRequest, true);
                    string[] arrHeaders = (string[])objWorkerRequestType.GetField("_knownRequestHeaders", enuBindingFlags).GetValue(httpWorkerRequest);
                    arrHeaders[HttpWorkerRequest.HeaderContentLength] = filteredRequest.Length.ToString();
                }
                else if (sTypeFullName == IIS7_REQUEST_INTEGRATED)
                {
                    HttpWorkerRequest objFilteredRequest = new FilteredHttpWorkerRequest(httpWorkerRequest, filteredRequest);
                    objRequestType.GetField("_wr", enuBindingFlags).SetValue(httpApplication.Context.Request, objFilteredRequest);
                }
                else if ((sTypeFullName == VS2005_REQUEST) || (sTypeFullName == CASSINI_REQUEST))
                {
                    objWorkerRequestType.GetField("_contentLength", enuBindingFlags).SetValue(httpWorkerRequest, filteredRequest.Length);
                    objWorkerRequestType.GetField("_preloadedContent", enuBindingFlags).SetValue(httpWorkerRequest, filteredRequest);
                    objWorkerRequestType.GetField("_preloadedContentLength", enuBindingFlags).SetValue(httpWorkerRequest, filteredRequest.Length);
                    string[] arrHeaders = (string[])objWorkerRequestType.GetField("_knownRequestHeaders", enuBindingFlags).GetValue(httpWorkerRequest);
                    arrHeaders[HttpWorkerRequest.HeaderContentLength] = filteredRequest.Length.ToString();
                }

                /*
                if (objRequestType.GetField("_files", enuBindingFlags).GetValue(httpApplication.Context.Request) != null)
                {
                    objRequestType.GetField("_contentLength", enuBindingFlags).SetValue(httpApplication.Context.Request, -1);
                    objRequestType.GetField("_files", enuBindingFlags).SetValue(httpApplication.Context.Request, null);
                    objRequestType.GetField("_form", enuBindingFlags).SetValue(httpApplication.Context.Request, null);
                    objRequestType.GetField("_headers", enuBindingFlags).SetValue(httpApplication.Context.Request, null);
                    objRequestType.GetField("_multipartContentElements", enuBindingFlags).SetValue(httpApplication.Context.Request, null);
                    objRequestType.GetField("_queryString", enuBindingFlags).SetValue(httpApplication.Context.Request, null);
                    objRequestType.GetField("_rawContent", enuBindingFlags).SetValue(httpApplication.Context.Request, null);
                    objRequestType.GetField("_serverVariables", enuBindingFlags).SetValue(httpApplication.Context.Request, null);
                }
                */
            }
        }
        /// <summary>
        /// CloseConnectionAfterError is inspired from HttpRequest.GetEntireRawContent
        /// </summary>
        /// <param name="httpResponse"></param>
        private static void CloseConnectionAfterError(HttpResponse httpResponse)
        {
            BindingFlags enuBindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
            
            //HttpRuntime objHttpRuntime = new HttpRuntime();
            Type objHttpRuntimeType = typeof(HttpRuntime);
            bool bUseIntegratedPipeline = (bool)objHttpRuntimeType.GetField("_useIntegratedPipeline", enuBindingFlags).GetValue(null);

            if(!bUseIntegratedPipeline)
            {
                enuBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
                Type objResponseType = httpResponse.GetType();
                objResponseType.GetField("_closeConnectionAfterError", enuBindingFlags).SetValue(httpResponse, true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Not sure which type of exception to catch considering extensible provider model")]
        private void DeleteUploadFiles(IList<UploadFile> uploadFiles)
        {
            if (uploadFiles == null)
                return;

            try
            {
                foreach (UploadFile objUploadFile in uploadFiles)
                {
                    FileStorage.DeleteFile(objUploadFile.ProviderFileKey);
                    objUploadFile.IsComplete = false;
                }
            }
            catch (Exception Ex)
            {
                HealthMonitoringManager.LogErrorEvent(
                    Resources.ExceptionUnhandled,
                    null, //<-- no upload data here
                    UploadRequestErrorEvent.RuntimeErrorExceptionUnhandled,
                    Ex);
            }
        }
        #endregion
    }
}
