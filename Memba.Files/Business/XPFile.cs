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
using System.Data;
//using System.Data.Common;
using System.ComponentModel; //DataObjectAttribute

namespace Memba.Files.Business
{
    [DataObject, Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace="http://schemas.memba.org/2006/attachments")]
    public sealed class File
    {
        #region Private Members
        private Guid _FileGuid;
        private string _FileName;
        private string _ContentType;
        private string _Key;
        private long _Size;
        private string _HashValue;
        private DateTime _CreatedOn;
        //Dirty flag
        private bool _IsSaveRequired;
        #endregion

        #region Constructors
        /// <summary>
        /// Parameterless constructor for Xml serialization
        /// </summary>
        public File()
        {
        }
        /// <summary>
        /// Public constructor for new objects
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="key"></param>
        /// <param name="size"></param>
        /// <param name="hashValue"></param>
        public File(
            string fileName,
            string contentType,
            string key,
            long size,
            string hashValue
            ) : this(
            Guid.NewGuid(),
            fileName,
            contentType,
            key,
            size,
            hashValue)
        {  }
        /// <summary>
        /// Public constructor with all parameters
        /// </summary>
        /// <param name="fileGuid"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="key"></param>
        /// <param name="size"></param>
        /// <param name="hashValue"></param>
        public File(
            Guid fileGuid,
            string fileName,
            string contentType,
            string key,
            long size,
            string hashValue
            )
        {
            _FileGuid = fileGuid;
            _FileName = fileName;
            _ContentType = contentType;
            _Key = key;
            _Size = size;
            _HashValue = hashValue;
            _CreatedOn = DateTime.UtcNow;
            //Dirty flag
            _IsSaveRequired = true;
        }
        #endregion

        /// <summary>
        /// Gets file Guid
        /// </summary>
        [DataObjectField(true, true, false)]
        public Guid Guid
        {
            get { return _FileGuid; }
            set { _FileGuid = value; } //_IsSaveRequired = true; }
        }
        /// <summary>
        /// Gets or sets message body
        /// </summary>
        [DataObjectField(false, false, false)]
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; _IsSaveRequired = true; }
        }
        /// <summary>
        /// Gets or sets content type
        /// </summary>
        [DataObjectField(false, false, false)]
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; _IsSaveRequired = true; }
        }
        /// <summary>
        /// Gets or sets the file provider key
        /// <remarks>The key can be a file system path or a database identifier</remarks>
        /// </summary>
        [DataObjectField(false, false, false)]
        public string Key
        {
            get { return _Key; }
            set { _Key = value; _IsSaveRequired = true; }
        }
        /// <summary>
        /// Gets or sets the file size
        /// </summary>
        [DataObjectField(false, false, false)]
        public long Size
        {
            get { return _Size; }
            set { _Size = value; _IsSaveRequired = true; }
        }
        /// <summary>
        /// Gets or sets the file hash value
        /// </summary>
        [DataObjectField(false, false, false)]
        public string HashValue
        {
            get { return _HashValue; }
            set { _HashValue = value; _IsSaveRequired = true; }
        }
        /// <summary>
        /// Gets the file creation date
        /// </summary>
        [DataObjectField(false, false, false)]
        public DateTime CreatedOn
        {
            get { return _CreatedOn; }
            set { _CreatedOn = value; } // _IsSaveRequired = true; }
        }
        /// <summary>
        /// Gets the file last modification date (kept here for compatibility with Velodoc enterprise edition)
        /// </summary>
        [DataObjectField(false, false, false)]
        [System.Xml.Serialization.XmlIgnore]
        public DateTime ModifiedOn
        {
            get { return _CreatedOn; }
        }
        /// <summary>
        /// Gets file's IsSaveRequired state
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsSaveRequired
        {
            get { return _IsSaveRequired; }
        }
        /// <summary>
        /// Confirms save on business object to reset dirty flag
        /// </summary>
        public void SaveCompleted()
        {
            _IsSaveRequired = false;
        }
    }
}
