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

namespace Memba.Tests
{
    public static class Constants
    {
        //Constants depend on your configuration
        //Whereas static private members in test fictures don't.
        //We provide here a central mean to update test config to match your system

        #region UITest
        public const string Sender = "john.smith@memba.org"; //These email addresses need to be configured in appSettings/UserList in web.config 
        public const string Recipient = "joe.bloggs@memba.org";
        public const string DevWebSite = "http://localhost:49855/XP/"; //This actually need to correspond to the URL to call Velodoc XP Edition 
        public const string File0 = "D:\\TEST\\Files\\10mb.bin"; //These files need to exist to be uploaded using IE automation
        public const string File1 = "D:\\TEST\\Files\\20mb.bin";
        public const string File2 = "D:\\TEST\\Files\\30mb.bin";
        public const string SecurityCode = "12345"; //This security code is configured in web.config
        #endregion

        #region XPFileBrokerTest
        public const string Directory = "D:\\STORAGE"; //File storage directory as configured in app.config and web.config
        public const string BadDirectory = "Z:\\zzzzzzzz";  //Hopefully, this directory does not exist
        #endregion

        #region XPMessageTest
        //All required constants are in region UITests
        #endregion

        #region WebInstallerTest
        public const string Port = "82";
        public const string ServerNum = "1"; //Check server bindings in IIS
        public const string VirtualDir = "VelodocXP"; //You need this virtual dir in IIS in order to run the tests
        #endregion

        #region wcfTransferServiceTest
        //Most required constants are in region UITests
        public const string IISWebSite = "http://localhost/XP/"; //This actually need to correspond to the URL to call Velodoc XP Edition 
        #endregion
    }
}
