<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ImageList.aspx.cs" Inherits="Debug_ImageList" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
<title>ImageList Test Page</title>
<style  type="text/css">
<!--
.cssList
{
    position: relative;
    overflow:auto;
    width:99.7%;
    height:100px;
    border:1px solid #0F0000;
    background-color:white;
}
.cssItem
{
    position:relative;
    width:70px;
    height:70px;
    padding:5px;
    margin:5px;
    text-align:center;
    border:1px solid white;
    float:left;
}
.cssItemHover
{
    position:relative;
    width:70px;
    height:70px;
    padding:5px;
    margin:5px;
    text-align:center;
    border:1px dotted red;
    float:left;
}
.cssImage
{
    border:0;
}
.cssText
{
    font-family:Calibri, Arial, Helvetica, sans-serif;
    font-size:8px;
    white-space:normal;
}
.cssRemove
{
    position:absolute;
    top:64px;
    left:64px;
    z-index:10;
    cursor:pointer;
}
//-->
</style>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/ScriptLibrary/Memba.Utils.js" />
        </Scripts>
    </asp:ScriptManager>
    <!-- ImageList -->
    <mbui:ImageList ID="ImageList" runat="server"
        CssClass="cssList"
        ItemCssClass="cssItem"
        ItemHoverCssClass="cssItemHover"
        ImageCssClass="cssImage"
        TextCssClass="cssText"
        RemoveCssClass="cssRemove"
        RemoveTooltip="Remove from selection"
        LinesOfText="2"
        Height="100px"
        Width="420px"></mbui:ImageList>
    <!-- ImageList -->
    <br />
    <input id="AddItem" type="button" value="add item" />
    <asp:Button ID="PostButton" runat="server" Text="Postback" OnClick="PostButton_Click" /> 
    <br />
    <textarea id="TraceConsole" cols="100" rows="20"></textarea>
    </form>
</body>
</html>
<!-- Atlas Scripting -->
<script type="text/javascript">
<!--
var g_AddItemButton;
var g_ImageList;
var g_Count=0;

//This function is called by the Ajax framework when the DOM is loaded
function pageLoad()
{
    //Get a reference to the AddItem button
    g_AddItemButton = $get("AddItem");
    //Add an event handler for the click event of the AddItem button
    $addHandler(g_AddItemButton, "click", onAddItem);
    //Get a reference to the ImageList control
    g_ImageList = $find("<%= ImageList.ClientID %>")
}
//Event handler for the click event of the AddItem button
function onAddItem()
{
    g_Count++;
    //Instantiate a new item
    var item = new Memba.WebControls.ImageListItem(
        Memba.Utils.newGuid(),
        '<%= this.ResolveClientUrl(Memba.Common.Presentation.Icons.GetIcon32("upload.gif")) %>',
        "Sample+ " + g_Count
    );
    //Add the item to the list
    g_ImageList.add_item(item);
}
//-->
</script>
