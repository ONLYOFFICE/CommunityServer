<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TreeBuilder.ascx.cs" Inherits="ASC.Web.Files.Controls.TreeBuilder" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<% if (FolderIDCurrentRoot == null)
   { %>
<asp:PlaceHolder runat="server" ID="TreeViewHolder"></asp:PlaceHolder>
<% }
   else
   { %>
<div id="treeViewContainer" class="jstree <%= AdditionalCssClass %>"></div>
<% } %>

<div id="treeViewPanelSelector" class="studio-action-panel webkit-scrollbar">
    <div class="select-folder-header">
        <b><%= FilesUCResource.SelectFolder %></b>
    </div>
    <asp:PlaceHolder runat="server" ID="TreeSelectorHolder"></asp:PlaceHolder>
</div>
