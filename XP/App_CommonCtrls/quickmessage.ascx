<%--
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
--%>
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="quickmessage.ascx.cs" Inherits="App_CommonCtrls_quickmessage" %>
<asp:ScriptManagerProxy ID="QuickMessageScriptManagerProxy" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/ScriptLibrary/Memba.Utils.js" />
    </Scripts>
    <Services>
        <asp:ServiceReference Path="~/uploadWebService.asmx" />
    </Services>
</asp:ScriptManagerProxy>
<!-- Send file panel -->
<asp:Panel ID="SendFilePanel" runat="server">
    <asp:HiddenField ID="MuidHiddenField" runat="server" />
    <table border="0" cellspacing="1" cellpadding="3" style="width: 100%">
        <tr>
            <td style="vertical-align: top; width: 48px">
                <asp:Image ID="SendStep1Image" runat="server" ImageUrl="~/App_Images/48x48/step1.gif" AlternateText="<%$ Resources:Web.glossary, QuickMessageControl_SendStep1 %>" />
            </td>
            <td style="width: 85%">
                <asp:Label ID="SendStep1Label" runat="server" Text="<%$ Resources:Web.glossary, QuickMessageControl_SendStep1 %>" SkinID="sknStepTitle"></asp:Label>
                <table border="0" cellpadding="0" cellspacing="3" style="width: 100%">
                    <tr>
                        <td style="width: 10%">
                            <asp:Label ID="SenderLabel" runat="server" SkinID="sknOptionalFieldLabel" Text="<%$ Resources:Web.glossary, QuickMessageControl_Sender %>"></asp:Label></td>
                        <td style="width: 90%">
                            <asp:TextBox ID="SenderTextBox" runat="server" SkinID="sknTextBox" AutoCompleteType="Email"></asp:TextBox>
                            <asp:DropDownList ID="SenderDropDownList" runat="server" SkinID="sknDropDownList"></asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 10%">
                            <asp:Label ID="RecipientLabel" runat="server" SkinID="sknOptionalFieldLabel" Text="<%$ Resources:Web.glossary, QuickMessageControl_Recipient %>"></asp:Label></td>
                        <td style="width: 90%">
                            <asp:TextBox ID="RecipientTextBox" runat="server" SkinID="sknTextBox" AutoCompleteType="Email"></asp:TextBox>
                            <asp:DropDownList ID="RecipientDropDownList" runat="server" SkinID="sknDropDownList"></asp:DropDownList>
                        </td>
                    </tr>
                </table>
            </td>
            <td style="width: 10%; text-align:right; vertical-align:text-bottom;">
                <asp:HyperLink ID="PoweredByHyperLink" runat="server" SkinID="sknCommandHyperlink" ImageUrl="~/App_Images/misc/poweredby.gif" Text="<%$ Resources:Web.glossary, QuickMessageControl_PoweredBy %>" NavigateUrl="http://www.velodoc.com" Target="velodoc"></asp:HyperLink>
            </td>
            <td style="width: 10%; text-align:right; vertical-align:text-bottom;">
                <asp:HyperLink ID="HelpImageHyperLink" runat="server" SkinID="sknCommandHyperlink" ImageUrl="~/App_Images/48x48/help.gif" Text="<%$ Resources:Web.glossary, QuickMessageControl_GetHelp %>" NavigateUrl="http://www.velodoc.com/help" Target="velodoc"></asp:HyperLink>
            </td>
        </tr>
        <tr>
            <td style="vertical-align: top; width: 48px">
                <asp:Image ID="SendStep2Image" runat="server" ImageUrl="~/App_Images/48x48/step2.gif" AlternateText="<%$ Resources:Web.glossary, QuickMessageControl_SendStep2 %>" />
            </td>
            <td colspan="3" style="width: 95%">
                <asp:Label ID="SendStep2Label" runat="server" Text="<%$ Resources:Web.glossary, QuickMessageControl_SendStep2 %>" SkinID="sknStepTitle"></asp:Label><br />
                <asp:TextBox ID="MessageTextBox" runat="server" Rows="8" TextMode="MultiLine" Width="100%" MaxLength="500" SkinID="sknTextArea" AutoCompleteType="Notes"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td style="vertical-align: top; width: 48px">
                <asp:Image ID="SendStep3Image" runat="server" ImageUrl="~/App_Images/48x48/step3.gif" AlternateText="<%$ Resources:Web.glossary, QuickMessageControl_SendStep3 %>" />
            </td>
            <td colspan="3" style="width: 95%">
                <asp:Label ID="SendStep3Label" runat="server" Text="<%$ Resources:Web.glossary, QuickMessageControl_SendStep3 %>" SkinID="sknStepTitle"></asp:Label><br />
                <asp:Image ID="UploadImage" runat="server" ImageUrl="~/App_Images/16x16/add.gif" AlternateText="<%$ Resources:web.glossary, sknMultiUpload_Text %>" ImageAlign="AbsBottom" BorderWidth="0" />
                <mbui:MultiUpload2 ID="MultiUpload" runat="server" SkinID="sknMultiUpload2" Text="<%$ Resources:web.glossary, sknMultiUpload_Text %>"></mbui:MultiUpload2>
                <mbui:ImageList ID="FileList" runat="server" SkinID="sknFileList" RemoveTooltip="<%$ Resources:web.glossary, sknImageList_RemoveTooltip %>" Height="92px" Width="100%"></mbui:ImageList>
            </td>
        </tr>
        <tr>
            <td style="vertical-align: top; width: 48px">
                <asp:Image ID="SendStep4Image" runat="server" ImageUrl="~/App_Images/48x48/step4.gif" AlternateText="<%$ Resources:Web.glossary, QuickMessageControl_SendStep4 %>" />
            </td>
            <td colspan="3" style="width: 95%">
                <asp:Label ID="SendStepLabel4" runat="server" Text="<%$ Resources:Web.glossary, QuickMessageControl_SendStep4 %>" SkinID="sknStepTitle"></asp:Label><br />
                <table border="0" cellpadding="0" cellspacing="0" style="width: 100%">
                    <tr>
                        <td>
                            <asp:CheckBox id="AcceptTermsCheckBox" runat="server" SkinID="sknCheckBox" Text="<%$ Resources:Web.glossary, QuickMessageControl_Terms %>" Width="100%"></asp:CheckBox>
                            <asp:Label ID="SecurityCodeLabel" runat="server" SkinID="sknMandatoryFieldLabel" Text="<%$ Resources:Web.glossary, QuickMessageControl_SecurityCode %>"></asp:Label>
                            <asp:TextBox ID="SecurityCodeTextBox" runat="server" SkinID="sknTextBox"></asp:TextBox>
                        </td>
                        <td style="text-align:right; padding-top:5px;">
                            <asp:Button id="SendButton" runat="server" SkinID="sknButton" Text="<%$ Resources:Web.glossary, QuickMessageControl_SendButton %>" OnClientClick="return SendButton_Click();" OnClick="SendButton_Click" UseSubmitBehavior="true" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Panel>
