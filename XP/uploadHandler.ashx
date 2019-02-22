<%--
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
--%>
<%@ WebHandler Language="C#" Class="uploadHandler" %>

using System;
using System.Web;
using System.Globalization; //CultureInfo

using System.Web.Script.Serialization; //JavaScriptSerializer

using Memba.FileUpload; //UploadMonitor, UploadData
using Memba.Files.Business; //BODisplay
using Memba.WebControls; //ProgressData

/// <summary>
/// The Upload Handler returns a JSON string with information on the progress of an upload
/// </summary>
public class uploadHandler : IHttpHandler
{
    /// <summary>
    /// The handler is reusable because it does not maintain state (no private variables)
    /// </summary>
    public bool IsReusable
    {
        get { return true; }
    }
    /// <summary>
    /// Only method which writes the upload progress in JSON to the response stream
    /// </summary>
    /// <param name="context"></param>
    public void ProcessRequest(HttpContext context)
    {
        context.Response.Clear();

        string sUploadId = context.Request.QueryString[UploadMonitor.UploadIdParam];
        if (String.IsNullOrEmpty(sUploadId))
        {
            context.Response.StatusCode = 400; //Bad Request, we do not want to be more specific
            context.ApplicationInstance.CompleteRequest();
            //See: http://support.microsoft.com/kb/312629
            //context.Response.End();
        }
        else
        {
            //Using headers or cache attributes to set the cache policy should be equivalent
            context.Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.AppendHeader("Expires", "-1");
            context.Response.AppendHeader("Pragma", "no-cache");

            //but the following does not work in IIS 7 (Vista 64-bit OS), which requires the code above
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            context.Response.Cache.SetNoStore();
            context.Response.Cache.SetNoServerCaching();
            context.Response.Cache.SetExpires(DateTime.Now);

            ProgressData4JS objProgressData4JSRet = new ProgressData4JS();

            try
            {
                System.Diagnostics.Trace.WriteLine("uploadHandler: Begin");

                //UrlReferrer is null from Firefox 1.5
                if ((HttpContext.Current.Request.Browser.Browser != "Firefox" || HttpContext.Current.Request.Browser.MajorVersion != 1)
                    && (!HttpContext.Current.Request.UrlReferrer.Authority.Equals(HttpContext.Current.Request.Url.Authority, StringComparison.InvariantCultureIgnoreCase)))
                    throw new ApplicationException(Resources.Web.glossary.WebService_NoCallFromOtherDomain);

                UploadData objUploadData = UploadMonitor.GetUploadData(sUploadId);

                System.Diagnostics.Trace.WriteLine("uploadHandler: UploadData initialized");

                if (objUploadData == null)
                {
                    objProgressData4JSRet.FillRatio = "0%";
                    objProgressData4JSRet.TextValues = new object[] {
                        BODisplay.PercentFormat(0d),
                        BODisplay.SizeFormat(0L, ByteFormat.Kb),
                        Resources.Web.glossary.UploadHandler_Unknown,
                        BODisplay.BandwidthFormat(0L, ByteFormat.Kb),
                        BODisplay.TimeSpanFormat(TimeSpan.Zero),
                        Resources.Web.glossary.UploadHandler_Unknown
                    };

                    System.Diagnostics.Trace.WriteLine("uploadHandler: No upload data, waiting...");
                }
                else if (objUploadData.ProgressStatus == UploadProgressStatus.Failed)
                {
                    if (objUploadData.Exception != null)
                        objProgressData4JSRet.ErrorMessage = objUploadData.Exception.Message;
                    else
                        objProgressData4JSRet.ErrorMessage = Resources.Web.glossary.UploadHandler_UnhandledException;

                    System.Diagnostics.Trace.WriteLine("uploadHandler: Error - " + objProgressData4JSRet.ErrorMessage);
                }
                else
                {
                    objProgressData4JSRet.FillRatio = BODisplay.Encode(
                            String.Format(
                            CultureInfo.InvariantCulture,
                            "{0}%",
                            (int)Math.Ceiling(100 * objUploadData.ProgressRatio)
                            ));

                    System.Diagnostics.Trace.WriteLine("uploadHandler: " + objProgressData4JSRet.FillRatio);

                    objProgressData4JSRet.TextValues = new object[] {
                        BODisplay.PercentFormat(objUploadData.ProgressRatio),
                        BODisplay.SizeFormat(objUploadData.ContentPosition, ByteFormat.Adapt),
                        BODisplay.SizeFormat(objUploadData.ContentLength, ByteFormat.Adapt),
                        BODisplay.BandwidthFormat(objUploadData.BytesPerSecond, ByteFormat.Adapt),
                        BODisplay.TimeSpanFormat(objUploadData.TimeElapsed),
                        BODisplay.TimeSpanFormat(objUploadData.TimeLeftEstimated)
                    };

                    UploadProgressStatus enuStatus = objUploadData.ProgressStatus;
                    System.Diagnostics.Trace.WriteLine("uploadHandler: Status " + enuStatus.ToString());

                    if (enuStatus == UploadProgressStatus.Completed)
                        objProgressData4JSRet.IsComplete = true;
                }
            }
            catch (HttpException Ex)
            {
                objProgressData4JSRet.ErrorMessage = Ex.Message;
            }
            catch (System.Net.WebException Ex)
            {
                objProgressData4JSRet.ErrorMessage = Ex.Message;
            }
            catch (Exception)
            {
                objProgressData4JSRet.ErrorMessage = Resources.Web.glossary.UploadHandler_UnhandledException;
            }
            finally
            {
                JavaScriptSerializer objJavaScriptSerializer = new JavaScriptSerializer();
                string sResponse = objJavaScriptSerializer.Serialize(objProgressData4JSRet);
                context.Response.ContentType = "text/json"; //Some use "application/json"
                context.Response.Write(sResponse);
                System.Diagnostics.Trace.WriteLine("uploadHandler: END");
                context.ApplicationInstance.CompleteRequest();
                //See: http://support.microsoft.com/kb/312629
                //context.Response.End();
            }
        }
    }
}