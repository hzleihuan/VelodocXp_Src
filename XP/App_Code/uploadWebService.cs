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
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Caching; //Cache
using System.Web.Configuration; //ConfigurationManager
using System.Web.Script.Services; //ScriptService
using Memba.Common.Presentation; //Constants

namespace Memba.Files.WebServices
{
    /// <summary>
    /// Summary description for uploadWebService
    /// </summary>
    [WebService(Namespace = Constants.FilesWebServicesNamespace)]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public class uploadWebService : System.Web.Services.WebService
    {
        [WebMethod(Description = "Checks security code")]
        [ScriptMethod(UseHttpGet = true)]
        public bool CheckSecurityCode(string securityCode)
        {
            System.Diagnostics.Trace.WriteLine("uploadWebService.CheckSecurityCode: Begin");

            //UrlReferrer is null from Firefox 1.5
            if ((HttpContext.Current.Request.Browser.Browser != "Firefox" || HttpContext.Current.Request.Browser.MajorVersion != 1)
                && (!HttpContext.Current.Request.UrlReferrer.Authority.Equals(HttpContext.Current.Request.Url.Authority, StringComparison.InvariantCultureIgnoreCase)))
                throw new ApplicationException(Resources.Web.glossary.WebService_NoCallFromOtherDomain);

            string sSecurityCode = (string)HttpRuntime.Cache[Constants.SecurityCode];
            if (String.IsNullOrEmpty(sSecurityCode))
            {
                sSecurityCode = WebConfigurationManager.AppSettings[Constants.SecurityCode];
                HttpRuntime.Cache.Add(
                    Constants.SecurityCode,
                    sSecurityCode,
                    null,
                    Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, Constants.SlidingExpiration, 0),
                    CacheItemPriority.Default,
                    null);
            }

            // Note: we could certainly discuss the level of security achieved with validating
            // the security code client side:
            // 1) Validating the security code server side in the same post, that is after files
            // have been uploaded to teh server is not an option because we want to avoid
            // consuming server resources if the sender does not have the proper security code.
            // 2) Using a login page where the security code or credentials are validated has been
            // deemed too complex for an XP = Express version which should be easy to install and use
            // For users wary about security, we can only recommend the enterprise version of Velodoc.

            return sSecurityCode.Equals(securityCode);
        }
    }
}