<% // This is a workaround for a missing tag in the output html  %>
<asp:Literal ID="MissingTagWorkaroundLiteral" runat="server" Text="</div>"></asp:Literal>
<!-- Send progress panel -->
<asp:Panel ID="SendProgressPanel" runat="server" style="display:none;">
    <p><asp:Label ID="ProgressReportLabel" runat="server" SkinID="sknStepTitle" Text="<%$ Resources:Web.glossary, QuickMessageControl_SendProgressReport %>"></asp:Label></p>
    <mbui:ProgressReport ID="ProgressReport" runat="server" SkinID="sknProgressReport" DefaultText="<%$ Resources:web.glossary, sknProgressReport_DefaultText %>" TextFormat="<%$ Resources:web.glossary, sknProgressReport_TextFormat %>" HandlerUrl="" Interval="3000" Timeout="3000" ></mbui:ProgressReport>
    <div id="divStopButton" style="text-align:right;margin-top:1em;margin-right:20px;">
        <asp:Button id="StopButton" runat="server" SkinID="sknButton" Text="<%$ Resources:Web.glossary, QuickMessageControl_StopButton %>" OnClientClick="return StopButton_Click();" OnClick="StopButton_Click" UseSubmitBehavior="true" />
    </div>
</asp:Panel>
<!-- Send final panel -->
<asp:Panel ID="SendFinalPanel" runat="server" >
    <asp:Panel ID="FinalReportPanel" runat="server" >
        <p><asp:Label ID="FinalReportLabel" runat="server" SkinID="sknStepTitle" Text="<%$ Resources:Web.glossary, QuickMessageControl_SendOKInfo %>"></asp:Label></p>
        <p><asp:Label ID="FinalDescriptionLabel" runat="server"></asp:Label></p>
        <p><asp:Label ID="FinalLinkLabel" runat="server"></asp:Label></p>
    </asp:Panel>
    <div id="divBackButton" style="text-align:right;margin-top:1em;margin-right:20px;">
        <asp:Button  id="BackButton" runat="server" SkinID="sknButton" Text="<%$ Resources:Web.glossary, QuickMessageControl_BackButton %>" OnClientClick="return BackButton_Click();" UseSubmitBehavior="False" />
    </div>
