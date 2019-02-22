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
using System.Collections.Generic;
using System.Text;
using System.IO;

using Memba.Files.Business;
using Memba.FileUpload.Providers;

namespace Memba.FileDownload
{
    /// <summary>
    /// This class is used to retrieve file information for the download process, and to manage the download state
    /// </summary>
    internal sealed class DownloadFile
    {
        #region Private objects and variables
        private Memba.Files.Business.File _File;
        private string _UserName;
        private DownloadState _State;
        #endregion

        #region Constructor
        public DownloadFile(Guid fileGuid, string userName)
        {
            DirectoryInfo objDirectoryInfo = new DirectoryInfo(FileStorage.Provider.ConnectionString);
            _File = FileBroker.SelectByGuid(fileGuid, objDirectoryInfo);
            _UserName = userName;
        }
        #endregion

        internal enum DownloadState
        {
            //Clear: No download in progress, 
            //the file can be manipulated
            Clear = 1,

            // Locked: A dynamically created file must
            // not be changed
            Locked = 2,

            // In Progress: File is locked, and download 
            // is currently in progress
            DownloadInProgress = 6,

            // Broken: File is locked, download was in
            // progress, but was cancelled 
            DownloadBroken = 10,

            // Finished: File is locked, download
            // was completed
            DownloadFinished = 18
        }

        #region Property Accessors
        /// <summary>
        /// 
        /// </summary>
        public bool Exists
        {
            get { return FileStorage.FileExists(_File.Key); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsExpired
        {
            get { return false; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool CanDownload
        {
            get { return true; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string FileName
        {
            get { return _File.FileName; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime LastWriteTimeUTC
        {
            get { return _File.ModifiedOn; }
        }
        /// <summary>
        /// Gets the file size in bytes
        /// </summary>
        public long Length
        {
            get { return _File.Size; }
        }
        /// <summary>
        /// MIME Type of the the file to download
        /// <remarks>See: http://www.microsoft.com/technet/prodtechnol/isa/2004/plan/mimetypes.mspx</remarks>
        /// </summary>
        public string ContentType
        {
            get { return _File.ContentType; }
        }
        /// <summary>
        /// The EntityTag used in the initial (200) response to, and in resume-Requests from the client software... 
        /// <remarks>Please note, that this unique code must keep the same as long as the file does not change. 
        /// If the file DOES change or is edited, however, the code MUST change.</remarks>
        /// </summary>
        public string EntityTag
        {
            get { return _File.Guid.ToString("N"); }
        }
        /// <summary>
        /// 
        /// </summary>
        public System.IO.Stream DownloadStream
        {
            get { return FileStorage.GetFileStream(_File.Key, AccessMode.Read); }
        }
        /// <summary>
        /// 
        /// </summary>
        public DownloadState State
        {
            //get { return _State; } //CA1811: No upstream public or protected callers
            set
            {
                _State = value;
                // ToDo - optional
                // At this point, you could delete the file automatically. 
                // If the state is set to Finished, your might not need
                // the file anymore:
                // if (_State == DownloadState.DownloadFinished)
                //   Clear()
                // Else
                //   Save()
                // End If
                Save();
            }
        }
        #endregion

        #region Public Methods
        /*
        public void Clear()
        {
            // Delete the source file and "clear" the file state...
            if ((State == DownloadState.DownloadBroken) || (State == DownloadState.DownloadInProgress))
            {
                // Do not allow deleting if the file download is in progress 
            }
            else
            {
                FileStorage.Providers[_File.Provider].DeleteFile(_File.Key);
                State = DownloadState.Clear;
            }
        }
        */
        /// <summary>
        /// Outputs a string representation of a download file, which is used in health monitoring and event log entries  
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder objStringBuilderRet = new StringBuilder();

            objStringBuilderRet.AppendLine(Properties.Resources.DownloadFile_Title);
            if (System.Web.HttpContext.Current != null)
                objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_Request, System.Web.HttpContext.Current.Request.RawUrl));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_DownloadState, this._State));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_UserName, this._UserName));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_FileGuid, this._File.Guid));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_ProviderKey, this._File.Key));
            //objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_ProviderName, this._File.Provider));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_OriginalName, this._File.FileName));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_ContentType, this._File.ContentType));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_Size, this._File.Size));
            //objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_Confidentiality, this._File.Confidentiality));
            //objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_ExpirationDate, this._File.ExpirationDate));
            //if(this._File.RemovalDate.HasValue)
            //    objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_RemovalDate, this._File.RemovalDate));
            //if (String.IsNullOrEmpty(this._File.Report))
            //    objStringBuilderRet.AppendLine(String.Format(Properties.Resources.DownloadFile_ScanReport, this._File.Report));

            return objStringBuilderRet.ToString();
        }
        #endregion

        #region Private Methods
        private void Save()
        {
            // ToDo - your code here (Save the state of this file's download to a database or XML file...)
            //
            // Do not use the Session or Application or Cache to
            // store this information, it must be independent from
            // Application, Session or Cache states!
            //
            // If you do not create attachments dynamically, 
            // you do not need to save the state, of course.

            if (_State == DownloadState.DownloadFinished)
            {
                //This would be the place where to add an history log
            }
        }
        #endregion
    }
}
