<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OpenIdAuth.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title></title>
    <script>
        function openLogin() {
            var id = document.getElementById('openid_identifier').value;
            window.open("openidlogin.ashx?returnurl=<%=HttpUtility.UrlEncode(Request.Url.OriginalString) %>&openid_identifier="+id)
        }
    </script>
</head>
<body>
<a href="Default.aspx">Home</a>
    
    <form id="form1" method="post" action="openidlogin.ashx?auth=openid&returnurl=<%=HttpUtility.UrlEncode(Request.Url.OriginalString) %>" target="_top">
    <div>
    
        <label for="openid_identifier">
            OpenID:
        </label>
        <input id="openid_identifier" name="openid_identifier" size="40" />
        <input type="submit" value="Login" />
        <input type="button" value="Open" onclick="javascript:openLogin()" />
        <a href="openidlogin.ashx?returnurl=<%=HttpUtility.UrlEncode(Request.Url.OriginalString) %>&auth=twitter">Twitter login</a> 
        <a href="openidlogin.ashx?returnurl=<%=HttpUtility.UrlEncode(Request.Url.OriginalString) %>&auth=facebook">Facebook login</a>
    </div>
    </form>
    <p>Auth error:<%=Error %></p>
</body>
</html>
