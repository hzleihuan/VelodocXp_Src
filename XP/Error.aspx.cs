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
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Memba.Common.Presentation; //Icons
using Memba.Files.Business; //BODisplay
using Memba.WebControls; //InfoBoxType

// http://support.microsoft.com/default.aspx?scid=kb;en-us;318380
// http://support.microsoft.com/default.aspx?scid=kb;fr-fr;318380

/// <summary>
/// The default error page for the application.
/// </summary>
public partial class Error : System.Web.UI.Page
{
	/// <summary>
	/// The PreRender event which occurs after page load
	/// </summary>
	/// <param name="e"></param>
    protected override void OnPreRender(EventArgs e)
    {
        WebMasterPage objMasterPage = Page.Master as WebMasterPage;
        Page.Header.Title = Resources.Web.glossary.App_Name + " - " + this.GetLocalResourceObject("PageTitle").ToString();

        InfoBox objPageTitle = objMasterPage.FindControl("PageTitle") as InfoBox;
        objPageTitle.ImageUrl = Icons.GetIcon32("errorreport.gif");
        objPageTitle.Text = this.GetLocalResourceObject("PageTitle").ToString();

        InfoBox objInfoBox = objMasterPage.FindControl("InfoBox") as InfoBox;
        objInfoBox.Type = InfoBoxType.Error;

        try
        {
            if (Session[Constants.LastError] != null)
            {
                objInfoBox.Text = BODisplay.Encode(Session[Constants.LastError]);
                Session[Constants.LastError] = null;
            }
            else
            {
                string errorCode = Context.Request.QueryString["e"];
                int iHttpCode;
                bool bTry = Int32.TryParse(errorCode, out iHttpCode);
                if ((bTry) && (iHttpCode > 200) && (iHttpCode < 600))
                    this.Response.StatusCode = iHttpCode;
                objInfoBox.Text = this.GetLocalResourceObject(errorCode).ToString();
            }
        }
        catch
        {
            objInfoBox.Text = this.GetLocalResourceObject("0").ToString();
        }

        //Required to avoid "Script controls may not be registered before PreRender."
        base.OnPreRender(e);
    }
}
