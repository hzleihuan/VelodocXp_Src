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
using System.IO;
using System.Web; //AspNetHostingPermission
using System.Security.Permissions; //SecurityAction

namespace Memba.FileUpload
{
    /// <summary>
    /// 
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    //Class is sealed [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class UploadFile
    {
        #region Private Members
        private string _HtmlInputId;
        private string _OriginalPath;
        private string _OriginalFileName;
        private string _ContentType;
        private long _ContentPosition;
        private object _ProviderFileKey;
        private bool _IsComplete;
        private string _HashValue;
        #endregion

        #region Constructors
        internal UploadFile (string htmlInputID, string originalPath, string contentType, object providerFileKey)
        {
            _HtmlInputId = htmlInputID;
            _OriginalPath = originalPath;
            //We have checked that Path.GetFileName also works with Unix (and Mac/OS) paths, i.e.
            //Path.GetFileName("/Users/DSA/Desktop/presentation.ppt") == "presenttaion.ppt" 
            _OriginalFileName = Path.GetFileName(_OriginalPath);
            _ContentType = contentType;
            _ProviderFileKey = providerFileKey;
            //_IsComplete = false; //CA1805
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// 
        /// </summary>
        public string HtmlInputId
        {
            get { return this._HtmlInputId; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string OriginalPath
        {
            get { return this._OriginalPath; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string OriginalFileName
        {
            get { return this._OriginalFileName; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
        }
        /// <summary>
        /// 
        /// </summary>
        public object ProviderFileKey
        {
            get { return _ProviderFileKey; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long ContentPosition
        {
            get { return _ContentPosition; }
            internal set { _ContentPosition = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long ContentLength
        {
            get
            {
                if (_IsComplete)
                    return _ContentPosition;
                else
                    return -1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsComplete
        {
            get { return _IsComplete; }
            internal set { _IsComplete = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string HashValue
        {
            get { return _HashValue; }
            internal set { _HashValue = value; }
        }
        #endregion
    }
}

