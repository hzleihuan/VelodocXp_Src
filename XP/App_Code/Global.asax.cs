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
using System.Web;
using System.Threading;
using System.Globalization;
using System.Web.Configuration;

/// <summary>
/// Summary description for Global
/// </summary>
public class Global : HttpApplication
{
    protected void Application_Error(object sender, EventArgs e)
    {
        Exception objException = Server.GetLastError().GetBaseException();
        //We want a generic message for most exceptions, except application exceptions which have specific localized messages
        //Note that Http exceptions raised by the upload module are forwarded to custom errors using their status code anyway
        if (objException is ApplicationException)
        {
            if (Session[Memba.Common.Presentation.Constants.LastError] == null)
                Session[Memba.Common.Presentation.Constants.LastError] = objException.Message;
            Server.ClearError(); //Otherwise a new session is created and Session[Constants.LastError] is null
            Response.Redirect(Memba.Common.Presentation.PageUrls.Error);
        }
    }
}