</asp:Panel>
<!-- Atlas scripting -->
<script type="text/javascript">
<!--
Sys.Application.add_load(quickmessageLoad);
Sys.Application.add_unload(quickmessageUnload);
//Panels
var g_SendFilePanel;
var g_SendProgressPanel;
//SendFilePanel
var g_Muid;
var g_SenderTextBox;
var g_SenderDropDownList;
var g_RecipientTextBox;
var g_RecipientDropDownList;
var g_MessageTextBox;
var g_MultiUpload;
var g_FileList;
var g_AcceptTermsCheckBox;
var g_SecurityCodeTextBox;
var g_SecurityCodeChecked;
var g_Trials = 0;
var g_SendButton;
//SendProgressPanel
var g_ProgressReport;
<% //Sets Javascript references and event handlers to HTML controls %>
function quickmessageLoad()
{
    <% //Give enough time for web requests to execute callbacks %>
    Memba.Files.WebServices.uploadWebService.set_timeout(<%= Memba.Common.Presentation.Constants.AjaxTimeOut %>);

    <% //References to panels %>
    g_SendFilePanel = $get("<%= SendFilePanel.ClientID %>");
    g_SendProgressPanel =  $get("<%= SendProgressPanel.ClientID %>");
    
    <% //References to controls used within the send file panel %>
    if (g_SendFilePanel)
    {
        g_Muid = Memba.Utils.newGuid();
        $get("<%= MuidHiddenField.ClientID %>").value = g_Muid;
        g_SenderTextBox = $get("<%= SenderTextBox.ClientID %>");
        if(g_SenderTextBox)
            $addHandler(g_SenderTextBox, "keyup", validateBeforeSend);
        g_SenderDropDownList = $get("<%= SenderDropDownList.ClientID %>");
        if(g_SenderDropDownList)
            $addHandler(g_SenderDropDownList, "change", validateBeforeSend);
        g_RecipientTextBox = $get("<%= RecipientTextBox.ClientID %>");
        if(g_RecipientTextBox)
            $addHandler(g_RecipientTextBox, "keyup", validateBeforeSend);
        g_RecipientDropDownList = $get("<%= RecipientDropDownList.ClientID %>");
        if(g_RecipientDropDownList)
            $addHandler(g_RecipientDropDownList, "change", validateBeforeSend);
        g_MessageTextBox = $get("<%= MessageTextBox.ClientID %>");
        $addHandler(g_MessageTextBox, "keyup", validateBeforeSend);
        g_MultiUpload = $find("<%= MultiUpload.ClientID %>");
        g_MultiUpload.add_browse(onFileBrowse);
        g_FileList = $find("<%= FileList.ClientID %>");
        g_FileList.clear(); <% //IMPORTANT: Clear so that when pressing browser's back button, file list is in sync with multi upload %>
        g_FileList.add_remove(onFileRemove);
        g_AcceptTermsCheckBox = $get("<%= AcceptTermsCheckBox.ClientID %>");
        if(g_AcceptTermsCheckBox)
            $addHandler(g_AcceptTermsCheckBox, "click", validateBeforeSend);
        g_SecurityCodeTextBox = $get("<%= SecurityCodeTextBox.ClientID %>");
        if(g_SecurityCodeTextBox)
            $addHandler(g_SecurityCodeTextBox, "keyup", checkSecurityCode);
        if(<%= Convert.ToString(this.DisplayFormat == DisplayFormat.QuickSend).ToLowerInvariant() %>)
            g_SecurityCodeChecked = false;
        else if (<%= Convert.ToString(this.DisplayFormat == DisplayFormat.DropBox).ToLowerInvariant() %>)
            g_SecurityCodeChecked = true;
        g_SendButton = $get("<%= SendButton.ClientID %>");
        g_SendButton.disabled = true;
    }
    <% //References to controls used within the send progress panel %>
    if (g_SendProgressPanel)
    {   
        g_ProgressReport = $find("<%= ProgressReport.ClientID %>");
    }
}
<% //Clear event handlers to HTML controls %>
function quickmessageUnload()
{
    if (g_SendFilePanel)
    {
        if(g_SenderTextBox)
            $clearHandlers(g_SenderTextBox);
        if(g_SenderDropDownList)
            $clearHandlers(g_SenderDropDownList);
        if(g_RecipientTextBox)
            $clearHandlers(g_RecipientTextBox);
        if(g_RecipientDropDownList)
            $clearHandlers(g_RecipientDropDownList);
        $clearHandlers(g_MessageTextBox);
        g_MultiUpload.remove_browse(onFileBrowse);
        g_FileList.remove_remove(onFileRemove);
        if(g_AcceptTermsCheckBox)
            $clearHandlers(g_AcceptTermsCheckBox);
        if(g_SecurityCodeTextBox)
            $clearHandlers(g_SecurityCodeTextBox);
    }
    if (g_SendProgressPanel)
    {
        var _ProgressReport = $find("<%= ProgressReport.ClientID %>");
        if (_ProgressReport)
            _ProgressReport.stop();
    }
}
<% //Add the file selected in the MultiUpload component to the ImageList %>
function onFileBrowse(sender, args)
{
    if (g_FileList.find_item(args.get_value()).length > 0)
    {
        alert("<%= Resources.Web.glossary.QuickMessageControl_FileAlreadyInList %>");
        g_MultiUpload.removeInput(args.get_id());
    }
    else
    {
        var item = new Memba.WebControls.ImageListItem(
            Memba.Utils.newGuid(),
            '<%= this.ResolveClientUrl("~/App_Images/32x32/upload.gif") %>',
            args.get_value(),
            args.get_value(),
            args.get_id()
        );
        g_FileList.add_item(item);
        validateBeforeSend();
    }
}
<% //Removes the file selected in the ImageList component from the MultiUpload %>
function onFileRemove(sender, args)
{
    g_MultiUpload.removeInput(args.get_tag());
    validateBeforeSend();
}
<% //form validation before enabling the 'send' button %>
function validateBeforeSend()
{
    if (g_SendButton)
    {
        var _ret = g_SecurityCodeChecked;
        var _sender;
        if (g_SenderTextBox)
            _sender = g_SenderTextBox.value.trim();
        else if (g_SenderDropDownList)
            _sender = g_SenderDropDownList.value.trim();
        if (!Memba.Utils.isEmailAddress(_sender))
            _ret = false;
        var _recipient;
        if (g_RecipientTextBox)
             _recipient = g_RecipientTextBox.value.trim();
        else if (g_RecipientDropDownList)
            _recipient = g_RecipientDropDownList.value.trim();
        if (!Memba.Utils.isEmailAddress(_recipient))
            _ret = false;
        var _message = g_MessageTextBox.value.trim();
        if (_message == "")
            _ret = false;
        Sys.Debug.assert(g_MultiUpload.get_count() == g_FileList.get_count(),
            "There are " + g_MultiUpload.get_count() + " files in the multiupload control and " + g_FileList.get_count() + " files in the file list control.");
        if (g_MultiUpload.get_count() < 1)
            _ret = false;
        if ((g_AcceptTermsCheckBox) && (!g_AcceptTermsCheckBox.checked))
            _ret = false;
        var _infoBox = $find('<%= this.InfoBoxClientID %>');
        if (_infoBox)
        {
            if (_ret)
            {
                if (<%= Convert.ToString(!String.IsNullOrEmpty(this.WelcomeInfo)).ToLowerInvariant() %>)
                    _infoBox.setAll(Memba.WebControls.InfoBoxType.Information, "<%= this.WelcomeInfo %>");
            }
            else
            {
                _infoBox.setAll(Memba.WebControls.InfoBoxType.Warning, "<%= Resources.Web.glossary.QuickMessageControl_InvalidSendInfo %>");
            }
        }
        g_SendButton.disabled = !_ret;
    }
}
<% //Web service which checks account credit before sending %>
function checkSecurityCode()
{
    var _securityCode = "";
    if (g_SecurityCodeTextBox)
        _securityCode = g_SecurityCodeTextBox.value;
    Sys.Debug.trace("Calling uploadWebService.CheckSecurityCode asynchronously");
    Memba.Files.WebServices.uploadWebService.CheckSecurityCode(
        _securityCode,
        onCheckSecurityCodeComplete,
        onCheckSecurityCodeError
        );
}
<% //Callback function for the web service which checks security code %>
function onCheckSecurityCodeComplete(result)
{
    Sys.Debug.trace("Callback from uploadWebService.CheckSecurityCode");
    g_Trials = 0;
    if (result)
    {
        g_SecurityCodeChecked = true;
        validateBeforeSend();
    }
    else
    {
        g_SecurityCodeChecked = false;
        var _infoBox = $find('<%= this.InfoBoxClientID %>');
        if (_infoBox)
            _infoBox.setAll(Memba.WebControls.InfoBoxType.Error, "<%= Resources.Web.glossary.QuickMessageControl_SecurityCodeFailInfo %>");
    }
}
function onCheckSecurityCodeError(result)
{
    g_Trials++;
    g_SecurityCodeChecked = false;
    if (result.get_timedOut() && g_Trials < <%= Memba.Common.Presentation.Constants.AjaxTrials %>)
        setTimeout('checkSecurityCode();', <%= Memba.Common.Presentation.Constants.AjaxWaitBeforeRetry %>); 
    else
    {
        var _infoBox = $find('<%= this.InfoBoxClientID %>');
        if (_infoBox)
        {
            if (result.get_timedOut())
                _infoBox.setAll(Memba.WebControls.InfoBoxType.Error, "<%= Resources.Web.glossary.WebService_TimedOut %>");
            else
                _infoBox.setAll(Memba.WebControls.InfoBoxType.Error, "<%= Resources.Web.glossary.QuickMessageControl_SecurityCodeFailInfo %>");
        }
    }
}
<% //Client-side click event handler for the send button %>
function SendButton_Click(){
    Sys.Debug.trace("Send button clicked");
    <% //Hide form and display results  %>
    var _infoBox = $find('<%= this.InfoBoxClientID %>');
    if (_infoBox)
    {
        _infoBox.set_type(Memba.WebControls.InfoBoxType.Information)
        _infoBox.set_text("<%= Resources.Web.glossary.QuickMessageControl_SendProgressInfo %>");
    }
    g_SendFilePanel.style.display="none";
    g_SendProgressPanel.style.display="block";
    <% //Get a reference to the progress bar %>
    var _ProgressReport = $find("<%= ProgressReport.ClientID %>");
    if (_ProgressReport)
    {
        _ProgressReport.set_handlerUrl('<%= this.ResolveClientUrl(Memba.Common.Presentation.PageUrls.UploadHandler + "?" + Memba.FileUpload.UploadMonitor.UploadIdParam + "=") %>' + g_Muid);
        window.setTimeout(Function.createDelegate(_ProgressReport, _ProgressReport.start), <%= Memba.Common.Presentation.Constants.StartProgress %>);
    }
    <% //Post back the page to upload the file %>
    document.forms[0].encoding = "multipart/form-data";
    document.forms[0].action = Memba.Utils.addParamToUrl(
        "<%= Memba.FileUpload.UploadMonitor.UploadIdParam %>",
        g_Muid,
        document.location.href);
    return true;
}
<% //Client-side click event handler for the stop button %>
function StopButton_Click(){
    if (window.document.execCommand)
        window.document.execCommand("Stop"); //for IE
    else if (window.stop)
        window.stop(); //for Mozilla
    else
        window.location.href = window.location.href;
    <% //Post back the page to record that upload has been stopped %>
    document.forms[0].encoding = "application/x-www-form-urlencoded";
    document.forms[0].action = Memba.Utils.removeParamFromUrl(
        "<%= Memba.FileUpload.UploadMonitor.UploadIdParam %>",
        document.location.href);
    return true;
}
<% //Client-side click event handler for the back button %>
function BackButton_Click(){
    parent.location.href = Memba.Utils.removeParamFromUrl(
        "<%= Memba.FileUpload.UploadMonitor.UploadIdParam %>",
        document.location.href);
    return true;
}
//-->
</script>