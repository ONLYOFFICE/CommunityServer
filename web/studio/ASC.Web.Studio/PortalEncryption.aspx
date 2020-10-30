<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PortalEncryption.aspx.cs" Inherits="ASC.Web.Studio.PortalEncryption" %><!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
    <head id="Head1" runat="server">
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <%= string.Format("<link href='{0}' rel='icon' type='image/x-icon' />", ASC.Web.Core.WhiteLabel.TenantLogoManager.GetFavicon(true, true)) %>
        
        <link type="text/css" rel="stylesheet" href="<%= VirtualPathUtility.ToAbsolute("~/UserControls/Common/PortalEncryption/css/portalencryption.less") %>"/>
    </head>
    <body>
    <asp:PlaceHolder runat="server" ID="PortalEncryptionContent" />
        <script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/jquery/jquery.core.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/asc/api/api.factory.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/asc/api/api.helper.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/asc/api/asc.teamlab.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/UserControls/Common/PortalEncryption/js/portalencryption.js") %>"></script>
    </body>
</html>
