<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MigrationPortal.aspx.cs" Inherits="ASC.Web.Studio.MigrationPortal" %>

<html>
    <head id="Head1" runat="server">
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <%= string.Format("<link href='{0}' rel='icon' type='image/x-icon' />", ASC.Web.Core.WhiteLabel.TenantLogoManager.GetFavicon(true, true)) %>
        
        <link type="text/css" rel="stylesheet" href="<%= VirtualPathUtility.ToAbsolute("~/UserControls/Common/MigrationPortal/css/migrationportal.less") %>"/>
    </head>
    <body>
    <asp:PlaceHolder runat="server" ID="MigrationPortalContent" />
        <script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/jquery/jquery.core.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/asc/api/api.factory.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/asc/api/api.helper.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/js/asc/api/asc.teamlab.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/UserControls/Common/MigrationPortal/js/migrationportal.js") %>"></script>
    </body>
</html>
