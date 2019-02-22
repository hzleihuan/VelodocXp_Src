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
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Memba.Common.Presentation
{
    /// <summary>
    /// Summary description for Constants
    /// </summary>
    public static class Constants
    {
        public const string FilesWebServicesNamespace = "http://schemas.memba.org/2006/files/webservices";

        public const string UserList = "UserList"; //Delimited list defined in Web.Config
        public const int UserListSize = 10; //max number of email addresses returned from the delimited list
        public const string SecurityCode = "SecurityCode"; //Security code defined in Web.Config
        public const string FileMaxAge = "FileMaxAge"; //File maximum age (in days) defined in Web.Config

        public const int SlidingExpiration = 10; //Cache sliding expiration in minutes

        public const string LastError = "LastError";

        public const int AjaxTimeOut = 6000;
        public const int AjaxTrials = 2;
        public const int AjaxWaitBeforeRetry = 100;
        public const int StartProgress = 500;
        public const int RecurringProgress = 3000;
        public const int EndProgress = 500;

        public const int WcfStreamingBufferSize = 4096; //Used for WCF Services
        public const int WcfStreamingTimeOut = 10000; //10 sec.
    }
}