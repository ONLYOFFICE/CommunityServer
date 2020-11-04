<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="PreparationPortal.aspx.cs" Inherits="ASC.Web.Studio.PreparationPortal" %>
<%@ Register TagPrefix="ucc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html>
    <head runat="server">
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <%= string.Format("<link href='{0}' rel='icon' type='image/x-icon' />", ASC.Web.Core.WhiteLabel.TenantLogoManager.GetFavicon(true, true)) %>
        
        <link type="text/css" rel="stylesheet" href="<%= VirtualPathUtility.ToAbsolute("~/UserControls/Common/PreparationPortal/css/preparationportal.css") %>"/>
    </head>
    <body>
    <asp:PlaceHolder runat="server" ID="PreparationPortalContent" />
        <script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/jquery/jquery.core.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/ajaxpro.core.js")%>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/UserControls/Common/PreparationPortal/js/preparationportal.js") %>"></script>
        <ucc:InlineScript ID="InlineScript" runat="server" />

    </body>
</html>
