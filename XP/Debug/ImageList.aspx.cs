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
using Memba.WebControls;

public partial class Debug_ImageList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            ImageList.ImageListItemCollection.Add(
                new ImageListItem(
                    Guid.NewGuid().ToString(),
                    Memba.Common.Presentation.Icons.GetIcon32("upload.gif"),
                    "Sample 1"
                    ));

            ImageList.ImageListItemCollection.Add(
                new ImageListItem(
                    Guid.NewGuid().ToString(),
                    Memba.Common.Presentation.Icons.GetIcon32("upload.gif"),
                    "Sample 2"
                    ));

            ImageList.ImageListItemCollection.Add(
                new ImageListItem(
                    Guid.NewGuid().ToString(),
                    Memba.Common.Presentation.Icons.GetIcon32("upload.gif"),
                    "Sample 3"
                    ));
        }
    }
    protected void PostButton_Click(object sender, EventArgs e)
    {
        foreach (ImageListItem item in ImageList.ImageListItemCollection)
        {
            Response.Write(item.Text + "<br />");            
        }
    }
}
