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
using System.Configuration; //ConfigurationSection

namespace Memba.FileUpload
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Source code derived from System.Web.Configuration.HttpRuntimeSection</remarks>
    public sealed class UploadRuntimeSection : ConfigurationSection
    {
        #region Private Members
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propMaxRequestLength = new ConfigurationProperty("maxRequestLength", typeof(int), -1, null, new IntegerValidator(-1, (int)(Int32.MaxValue/Constants.Kilo)), ConfigurationPropertyOptions.None);
        #endregion

        #region Constructor
        static UploadRuntimeSection()
        {
            _properties.Add(_propMaxRequestLength);
        }
        #endregion

        #region Property Accessors
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [IntegerValidator(MinValue = -1), ConfigurationProperty("maxRequestLength", DefaultValue = -1)]
        public int MaxRequestLength
        {
            get
            {
                return (int)base[_propMaxRequestLength];
            }
            set
            {
                //TODO: We should throw a ConfigurationErrorsException if value > maxRequestLength set in httpRuntime section
                base[_propMaxRequestLength] = value;
            }
        }

        /*
        internal int MaxRequestLengthBytes
        {
            get
            {
                long lMaxRequestLengthBytesRet = (long)(this.MaxRequestLength * Constants.Kilo);
                if (lMaxRequestLengthBytesRet > Constants.TopMaxRequestLengthBytes)
                    lMaxRequestLengthBytesRet = Constants.TopMaxRequestLengthBytes;
                return (int)lMaxRequestLengthBytesRet;
            }
        }
        */
        #endregion
    }
}
