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
using System.Collections.Generic; //List<T>
using System.Collections.ObjectModel; //ReadOnlyCollection<T>
using System.Text;
using System.Web; //HttpContext
using System.Globalization; //CultureInfo
using System.Security.Permissions; //SecurityAction

using Memba.FileUpload.Properties;

namespace Memba.FileUpload
{
    /// <summary>
    /// 
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    //Class is sealed [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class UploadData
    {
        #region Private Members
        //TODO: Add the following to get the instant bandwidth
        //private DateTime _LastSaved;
        //private long _LastContentLength;

        private string _UploadId;
        private long _ContentPosition;
        private long _ContentLength;
        private DateTime _StartDate;
        private DateTime _EndDate;
        private string _UserName;
        private bool _IsAuthenticated;
        private UploadProgressStatus _ProgressStatus;
        private List<UploadFile> _UploadFiles;
        private Exception _Exception;
        private bool _IsProcessed;
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        internal UploadData(string uploadID, string userName, bool isAuthenticated)
        {
            _UploadId = uploadID;
            _ContentPosition = -1;
            _ContentLength = -1;
            _StartDate = DateTime.MinValue;
            _EndDate = DateTime.MaxValue;
            _UserName = userName;
            _IsAuthenticated = isAuthenticated;
            _ProgressStatus = UploadProgressStatus.Unknown;
            _UploadFiles = new List<UploadFile>();
            //_Exception = null; //CA1805
        }

        #endregion

        #region Property Accessors
        /// <summary>
        /// 
        /// </summary>
        public string UploadId
        {
            get { return _UploadId; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long ContentPosition
        {
            get { return _ContentPosition; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long ContentLength
        {
            get { return _ContentLength; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime StartDate
        {
            get { return _StartDate; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime EndDate
        {
            get { return _EndDate; }
        }
        /// <summary>
        /// 
        /// </summary>
        internal string UserName
        {
            get { return _UserName; }
        }
        /// <summary>
        /// 
        /// </summary>
        internal bool IsAuthenticated
        {
            get { return _IsAuthenticated; }
        }
        /// <summary>
        /// 
        /// </summary>
        public UploadProgressStatus ProgressStatus
        {
            get { return _ProgressStatus; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyCollection<UploadFile> UploadFiles
        {
            get
            {
                System.Diagnostics.Debug.Assert(_UploadFiles != null);
                return _UploadFiles.AsReadOnly();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public UploadFile UploadFileInProgress
        {
            get
            {
                System.Diagnostics.Debug.Assert(_UploadFiles != null);
                return _UploadFiles[_UploadFiles.Count - 1];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Exception Exception
        {
            get { return _Exception; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsProcessed
        {
            get { return _IsProcessed; }
        }
        /// <summary>
        /// Gets the progress ratio (a number between 0 and 1). 
        /// </summary>
        public double ProgressRatio
        {
            get
            {
                if (_ContentLength == 0)
                    return 0d;
                else
                    return ((double)_ContentPosition/_ContentLength);
            }
        }
        /// <summary>
        /// Gets the time elasped since the download has started.
        /// </summary>
        public TimeSpan TimeElapsed
        {
            get
            {
                if (_ProgressStatus == UploadProgressStatus.InProgress)
                    return DateTime.UtcNow.Subtract(_StartDate);
                if ((_ProgressStatus == UploadProgressStatus.Completed) || (_ProgressStatus == UploadProgressStatus.Failed))
                    return _EndDate.Subtract(_StartDate);
                else
                    return TimeSpan.Zero;
            }
        }
        /// <summary>
        /// Gets an estimate of the time left to complete the upload.
        /// </summary>
        public TimeSpan TimeLeftEstimated
        {
            get
            {
                if (TimeElapsed < TimeTotalEstimated)
                    return TimeTotalEstimated - TimeElapsed;
                else
                    return TimeSpan.Zero;
            }
        }
        /// <summary>
        /// Gets an estimate of the total time required for the upload.
        /// </summary>
        public TimeSpan TimeTotalEstimated
        {
            get
            {
                if (this.ProgressRatio == 0)
                    return TimeSpan.MaxValue;
                else
                    return TimeSpan.FromMilliseconds((1d/ProgressRatio)*this.TimeElapsed.TotalMilliseconds);
            }
        }
        /// <summary>
        /// Gets the bandwidth in bytes per second.
        /// </summary>
        public double BytesPerSecond
        {
            get
            {
                if (TimeElapsed.TotalSeconds == 0)
                    return 0d;
                else
                    return ((long)_ContentPosition) / TimeElapsed.TotalSeconds;
            }
        }
        #endregion

        #region Reporting Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentLength"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportStart(long contentLength)
        {
#if(TRACE)
            ThreadLog.WriteLine("Upload started with Content-Length = " + contentLength.ToString(CultureInfo.InvariantCulture));
#endif

            _ContentLength = contentLength;
            _ContentPosition = 0;
            _StartDate = DateTime.UtcNow;
            _ProgressStatus = UploadProgressStatus.InProgress;

            if (HttpContext.Current != null) //Support for unit tests
            {
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentLength == contentLength);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.InProgress);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentPosition"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportPosition(long contentPosition)
        {
#if(TRACE)
            ThreadLog.WriteLine("Content position = " + contentPosition.ToString(CultureInfo.InvariantCulture));
#endif

            _ContentPosition = contentPosition;

            if (HttpContext.Current != null) //Support for unit tests
            {
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentPosition == contentPosition);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.InProgress);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportFileCreated(long contentPosition, string htmlInputID, string fileName, string contentType, object providerFileKey)
        {
#if(TRACE)
            ThreadLog.WriteLine("New file = " + fileName);
#endif

            _ContentPosition = contentPosition;
            UploadFile objUploadFile = new UploadFile(htmlInputID, fileName, contentType, providerFileKey);
            _UploadFiles.Add(objUploadFile);

            if (HttpContext.Current != null) //Support for unit tests
            {
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentPosition == contentPosition);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.InProgress);
            }
        }
        ///
        /// 
        /// 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportFilePosition(long contentPosition, int fileContentProcessed)
        {
#if(TRACE)
            ThreadLog.WriteLine("File position = " + contentPosition.ToString(CultureInfo.InvariantCulture));
#endif

            _ContentPosition = contentPosition;
            this.UploadFileInProgress.ContentPosition += fileContentProcessed;

            if (HttpContext.Current != null) //Support for unit tests
            {
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentPosition == contentPosition);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.InProgress);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportFileCompleted(long contentPosition, byte[] hash)
        {
#if(TRACE)
            ThreadLog.WriteLine("File completed " + contentPosition.ToString(CultureInfo.InvariantCulture));
#endif

            // Create a new Stringbuilder to collect the bytes and create a string.
            StringBuilder objStringBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < hash.Length; i++)
            {
                objStringBuilder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            _ContentPosition = contentPosition;
            this.UploadFileInProgress.HashValue = objStringBuilder.ToString();
            this.UploadFileInProgress.IsComplete = true;

            if (HttpContext.Current != null) //Support for unit tests
            {
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentPosition == contentPosition);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.InProgress);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportCompleted(long contentPosition)
        {
#if(TRACE)
            ThreadLog.WriteLine("File completed " + contentPosition.ToString(CultureInfo.InvariantCulture));
#endif

            _ContentPosition = contentPosition;
            _EndDate = DateTime.UtcNow;
            _ProgressStatus = UploadProgressStatus.Completed;

            if (HttpContext.Current != null) //Support for unit tests
            {
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentPosition == contentPosition);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.Completed);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportCanceled()
        {
#if(TRACE)
            ThreadLog.WriteLine("Upload canceled");
#endif

            _EndDate = DateTime.UtcNow;
            _ProgressStatus = UploadProgressStatus.Canceled;

            if (HttpContext.Current != null) //Support for unit tests
            {
                //System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentPosition == contentPosition);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.Canceled);
            }

            HealthMonitoringManager.LogErrorEvent(
                Resources.ExceptionRequestCanceled,
                this,
                UploadRequestErrorEvent.RuntimeErrorRequestAbort,
                null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ex"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Memba.FileUpload.ThreadLog.WriteLine(System.String)", Justification = "ThreadLog.WriteLine is not part of the Release")]
        internal void ReportFailed(Exception exception)
        {
#if(TRACE)
            ThreadLog.WriteLine("Parsing failed: " + exception.Message);
#endif

            _EndDate = DateTime.UtcNow;
            _ProgressStatus = UploadProgressStatus.Failed;
            _Exception = exception;

            if (HttpContext.Current != null) //Support for unit tests
            {
                //System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ContentPosition == contentPosition);
                System.Diagnostics.Debug.Assert(((UploadData)HttpContext.Current.Cache.Get(UploadMonitor.UPLOAD_DATA_PREFIX + _UploadId)).ProgressStatus == UploadProgressStatus.Failed);
            }
        }
        /// <summary>
        /// Can be used to track the processing of files uploads in an application 
        /// </summary>
        public void ReportProcessed()
        {
            if (_ProgressStatus != UploadProgressStatus.Completed)
                throw new InvalidOperationException(Resources.ExceptionCannotProcessIfNotComplete);
            _IsProcessed = true;
        }
        #endregion

        #region Utility functions
        /// <summary>
        /// Formats upload progress status into a localized display string
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string Format(UploadProgressStatus status)
        {
            switch (status)
            {
                case UploadProgressStatus.Canceled:
                    return Resources.UploadProgressStatus_UploadCanceled;
                case UploadProgressStatus.Completed:
                    return Resources.UploadProgressStatus_UploadCompleted;
                case UploadProgressStatus.Failed:
                    return Resources.UploadProgressStatus_UploadFailed;
                case UploadProgressStatus.InProgress:
                    return Resources.UploadProgressStatus_UploadInProgress;
                case UploadProgressStatus.Unknown:
                    return Resources.UploadProgressStatus_UploadUnknown;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Outputs a string representation of a upload data, which is used in health monitoring and event log entries  
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder objStringBuilderRet = new StringBuilder();
            objStringBuilderRet.AppendLine(Properties.Resources.UploadData_Title);
            if(HttpContext.Current != null)
                objStringBuilderRet.AppendLine(String.Format(Properties.Resources.UploadData_Request, HttpContext.Current.Request.RawUrl));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.UploadData_UploadId, this.UploadId));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.UploadData_UserName, this.UserName));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.UploadData_UploadProgressStatus, UploadData.Format(this.ProgressStatus), this.ProgressRatio));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.UploadData_BytesProgress, this.ContentPosition, this.ContentLength, this.BytesPerSecond));
            objStringBuilderRet.AppendLine(String.Format(Properties.Resources.UploadData_TimeProgress, this.TimeElapsed, this.TimeLeftEstimated, this.TimeTotalEstimated));
            objStringBuilderRet.AppendLine(Properties.Resources.UploadData_Files);
            foreach (UploadFile objUploadFile in this.UploadFiles)
            {
                objStringBuilderRet.AppendLine(String.Format(
                    Properties.Resources.UploadData_UploadFile,
                    objUploadFile.OriginalPath,
                    objUploadFile.ContentType,
                    objUploadFile.ContentLength,
                    objUploadFile.ProviderFileKey));
            }
            if (this.Exception != null)
                objStringBuilderRet.AppendLine(String.Format(Properties.Resources.UploadData_Exception, this.Exception));
            return objStringBuilderRet.ToString();
        }
        #endregion
    }
    /// <summary>
    /// Upload progress status
    /// </summary>
    public enum UploadProgressStatus
    {
        Unknown,
        InProgress,
        Completed,
        Canceled,
        Failed
    }
}
