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
using System.Data;
using System.Configuration;
using System.Collections.Generic; //List<T>
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Web.Configuration; //WebConfigurationManager
using System.Web.Caching; //Cache
using System.Text.RegularExpressions; //Regex
using System.Text; //StringBuilder

using Memba.Files.Business; //BODisplay
using Memba.FileUpload; //UploadMonitor, UploadData
using Memba.FileUpload.Providers; //FileStorage
using Memba.WebControls; //InfoBox;

public enum DisplayFormat
{
    QuickSend,
    DropBox
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// IMPORTANT: Make sure the ScriptReference or ScriptReferenceProxy control on the page (or master) which hosts the quickmessage user control has
/// - Script references to Memba.Utils and Memba.Infragistics, and
/// - Service references to uploadWebService.asmx
/// </remarks>
public partial class App_CommonCtrls_quickmessage : System.Web.UI.UserControl
{
    #region Private Members
    private Unit _Height;
    private Unit _Width;
    private DisplayFormat _DisplayFormat;
    private string _WelcomeInfo;
    private InfoBox _InfoBox;
    #endregion

    #region Property Accessors
    public Unit Height
    {
        get { return _Height; }
        set { _Height = value; }
    }
    public Unit Width
    {
        get { return _Width; }
        set { _Width = value; }
    }
    public DisplayFormat DisplayFormat
    {
        get { return _DisplayFormat; }
        set { _DisplayFormat = value; }
    }
    public string WelcomeInfo
    {
        get { return _WelcomeInfo; }
        set { _WelcomeInfo = value; }
    }
    public InfoBox InfoBox
    {
        get { return _InfoBox; }
        set { _InfoBox = value; }
    }
    protected string InfoBoxClientID
    {
        get { return (_InfoBox == null) ? String.Empty : _InfoBox.ClientID; }
    }
    #endregion

    #region Page LifeCycle Events
    /// <summary>
    /// Page Load event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load(object sender, EventArgs e)
    {
        //Sets the height and width
        if (this._Height != Unit.Empty)
        {
            SendFilePanel.Height = this._Height;
            SendProgressPanel.Height = this._Height;
            SendFinalPanel.Height = this._Height;
        }
        if (this._Width != Unit.Empty)
        {
            SendFilePanel.Width = this._Width;
            SendProgressPanel.Width = this._Width;
            SendFinalPanel.Width = this._Width;
        }

        //We not only need to check whether it is a postback but also if the URL contains a muid 
        if (Page.IsPostBack && !String.IsNullOrEmpty(Request.QueryString[UploadMonitor.UploadIdParam]))
        {
            //Manage panel visibility
            SendFilePanel.Visible = false;
            //See comment below about MissingTagWorkaroundLiteral
            //Note that the issue does not occur in this context, so the fix hidden
            MissingTagWorkaroundLiteral.Visible = false;
            SendProgressPanel.Visible = false;
            SendFinalPanel.Visible = true;

            //Display description on final panel
            string sFileMaxAge = (string)HttpRuntime.Cache[Memba.Common.Presentation.Constants.FileMaxAge];
            if (String.IsNullOrEmpty(sFileMaxAge))
            {
                sFileMaxAge = ConfigurationManager.AppSettings[Memba.Common.Presentation.Constants.FileMaxAge];
                HttpRuntime.Cache.Add(
                    Memba.Common.Presentation.Constants.FileMaxAge,
                    sFileMaxAge,
                    null,
                    Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, Memba.Common.Presentation.Constants.SlidingExpiration, 0),
                    CacheItemPriority.Default,
                    null);
            }

            FinalDescriptionLabel.Text = String.Format(
                Resources.Web.glossary.Culture,
                Resources.Web.glossary.QuickMessageControl_SendFinalDescription,
                sFileMaxAge
            );
        }
        else
        {
            //Display InfoBox welcome message
            if (_InfoBox != null)
            {
                _InfoBox.Type = InfoBoxType.Information;
                _InfoBox.Text = _WelcomeInfo;
            }
            
            //Manage panel visibility
            SendFilePanel.Visible = true;
            //For an unexplained reason (ASP.NET bug?) </asp:Panel> should be transcripted into </div>,
            //but this does not happen and the SendProgressPanel gets included into the SendFilePanel
            MissingTagWorkaroundLiteral.Visible = true;
            SendProgressPanel.Visible = true;
            SendFinalPanel.Visible = false;

            //Load recipient list for drop box
            if (this.DisplayFormat == DisplayFormat.DropBox)
            {
                SenderTextBox.Visible = true;
                SenderDropDownList.Visible = false;
                RecipientTextBox.Visible = false;
                RecipientDropDownList.Visible = true;
                string sRecipientList = (string)HttpRuntime.Cache[Memba.Common.Presentation.Constants.UserList];
                if (String.IsNullOrEmpty(sRecipientList))
                {
                    sRecipientList = ConfigurationManager.AppSettings[Memba.Common.Presentation.Constants.UserList];
                    HttpRuntime.Cache.Add(
                        Memba.Common.Presentation.Constants.UserList,
                        sRecipientList,
                        null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, Memba.Common.Presentation.Constants.SlidingExpiration, 0),
                        CacheItemPriority.Default,
                        null);
                }
                string[] arrRecipients = sRecipientList.Split(
                    new char[] { ' ', ';', ',' },
                    Memba.Common.Presentation.Constants.UserListSize,
                    StringSplitOptions.RemoveEmptyEntries);
                Array.Sort<string>(arrRecipients);
                RecipientDropDownList.DataSource = arrRecipients;
                RecipientDropDownList.DataBind();
            }
            //Load sender list for quick send
            else
            {
                SenderTextBox.Visible = false;
                SenderDropDownList.Visible = true;
                string sSenderList = (string)HttpRuntime.Cache[Memba.Common.Presentation.Constants.UserList];
                if (String.IsNullOrEmpty(sSenderList))
                {
                    sSenderList = ConfigurationManager.AppSettings[Memba.Common.Presentation.Constants.UserList];
                    HttpRuntime.Cache.Add(
                        Memba.Common.Presentation.Constants.UserList,
                        sSenderList,
                        null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, Memba.Common.Presentation.Constants.SlidingExpiration, 0),
                        CacheItemPriority.Default,
                        null);
                }
                string[] arrSenders = sSenderList.Split(
                    new char[] { ' ', ';', ',' },
                    Memba.Common.Presentation.Constants.UserListSize,
                    StringSplitOptions.RemoveEmptyEntries);
                Array.Sort<string>(arrSenders);
                SenderDropDownList.DataSource = arrSenders;
                SenderDropDownList.DataBind();
                RecipientTextBox.Visible = true;
                RecipientDropDownList.Visible = false;
            }

