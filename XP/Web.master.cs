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

using Memba.Common.Presentation; //Icons, Constants
using Memba.WebControls; //InfoBoxType
using Memba.Files.Business; //BODisplay

public partial class WebMasterPage : System.Web.UI.MasterPage
{
    //protected void Page_Load(object sender, EventArgs e)
    //{
    //
    //}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPreRender(EventArgs e)
    {
        //get current sitemap node to determine page icon and title
        SiteMapProvider objSiteMapProvider = SiteMap.Providers["RootSiteMapProvider"];
        System.Diagnostics.Debug.Assert(objSiteMapProvider != null);
        SiteMapNode objCurrentNode = objSiteMapProvider.CurrentNode;

        if (objCurrentNode != null) //This is for defaultErr.aspx which is not referenced as a node
        {
            //set page icon and title
            Page.Header.Title = Resources.Web.glossary.App_Name + " - " + objCurrentNode.Title;
            if (string.IsNullOrEmpty(PageTitle.Text))
                PageTitle.Text = objCurrentNode.Title;
            if (string.IsNullOrEmpty(PageTitle.ImageUrl))
                PageTitle.ImageUrl = Icons.GetIcon32(objCurrentNode["image"]);
        }

        //Display errors of postback events
        if (Page.Session[Constants.LastError] != null) //&& Page.IsPostBack
        {
            InfoBox.Type = InfoBoxType.Error;
            InfoBox.Text = BODisplay.Encode(Session[Constants.LastError]);
            Session[Constants.LastError] = null;
        }

        //Required to avoid "Script controls may not be registered before PreRender."
        base.OnPreRender(e);
    }
}
