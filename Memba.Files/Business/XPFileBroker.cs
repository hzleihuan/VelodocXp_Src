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
using System.Xml;
using System.Xml.Serialization;

namespace Memba.Files.Business
{
    public static class FileBroker
    {
        /// <summary>
        /// Creates a definition file for a File object
        /// </summary>
        /// <param name="f">The File object to persist</param>
        /// <param name="di">The directory where to write the definition file</param>
        public static void Insert(File f, DirectoryInfo di)
        {
            if (f == null)
                throw new ArgumentNullException("f", Properties.Resources.BLExceptionNullFile);

            if(!di.Exists)
                throw new ArgumentException(Properties.Resources.BLExceptionMissingDirectory, "di");

            if (f.IsSaveRequired)
            {
                FileInfo[] arrFileInfo = di.GetFiles(FileBroker.GetDefinitionFile(f.Guid));
                if ((arrFileInfo != null) && (arrFileInfo.Length>0))
                    throw new ArgumentException(Properties.Resources.BLExceptionDefinitionExists, "f");

                using (XmlWriter objXmlWriter = XmlWriter.Create(Path.Combine(di.FullName, FileBroker.GetDefinitionFile(f.Guid))))
                {
                    XmlSerializer objXmlSerializer = new XmlSerializer(typeof(File));
                    objXmlSerializer.Serialize(objXmlWriter, f);
                }

                f.SaveCompleted();
            }
        }
        /// <summary>
        /// Loads a File object designated by its Guid from its definition file
        /// </summary>
        /// <param name="g">The guid of the file to load</param>
        /// <param name="di">The directory where to read the definition file</param>
        /// <returns>The File designated by g</returns>
        public static File SelectByGuid(Guid g, DirectoryInfo di)
        {
            if (g.Equals(Guid.Empty))
                throw new ArgumentException(Properties.Resources.BLExceptionEmptyFileGuid, "g");

            if (!di.Exists)
                throw new ArgumentException(Properties.Resources.BLExceptionMissingDirectory, "di");

            FileInfo[] arrFileInfo = di.GetFiles(FileBroker.GetDefinitionFile(g));
            if ((arrFileInfo == null) || (arrFileInfo.Length < 1))
                throw new ArgumentException(Properties.Resources.BLExceptionMissingDefinition, "g");

            File objFileRet;
            using (XmlReader objXmlReader = XmlReader.Create(Path.Combine(di.FullName, FileBroker.GetDefinitionFile(g))))
            {
                XmlSerializer objXmlSerializer = new XmlSerializer(typeof(File));
                objFileRet = objXmlSerializer.Deserialize(objXmlReader) as File;
            }

            return objFileRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private static string GetDefinitionFile(Guid g)
        {
            return g.ToString("N") + ".def";
        }
    }
}
