<%@ Assembly Name="ASC.Data.Storage" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginDialog.ascx.cs" Inherits="ASC.Web.Files.Controls.LoginDialog" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<link rel="stylesheet" type="text/css" href="<%= VirtualPathUtility.ToAbsolute("~/products/files/controls/logindialog/logindialog.css") %>" />
<link rel="stylesheet" type="text/css" href="<%= VirtualPathUtility.ToAbsolute("~/usercontrols/users/userprofile/css/accountlink_style.less") %>" />
<script type="text/javascript" language="javascript" src="<%= ResolveUrl("~/usercontrols/users/userprofile/js/accountlinker.js") %>" ></script>

<div class="block-auth">
    <div class="block-overlay"></div>
    <div class="block-dialog">
        <div class="header">
            <%= FilesCommonResource.DemoVersion %>
            <div class="close" title="<%= FilesCommonResource.ButtonClose %>"></div>
        </div>
        <div class="social-login">
            <input type="hidden" name="additionalMember" id="additionalMember" />
            <asp:PlaceHolder runat="server" ID="CommonPlaceHolder"></asp:PlaceHolder>
        </div>
    </div>
</div>
