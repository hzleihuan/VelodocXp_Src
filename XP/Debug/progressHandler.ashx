<%@ WebHandler Language="C#" Class="progressHandler" %>

using System;
using System.Web;

using Memba.WebControls; //ProgressData4JS
using System.Web.Script.Serialization; //JavaScriptSerializer


public class progressHandler : IHttpHandler
{
    private static int _Counter;
    
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

        //Fill the progress data until progress reaches 100%
        if (_Counter < 100)
        {
            objProgressData4JS.IsComplete = false;
            objProgressData4JS.FillRatio = String.Format("{0}%", _Counter);
            objProgressData4JS.TextValues = new object[] { "PENDING", _Counter };
            objProgressData4JS.ErrorMessage = null;
            _Counter+=10;
        }
        else //Once progress reaches 100% we are done
        {
            
            objProgressData4JS.IsComplete = true;
            objProgressData4JS.FillRatio = "100%";
            objProgressData4JS.TextValues = new object[] { "COMPLETE", _Counter };
            objProgressData4JS.ErrorMessage = null;
            _Counter = 0;
        }
        
        //Serialize progress data
        JavaScriptSerializer objJavaScriptSerializer = new JavaScriptSerializer();
        string sResponse = objJavaScriptSerializer.Serialize(objProgressData4JS);
        context.Response.ContentType = "text/json"; //Some use "application/json"
        //Write the response
        context.Response.Write(sResponse);
        context.ApplicationInstance.CompleteRequest();
        //See: http://support.microsoft.com/kb/312629
        //context.Response.End();
    }
 
    public bool IsReusable {
        get
        {
            return true;
        }
    }
}