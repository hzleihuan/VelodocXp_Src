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
using System.Text;
using Memba.FileUpload.Properties;
using System.Threading;

namespace Memba.FileUpload
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>See: http://www.faqs.org/rfcs/rfc1867.html for more information on multipart/form-data.</remarks>
    internal sealed class MimeParser
    {
        #region Private Members
        //private const int THREAD_SLEEP_INCREMENT = 1;
        //private const double LOG_BASE = 2;

        //private static int iInstanceCount;
        //The more instances you have the more the parser will give time for other threads to execute 
        //using Thread.Sleep((int)Math.Floor(n * THREAD_SLEEP_INCREMENT * Math.Log(iInstanceCount + 1, LOG_BASE)));
        
        private RequestFilter _RequestFilter;
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestFilter"></param>
        internal MimeParser(RequestFilter requestFilter)
        {
            _RequestFilter = requestFilter;
            //iInstanceCount++;
        }
        #endregion

        #region Destructor
        //~MimeParser()
        //{
        //    iInstanceCount--;
        //}
        #endregion

        #region Other Members
        /// <summary>
        /// The Parse method which extracts files and forward a request without files
        /// </summary>
        public void Parse()
        {
            //Current Mime block being handled by the parser
            CurrentMimeBlock enuCurrentMimeBlock = CurrentMimeBlock.Unknown;

            //End of request for FlashReference
            string sExpectedEndOfFlashRequest = "Content-Disposition: form-data; name=\"Upload\"\r\nSubmit Query" + _RequestFilter.Encoding.GetString(_RequestFilter.MultiPartBoundary2);

            //buffer receiving the http request
            byte[] arrRequestBuffer = new byte[Constants.BufferSize];
            //use a string and not a StringBuilder to buffer the header because it is only concatenated once or twice
            //when split across two buffers
            string sPartHeadersBuffer = String.Empty;
           
            //sequence of bytes for a CRLF
            byte[] arrLineSeparator = _RequestFilter.Encoding.GetBytes(Constants.LineSeparator);
            //sequence of bytes for a CRLFCRLF
            byte[] arrHeadersFromDataSeparator = _RequestFilter.Encoding.GetBytes(Constants.HeadersFromDataSeparator);
            //sequence of bytes for '--'
            byte[] arrEndOfRequest = _RequestFilter.Encoding.GetBytes(Constants.EndOfRequest);

            //We call the following buffers cut-off buffers.
            byte[] arrHeadersFromDataCutOffBuffer = new byte[2*arrHeadersFromDataSeparator.Length];
            byte[] arrBoundary2CutOffBuffer = new byte[2*_RequestFilter.MultiPartBoundary2.Length];
            //The use of cut-off buffers is simple. Let say we are looking for 'WXYZ' and it is not found in the current arrRequestBuffer.
            //There is a chance that there is 'W', 'WX' or 'WXY' at the end of the buffer and respectively 'XYZ', 'YZ' or 'Z' at the beginning of teh following buffer.
            //If we take the last n bytes of arrRequestBuffer which correspond to 'WXYZ' plus the first n bytes from arrRequestBuffer after reading a new set of bytes
            //and place them in a cut-off buffer, we should find 'WXYZ' in this cut-off buffer if it is split across.
            //Please also note:
            //  From previous buffer |
            //                       | From the following buffer    
            //               [0 1 2 3|0 1 2 3] in the cut-off buffer
            //                  W X Y Z 
            //                  |       |
            //                  iPos = 1 is the position where WXYZ is found in the cut-off buffer
            //                           is also the position from which to start in the following buffer when it is found

            //Number of bytes read into arrRequestBuffer
            int iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length); 
            int iCurrentIndex = 0; //Current position of the parser
            int iPos = -1; //used with IndexOf to find boundaries and other separators

            //First loop splits the request into buffers of size ParsingConstants.BufferSize 
            while (enuCurrentMimeBlock != CurrentMimeBlock.EndOfRequest)
            {   
                //Nested loop iterates on Mime blocks within the buffer
                while (iCurrentIndex < iBytesRead)
                {
                    switch (enuCurrentMimeBlock)
                    {
                        #region Starting Processing
                        case CurrentMimeBlock.Unknown: //Find first boundary
                            {

                                // The first boundary is not contained in the buffer only in the theoretical case where
                                // ParsingConstants.BufferSize is too small to contain the first boundary. Not worth to handle that case.
                                System.Diagnostics.Debug.Assert(iCurrentIndex + _RequestFilter.MultiPartBoundary.Length <= iBytesRead);
                                
                                iPos = ByteArray.IndexOf(arrRequestBuffer, _RequestFilter.MultiPartBoundary, 0, iBytesRead);
                                if (iPos >= 0)
                                {
                                    iCurrentIndex = iPos + _RequestFilter.MultiPartBoundary.Length - 1;
                                    //At this stage, the current index is positioned at the end of the boundary                                    
                                    enuCurrentMimeBlock = CurrentMimeBlock.BoundarySuffix;
                                }
                                else
                                    throw new InvalidDataException(Resources.ExceptionMultipartBoundaryNotFound);
                                break;
                            }
                        #endregion

                        #region Analysing Boundary Suffix
                        case CurrentMimeBlock.BoundarySuffix: //Analyze the boundary suffix which is '\r\n' or '--'
                            {
                                //we only check the first and last byte of the boundary suffix
                                //considering that '\r\n' and '--' take two bytes in UTF8

                                if (iCurrentIndex < iBytesRead - 1) //The first byte is available in the buffer
                                {
                                    iCurrentIndex++; //position iCurrentIndex to the first byte of the boundary suffix
                                }
                                else //we need more data to access the first byte
                                {
                                    iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length);
                                    iCurrentIndex = 0; //position iCurrentIndex to the first byte of the boundary suffix
                                }

                                int iMissingBytes;
                                if (arrRequestBuffer[iCurrentIndex] == arrLineSeparator[0]) //There are other POST fields
                                {
                                    iMissingBytes = arrLineSeparator.Length - 1 - (iBytesRead - iCurrentIndex);
                                    if (iMissingBytes < 0)
                                    {
                                        iCurrentIndex += arrLineSeparator.Length - 1; //position iCurrentIndex to the last byte of the boundary suffix
                                    }
                                    else
                                    {
                                        iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length);
                                        iCurrentIndex = iMissingBytes; //position iCurrentIndex to the last byte of the boundary suffix
                                    }
                                    System.Diagnostics.Debug.Assert(
                                        (iCurrentIndex >= 0) && (iCurrentIndex < arrRequestBuffer.Length)
                                        && (arrRequestBuffer[iCurrentIndex] == arrLineSeparator[arrLineSeparator.Length - 1]));
                                    enuCurrentMimeBlock = CurrentMimeBlock.PartHeaders;
                                }
                                else if (arrRequestBuffer[iCurrentIndex] == arrEndOfRequest[0]) //This is the end of request
                                {
                                    iMissingBytes = arrLineSeparator.Length - 1 - (iBytesRead - iCurrentIndex);
                                    if (iMissingBytes < 0)
                                    {
                                        iCurrentIndex += arrLineSeparator.Length - 1; //position iCurrentIndex to the last byte of the boundary suffix
                                    }
                                    else
                                    {
                                        iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length);
                                        iCurrentIndex = iMissingBytes; //position iCurrentIndex to the last byte of the boundary suffix
                                    }
                                    System.Diagnostics.Debug.Assert(
                                        (iCurrentIndex >= 0) && (iCurrentIndex < arrRequestBuffer.Length)                                       
                                        && (arrRequestBuffer[iCurrentIndex] == arrEndOfRequest[arrEndOfRequest.Length - 1]));
                                    iCurrentIndex += arrLineSeparator.Length;  //position iCurrentIndex to the line after the boundary suffix for an end of request
                                    //System.Diagnostics.Debug.Assert(iCurrentIndex == iBytesRead - 1); //Make sure this is the end of buffer
                                    //We had to replace the line above because of Flash. The problem is not systematic, but occurs from Vista with our gadget
                                    System.Diagnostics.Debug.Assert(iCurrentIndex >= iBytesRead - 1); //Make sure this is the end of buffer
                                    _RequestFilter.ProcessEndOfRequest();
                                    iCurrentIndex++; //exits the inner loop
                                    enuCurrentMimeBlock = CurrentMimeBlock.EndOfRequest; //exits the outer loop
                                }
                                break;
                            }
                        #endregion

                        #region Processing Headers
                        case CurrentMimeBlock.PartHeaders:
                            {
                                if (iCurrentIndex < iBytesRead - 1) //The first byte is available in the buffer
                                {
                                    iCurrentIndex++; //position iCurrentIndex to the first byte of the header
                                }
                                else //we need more data to access the first byte
                                {
                                    iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length);
                                    iCurrentIndex = 0; //position iCurrentIndex to the first byte of the header
                                }
                                
                                //Search for arrHeadersFromDataSeparator
                                iPos = ByteArray.IndexOf(arrRequestBuffer, arrHeadersFromDataSeparator, iCurrentIndex, iBytesRead - iCurrentIndex);

                                if (iPos >=0)  //arrHeadersFromDataSeparator is found in the current buffer
                                {
                                    sPartHeadersBuffer += _RequestFilter.Encoding.GetString(arrRequestBuffer, iCurrentIndex, iPos - iCurrentIndex);
                                    _RequestFilter.ProcessPartHeaders(sPartHeadersBuffer);
                                    sPartHeadersBuffer = String.Empty;
                                    iCurrentIndex = iPos + arrHeadersFromDataSeparator.Length - 1; //position iCurrentIndex to the last byte of the headers from data separator 
                                    enuCurrentMimeBlock = CurrentMimeBlock.PartData;
                                }
                                else //arrHeadersFromDataSeparator is not found in the current buffer but it could be split
                                {
                                    //We have to make an exception because Flash produces an invalid multipart request
                                    //Not to have to make this exception, the following is required in sExpectedEndOfFlashRequest:
                                    //  1) Separate [name="upload"] from [Submit Query] by [\r\n\r\n] and not [\r\n]
                                    //  2) End request with [--] after the multi part boundary (note that some requests have it????)
                                    string sActualEndOfFlashRequest = _RequestFilter.Encoding.GetString(arrRequestBuffer, iCurrentIndex, iBytesRead - iCurrentIndex);
                                    if (sActualEndOfFlashRequest.Equals(sExpectedEndOfFlashRequest))
                                    {
                                        //Then this is the end of a FlashReference upload request and we are done!
                                        enuCurrentMimeBlock = CurrentMimeBlock.EndOfRequest;
                                        iCurrentIndex = iBytesRead; //Otherwise we create an endless loop
                                        _RequestFilter.ProcessEndOfRequest();
                                    }
                                    else //This is the normal course of action when we are not dealing with FlashReference
                                    {
                                        int iCutOffOffset = -(iBytesRead - iCurrentIndex - arrHeadersFromDataSeparator.Length);
                                        //Get the header till the end of the buffer minus the size of arrHeadersFromDataSeparator
                                        //unless iCurrentIndex > iBytesRead - arrHeadersFromDataSeparator.Length 
                                        if (iCutOffOffset < 0) //if positive, we have already processed bytes which are included in the cut-off buffer
                                        {
                                            sPartHeadersBuffer += _RequestFilter.Encoding.GetString(arrRequestBuffer, iCurrentIndex, -iCutOffOffset);
                                            iCutOffOffset = 0;
                                        }

                                        //use arrHeadersFromDataCutOffBuffer as a cut-off buffer
                                        Buffer.BlockCopy(arrRequestBuffer, iBytesRead - arrHeadersFromDataSeparator.Length, arrHeadersFromDataCutOffBuffer, 0, arrHeadersFromDataSeparator.Length);
                                        //Read more data
                                        iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length);
                                        //iCurrentIndex = 0; //position iCurrentIndex to the first byte of the header
                                        Buffer.BlockCopy(arrRequestBuffer, 0, arrHeadersFromDataCutOffBuffer, arrHeadersFromDataSeparator.Length, arrHeadersFromDataSeparator.Length);
                                        iPos = ByteArray.IndexOf(arrHeadersFromDataCutOffBuffer, arrHeadersFromDataSeparator, iCutOffOffset, arrHeadersFromDataCutOffBuffer.Length - iCutOffOffset);
                                        if (iPos >= 0) //it is split
                                        {
                                            System.Diagnostics.Debug.Assert(iPos >= iCutOffOffset);
                                            sPartHeadersBuffer += _RequestFilter.Encoding.GetString(arrHeadersFromDataCutOffBuffer, iCutOffOffset, iPos - iCutOffOffset);
                                            _RequestFilter.ProcessPartHeaders(sPartHeadersBuffer);
                                            sPartHeadersBuffer = String.Empty;
                                            iCurrentIndex = iPos - 1; //position iCurrentIndex to the last byte processed
                                            enuCurrentMimeBlock = CurrentMimeBlock.PartData;
                                        }
                                        else //it is not split
                                        {
                                            sPartHeadersBuffer += _RequestFilter.Encoding.GetString(arrHeadersFromDataCutOffBuffer, iCutOffOffset, arrHeadersFromDataSeparator.Length - iCutOffOffset);
                                            iCurrentIndex = -1; //if we want to position iCurrentIndex to the last byte processed we need -1 so that it starts at 0 during next round
                                            //Loop with enuCurrentMimeBlock = CurrentMimeBlock.PartHeaders
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Processing Part Data
                        case CurrentMimeBlock.PartData:
                            {
                                if (iCurrentIndex < iBytesRead - 1) //The first byte is available in the buffer
                                {
                                    iCurrentIndex++; //position iCurrentIndex to the first byte of the data
                                }
                                else //we need more data to access the first byte
                                {
                                    iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length);
                                    iCurrentIndex = 0; //position iCurrentIndex to the first byte of the data
                                }

                                //Search for the next boundary
                                //we use MultiPartBoundary2 which is a boundary prefixed with CRLF
                                iPos = ByteArray.IndexOf(arrRequestBuffer, _RequestFilter.MultiPartBoundary2, iCurrentIndex, iBytesRead - iCurrentIndex);

                                if (iPos >= 0)  //a new boundary is found in the current buffer
                                {
                                    _RequestFilter.ProcessPartData(ref arrRequestBuffer, iCurrentIndex, iPos - iCurrentIndex);
                                    iCurrentIndex = iPos + _RequestFilter.MultiPartBoundary2.Length - 1; //position iCurrentIndex to the last byte of the boundary 
                                    _RequestFilter.ProcessEndOfPart();
                                    enuCurrentMimeBlock = CurrentMimeBlock.BoundarySuffix;
                                }
                                else //no boundary can be found in the current buffer but it could be split
                                {
                                    //Check the presence of the first byte of _RequestFilter.MultiPartBoundary2 in the last bytes read                                   
                                    iPos = Array.IndexOf<Byte>(arrRequestBuffer, _RequestFilter.MultiPartBoundary2[0], iBytesRead - _RequestFilter.MultiPartBoundary2.Length);

                                    if (iPos < 0) //It is not split because there is not even the first byte at the end of the current buffer
                                    {
                                        _RequestFilter.ProcessPartData(ref arrRequestBuffer, iCurrentIndex, iBytesRead - iCurrentIndex);
                                        iCurrentIndex = iBytesRead - 1;
                                    }
                                    else //There is the first byte of a boundary at the end of the current buffer, so we need a cut-off buffer
                                    {
                                        //iCutOffOffset is the length of data to process before reaching the data making the first half of the cut-off buffer
                                        int iCutOffOffset = -(iBytesRead - iCurrentIndex - _RequestFilter.MultiPartBoundary2.Length);
                                        
                                        //Process the data till the end of the buffer minus the size of the boundary unless iCurrentIndex > iBytesRead - _RequestFilter.MultiPartBoundary2.Length;
                                        if (iCutOffOffset < 0) //if positive, we have already processed bytes which are included in the cut-off buffer
                                        {
                                            //Process up to the cutoff buffer
                                            _RequestFilter.ProcessPartData(ref arrRequestBuffer, iCurrentIndex, -iCutOffOffset);
                                            iCutOffOffset = 0;
                                        }
                                        //iCutOffOffset is now where to start searching for the boundary in the cutoff buffer
                                        //use arrBoundary2CutOffBuffer as a cut-off buffer
                                        Buffer.BlockCopy(arrRequestBuffer, iBytesRead - _RequestFilter.MultiPartBoundary2.Length, arrBoundary2CutOffBuffer, 0, _RequestFilter.MultiPartBoundary2.Length);
                                        //Read more data
                                        iBytesRead = _RequestFilter.RequestStream.Read(arrRequestBuffer, 0, arrRequestBuffer.Length);
                                        //iCurrentIndex = 0; //position iCurrentIndex to the first byte of the header
                                        Buffer.BlockCopy(arrRequestBuffer, 0, arrBoundary2CutOffBuffer, _RequestFilter.MultiPartBoundary2.Length, _RequestFilter.MultiPartBoundary2.Length);
                                        iPos = ByteArray.IndexOf(arrBoundary2CutOffBuffer, _RequestFilter.MultiPartBoundary2, 0, arrBoundary2CutOffBuffer.Length);
                                        if (iPos >= 0) //it is split
                                        {
                                            System.Diagnostics.Debug.Assert(iPos >= iCutOffOffset);
                                            _RequestFilter.ProcessPartData(ref arrBoundary2CutOffBuffer, iCutOffOffset, iPos - iCutOffOffset);
                                            iCurrentIndex = iPos - 1; //position iCurrentIndex to the last byte processed
                                            _RequestFilter.ProcessEndOfPart();
                                            enuCurrentMimeBlock = CurrentMimeBlock.BoundarySuffix;
                                        }
                                        else //it is not split
                                        {
                                            _RequestFilter.ProcessPartData(ref arrBoundary2CutOffBuffer, iCutOffOffset, _RequestFilter.MultiPartBoundary2.Length - iCutOffOffset);
                                            iCurrentIndex = -1; //if we want to position iCurrentIndex to the last byte processed we need -1 so that it starts at 0 during next round
                                            //Loop with enuCurrentMimeBlock = CurrentMimeBlock.PartData
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion
                    }

                    //Give room for other threads to execute
                    //(int)Math.Floor(THREAD_SLEEP_INCREMENT * Math.Log(iInstanceCount + 1, LOG_BASE)));
                }
            }
        }

        #endregion

        private enum CurrentMimeBlock
        {
            Unknown,            // before starting processing
            BoundarySuffix,     //'\r\n' or '--' after a boundary
            PartHeaders,
            PartData,
            EndOfRequest
        }
    }
}

