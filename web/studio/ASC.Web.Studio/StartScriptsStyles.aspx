<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StartScriptsStyles.aspx.cs" Inherits="ASC.Web.Studio.UserControls.FirstTime.StartScriptsStyles" %>

<!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=9" />
    <meta name="description" content="" />
    <meta name="keywords" content="" />
    <style type="text/css"></style>
</head>
<body>
    <% foreach (var uri in ListUri)
       { %>
    <% if (uri.EndsWith(".css"))
       { %>
    <link type="text/css" href="<%= uri %>" rel="stylesheet" />
    <% } %>

    <% if (uri.EndsWith(".js"))
       { %>
    <script type="text/javascript" src="<%= uri %>"></script>
    <% } %>
    <% } %>
</body>
</html>
