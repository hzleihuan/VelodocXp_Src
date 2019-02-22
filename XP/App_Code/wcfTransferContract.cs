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
using System.ServiceModel;

namespace Memba.Files.WebServices
{
    /// <summary>
    /// The service contract
    /// </summary>
    [ServiceContract(Namespace = Memba.Common.Presentation.Constants.FilesWebServicesNamespace)]
    public interface ITransferService
    {
        [OperationContract(IsOneWay = true)]
        void Upload(RemoteFileDescriptor request);

        [OperationContract]
        //RemoteFileDescriptor Download(Guid fileGuid); //<-- Does not work - see below
        RemoteFileDescriptor Download(RemoteFileIdentifier request);

        [OperationContract]
        long GetFileSize(Uri remoteFile);

        [OperationContract]
        bool Authenticate(string email, string securityCode);
    }
    /// <summary>
    /// The message contract for a remote file descriptor
    /// </summary>
    /// <remarks>Message contracts are documented at http://msdn2.microsoft.com/en-us/library/ms732038.aspx</remarks>
    [MessageContract]
    public class RemoteFileDescriptor
    {
        [MessageHeader(MustUnderstand = true)]
        public Guid FileGuid;

        [MessageHeader(MustUnderstand = true)]
        public string FileName;

        [MessageHeader(MustUnderstand = true)]
        public string ContentType;

        [MessageHeader(MustUnderstand = true)]
        public string HashCode;

        [MessageHeader(MustUnderstand = true)]
        public long Length;

        //It would have made more sense to have Email and SecurityCode as OperationContract parameters
        //But in order to provide streaming the OperationContract can have only one MessageContract
        //where the Stream is the body member and all other paraneters are passed as message headers
        //Email and SecurityCode are null when RemoteFileDescriptor is returned by the Download method
        [MessageHeader(MustUnderstand = true)]
        public string Email;

        [MessageHeader(MustUnderstand = true)]
        public string SecurityCode;

        //Note: the stream is the only body member
        //See: http://msdn2.microsoft.com/en-us/library/ms789010.aspx
        [MessageBodyMember(Order = 1)]
        public Stream Stream;
    }
    /// <summary>
    /// The message contract for a remote file identifier
    /// </summary>
    [MessageContract]
    public class RemoteFileIdentifier
    {
        //We need this contract, because if we pass directly the Uri as paramater of the Download
        //method we get the following exception:
        //The operation 'Download' could not be loaded because it has a parameter or return type
        //of type System.ServiceModel.Channels.Message or a type that has MessageContractAttribute
        //and other parameters of different types. When using System.ServiceModel.Channels.Message
        //or types with MessageContractAttribute, the method must not use any other types of parameters. 
        [MessageBodyMember]
        public Uri RemoteFile;
    }
}