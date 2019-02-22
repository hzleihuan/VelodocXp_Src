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
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="mainmenu.ascx.cs" Inherits="App_CommonCtrls_mainmenu" %>
<table border="0" cellpadding="0" cellspacing="0" style="WIDTH:100%;">
    <tr>
        <td id="tdTabbedMenuTabs">
            <asp:Menu ID="TabbedMenu" runat="server" SkinID="sknTabbedMenu" DataSourceID="TabbedMenuSiteMapDataSource" Orientation="Horizontal" MaximumDynamicDisplayLevels="0" OnMenuItemDataBound="TabbedMenu_MenuItemDataBound">
            </asp:Menu>
            <asp:SiteMapDataSource ID="TabbedMenuSiteMapDataSource" runat="server" ShowStartingNode="False"
                SiteMapProvider="RootSiteMapProvider" />
        </td>
        <td id="tdTabbedMenuButtons">
        </td>
    </tr>
</table>
