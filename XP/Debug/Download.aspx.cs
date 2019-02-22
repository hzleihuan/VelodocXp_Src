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

using Memba.FileUpload; //UploadMonitor, UploadData
using Memba.FileUpload.Providers; //FileStorage
using Memba.Files.Business; //FileBroker, File, BODisplay

public partial class Debug_Download : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Guid gUploadId = Guid.NewGuid();
        //Postback to the same url, but add the identifier to the query string
        SubmitButton.PostBackUrl = Request.Url.AbsolutePath + "?muid=" + gUploadId.ToString();
    }
    protected void SubmitButton_Click(object sender, EventArgs e)
    {
        try
        {
            //Confirm that we have a muid to access upload data
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(Request.QueryString[UploadMonitor.UploadIdParam]));
            UploadData objUploadData = UploadMonitor.GetUploadData(Request.QueryString[UploadMonitor.UploadIdParam]);
            if (objUploadData == null)
                throw new ApplicationException("Oops! No upload data");

            //Check any exception
            Exception objUploadException = objUploadData.Exception;
            if (objUploadException != null)
                throw new ApplicationException(objUploadException.Message, objUploadException);

            //Ensure that upload is complete
            if (objUploadData.ProgressStatus != UploadProgressStatus.Completed)
                throw new ApplicationException("Oops! Upload not complete");

            //Ensure we have at least one uploaded file (we should have at least one file input control)
            if ((objUploadData.UploadFiles == null) || (objUploadData.UploadFiles.Count < 1))
                throw new ApplicationException("Oops! No uploaded file");

            //Keep it simple: we now there is only one file upload control on the page
            UploadFile objUploadFile = objUploadData.UploadFiles[0];
            if ((objUploadFile == null) || (!objUploadFile.IsComplete))
                throw new ApplicationException("Oops! Uploaded file is not complete");

            //Create file object
            File f = new File(
                objUploadFile.OriginalFileName,
                objUploadFile.ContentType,
                objUploadFile.ProviderFileKey.ToString(),
                objUploadFile.ContentLength,
                objUploadFile.HashValue);

            //Get path to file storage
            string sPath = FileStorage.Provider.ConnectionString;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sPath);

            //Add definition to file storage
            FileBroker.Insert(f, di);

            //Create download link
            DownloadLink.Text = "Download: " + f.FileName + " (" + BODisplay.SizeFormat(f.Size, ByteFormat.Adapt) + ")";
            DownloadLink.NavigateUrl = System.IO.Path.Combine(this.Request.ApplicationPath, f.Guid.ToString("N") + ".dat");

            //Release upload data
            UploadMonitor.Release(objUploadData.UploadId);
        }
        catch (Exception Ex)
        {
            Response.Write(Ex.Message);
        }
    }
}
