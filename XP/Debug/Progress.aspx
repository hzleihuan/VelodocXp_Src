<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Progress.aspx.cs" Inherits="Debug_Progress" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Progress Test Page</title>
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
    <mbui:ProgressReport ID="ProgressReport" runat="server" DefaultText="Waiting..." Width="400px" Interval="1000" BarCssClass="cssProgressBar" FillerCssClass="cssProgressFiller" HandlerUrl="~/Debug/progressHandler.ashx" TextFormat="Value 1: {0}<br/>Value 2: {1}" ></mbui:ProgressReport>
    <input type="button" onclick="go();" value="Go" /><br />
    <textarea id="TraceConsole" cols="100" rows="20"></textarea>
    </form>
</body>
<script type="text/javascript">
<!--
//Click event handler for the Go button
function go()
{
    //Get a reference to the progress control
    var _c = $find("<%= ProgressReport.ClientID %>");
    //Add an event handler for the complete event
    _c.add_complete(onComplete);
    //Start progress report
    _c.start();
}
//Complete event handler for the progress report
function onComplete(e)
{
    alert("complete");
}
//-->
</script>
</html>
