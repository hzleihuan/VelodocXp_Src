<%@ Page Language="C#" AutoEventWireup="true" CodeFile="multiUpload.aspx.cs" Inherits="Debug_multiUpload" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>MultiUpload Test Page</title>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server">
    </asp:ScriptManager>
    <mbui:MultiUpload ID="MultiUpload" runat="server" Text="Click here to upload a new file"></mbui:MultiUpload>
    <br />
    <textarea id="TraceConsole" cols="100" rows="20"></textarea><br />
    Note: MultiUpload does not work in Firefox. Use MultiUpload2.
</form>
</body>
</html>
<script type="text/javascript">
<!--
//This function is called by the Ajax fremework when the DOM is loaded
function pageLoad()
{
    //Get a reference to the MultiUpload control
    var _c = $find("<%= MultiUpload.ClientID %>");
    //Add en event handler for the browse event
    _c.add_browse(onBrowse);
}
//Event handler for the browse (click) event of the MultiUpload control
function onBrowse(sender, args)
{
    //Display the file path in an alert box
    alert("File is: " + args.get_value());
}
//-->
</script>