<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreateMenu.ascx.cs" Inherits="ASC.Web.Files.Controls.CreateMenu" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<% if (EnableCreateFile)
   { %>
<li>
    <a id="createDocument" class="dropdown-item"><%= FilesUCResource.ButtonCreateText %></a>
</li>
<li>
    <a id="createSpreadsheet" class="dropdown-item"><%= FilesUCResource.ButtonCreateSpreadsheet %></a>
</li>
<li>
    <a id="createPresentation" class="dropdown-item"><%= FilesUCResource.ButtonCreatePresentation %></a>
</li>
<li>
    <div class="dropdown-item-seporator"></div>
</li>
<% } %>
<li>
    <a id="createNewFolder" class="dropdown-item"><%= FilesUCResource.ButtonCreateFolder %></a>
</li>
