using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Debug_Upload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //Create a new identifier for the upload data
        Guid gUploadId = Guid.NewGuid();
        //Set the handler with the proper identifier
        ProgressReport.HandlerUrl = "uploadHandler.ashx?muid=" + gUploadId.ToString();
        //Postback to the same url, but add the identifier to the query string
        SubmitButton.PostBackUrl = Request.Url.AbsolutePath + "?muid=" + gUploadId.ToString();
    }
}
