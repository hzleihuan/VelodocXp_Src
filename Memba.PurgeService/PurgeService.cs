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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

using System.IO; //DirectoryInfo
using System.Threading; //Timer

namespace Memba.PurgeService
{
    public partial class PurgeService : ServiceBase
    {
        #region Private Members
        private static string Directory;
        private static int FileMaxAge;

        private Timer _Timer;
        private bool _IsStopping;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public PurgeService()
        {
            InitializeComponent();

            //Event log
            try
            {
                eventLog1.Log = Constants.EventLog;
                if (!System.Diagnostics.EventLog.SourceExists(Constants.EventSource))
                    System.Diagnostics.EventLog.CreateEventSource(Constants.EventSource, Constants.EventLog);
                eventLog1.Source = Constants.EventSource;
            }
            catch
            { }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// OnStart event handler
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
#if(DEBUG)
            //Give us some time to attach debugger
            //Note the SCM times out after 30 sec.
            System.Threading.Thread.Sleep(25000);
#endif  
            //Write to event log
            try { eventLog1.WriteEntry(Properties.Resources.PurgeService_Started); }
            catch { }

            try
            {
                //Set static members
                PurgeService.Directory = System.Configuration.ConfigurationManager.AppSettings[Constants.Directory];
                bool bTry = Int32.TryParse(
                    System.Configuration.ConfigurationManager.AppSettings[Constants.FileMaxAge],
                    out PurgeService.FileMaxAge);
                if (!bTry)
                    PurgeService.FileMaxAge = Constants.FileMaxAgeDefault;
            }
            catch
            {
                try { eventLog1.WriteEntry(Properties.Resources.PurgeService_BadAppSettings); }
                catch { }
            }

            //Start recurring timer
            _IsStopping = false;
            _Timer = new Timer(new TimerCallback(Tick), null, 0, Constants.Interval);
        }
        /// <summary>
        /// OnStop event handler
        /// </summary>
        protected override void OnStop()
        {
            //Stop timer
            _IsStopping = true;
            _Timer.Dispose();

            //Write to event log
            try { eventLog1.WriteEntry(Properties.Resources.PurgeService_Stopped); }
            catch { }
        }
        /// <summary>
        /// Tick callback for the timer
        /// </summary>
        /// <param name="state"></param>
        private void Tick(object state)
        {
            DateTime dtNow = DateTime.UtcNow;
            int iDeletedFiles = 0;
            DirectoryInfo di = new DirectoryInfo(PurgeService.Directory);
            if (!di.Exists)
                eventLog1.WriteEntry(Properties.Resources.PurgeService_BadDirectory);

            FileInfo[] arrFileInfo = di.GetFiles();
            foreach (FileInfo fi in arrFileInfo)
            {
                if (_IsStopping)
                    break;

                if (fi.Exists)
                {
                    if (fi.CreationTimeUtc.AddDays(Convert.ToDouble(PurgeService.FileMaxAge)) < dtNow)
                    {
                        try
                        {
                            //Just in case a file is read-only
                            fi.Attributes &= ~FileAttributes.ReadOnly;
                            //Delete file
                            fi.Delete();
                            iDeletedFiles++;
                        }
                        catch
                        {
                            //Write to event log
                            try { eventLog1.WriteEntry(String.Format(Properties.Resources.PurgeService_DeleteFail, fi.Name)); }
                            catch { }
                        }
                    }
                }
            }

            //Write to event log
            try { eventLog1.WriteEntry(String.Format(Properties.Resources.PurgeService_DeleteSuccess, iDeletedFiles)); }
            catch { }
        }
        #endregion
    }
}
