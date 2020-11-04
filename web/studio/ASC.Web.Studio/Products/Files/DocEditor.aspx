<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DocEditor.aspx.cs" Inherits="ASC.Web.Files.DocEditor" %>

<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="master" TagName="EditorScripts" Src="Masters/EditorScripts.ascx" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, minimum-scale=1, user-scalable=no" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="black" />
    <meta name="mobile-web-app-capable" content="yes" />
    <meta name="apple-touch-fullscreen" content="yes" />

    <%= string.Format("<link href='{0}' rel='shortcut icon' type='image/x-icon' id='docsEditorFavicon'/>", Favicon) %>

    <style type="text/css">
        html {
            height: 100%;
            width: 100%;
        }

        body {
            background: #f4f4f4;
            color: #111;
            font-family: Arial, Tahoma,sans-serif;
            font-size: 12px;
            font-weight: normal;
            height: 100%;
            margin: 0;
            padding: 0;
            text-decoration: none;
        }

        div {
            margin: 0;
            padding: 0;
        }
    </style>

</head>
<body class="<%= IsMobile ? "mobile" : "" %>">
    <noscript>
        <div class="info-box excl"><%= Resource.ErrorNoscript %></div>
    </noscript>
    <form id="form1" runat="server">
        <div id="wrap">
            <div id="iframeEditor"></div>
        </div>

        <%= RenderCustomScript() %>

        <master:EditorScripts runat="server" />
        <sc:InlineScript ID="InlineScripts" runat="server" />

        <script language="javascript" type="text/javascript" src="<%= DocServiceApiUrl.HtmlEncode() %>"></script>
    </form>
</body>
</html>
