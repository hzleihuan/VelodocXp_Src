<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
protected void Page_Init(object sender, EventArgs e)
{
    //We need a redirection to avoid "The HTTP verb POST used to access path '/<virtualdir>/' is not allowed."
    Response.Redirect(Memba.Common.Presentation.PageUrls.DropBox);
}
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Velodoc XP Edition</title>
</head>
<body>
    <div>Redirection page</div>
</body>
</html>
