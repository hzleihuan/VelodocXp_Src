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

using Memba.Common.Presentation;

public partial class App_CommonCtrls_mainmenu : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void TabbedMenu_MenuItemDataBound(object sender, MenuEventArgs e)
    {
        HttpContext objHttpContext = HttpContext.Current;
        SiteMapNode objNode = (SiteMapNode)e.Item.DataItem;

        // see http://weblogs.asp.net/dannychen/archive/2005/03/28/396099.aspx
        e.Item.ImageUrl = Icons.GetIcon16(objNode["image"]);
        e.Item.Text = "&nbsp;&nbsp;" + e.Item.Text;

        if (objHttpContext != null)
        {
            if ((objNode.ResourceKey == "Files")
                && (objHttpContext.Request.RawUrl.ToLowerInvariant().Contains("/files/")))
                e.Item.Selected = true;
            else if ((objNode.ResourceKey == "Admin")
                && (objHttpContext.Request.RawUrl.ToLowerInvariant().Contains("/admin/")))
                e.Item.Selected = true;
        }
    }
}
