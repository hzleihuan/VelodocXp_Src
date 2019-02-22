<%@ WebHandler Language="C#" Class="uploadHandler" %>

using System;
using System.Web;

using Memba.FileUpload; //UploadMonitor
using Memba.Files.Business; //BODIsplay
using Memba.WebControls; //ProgressData4JS
using System.Web.Script.Serialization; //JavaScriptSerializer


public class uploadHandler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context)
    {
        //First, make sure the content is not cached because we need "real-time" progress report

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

        //Create a progress data object which will be used for JSON serialization
        ProgressData4JS objProgressData4JS = new ProgressData4JS();
        
        try
        {
            //Use the identifier in query string to access UploadData
            UploadData objUploadData = UploadMonitor.GetUploadData(context.Request.QueryString[UploadMonitor.UploadIdParam]);
            if (objUploadData != null)
            {
                //There is some relevant UploadData
                objProgressData4JS.IsComplete = (objUploadData.ProgressStatus == UploadProgressStatus.Completed);
                objProgressData4JS.FillRatio = String.Format("{0:N0}%", 100 * objUploadData.ProgressRatio);
                objProgressData4JS.TextValues = new object[] {
                    objUploadData.ProgressStatus.ToString(),
                    BODisplay.BandwidthFormat(objUploadData.BytesPerSecond, ByteFormat.Adapt)
                };
                objProgressData4JS.ErrorMessage = null;
            }
            else
            {
                //UploadData cannot be found; Maybe it is not yet available
                objProgressData4JS.IsComplete = false;
                objProgressData4JS.FillRatio = "0%";
                objProgressData4JS.TextValues = new object[] { "Unknown", "0MB/s" };
                objProgressData4JS.ErrorMessage = null;
            }
        }
        catch (Exception Ex)
        {
            //Oops! Houston, there is a problem
            objProgressData4JS.IsComplete = true;
            objProgressData4JS.FillRatio = null;
            objProgressData4JS.TextValues = null;
            objProgressData4JS.ErrorMessage = Ex.Message;
        }

        //Serialize progress data
        JavaScriptSerializer objJavaScriptSerializer = new JavaScriptSerializer();
        string sResponse = objJavaScriptSerializer.Serialize(objProgressData4JS);
        //Write the response
        context.Response.ContentType = "text/json"; //Some use "application/json"
        context.Response.Write(sResponse);
        context.ApplicationInstance.CompleteRequest();
        //See: http://support.microsoft.com/kb/312629
        //context.Response.End();
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }
}