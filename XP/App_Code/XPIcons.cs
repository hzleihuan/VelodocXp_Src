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
    /// Icons class is a helper class to get a proper icon corresponding to the content-type of a file
    /// </summary>
    public static class Icons
    {
        public const string Path16 = "~/App_Images/16x16/";
        public const string Path24 = "~/App_Images/24x24/";
        public const string Path32 = "~/App_Images/32x32/";
        public const string Path48 = "~/App_Images/48x48/";
        public const string PathXY = "~/App_Images/misc/";

        #region Content Type Icons
        /// <summary>
        /// Gets a 16x16 image icon according to file content type 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string GetFileIcon16(object contentType)
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path16 + GetFileIconName((string)contentType));
        }
        /// <summary>
        /// Gets a 32x32 image icon according to file content type 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string GetFileIcon32(object contentType)
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path32 + GetFileIconName((string)contentType));
        }
        /// <summary>
        /// Helper function that maps file icons to content types
        /// </summary>
        /// <returns></returns>
        public static string GetFileIconName(string contentType)
        {
            if (String.IsNullOrEmpty(contentType))
                return String.Empty;
            
            contentType = contentType.ToLowerInvariant();

            if ((contentType == "application/x-zip-compressed")
                || (contentType == "application/x-compressed")
                || (contentType == "application/x-compress")
                || (contentType == "application/x-gzip")) //zip, rar
                return "ext-zip.gif";
            else if ((contentType == "application/msword") //word
                || (contentType == "application/rtf")
                || (contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")) //word
                return "ext-doc.gif";
            else if ((contentType == "application/vnd.ms-excel") //excel
                || (contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                return "ext-xls.gif";
            else if ((contentType == "application/vnd.ms-powerpoint") //powerpoint
                || (contentType == "application/vnd.openxmlformats-officedocument.presentationml.presentation"))
                return "ext-ppt.gif";
            else if ((contentType == "application/pdf")
                || (contentType == "application/postscript")) //acrobat
                return "ext-pdf.gif";
            else if (contentType.StartsWith("text/")) //text
                return "ext-txt.gif";
            else if ((contentType.StartsWith("image/")) //image
                || (contentType == "application/tiff"))
                return "ext-img.gif";
            else if (contentType.StartsWith("audio/")) //audio
                return "ext-wma.gif";
            else if ((contentType == "application/x-msmediaview")
                || (contentType.StartsWith("video/"))) //multimedia
                return "ext-wmv.gif";
            else //incl. application/octet-_Input
                return "file.gif";
        }
        #endregion

        #region Any Icon
        public static string GetIcon16(string gifFileName)
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path16 + gifFileName);
        }
        public static string GetIcon24(string gifFileName)
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path24 + gifFileName);
        }
        public static string GetIcon32(string gifFileName)
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path32 + gifFileName);
        }
        public static string GetIcon48(string gifFileName)
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path48 + gifFileName);
        }
        public static string GetIconXY(string gifFileName)
        {
            return VirtualPathUtility.ToAbsolute(Icons.PathXY + gifFileName);
        }
        #endregion

        #region Any Path
        public static string GetPath16()
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path16);
        }
        public static string GetPath24()
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path24);
        }
        public static string GetPath32()
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path32);
        }
        public static string GetPath48()
        {
            return VirtualPathUtility.ToAbsolute(Icons.Path48);
        }
        public static string GetPathXY()
        {
            return VirtualPathUtility.ToAbsolute(Icons.PathXY);
        }
        #endregion
    }
}
