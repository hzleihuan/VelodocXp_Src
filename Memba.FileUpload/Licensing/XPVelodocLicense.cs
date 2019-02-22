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
using System.ComponentModel; //License

namespace Memba.FileUpload.Licensing
{
    /// <summary>
    /// VelodocLicense class
    /// </summary>
    public class VelodocLicense : License
    {
        #region Private Members
        private Nullable<Guid> _LicenseKey;
        private Nullable<Guid> _ActivationKey;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="licenseKey"></param>
        /// <param name="activationKey"></param>
        public VelodocLicense(string licenseKey, string activationKey)
        {
            try
            {
                _LicenseKey = new Guid(licenseKey);
                _ActivationKey = new Guid(activationKey);
            }
            catch
            {
                _LicenseKey = null;
                _ActivationKey = null;
            }
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Gets the license key
        /// </summary>
        public override string LicenseKey
        {
            get { return (_LicenseKey.HasValue) ? _LicenseKey.Value.ToString() : null; }
        }
        /// <summary>
        /// Gets the activation key
        /// </summary>
        public string ActivationKey
        {
            get { return (_ActivationKey.HasValue) ? _ActivationKey.Value.ToString() : null; }
        }
        /// <summary>
        /// Gets the  license type (EULA or GPL)
        /// </summary>
        public LicenseType LicenseType
        {
            get
            {
                //Without license and activation keys, it's GPL otherwise it is Memba EULA
                if (_LicenseKey.HasValue && _ActivationKey.HasValue)
                    return LicenseType.EULA;
                else
                    return LicenseType.GPL;
            }
        }
        #endregion

        #region Other Members
        /// <summary>
        /// Disposes of resources
        /// </summary>
        public override void Dispose()
        {
            //base.Dispose();
        }
        #endregion
    }
    /// <summary>
    /// Velodoc License Type (GPL or Memba EULA)
    /// </summary>
    public enum LicenseType
    {
        GPL,
        EULA
    }
}
