<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="PreparationPortal.aspx.cs" Inherits="ASC.Web.Studio.PreparationPortal" %>
<%@ Register TagPrefix="ucc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html>
    <head runat="server">
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <link rel="icon" href="./favicon.ico" type="image/x-icon" />
        <link href='https://fonts.googleapis.com/css?family=Open+Sans:900,800,700,600,500,400,300&subset=latin,cyrillic-ext,cyrillic,latin-ext' rel="stylesheet" type="text/css" />
        <link type="text/css" rel="stylesheet" href="<%= VirtualPathUtility.ToAbsolute("~/usercontrols/common/preparationportal/css/preparationportal.css") %>"/>
        <title>ONLYOFFICE™</title>
    </head>
    <body>
    <asp:PlaceHolder runat="server" ID="PreparationPortalContent" />
        <script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/jquery/jquery.core.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/ajaxpro.core.js")%>" notobfuscate="true"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/usercontrols/common/preparationportal/js/preparationportal.js") %>"></script>
        <ucc:InlineScript ID="InlineScript" runat="server" />

    </body>
</html>
