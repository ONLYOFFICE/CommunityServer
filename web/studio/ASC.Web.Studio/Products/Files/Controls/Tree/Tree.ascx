<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Tree.ascx.cs" Inherits="ASC.Web.Files.Controls.Tree" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Files.Core.Security" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%-- TREE --%>
<div id="<%= ID %>" class="jstree webkit-scrollbar <%= AdditionalCssClass %>">
    <ul>
        <% if (!IsVisitor)
           { %>
        <li data-id="<%= Global.FolderMy %>" class="tree-node jstree-closed">
            <span class="jstree-icon jstree-expander"></span>
            <a data-id="<%= Global.FolderMy %>" title="<%= FilesUCResource.MyFiles %>" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "#" + Global.FolderMy %>">
                <span class="jstree-icon myFiles"></span><%= FilesUCResource.MyFiles %>
                <input type="hidden" name="entry_data" data-id="<%= Global.FolderMy %>" value='{ "entryType": "folder", "access" : "<%= (int)FileShare.None %>" }' />
            </a>
            <% if (WithNew)
               { %>
            <span class="new-label-menu is-new" title="<%= FilesUCResource.RemoveIsNew %>" data-id="<%= Global.FolderMy %>"></span>
            <% } %>
        </li>
        <% } %>

        <% if (!CoreContext.Configuration.Personal)
           { %>

        <% if (!Global.IsOutsider)
           { %>
        <li data-id="<%= Global.FolderShare %>" class="tree-node jstree-closed access-read">
            <span class="jstree-icon jstree-expander"></span>
            <a data-id="<%= Global.FolderShare %>" title="<%= FilesUCResource.SharedForMe %>" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "#" + Global.FolderShare %>">
                <span class="jstree-icon shareformeFiles"></span><%= FilesUCResource.SharedForMe %>
                <input type="hidden" name="entry_data" data-id="<%= Global.FolderShare %>" value='{ "entryType": "folder", "access" : "<%= (int)FileShare.Read %>" }' />
            </a>
            <% if (WithNew)
               { %>
            <span class="new-label-menu is-new" title="<%= FilesUCResource.RemoveIsNew %>" data-id="<%= Global.FolderShare %>"></span>
            <% } %>
        </li>
        <% } %>

        <li data-id="<%= Global.FolderCommon %>" class="tree-node jstree-closed <%= Global.IsAdministrator ? string.Empty : "access-read" %>">
            <span class="jstree-icon jstree-expander"></span>
            <a data-id="<%= Global.FolderCommon %>" title="<%= FilesUCResource.CorporateFiles %>" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "#" + Global.FolderCommon %>">
                <span class="jstree-icon corporateFiles"></span><%= FilesUCResource.CorporateFiles %>
                <input type="hidden" name="entry_data" data-id="<%= Global.FolderCommon %>" value='{ "entryType": "folder", "access" : "<%= (int)(Global.IsAdministrator ? FileShare.ReadWrite : FileShare.Read) %>" }' />
            </a>
            <% if (WithNew)
               { %>
            <span class="new-label-menu is-new" title="<%= FilesUCResource.RemoveIsNew %>" data-id="<%= Global.FolderCommon %>"></span>
            <% } %>
        </li>

        <% if (FolderIDCurrentRoot == null)
           { %>

        <% if (Global.FolderProjects != null)
           { %>
        <li data-id="<%= Global.FolderProjects %>" class="tree-node jstree-closed access-read">
            <span class="jstree-icon jstree-expander"></span>
            <a data-id="<%= Global.FolderProjects %>" title="<%= FilesUCResource.ProjectFiles %>" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "#" + Global.FolderProjects %>">
                <span class="jstree-icon projectFiles"></span><%= FilesUCResource.ProjectFiles %>
                <input type="hidden" name="entry_data" data-id="<%= Global.FolderProjects %>" value='{ "entryType": "folder", "access" : "<%= (int)FileShare.Read %>" }' />
            </a>
            <% if (WithNew)
               { %>
            <span class="new-label-menu is-new" title="<%= FilesUCResource.RemoveIsNew %>" data-id="<%= Global.FolderProjects %>"></span>
            <% } %>
        </li>
        <% } %>

        <% }
           else
           { %>
        <li data-id="<%= FolderIDCurrentRoot %>" class="tree-node jstree-closed">
            <span class="jstree-icon jstree-expander"></span>
            <a data-id="<%= FolderIDCurrentRoot %>" title="<%= FilesUCResource.ProjectFiles %>" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "#" + FolderIDCurrentRoot %>">
                <span class="jstree-icon projectFiles"></span><%= FilesUCResource.ProjectFiles %>
                <input type="hidden" name="entry_data" data-id="<%= FolderIDCurrentRoot %>" value='{ "entryType": "folder", "access" : "<%= (int)FileShare.ReadWrite %>" }' />
            </a>
        </li>
        <% } %>

        <% } %>

        <% if (!WithoutTrash)
           { %>
        <li data-id="<%= Global.FolderTrash %>" class="tree-node jstree-closed">
            <span class="jstree-icon visibility-hidden"></span>
            <a data-id="<%= Global.FolderTrash %>" title="<%= FilesUCResource.Trash %>" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "#" + Global.FolderTrash %>">
                <span class="jstree-icon trashFiles"></span><%= FilesUCResource.Trash %>
                <input type="hidden" name="entry_data" data-id="<%= Global.FolderTrash %>" value='{ "entryType": "folder", "access" : "<%= (int)FileShare.ReadWrite %>" }' />
            </a>
        </li>
        <% } %>
    </ul>
</div>
