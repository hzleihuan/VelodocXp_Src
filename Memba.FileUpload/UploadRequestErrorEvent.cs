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
using System.Web.Management; //WebRequestErrorEvent

namespace Memba.FileUpload
{
    //For more information on instrumentation and web events,
    //see: http://msdn.microsoft.com/library/en-us/dnpag2/html/PAGHT000016.asp
    
    internal class UploadRequestErrorEvent : WebRequestErrorEvent
    {
        public const int RuntimeErrorUploadCodeBase = UploadRequestSuccessEvent.RuntimeUploadSuccessCodeBase + 500;
        public const int RuntimeErrorRequestAbort = RuntimeErrorUploadCodeBase + 1;
        public const int RuntimeErrorZeroContentLength = RuntimeErrorUploadCodeBase + 2;
        public const int RuntimeErrorMalformedContentType = RuntimeErrorUploadCodeBase + 3;
        public const int RuntimeErrorPostTooLarge = RuntimeErrorUploadCodeBase + 4;
        public const int RuntimeErrorMissingUploadID = RuntimeErrorUploadCodeBase + 5;
        public const int RuntimeErrorExceptionUnhandled = RuntimeErrorUploadCodeBase + 6;

        #region Private Members
        private object _EventSource;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor invoked in case of events identified only by their event code.
        /// </summary>
        /// <param name="msg">The message associated with the event.</param>
        /// <param name="eventSource">The object that is the source of the event.</param>
        /// <param name="eventCode">The code associated with the event. Use the constants defined in UploadRequestErrorEvent.</param>
        /// <param name="e">The Exception associated with the error.</param>
        public UploadRequestErrorEvent(string msg, object eventSource, int eventCode, Exception e)
            : base(msg, eventSource, eventCode, e)
        {
            _EventSource = eventSource;
        }
        /// <summary>
        /// Constructor invoked in case of events identified by their event code.and related event detailed code.
        /// </summary>
        /// <param name="msg">The message associated with the event.</param>
        /// <param name="eventSource">The object that is the source of the event.</param>
        /// <param name="eventCode">The code associated with the event. Use the constants defined in UploadRequestErrorEvent.</param>
        /// <param name="detailedCode">The event detail code identifier.</param>
        /// <param name="e">The Exception associated with the error.</param>
        public UploadRequestErrorEvent(string msg, object eventSource, int eventCode, int detailedCode, Exception e)
            : base(msg, eventSource, eventCode, detailedCode, e)
        {
            _EventSource = eventSource;
        }
        #endregion

        #region Other Members
        /// <summary>
        /// Raises the UploadRequestErrorEvent.
        /// </summary>
        //public override void Raise()
        //{
        //    base.Raise();
        //}
        /// <summary>
        /// Formats Web request event information.
        /// </summary>
        /// <param name="formatter"></param>
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);

            // Add event details
            if (_EventSource != null)
            {
                try
                {
                    formatter.AppendLine(String.Empty);
                    //formatter.IndentationLevel += 1;
                    //formatter.TabSize = 4;
                    formatter.AppendLine(_EventSource.ToString());
                }
                catch (Exception Ex)
                {
                    formatter.AppendLine(Ex.ToString());
                }
            }
        }
        #endregion
    }
}
