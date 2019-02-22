<%@ Page Language="C#" AutoEventWireup="true" CodeFile="InfoBox.aspx.cs" Inherits="Debug_InfoBox" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>InfoBox Test Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server"></asp:ScriptManager>
    <!-- InfoBox1 -->
    <mbui:InfoBox ID="InfoBox1" runat="server" SkinID="sknPageTitle" Text="Hello" EnableViewState="False" Width="100%" ImageUrl="~/App_Images/32x32/uploads.gif"></mbui:InfoBox>
    <!-- InfoBox1 -->
    <textarea id="MyText" rows="5" cols="40"></textarea>
    <br />
    <select id="MyType">
        <option value="Error">Error</option>
        <option value="Information">Information</option>
        <option value="OK">OK</option>
        <option value="Warning">Warning</option>
    </select>
    <br />
    <input type="button" value="Fill" onclick="fill();" />
    <input type="button" value="setAll" onclick="setAll();" />
    <input type="button" value="setTemp" onclick="setTemp();" />
    <br />
    <!-- InfoBox2 -->
    <mbui:InfoBox ID="InfoBox2" runat="server" SkinID="sknInfoBox" Text="<%$ Resources:Web.glossary, DropBox_WelcomeInfo %>" EnableViewState="False" Width="100%"></mbui:InfoBox>
    <!-- InfoBox2 -->
    <textarea id="TraceConsole" cols="100" rows="20"></textarea>
    </form>
</body>
<script type="text/javascript">
function fill()
{
    var _text = $get("MyText").innerHTML;
    var _infobox = $find("<%= InfoBox2.ClientID %>");
    _infobox.set_text(_text);
    _infobox.set_type(getSelectedType());
}
function setAll()
{
    var _text = $get("MyText").innerHTML;
    var _infobox = $find("<%= InfoBox2.ClientID %>");
    _infobox.setAll(getSelectedType(), _text);
}
function setTemp()
{
    var _text = $get("MyText").innerHTML;
    var _infobox = $find("<%= InfoBox2.ClientID %>");
    _infobox.setTemp(getSelectedType(), _text, 500);
}
function getSelectedType()
{
    switch($get("MyType").value)
    {
        case "Error":
            return Memba.WebControls.InfoBoxType.Error;
        case "Information":
            return Memba.WebControls.InfoBoxType.Information;
        case "OK":
            return Memba.WebControls.InfoBoxType.OK;
        case "Warning":
            return Memba.WebControls.InfoBoxType.Warning;
        default:
            return null;
    }
}
</script>
</html>
