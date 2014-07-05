<%@ Assembly Name="ASC.Web.Files" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BoxLogin.aspx.cs" Inherits="ASC.Web.Files.Import.Boxnet.BoxLogin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style type="text/css">
       html, body, div, iframe { margin:0; padding:0; height:100%; }
       iframe { display:block; width:100%; border:none; }
    </style>
    <script type="text/javascript">
        function snd(authToken) {
            try {
                window.opener.OAuthCallback(authToken, "<%=Source%>");
            } catch (err) {
                alert(err);
            }
            window.close();
        }

        function listener(msg) {
            snd(msg.data);
        }

        if (window.addEventListener) {
            window.addEventListener("message", listener, false);
        } else {
            window.attachEvent("onmessage", listener);
        }
    </script>
</head>
<body>
    <iframe src="<%=LoginUrl%>" id="boxframe" frameborder="0"  scrolling="no">
    </iframe>
</body>
</html>
