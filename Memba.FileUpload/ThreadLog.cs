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
using System.Collections.Generic;
using System.Text;
using System.Globalization; //CultureInfo

namespace Memba.FileUpload
{
    internal static class ThreadLog
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "message"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "ThreadLog.WriteLine is not part of the Release")]
        public static void WriteLine(String message)
        {
#if(TRACE)
            string sFilePath = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%\\Temp"),
                DateTime.Now.ToString("yyMMdd") + "-" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture) + ".log");
            if (!File.Exists(sFilePath))
            {
                FileStream fs = File.Create(sFilePath);
                fs.Close();
            }
            try
            {
                System.Diagnostics.Trace.WriteLine(message);

                DateTime dtNow = DateTime.Now;
                StreamWriter objSW = File.AppendText(sFilePath);
                objSW.WriteLine(dtNow.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture) + "\t" + message);
                objSW.Flush();
                objSW.Close();
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Trace.WriteLine(Ex.Message.ToString());
            }
#endif
        }
    }
}


