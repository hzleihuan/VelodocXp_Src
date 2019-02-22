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

namespace Memba.FileUpload
{
    /// <summary>
    /// These constants are shared amongst RequestFilter and MimeParser
    /// </summary>
    public static class Constants
    {
        //internal const int BufferSize = 2048;
        //internal const int BufferSize = 4096;  //----------> 2.8 Mb/s in Dev Env |||| 12.9 Mb/s in IIS
        //internal const int BufferSize = 8192;  //----------> 4.6 Mb/s in Dev Env - 4.0 |||| 14.8 Mb/s in IIS
        internal const int BufferSize = 9216;  //----------> 4.9 Mb/s in Dev Env - 4.8 - 4.3 |||| 15.4 Mb/s in IIS
        //internal const int BufferSize = 10240;  //----------> 4.7 Mb/s in Dev Env - 4.9 |||| 15.4 Mb/s in IIS
        //internal const int BufferSize = 12288; //----------> 4.7 Mb/s in Dev Env - 4.9 |||| 14.0 Mb/s in IIS
        //internal const int BufferSize = 16384; //----------> 3.4 Mb/s in Dev Env - 4.8
        //internal const int BufferSize = 32768; //----------> 3.2 Mb/s in Dev Env
        //internal const int BufferSize = 49152;
        //internal const int BufferSize = 65536;
        internal const int StringBuilderCapacity = 128;
        internal const string MultiPartFormData = "multipart/form-data";
        internal const string MultiPartBoundary = "boundary=";
        internal const string MultiPartName = "name";
        internal const string MultiPartFilename = "filename";
        internal const string MultiPartContentType = "contenttype"; //and not content-type because hiphen is not supported in regex group names
        internal const string BoundaryPrefix = "--";
        internal const string EndOfRequest = "--";
        internal const string LineSeparator = "\r\n";
        internal const string HeadersFromDataSeparator = "\r\n\r\n";

        // See http://msdn2.microsoft.com/en-US/library/system.security.cryptography.cryptoconfig.aspx
        // For possible values for HashAlgorithName
        internal const string HashAlgorithmName = "SHA1"; //SHA1 by default
        
        public const string HttpRuntimeSection = "system.web/httpRuntime";
        public const string UploadRuntimeSection = "system.web/uploadRuntime";
        public const short Kilo = 1024; //0x400
        public const long DefaultMaxRequestLengthBytes = 4194304L; // = 0x400000L = 4096 KB is the default request length in IIS
        
        //We should normally use 2147483647L = 0x7FFFFFFF = Int32.MaxValue for TopMaxRequestLengthBytes
        //but we have experienced that in VISTA, IE7 reports to IIS 7 a content length of 2140709491
        //when trying to upload a file greater than 6GB, so we should top at 2140709491 - 1
        public const long TopMaxRequestLengthBytes = 2140709490L;
        
        //For iis7
        public const long DefaultMaxAllowedContentLengthBytes = 30000000L; //For iis7, see: system.webServer/security/requestFiltering/requestLimits

        internal const int SlidingExpiration = 20; //Cache sliding expiration 
    }
}
