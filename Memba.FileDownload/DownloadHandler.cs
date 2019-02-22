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
using System.Web;
using System.Security.Permissions; //SecurityAction
using System.Globalization; //CultureInfo
using System.IO;

using Memba.FileDownload.Properties;

namespace Memba.FileDownload
{
    /// <summary>
    /// The download handler which permits resumable downloads
    /// <remarks>For more information, please check http://msdn.microsoft.com/msdnmag/issues/06/09/WebDownloads/
    /// See also http://support.microsoft.com/kb/q260519/ for changing the file name and raising the download dialog box</remarks>
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    //Class is sealed [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class DownloadHandler : IHttpHandler
    {
        //MULTIPART_BOUNDARY deliniate "chunks.
        //It should be as unique possible so as not to look like data
        private const string MULTIPART_BOUNDARY = "<a1b2c3d4e5f6g7h8i9j0>";
        private const string MULTIPART_CONTENTTYPE = "multipart/byteranges; boundary=" + MULTIPART_BOUNDARY;
        private const string HTTP_HEADER_ACCEPT_RANGES = "Accept-Ranges";
        private const string HTTP_HEADER_ACCEPT_RANGES_BYTES = "bytes";
        private const string HTTP_HEADER_CONTENT_TYPE = "Content-Type";
        private const string HTTP_HEADER_CONTENT_DISPOSITION = "Content-Disposition";
        private const string HTTP_HEADER_CONTENT_DISPOSITION_ATTACHMENT = "attachment; filename=";
        private const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
        private const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
        private const string HTTP_HEADER_ENTITY_TAG = "ETag";
        private const string HTTP_HEADER_LAST_MODIFIED = "Last-Modified";
        private const string HTTP_HEADER_RANGE = "Range";
        private const string HTTP_HEADER_IF_RANGE = "If-Range";
        private const string HTTP_HEADER_IF_MATCH = "If-Match";
        private const string HTTP_HEADER_IF_NONE_MATCH = "If-None-Match";
        private const string HTTP_HEADER_IF_MODIFIED_SINCE = "If-Modified-Since";
        private const string HTTP_HEADER_IF_UNMODIFIED_SINCE = "If-Unmodified-Since";
        private const string HTTP_HEADER_UNLESS_MODIFIED_SINCE = "Unless-Modified-Since";
        private const string HTTP_METHOD_GET = "GET";
        private const string HTTP_METHOD_HEAD = "HEAD";

        private const int BUFFER_SIZE = 8192;

        #region IHttpHandler Members
        /// <summary>
        /// 
        /// </summary>
        public bool IsReusable
        {
            get { return true; } // Allow ASP.NET to reuse instances of this class.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Low priority")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Too many catching blocks with same code to consider otherwise")]
        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context"); //Very theoretical

            HttpRequest objRequest = context.Request;
            System.Diagnostics.Debug.Assert(objRequest != null);
            HttpResponse objResponse = context.Response;
            System.Diagnostics.Debug.Assert(objResponse != null);
            
            //TODO: Maybe we can calculate a timeout in propertion of content length
            context.Server.ScriptTimeout = 24 * 60 * 60; //1 day

            string sUserName = context.User.Identity.Name;
            DownloadFile objDownload = null; // Custom File information object...
            Stream objDownloadStream;
            long[] arliRequestedRangesBegin = new long[0]; // Start of Chunk
            long[] arliRequestedRangesEnd = new long[0]; // End of Chunk       
            int iResponseContentLength = 0;
            int iBytesToRead;
            int iLengthOfReadChunk;
            bool bWasDownloadInterrupted = false; //Was download Interupted
            bool bIsChunkRequest = false;
            bool bMultipart = false;
            byte[] arrBuffer = new byte[BUFFER_SIZE];

            //Get to the file that was requested. 
            //ONLY FILE TYPES WITH EXTENSIONS MAPPED IN WEB.CONFIG WILL CALL THIS CODE
            try
            {
                Guid gFileGuid = new Guid(System.IO.Path.GetFileNameWithoutExtension(objRequest.FilePath));
                objDownload = new DownloadFile(gFileGuid, sUserName);
            }
            catch
            {
                //ArgumentNullException, FormatException, OverflowException, DataLayerException, BusinessLayerException
            }
            
            //Clear the response
            objResponse.Clear();

            //Added by JLC after the following article
            //http://support.softartisans.com/kbview.aspx?ID=766
            objResponse.Buffer = false;
            
            string strMethod = objRequest.HttpMethod;
            //We could support more request types
            if (!strMethod.Equals(HTTP_METHOD_GET) && !strMethod.Equals(HTTP_METHOD_HEAD))
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionRequestTypeNotImplemented, strMethod);
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorRequestTypeNotImplemented,
                    null);

