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
using System.Collections.Generic; //List<T>
using System.Text; //StringBuilder
using System.Net.Mail; //Message, SmtpCLient
using System.Web.Configuration;
using Memba.Dns; //Resolver, Request, Response

namespace Memba.Files.Business
{
    public class Message
    {
        #region private members
        private string _Sender;
        private string _Recipient;
        private string _Text;
        private List<File> _Attachments;
        private string _DownloadRoot;

        private static Nullable<bool> _EnableTLS = null;
        private static Nullable<bool> _AddSenderToBcc = null;
        private static Nullable<bool> _GetReadReceipt = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Only constructor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <param name="text"></param>
        /// <param name="attachments"></param>
        /// <param name="downloadRoot"></param>
        public Message(
            string sender,
            string recipient,
            string text,
            List<File> attachments,
            string downloadRoot
        )
        {
            //TODO: Validate email addresses
            _Sender = sender;
            _Recipient = recipient;
            _Text = text;
            _Attachments = attachments;
            _DownloadRoot = downloadRoot;
        }
        #endregion

        #region Other Methods
        /// <summary>
        /// Send method
        /// </summary>
        public void Send()
        {
            //There is a complex trade-off to consider here:
            //option 1 is using the email of the sender as the from address of the notification
            // - benefit: the sender receives delivery notifications (DSN), for example when the recipient does not exist becuase it is misspelled
            // - drawbacks: less secure from the SMTP server perspective and messages are identified as spam on servers which check SPF DNS records
            //option 2 is using a website@domain.tld address defined in web.config as the from address of all notifications
            // - benefits and drawbacks are exactly the contrary of option 1
            // - the DSN issue in option 2 can be partially solved with checking MX records

            //Addresses
            if (!ValidateMXRecord(_Sender))
                throw new ApplicationException(String.Format(Properties.Resources.BLExceptionMXRecordNotFound, _Sender));
            if (!ValidateMXRecord(_Recipient))
                throw new ApplicationException(String.Format(Properties.Resources.BLExceptionMXRecordNotFound, _Recipient));

            //Read application settings configuration
            if (!_EnableTLS.HasValue)
            {
                bool bEnableTLS = false;
                Boolean.TryParse(WebConfigurationManager.AppSettings["EnableTLS"], out bEnableTLS);
                _EnableTLS = bEnableTLS;
                System.Diagnostics.Debug.Assert(_EnableTLS.HasValue);
            }
            if (!_AddSenderToBcc.HasValue)
            {
                bool bAddSenderToBcc = false;
                Boolean.TryParse(WebConfigurationManager.AppSettings["AddSenderToBcc"], out bAddSenderToBcc);
                _AddSenderToBcc = bAddSenderToBcc;
                System.Diagnostics.Debug.Assert(_AddSenderToBcc.HasValue);
            }
            if (!_GetReadReceipt.HasValue)
            {
                bool bGetReadReceipt = false;
                Boolean.TryParse(WebConfigurationManager.AppSettings["GetReadReceipt"], out bGetReadReceipt);
                _GetReadReceipt = bGetReadReceipt;
                System.Diagnostics.Debug.Assert(_GetReadReceipt.HasValue);
            }
            
            //Create new System.Net.Mail message object
            MailMessage objMailMessage = new MailMessage();
            
            //Use the from address configured in web.config (computer) as the sender
            //objMailMessage.Sender = objMailMessage.From;
            //The mail is sent by the sender (computer) on behalf of the from address (user) 
            //objMailMessage.From = new MailAddress(_Sender);
            //This way mail servers can perform a reverse DNS lookup to check the PTR record
            //of the IP address sending the email and find the domain of the sender
            //They can also check SPF records and DomainKeys if appropriate

            //But apparently some mail servers check the SMTP from and not the SMTP sender
            //so the above does not escape spam filters and the from address has to be the computer

            //Make sure replying to an email does not reply to the computer address but the user address
            objMailMessage.ReplyTo = new MailAddress(_Sender);
            
            //Add recipients
            objMailMessage.To.Add(new MailAddress(_Recipient));
            if(_AddSenderToBcc.Value)
                objMailMessage.Bcc.Add(new MailAddress(_Sender));
            
            //It is sure a good thing to get a read receipt
            if (_GetReadReceipt.Value)
            {
                objMailMessage.Headers.Add("Return-Receipt-To", _Sender); //the old way
                objMailMessage.Headers.Add("Disposition-Notification-To", _Sender); //the new way
            }

            //Note: setting "Return-Path" ro receive delivery status notifications is useless
            //because it is overwritten by the SMTP server with the sender address 
            //So we need to forward delivery status notifications (DSN) with an SMTP transport event sink
            //which requires setting a custom header to let our event sink know to whom DSNs should be forwarded
            objMailMessage.Headers.Add("X-Forward-DSN-To", _Sender); //this is used by our SMTP event sink to forward DSN

            //Subject
            objMailMessage.Subject = String.Format(
                Properties.Resources.Culture,
                Properties.Resources.Message_Subject,
                _Sender);

            //Body
            StringBuilder objStringBuilder = new StringBuilder();
            objStringBuilder.AppendLine(BODisplay.RemoveAllHtmlTags(_Text));
            objStringBuilder.AppendLine(Properties.Resources.Message_FileListHeader);
            foreach (File f in _Attachments)
            {
                string sDownloadUrl = System.IO.Path.Combine(_DownloadRoot, f.Guid.ToString("N") + ".dat");
                objStringBuilder.AppendFormat(
                    Properties.Resources.Culture,
                    Properties.Resources.Message_FileListItem,
                    f.FileName,
                    BODisplay.SizeFormat(f.Size, ByteFormat.Adapt),
                    sDownloadUrl);
                objStringBuilder.AppendLine();
            }
            objStringBuilder.AppendLine(Properties.Resources.Message_FileListFooter);
            objMailMessage.IsBodyHtml = false;
            objMailMessage.Body = objStringBuilder.ToString();

            //Send using settings in web.config
            SmtpClient objSmtpClient = new SmtpClient();
            objSmtpClient.EnableSsl = _EnableTLS.Value;
            objSmtpClient.Send(objMailMessage);
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Checks that the domain name of an email address has an MX record designating a mail server
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private bool ValidateMXRecord(string email)
        {
            bool bIsValidRet = true;
            try
            {
                //We know we have a valid email address here
                //since we have already validated it using a regular expression
                int iPos = email.IndexOf('@');
                System.Diagnostics.Debug.Assert(iPos > -1);
                string sDomainName = email.Substring(iPos + 1);
                Request objRequest = new Request();
                objRequest.AddQuestion(new Question(sDomainName, DnsType.MX, DnsClass.IN));
                Response objResponse = Resolver.Lookup(objRequest);
                bIsValidRet =((objResponse != null)
                    && (objResponse.ReturnCode == ReturnCode.Success)
                    && (objResponse.Answers.Length > 0)
                    && (objResponse.Answers[0].Record is MXRecord));
            }
            catch
            {
                //If for any reason including a firewall, DNS requests fail
                //We shall not block the process
            }
            return bIsValidRet;
        }
        #endregion
    }
}