            //Diplay upload file size
            string sSendStep3Text = String.Format(
                Resources.Web.glossary.Culture,
                Resources.Web.glossary.QuickMessageControl_SendStep3,
                this.GetMaxFileSize());

            SendStep3Image.AlternateText = sSendStep3Text;
            SendStep3Label.Text = sSendStep3Text;

            //Display terms checkbox for drop box
            if (this.DisplayFormat == DisplayFormat.DropBox)
            {
                AcceptTermsCheckBox.Visible = true;
                SecurityCodeLabel.Visible = false;
                SecurityCodeTextBox.Visible = false;
            }
            //Display security code for quick send
            else
            {
                AcceptTermsCheckBox.Visible = false;
                SecurityCodeLabel.Visible = true;
                SecurityCodeTextBox.Visible = true;
            }

            //Progress bar interval and timeout
            ProgressReport.Interval = Memba.Common.Presentation.Constants.RecurringProgress;
            ProgressReport.Timeout = Memba.Common.Presentation.Constants.AjaxTimeOut;
        }
    }
    #endregion

    #region Control Commands
    /// <summary>
    /// Click event handler for the Send button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void SendButton_Click(object sender, EventArgs e)
    {
        try
        {
            //Get posted values
            string sSender = SenderTextBox.Text.Trim().ToLowerInvariant();
            string sRecipient = String.Empty;
            if (this._DisplayFormat == DisplayFormat.DropBox)
            {
                sSender = SenderTextBox.Text.Trim().ToLowerInvariant();
                sRecipient = RecipientDropDownList.SelectedValue;
            }
            else
            {
                sSender = SenderDropDownList.SelectedValue;
                sRecipient = RecipientTextBox.Text.Trim().ToLowerInvariant();
            }
            string sText = MessageTextBox.Text; //.Trim();

            //Confirm that we have a muid to access upload data
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(MuidHiddenField.Value) && (MuidHiddenField.Value == Request.QueryString[UploadMonitor.UploadIdParam]));

            //We could access it from Url, but this confirms it is a genuine post 
            UploadData objUploadData = UploadMonitor.GetUploadData(MuidHiddenField.Value);
            if (objUploadData == null)
                throw new ApplicationException(Resources.Web.glossary.QuickMessageControl_MissingUploadData);

            //Recheck security code
            if(_DisplayFormat == DisplayFormat.QuickSend)
            {
                string sSecurityCode = (string)HttpRuntime.Cache[Memba.Common.Presentation.Constants.SecurityCode];
                if (String.IsNullOrEmpty(sSecurityCode))
                {
                    sSecurityCode = WebConfigurationManager.AppSettings[Memba.Common.Presentation.Constants.SecurityCode];
                    HttpRuntime.Cache.Add(
                        Memba.Common.Presentation.Constants.SecurityCode,
                        sSecurityCode,
                        null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, Memba.Common.Presentation.Constants.SlidingExpiration, 0),
                        CacheItemPriority.Default,
                        null);
                }
                if (!sSecurityCode.Equals(SecurityCodeTextBox.Text))
                    throw new ApplicationException(Resources.Web.glossary.QuickMessageControl_SecurityCodeFailInfo);
            }

            //Check any exception
            Exception objUploadException = objUploadData.Exception;
            if (objUploadException != null)
                throw new ApplicationException(objUploadException.Message, objUploadException);

            //Ensure that upload is complete
            if (objUploadData.ProgressStatus != UploadProgressStatus.Completed)
                throw new ApplicationException(Resources.Web.glossary.QuickMessageControl_UploadNotComplete);

            //Ensure we have at least one uploaded file (we should have at least one file input control)
            if ((objUploadData.UploadFiles == null) || (objUploadData.UploadFiles.Count < 1))
                throw new ApplicationException(Resources.Web.glossary.QuickMessageControl_NoCompleteFile);

            //Get path to file storage
            string sPath = FileStorage.Provider.ConnectionString;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sPath);
            //Existence will be checked in FileBroker
            
            //Start building download links
            StringBuilder objStringBuilder = new StringBuilder();
            objStringBuilder.Append("<ul>");
            string sDownloadRoot = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Authority +
                this.ResolveUrl(Memba.Common.Presentation.PageUrls.Download);

            bool bHasCompleteUploadFiles = false;
            List<File> lstAttachments = new List<File>();
            foreach (UploadFile objUploadFile in objUploadData.UploadFiles)
            {
                if ((objUploadFile != null) && objUploadFile.IsComplete)
                {
                    bHasCompleteUploadFiles = true;

                    //Create file object
                    File f = new File(
                        objUploadFile.OriginalFileName,
                        objUploadFile.ContentType,
                        objUploadFile.ProviderFileKey.ToString(),
                        objUploadFile.ContentLength,
                        objUploadFile.HashValue);

                    //Add definition to file storage
                    FileBroker.Insert(f, di);

                    //Add to message attachments
                    lstAttachments.Add(f);

                    //Continue building download links
                    objStringBuilder.Append(
                        String.Format(
                            Resources.Web.glossary.Culture,
                            "<li><a href=\"{0}\" class=\"cssFieldHyperlink\">{1} ({2})</a></li>",
                            System.IO.Path.Combine(sDownloadRoot, f.Guid.ToString("N") + ".dat"),
                            f.FileName,
                            BODisplay.SizeFormat(f.Size, ByteFormat.Adapt)
                        )
                    );
                }
            }

            //Ensure we have at least one COMPLETED uploaded file to display
            if (!bHasCompleteUploadFiles)
                throw new ApplicationException(Resources.Web.glossary.QuickMessageControl_NoCompleteFile);

            //Create Message
            sText = BODisplay.RemoveAllHtmlTags(sText); //just in case

            Message m = new Message(
                sSender,
                sRecipient,
                sText,
                lstAttachments,
                sDownloadRoot);

            //Send message
            m.Send();

            //Release upload data
            UploadMonitor.Release(objUploadData.UploadId);

            //Finalize download links and display
            objStringBuilder.AppendLine("</ul>");
            FinalLinkLabel.Text = objStringBuilder.ToString();

            //Show InfoBox Message
            if (_InfoBox != null)
            {
                _InfoBox.Type = InfoBoxType.OK;
                _InfoBox.Text = Resources.Web.glossary.QuickMessageControl_SendOKInfo;
            }
        }
        catch(Exception Ex)
        {
            Session[Memba.Common.Presentation.Constants.LastError] = Ex.Message;
            //Response.Redirect(Memba.Common.Presentation.PageUrls.Error);
            throw; //Redirects to error handler in Global.asax
        }
    }
    /// <summary>
    /// Click event handler for the Stop button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void StopButton_Click(object sender, EventArgs e)
    {
        UploadMonitor.CancelUpload(MuidHiddenField.Value);

        //Update InfoBox
        if (_InfoBox != null)
        {
            _InfoBox.Type = InfoBoxType.Warning;
            _InfoBox.Text = Resources.Web.glossary.QuickMessageControl_StopInfo;
        }
    }
    #endregion

    #region Data Loading Helpers
    /// <summary>
    /// Gets a formatted maximum total size of uploads
    /// </summary>
    /// <returns></returns>
    protected string GetMaxFileSize()
    {
        long lMaxRequestLengthBytes = Memba.FileUpload.UploadHttpModule.GetMaxRequestLengthBytes(this.Context, true);
        return BODisplay.SizeFormat(lMaxRequestLengthBytes, ByteFormat.Adapt);
    }
    #endregion
}
