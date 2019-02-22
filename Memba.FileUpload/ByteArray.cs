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
    internal static class ByteArray
    {
        //We need to look at optimized algorithms
        //1) Karp-Rabin
        //2) Knuth-Morris-Pratt
        //3) Boyer-Moore (preferred) --> http://msdn2.microsoft.com/en-us/library/microsoft.web.services3.mime.bmmatcher.aspx

        //Searching for "Hello World" in the middle of a 8192-byte buffer 100 000 times takes 0.62 seconds with BMMatcher and 0.87 seconds with ByteArray
        //in the VS debugger on a Pentium M 1.5GHz. See: $\recorWeb.v1\Prototypes\ByteArray
        //There is not much to win here. Not worth the trouble modifying the code.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arraySearchedIn">The data to be searched.</param>
        /// <param name="arraySearchedFor">The value to search for</param>
        /// <param name="startIndex">Where to start the search</param>
        /// <param name="count">The number of bytes to search</param>
        /// <returns>The index location in the buffer where the match is found.</returns>
        public static int IndexOf(byte[] arraySearchedIn, byte[] arraySearchedFor, int startIndex, int count)
        {
            if ((arraySearchedIn == null) || (arraySearchedFor == null) || (startIndex + arraySearchedFor.Length > arraySearchedIn.Length) || (count == 0))
                return -1;
            
            System.Diagnostics.Debug.Assert(startIndex >= 0);
            System.Diagnostics.Debug.Assert(count > 0);
            System.Diagnostics.Debug.Assert(startIndex + count <= arraySearchedIn.Length);

            int iPos = startIndex;
            byte yFirstByte = arraySearchedFor[0];
            byte[] arrPotentialMatch = new byte[arraySearchedFor.Length];
            bool bFound = false;

            //As long as a potential objMatch has not been found and there is still enough bytes to search in
            //while (!bFound && (iPos >= startIndex) && ((startIndex + count - iPos) >= arraySearchedFor.Length))
            while (!bFound && (iPos >= 0) && (iPos <= startIndex + count - arraySearchedFor.Length))
            {
                //Find the first byte to get a potential objMatch
                iPos = Array.IndexOf<byte>(arraySearchedIn, yFirstByte, iPos, startIndex + count - iPos);
                //If found try a potential objMatch for arraySearchedFor with the following sequence
                if ((iPos >= 0) && (iPos <= startIndex + count - arraySearchedFor.Length))
                {
                    System.Diagnostics.Debug.Assert(iPos >= startIndex);
                    Buffer.BlockCopy(arraySearchedIn, iPos, arrPotentialMatch, 0, arraySearchedFor.Length);
                    bFound = CompareByteArrays(arraySearchedFor, arrPotentialMatch);
                    if (!bFound)
                        iPos++;
                }
            }
            
            //return the matching position
            if (bFound)
                return iPos;
            else
                return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>TODO: This code may be optimized using bitwise XOR to compare two byte arrays
        /// See: http://blogs.msdn.com/juane/archive/2004/09/20/232218.aspx</remarks>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            //This code is executed thousands of times and we can safely assume
            System.Diagnostics.Debug.Assert(array1 != null);
            System.Diagnostics.Debug.Assert(array2 != null);

            /*
            // If both are null, they're equal
            if (array1 == null && array2 == null)
            {
                return true;
            }
            // If either but not both are null, they're not equal
            if (array1 == null || array2 == null)
            {
                return false;
            }
            */
            
            //If they do not have the same length they are not equal
            if (array1.Length != array2.Length)
            {
                return false;
            }
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
