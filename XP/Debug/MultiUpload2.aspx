<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MultiUpload2.aspx.cs" Inherits="Debug_MultiUpload2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
<title>MultiUpload2 Test Page</title>
<style  type="text/css">
<!--
.cssList
{
    overflow:auto;
    width:99.7%;
    height:92px;
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
    border:1px dotted black;
    background-color:whitesmoke;
    float:left;
}
.cssImage
{
    border:0;
}
.cssText
{
    font-family:Calibri, Arial, Helvetica, sans-serif;
    font-size:10px;
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
.cssMultiUpload
{
    white-space:nowrap;
    text-decoration:none;
}
.cssMultiUploadHover
{
    white-space:nowrap;
    text-decoration:underline;
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
    <mbui:MultiUpload2 ID="MultiUpload" runat="server" Text="Choose file..." Width="100px" CssClass="cssMultiUpload" HoverCssClass="cssMultiUploadHover"></mbui:MultiUpload2>
    <mbui:ImageList ID="ImageList" runat="server"
        CssClass="cssList"
        ItemCssClass="cssItem"
        ItemHoverCssClass="cssItemHover"
        ImageCssClass="cssImage"
        TextCssClass="cssText"
        RemoveCssClass="cssRemove"
        RemoveTooltip="Remove from selection"
        LinesOfText="2"
        Height="92px"
        Width="420px"></mbui:ImageList>
    <br />
    <input type="button" id="ClearButton" value="Clear" onclick="onClear();" />
    <div class="cssList">Make sure elements are properly positioned</div>
    <textarea id="TraceConsole" cols="100" rows="20"></textarea>
    </form>
</body>
</html>
<script type="text/javascript">
<!--
var g_MultiUpload;
var g_ImageList;
var g_ClearButton;

//This function is called by the Ajax framework when the DOM is loaded
function pageLoad()
{
    //Get a reference to the MultiUpload control
    g_MultiUpload = $find("<%= MultiUpload.ClientID %>");
    //Add en event handler for the browse event
    g_MultiUpload.add_browse(onBrowse);
    //Get a reference to the ImageList control  
    g_ImageList = $find("<%= ImageList.ClientID %>");
    //Add en event handler for the browse event
    g_ImageList.add_remove(onRemove);   
}
//This function is called by the Ajax framework when unloading the page
function pageUnload()
{
    //This is good practice to clear your event handlers
    g_MultiUpload.remove_browse(onBrowse);
    g_ImageList.remove_remove(onRemove);    
}
//Event handler for the browse (click) event of the MultiUpload control
function onBrowse(sender, args)
{
    //Search for the item in the Imagelist
    if (g_ImageList.find_item(args.get_value()).length > 0)
    {
        alert("file already in list");
        //The item already exists, we can remove the duplicate INPUT in the MultiUpload control
        g_MultiUpload.removeInput(args.get_id());
    }
    else
    {
        //Since teh item is not found in the ImageList, create a new item
        var item = new Memba.WebControls.ImageListItem(
            Memba.Utils.newGuid(),
            '<%= this.ResolveClientUrl(Memba.Common.Presentation.Icons.GetIcon32("upload.gif")) %>',
            args.get_value(),
            args.get_value(),
            args.get_id()
        );
        //Add teh new item to the ImageList
        g_ImageList.add_item(item);
    }
    //We can do some tracing which will display in the TraceConsole textarea 
    Sys.Debug.trace(g_ImageList.get_count() + " files in image list, and " + g_MultiUpload.get_count() + " files in MultiUpload control");
}
//Event handler for the remove event of the ImageList control
function onRemove(sender, args)
{
    //Upon clicking the remove icon in the ImageList, remove the corresponding INPUT in the MultiUpload control
    g_MultiUpload.removeInput(args.get_tag());
    //We can do some tracing which will display in the TraceConsole textarea 
    Sys.Debug.trace(g_ImageList.get_count() + " files in image list, and " + g_MultiUpload.get_count() + " files in MultiUpload control");
}
function onClear()
{
    g_MultiUpload.clear();
    g_ImageList.clear();
}
//-->
</script>