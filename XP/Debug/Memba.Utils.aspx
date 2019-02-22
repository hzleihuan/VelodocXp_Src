<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Memba.Utils.aspx.cs" Inherits="Debug_Memba_Utils" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Memba.Util Test Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/ScriptLibrary/Memba.Utils.js" />
        </Scripts>
    </asp:ScriptManager>
    <div>
        Param:<input id="Text1" type="text" value="a" /><br />
        Url:<input id="Text2" type="text" value="http://www.memba.com?a=1&b=2&c=3&d=4&e=5"/ size="80"><br />
        <input id="Button" type="button" value="Remove Param" /><br />
        <div id="Result"></div>
    </div>
    </form>
</body>
</html>
<script type="text/ecmascript">
<!--
var g_Text1;
var g_Text2;
var g_Button;
var g_Result;

function pageLoad()
{
    g_Text1 = $get("Text1");
    g_Text2 = $get("Text2");
    g_Button = $get("Button");
    $addHandler(g_Button, "click", onClickHandler);
    g_Result = $get("Result");
}
function pageUnload()
{
    $clearHandlers(g_Button);
}
function onClickHandler()
{
    g_Result.innerHTML = Memba.Utils.removeParamFromUrl(g_Text1.value, g_Text2.value); 
}
//-->
</script>