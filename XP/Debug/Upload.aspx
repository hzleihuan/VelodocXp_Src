<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Upload.aspx.cs" Inherits="Debug_Upload" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Upload Test Page</title>
<style  type="text/css">
<!--
html,body
{
    margin:10px !important;
}
div.cssProgressReport
{
	text-align:left;
}
div.cssProgressBar
{
	border:solid 1px DarkGreen !important;
	padding:1px;
	background-color:White;
}
div.cssProgressFiller
{
	background-color:DarkGreen !important;
	height:10px;
}
div.cssProgressText
{
}
//-->
</style>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server" ></asp:ScriptManager>
    <div>
        <mbui:ProgressReport ID="ProgressReport" runat="server" DefaultText="Waiting..." Width="400px" Interval="3000" BarCssClass="cssProgressBar" FillerCssClass="cssProgressFiller" TextFormat="Value 1: {0}<br/>Value 2: {1}" ></mbui:ProgressReport>
        <asp:FileUpload ID="FileUpload" runat="server" />
        <asp:Button ID="SubmitButton" runat="server" OnClientClick="go();" Text="Submit" />
    </div>
    </form>
</body>
<script type="text/javascript">
var g_c;
//This function is called by the Ajax framework when the DOM is loaded
function pageLoad()
{
    //Get a reference to the ProgressReport control
    g_c = $find("<%= ProgressReport.ClientID %>");
    if (g_c)
    {
        g_c.set_visibilityMode(Sys.UI.VisibilityMode.collapse); //Sys.UI.VisibilityMode.hide
        g_c.set_visible(false);
    }
}
//Click event handler for the Go button
function go()
{
    if (g_c)
    {
        g_c.set_visible(true);
        //Start progress report
        g_c.start();
    }
}
</script>
</html>