                //objResponse.StatusCode = 501;  //HTTP Request Type Not implemented
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(501, sErrorMessage);
            }
            else if ((objDownload == null) || (!objDownload.Exists))
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionCannotRetrieveDownload, objRequest.FilePath);
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorCannotRetrieveDownload,
                    new HttpException(404, sErrorMessage));

                //objResponse.StatusCode = 404;  //File Not found
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(404, sErrorMessage);
            }
            else if (objDownload.IsExpired)
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionDownloadExpired, objRequest.FilePath);
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorDownloadExpired,
                    new HttpException(410, sErrorMessage));

                //objResponse.StatusCode = 410;  //File no more available. Probably expired.
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(410, sErrorMessage);
            }
            else if (!objDownload.CanDownload)
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionUnauthorizedDownload, sUserName, objRequest.FilePath);
                    HealthMonitoringManager.LogErrorEvent(
                       sErrorMessage,
                       objDownload,
                       DownloadRequestErrorEvent.RuntimeErrorUnauthorizedDownload,
                       new HttpException(403, sErrorMessage));

                    //objResponse.StatusCode = 403; //Forbidden
                    //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                    //and display a nice custom error message
                    throw new HttpException(403, sErrorMessage);
                }
                else
                {
                    string sErrorMessage = Resources.ExceptionAuthenticationRequired;
                    HealthMonitoringManager.LogErrorEvent(
                        sErrorMessage,
                        objDownload,
                        DownloadRequestErrorEvent.RuntimeErrorAuthenticationRequired,
                        new HttpException(401, sErrorMessage));

                    //objResponse.StatusCode = 401; //Authentication required
                    //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                    //and display a nice custom error message
                    throw new HttpException(401, sErrorMessage);
                }
            }
            else if (objDownload.Length > Int32.MaxValue)
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionResponseTooLarge, objRequest.FilePath);
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorResponseTooLarge,
                    new HttpException(413, sErrorMessage));

                //objResponse.StatusCode = 413;  //Request for too many bytes 
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(413, sErrorMessage);
            }
            else if (!DownloadHandler.ParseRequestHeaderRange(objRequest, objDownload.Length, ref arliRequestedRangesBegin, ref arliRequestedRangesEnd, ref bIsChunkRequest))
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionInvalidRanges, objRequest.FilePath);
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorInvalidRanges,
                    new HttpException(400, sErrorMessage));

                //objResponse.StatusCode = 400;  //Bad request
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(400, sErrorMessage);
            }
            else if (!DownloadHandler.CheckIfModifiedSince(objRequest, objDownload))
            {
                //Not an error - just tells the browser to download from its cache
                objResponse.StatusCode = 304;  //Not modified
            }
            else if (!DownloadHandler.CheckIfUnmodifiedSince(objRequest, objDownload))
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionPreconditionFailed, objRequest.FilePath, "If-Unmodified-Since");
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorPreconditionFailed,
                    new HttpException(412, sErrorMessage));

                //objResponse.StatusCode = 412;  //modified pre-condition failed
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(412, sErrorMessage);
            }
            else if (!DownloadHandler.CheckIfMatch(objRequest, objDownload))
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionPreconditionFailed, objRequest.FilePath, "If-Match");
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorPreconditionFailed,
                    new HttpException(412, sErrorMessage));

                //objResponse.StatusCode = 412;  //Entitiy Precondition failed
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(412, sErrorMessage);
            }
            else if (!DownloadHandler.CheckIfNoneMatch(objRequest, objResponse, objDownload))
            {
                string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionPreconditionFailed, objRequest.FilePath, "If-None-Match");
                HealthMonitoringManager.LogErrorEvent(
                    sErrorMessage,
                    objDownload,
                    DownloadRequestErrorEvent.RuntimeErrorPreconditionFailed,
                    new HttpException(412, sErrorMessage));

                System.Diagnostics.Trace.WriteLine("Nothing to download");
                //objResponse.StatusCode = 412;  //Entitiy Precondition failed
                //Same issue as with upload module: we raise an exception to be redirected to Application_Error and Custom Errors
                //and display a nice custom error message
                throw new HttpException(412, sErrorMessage);
            }
            else
            {
                //Everything is good so far. 
                if (bIsChunkRequest && DownloadHandler.CheckIfRange(objRequest, objDownload)) // Valid Chunk Request
                {
                    bMultipart = (bool)(arliRequestedRangesBegin.GetUpperBound(0) > 0);
                    // Loop through chunks to calculate the total length
                    for (int iLoop = arliRequestedRangesBegin.GetLowerBound(0); iLoop <= arliRequestedRangesBegin.GetUpperBound(0); iLoop++)
                    {
                        iResponseContentLength += Convert.ToInt32(arliRequestedRangesEnd[iLoop] - arliRequestedRangesBegin[iLoop]) + 1;
                        if (bMultipart)
                        {
                            // Calc length of the headers
                            iResponseContentLength += MULTIPART_BOUNDARY.Length;
                            iResponseContentLength += objDownload.ContentType.Length;
                            iResponseContentLength += arliRequestedRangesBegin[iLoop].ToString(CultureInfo.InvariantCulture).Length;
                            iResponseContentLength += arliRequestedRangesEnd[iLoop].ToString(CultureInfo.InvariantCulture).Length;
                            iResponseContentLength += objDownload.Length.ToString(CultureInfo.InvariantCulture).Length;
                            iResponseContentLength += 49; // add length needed for multipart header
                        }
                    }

                    if (bMultipart)
                    {
                        // Calc length of last intermediate header 
                        iResponseContentLength += MULTIPART_BOUNDARY.Length;
                        iResponseContentLength += 8; // length of dash and line break 
                    }
                    else
                    {
                        objResponse.AppendHeader(HTTP_HEADER_CONTENT_RANGE, "bytes "
                                                + arliRequestedRangesBegin[0].ToString(CultureInfo.InvariantCulture) + "-"
                                                + arliRequestedRangesEnd[0].ToString(CultureInfo.InvariantCulture) + "/"
                                                + objDownload.Length.ToString(CultureInfo.InvariantCulture));
                    }
                    objResponse.StatusCode = 206; // Partial Response
                }
                else
                {
                    //Not a Chunck  request or Entity doesn't match
                    //Start a new download of the entire file
                    iResponseContentLength = Convert.ToInt32(objDownload.Length);
                    objResponse.StatusCode = 200; //Status OK
                }

                string sEncodedFileName = HttpUtility.UrlPathEncode(objDownload.FileName); //This is for Unicode file names like chinese file names
                objResponse.AppendHeader(HTTP_HEADER_CONTENT_DISPOSITION, HTTP_HEADER_CONTENT_DISPOSITION_ATTACHMENT + sEncodedFileName);
                objResponse.AppendHeader(HTTP_HEADER_CONTENT_LENGTH, iResponseContentLength.ToString(CultureInfo.InvariantCulture));
                objResponse.AppendHeader(HTTP_HEADER_LAST_MODIFIED, objDownload.LastWriteTimeUTC.ToString("r", CultureInfo.InvariantCulture));
                objResponse.AppendHeader(HTTP_HEADER_ACCEPT_RANGES, HTTP_HEADER_ACCEPT_RANGES_BYTES);
                objResponse.AppendHeader(HTTP_HEADER_ENTITY_TAG, "\"" + objDownload.EntityTag + "\""); // Entity Tag muss be Quote Enclosed

                if (bMultipart)
                {
                    // File real MIME type gets pushed into the response object later
                    objResponse.ContentType = MULTIPART_CONTENTTYPE;
                }
                else
                {
                    objResponse.ContentType = objDownload.ContentType;
                }

                if (objRequest.HttpMethod.Equals(HTTP_METHOD_HEAD))
                {
                    // Only the HEAD was requested
                }
                else
                {
                    objResponse.Flush();
                    // We're Downloading !!
                    objDownload.State = DownloadFile.DownloadState.DownloadInProgress;
                    objDownloadStream = objDownload.DownloadStream;

                    // Process all the requested chunks.
                    for (int iLoop = arliRequestedRangesBegin.GetLowerBound(0); iLoop <= arliRequestedRangesBegin.GetUpperBound(0); iLoop++)
                    {
                        //TODO: Problem is CanSeek is not supported
                        objDownloadStream.Seek(arliRequestedRangesBegin[iLoop], System.IO.SeekOrigin.Begin);
                        iBytesToRead = Convert.ToInt32(arliRequestedRangesEnd[iLoop] - arliRequestedRangesBegin[iLoop]) + 1;
                        if (bMultipart)
                        {
                            // Send Headers
                            objResponse.Output.WriteLine("--" + MULTIPART_BOUNDARY); // Indicate the part boundry
                            objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_TYPE + ": " + objDownload.ContentType);
                            objResponse.Output.WriteLine(HTTP_HEADER_CONTENT_RANGE + ": bytes "
                                                        + arliRequestedRangesBegin[iLoop].ToString(CultureInfo.InvariantCulture) + "-"
                                                        + arliRequestedRangesEnd[iLoop].ToString(CultureInfo.InvariantCulture) + "/"
                                                        + objDownload.Length.ToString(CultureInfo.InvariantCulture));
                            objResponse.Output.WriteLine();
                        }

                        // Send the Data.
                        while (iBytesToRead > 0)
                        {
                            if (objResponse.IsClientConnected)
                            {
                                iLengthOfReadChunk = objDownloadStream.Read(arrBuffer, 0, Math.Min(arrBuffer.Length, iBytesToRead));
                                objResponse.OutputStream.Write(arrBuffer, 0, iLengthOfReadChunk);
                                objResponse.Flush();
                                Array.Resize<byte>(ref arrBuffer, BUFFER_SIZE);
                                iBytesToRead -= iLengthOfReadChunk;
                            }
                            else
                            {
                                // DOWNLOAD INTERRUPTED
                                iBytesToRead = -1;
                                bWasDownloadInterrupted = true;
                            }
                        }

                        if (bMultipart)
                        {
                            objResponse.Output.WriteLine(); // Mark the end of the part
                        }

                        if (bWasDownloadInterrupted)
                        {
                            break;
                        }
                    }

                    // Download finished or cancelled... 
                    if (bWasDownloadInterrupted)
                    {
                        //Download broken...
                        objDownload.State = DownloadFile.DownloadState.DownloadBroken;

                        string sErrorMessage = String.Format(Resources.Culture, Resources.ExceptionRequestAbort, objRequest.FilePath);
                        HealthMonitoringManager.LogErrorEvent(
                            sErrorMessage,
                            objDownload,
                            DownloadRequestErrorEvent.RuntimeErrorRequestAbort,
                            new System.Net.WebException(sErrorMessage, System.Net.WebExceptionStatus.ConnectionClosed));

                    }
                    else
                    {
                        if (bMultipart)
                        {
                            objResponse.Output.WriteLine("--" + MULTIPART_BOUNDARY + "--");
                            objResponse.Output.WriteLine();
                        }

                        // Download Complete
                        objDownload.State = DownloadFile.DownloadState.DownloadFinished;

                        HealthMonitoringManager.LogSucessEvent(
                            Resources.MessageDownloadCompleted,
                            objDownload,
                            DownloadRequestSuccessEvent.RuntimeDownloadSuccessCompleted);
                    }
                    objDownloadStream.Close();
                }
            }

            objResponse.End();
        }

        #endregion

        #region Private Helper Functions
        /// <summary>
        /// Check If-Range header
        /// </summary>
        /// <param name="objRequest"></param>
        /// <param name="objDownload"></param>
        /// <returns></returns>
        private static bool CheckIfRange(HttpRequest objRequest, DownloadFile objFile) //Careful ByVal
        {
            // Get Requests If-Range Header value
            string sRequestHeaderIfRange = DownloadHandler.RetrieveHeader(objRequest, HTTP_HEADER_IF_RANGE, objFile.EntityTag);

            // if (the requested file entity matches the current
            // entity uploadId, return True, otherwise return False
            return sRequestHeaderIfRange.Equals(objFile.EntityTag);
        }
        /// <summary>
        /// Check If-Match header
        /// </summary>
        /// <param name="objRequest"></param>
        /// <param name="objDownload"></param>
        /// <returns></returns>
        private static bool CheckIfMatch(HttpRequest objRequest, DownloadFile objFile) //Careful ByVal
        {
            // Get Request If-Match Header, * is there was none
            string sRequestHeaderIfMatch = DownloadHandler.RetrieveHeader(objRequest, HTTP_HEADER_IF_MATCH, "*");
            string[] arrEntityIDs;
            bool bReturn = false;

            if (sRequestHeaderIfMatch.Equals("*"))
            {
                bReturn = true; // No Match
            }
            else
            {
                arrEntityIDs = sRequestHeaderIfMatch.Replace("bytes=", "").Split(",".ToCharArray());
                // Iterate entity IDs to see if there's a match to the current file ID 
                for (int iLoop = arrEntityIDs.GetLowerBound(0); iLoop <= arrEntityIDs.GetUpperBound(0); iLoop++)
                {
                    if (arrEntityIDs[iLoop].Trim().Equals(objFile.EntityTag))
                    {
                        bReturn = true;
                    }
                }
            }
            return bReturn;
        }
        /// <summary>
        /// Check If-None-Match header
        /// </summary>
        /// <param name="objRequest"></param>
        /// <param name="objResponse"></param>
        /// <param name="objDownload"></param>
        /// <returns></returns>
        private static bool CheckIfNoneMatch(HttpRequest objRequest, HttpResponse objResponse, DownloadFile objFile)  //Careful ByVal
        {
            string sRequestHeaderIfNoneMatch;
            string[] arrEntityIDs;
            bool bReturn = true;
            string sReturn = "";
            // Get Request If-None-Match Header value
            sRequestHeaderIfNoneMatch = DownloadHandler.RetrieveHeader(objRequest, HTTP_HEADER_IF_NONE_MATCH, String.Empty);
            if (String.IsNullOrEmpty(sRequestHeaderIfNoneMatch))
            {
                bReturn = true; // Perform request normally
            }
            else if (sRequestHeaderIfNoneMatch.Equals("*"))
            {
                objResponse.StatusCode = 412; // logically invalid request
                bReturn = false;
            }
            else
            {
                arrEntityIDs = sRequestHeaderIfNoneMatch.Replace("bytes=", "").Split(",".ToCharArray());
                // EntIDs were sent - Look for a match to the current one
                for (int iLoop = arrEntityIDs.GetLowerBound(0); iLoop <= arrEntityIDs.GetUpperBound(0); iLoop++)
                {
                    if (arrEntityIDs[iLoop].Trim().Equals(objFile.EntityTag))
                    {
                        sReturn = arrEntityIDs[iLoop];
                        // One of the requested entities matches the current file's tag,
                        objResponse.AppendHeader(HTTP_HEADER_ENTITY_TAG, sReturn);
                        objResponse.StatusCode = 304; // Not Modified
                        bReturn = true;
                    }
                }
            }
            return bReturn;
        }
        /// <summary>
        /// Check If-Modified-Since header
        /// </summary>
        /// <param name="objRequest"></param>
        /// <param name="objDownload"></param>
        /// <returns></returns>
        private static bool CheckIfModifiedSince(HttpRequest objRequest, DownloadFile objFile) //Careful ByVal
        {
            string sSinceDate;
            DateTime dtSinceDate;
            bool bReturn;

            // Retrieve If-Modified-Since Header
            sSinceDate = DownloadHandler.RetrieveHeader(objRequest, HTTP_HEADER_IF_MODIFIED_SINCE, String.Empty);
            if (String.IsNullOrEmpty(sSinceDate))
            {
                bReturn = true; // if (no date was sent - assume we need to re-download the whole file
            }
            else
            {
                bool bTry = DateTime.TryParse(sSinceDate, out dtSinceDate); //dtSinceDate is a local date without millisecs
                DateTime dtSinceDateUTC = dtSinceDate.ToUniversalTime();
                DateTime dtLastWriteTimeUTC = new DateTime(
                    objFile.LastWriteTimeUTC.Year,
                    objFile.LastWriteTimeUTC.Month,
                    objFile.LastWriteTimeUTC.Day,
                    objFile.LastWriteTimeUTC.Hour,
                    objFile.LastWriteTimeUTC.Minute,
                    objFile.LastWriteTimeUTC.Second,
                    DateTimeKind.Utc);
                if (bTry)
                {
                    bReturn = (dtLastWriteTimeUTC > dtSinceDateUTC); // True if the file was actually modified
                }
                else
                {
                    bReturn = false;
                }
            }

            return bReturn;
        }
        /// <summary>
        /// Check If-Unmodified-Since header
        /// </summary>
        /// <param name="objRequest"></param>
        /// <param name="objDownload"></param>
        /// <returns></returns>
        private static bool CheckIfUnmodifiedSince(HttpRequest objRequest, DownloadFile objFile)
        {
            string sSinceDate;
            DateTime dtSinceDate;
            bool bReturn;

            // Retrieve If-Unmodified-Since Header 
            sSinceDate = DownloadHandler.RetrieveHeader(objRequest, HTTP_HEADER_IF_UNMODIFIED_SINCE, String.Empty);
            if (String.IsNullOrEmpty(sSinceDate))
            {
                sSinceDate = DownloadHandler.RetrieveHeader(objRequest, HTTP_HEADER_UNLESS_MODIFIED_SINCE, String.Empty);
            }
            if (String.IsNullOrEmpty(sSinceDate))
            {
                bReturn = true;
            }
            else
            {
                bool bTry = DateTime.TryParse(sSinceDate, out dtSinceDate); //dtSinceDate is a local date without millisecs
                DateTime dtSinceDateUTC = dtSinceDate.ToUniversalTime();
                DateTime dtLastWriteTimeUTC = new DateTime(
                    objFile.LastWriteTimeUTC.Year,
                    objFile.LastWriteTimeUTC.Month,
                    objFile.LastWriteTimeUTC.Day,
                    objFile.LastWriteTimeUTC.Hour,
                    objFile.LastWriteTimeUTC.Minute,
                    objFile.LastWriteTimeUTC.Second,
                    DateTimeKind.Utc);
                if (bTry)
                {
                    bReturn = (dtLastWriteTimeUTC <= dtSinceDateUTC); // True if the file was not modified
                }
                else
                {
                    bReturn = false;
                }
            }
            return bReturn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objRequest"></param>
        /// <param name="lBegin"></param>
        /// <param name="lEnd"></param>
        /// <param name="lMax"></param>
        /// <param name="bRangeRequest"></param>
        /// <returns></returns>
        private static bool ParseRequestHeaderRange(HttpRequest objRequest, long lMax, ref long[] lBegin, ref long[] lEnd, ref bool bRangeRequest) //Some ByVal some ByRef
        {
            bool bValidRangesRet;
            string sSource;
            //int iLoop;
            string[] arrRanges;

            // Retrieve Range Header from Request
            sSource = DownloadHandler.RetrieveHeader(objRequest, HTTP_HEADER_RANGE, String.Empty);
            if (String.IsNullOrEmpty(sSource)) // No Range was requested
            {
                Array.Resize<long>(ref lBegin, 1);
                Array.Resize<long>(ref lEnd, 1);
                lBegin[0] = 0;
                lEnd[0] = lMax - 1;
                bValidRangesRet = true;
                bRangeRequest = false;
            }
            else
            {
                bValidRangesRet = true;
                bRangeRequest = true;

                // Remove "bytes=" string, and split the rest 
                arrRanges = sSource.Replace("bytes=", "").Split(",".ToCharArray());

                Array.Resize<long>(ref lBegin, arrRanges.GetUpperBound(0) + 1);
                Array.Resize<long>(ref lEnd, arrRanges.GetUpperBound(0) + 1);

                // Check each Range request
                for (int iLoop = arrRanges.GetLowerBound(0); iLoop <= arrRanges.GetUpperBound(0); iLoop++)
                {
                    // arrRange(0) is the begin value - arrRange(1) is the end value.
                    string[] arrRange = arrRanges[iLoop].Split("-".ToCharArray());
                    if (String.IsNullOrEmpty(arrRange[1])) // No end was specified
                    {
                        lEnd[iLoop] = lMax - 1;
                    }
                    else
                    {
                        lEnd[iLoop] = long.Parse(arrRange[1], CultureInfo.InvariantCulture);
                    }
                    if (String.IsNullOrEmpty(arrRange[0]))
                    {
                        // Calculate the beginning and end.
                        lBegin[iLoop] = lMax - 1 - lEnd[iLoop];
                        lEnd[iLoop] = lMax - 1;
                    }
                    else
                    {
                        lBegin[iLoop] = long.Parse(arrRange[0], CultureInfo.InvariantCulture);
                    }
                    // Begin and end must not exceed the total file size
                    if ((lBegin[iLoop] > (lMax - 1)) || (lEnd[iLoop] > (lMax - 1)))
                    {
                        bValidRangesRet = false;
                    }
                    // Begin and end cannot be < 0
                    if ((lBegin[iLoop] < 0) || (lEnd[iLoop] < 0))
                    {
                        bValidRangesRet = false;
                    }
                    // End must be larger or equal to begin value
                    if (lEnd[iLoop] < lBegin[iLoop])
                    {
                        bValidRangesRet = false;
                    }
                }
            }
            return bValidRangesRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objRequest"></param>
        /// <param name="sHeader"></param>
        /// <param name="sDefault"></param>
        /// <returns></returns>
        private static string RetrieveHeader(HttpRequest objRequest, string sHeader, string sDefault)
        {
            string sValueRet = objRequest.Headers[sHeader]; //Careful was objRequest.Headers.Item(sHeader)
            if (String.IsNullOrEmpty(sValueRet)) // No Header Found
                return sDefault;
            else
                return sValueRet.Replace("\"", ""); // Clean quotes from header before return
        }
        #endregion
    }
}
