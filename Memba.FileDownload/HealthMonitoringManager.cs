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
using System.Configuration;
using System.Web.Configuration;
using System.Diagnostics;
using System.Xml;

namespace Memba.FileDownload
{
    /// <summary>
    /// Event logger
    /// </summary>
    internal static class HealthMonitoringManager
    {
        public const string DefaultLog = "Application";
        public const string DefaultSource = "File Download";

        //Cached values (no need for a lock: we write once and read many times)
        private static Nullable<bool> _Enabled = null;
        private static string _Log = null;

        #region Property Accessors
        /// <summary>
        /// Returns true of health monitoring is true, otherwise false
        /// </summary>
        public static bool Enabled
        {
            get
            {
                //If the cache is empty, load value into cache
                if (!_Enabled.HasValue)
                {
                    //Assume false
                    _Enabled = false;

                    //Read configuration
                    try
                    {
                        HealthMonitoringSection objSection = WebConfigurationManager.GetSection("system.web/healthMonitoring") as HealthMonitoringSection;
                        if (objSection != null)
                            _Enabled = objSection.Enabled;
                    }
                    catch { }
                }

                System.Diagnostics.Debug.Assert(_Enabled.HasValue);
                return _Enabled.Value;
            }
        }
        /// <summary>
        /// Gets the event log configured in Enterprise Library, or "Application" by default
        /// </summary>
        public static string Log
        {
            get
            {
                if (String.IsNullOrEmpty(_Log))
                {
                    try
                    {
                        //The following code raises an InvalidOperationException : This operation does not apply at runtime.
                        //ConfigurationSection objConfigurationSection = System.Configuration.ConfigurationManager.GetSection("loggingConfiguration") as ConfigurationSection;
                        //string sXml = objConfigurationSection.SectionInformation.GetRawXml();

                        //Read web.config
                        Configuration objConfiguration = WebConfigurationManager.OpenWebConfiguration(System.Web.HttpContext.Current.Request.ApplicationPath);
                        foreach (ConfigurationSection objSection in objConfiguration.Sections)
                        {
                            //Get "loggingConfiguration" section
                            if (objSection.SectionInformation.Name == "loggingConfiguration")
                            {
                                string sXmlFragment = objSection.SectionInformation.GetRawXml();
                                using (XmlTextReader objXmlSectionReader = new XmlTextReader(sXmlFragment, XmlNodeType.Element, null))
                                {
                                    //Find the "listeners" element
                                    while (objXmlSectionReader.Read())
                                    {
                                        if ((objXmlSectionReader.NodeType == XmlNodeType.Element)
                                            && (objXmlSectionReader.LocalName.Equals("listeners")))
                                        {
                                            using (XmlReader objXmlListenerReader = objXmlSectionReader.ReadSubtree())
                                            {
                                                //Find the "add" element 
                                                while (objXmlListenerReader.Read())
                                                {
                                                    if ((objXmlListenerReader.NodeType == XmlNodeType.Element)
                                                        && (objXmlListenerReader.LocalName.Equals("add")))
                                                    {
                                                        objXmlListenerReader.MoveToAttribute("log");
                                                        if (objXmlListenerReader.ReadAttributeValue())
                                                            _Log = objXmlListenerReader.Value;
                                                    }
                                                    if (!String.IsNullOrEmpty(_Log))
                                                        break;
                                                }
                                            }
                                        }
                                        if (!String.IsNullOrEmpty(_Log))
                                            break;
                                    }
                                }
                                //"loggingCOnfiguration" section processed, no need to iterate further
                                break;
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (String.IsNullOrEmpty(_Log))
                            _Log = DefaultLog;
                    }
                }
                System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(_Log));
                return _Log;
            }
        }
        /// <summary>
        /// Gets the source of the event log entry 
        /// </summary>
        public static string Source
        {
            get { return HealthMonitoringManager.DefaultSource; }
        }
        #endregion

        #region Log Functions
        /// <summary>
        /// Logs an error event either through the health monitoring framework or directly to the event log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="eventCode"></param>
        /// <param name="e"></param>
        public static void LogErrorEvent(string msg, DownloadFile data, int eventCode, Exception e)
        {
            if (HealthMonitoringManager.Enabled)
            {
                DownloadRequestErrorEvent objDownloadRequestErrorEvent = new DownloadRequestErrorEvent(msg, data, eventCode, e);
                objDownloadRequestErrorEvent.Raise();
            }
            else
            {
                try
                {
                    if (!EventLog.SourceExists(HealthMonitoringManager.Source))
                        System.Diagnostics.EventLog.CreateEventSource(HealthMonitoringManager.Source, HealthMonitoringManager.Log);

                    System.Diagnostics.EventLog.WriteEntry(
                        HealthMonitoringManager.Source,
                        String.Format("{0}\r\n\r\n{1}\r\n\r\n{2}", msg, data, e),
                        EventLogEntryType.Warning,
                        eventCode);
                }
                catch { }
            }
        }
        /// <summary>
        /// Logs a success event either through the health monitoring framework or directly to the event log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="eventCode"></param>
        public static void LogSucessEvent(string msg, DownloadFile data, int eventCode)
        {
            if (HealthMonitoringManager.Enabled)
            {
                DownloadRequestSuccessEvent objDownloadRequestSuccessEvent = new DownloadRequestSuccessEvent(msg, data, eventCode);
                objDownloadRequestSuccessEvent.Raise();
            }
            else
            {
                try
                {
                    if (!EventLog.SourceExists(HealthMonitoringManager.Source))
                        System.Diagnostics.EventLog.CreateEventSource(HealthMonitoringManager.Source, HealthMonitoringManager.Log);

                    System.Diagnostics.EventLog.WriteEntry(
                        HealthMonitoringManager.Source,
                        String.Format("{0}\r\n\r\n{1}", msg, data),
                        EventLogEntryType.Information,
                        eventCode);
                }
                catch { }
            }
        }
        #endregion
    }
}
