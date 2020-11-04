<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocService.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.DocService" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="docServiceBlock" class="settings-block">
    <div class="header-base"><%= Resource.DocServiceUrl %></div>

    <p><%= Resource.DocServiceText %></p>

    <div class="doc-service-item">
        <div class="header-base-small"><%= Resource.DocServiceUrlApi %></div>
        <input id="docServiceUrl" type="text" class="doc-service-value textEdit"
            value="<%= FilesLinkUtility.DocServiceUrl.HtmlEncode() %>" placeholder="https://<editors-dns-name>/" />
        <div class="gray-text"><%= string.Format(Resource.DocServiceUrlExample, "https://&lt;editors-dns-name&gt;/") %> </div>
    </div>
    <div class="doc-service-item">
        <div class="header-base-small"><%= Resource.DocServiceUrlInternal %></div>
        <input id="docServiceUrlInternal" type="text" class="doc-service-value textEdit"
            value="<%= FilesLinkUtility.DocServiceUrlInternal.HtmlEncode() %>" placeholder="https://<editors-dns-name>/" />
        <div class="gray-text"><%= string.Format(Resource.DocServiceUrlExample, "https://&lt;editors-dns-name&gt;/") %> </div>
    </div>

    <div class="doc-service-item">
        <div class="header-base-small"><%= Resource.DocServiceUrlPortal2 %></div>
        <input id="docServiceUrlPortal" type="text" class="doc-service-value textEdit"
            value="<%= FilesLinkUtility.DocServicePortalUrl.HtmlEncode() %>" placeholder="<%= CommonLinkUtility.ServerRootPath %>/" />
        <div class="gray-text"><%= string.Format(Resource.DocServiceUrlExample, CommonLinkUtility.ServerRootPath + "/") %> </div>
    </div>

    <div class="middle-button-container">
        <span id="docServiceButtonSave" class="button blue"><%= Resource.SaveButton %></span>
        <span class="splitter-buttons"></span>
        <span id="docServiceButtonReset" class="button gray"><%= Resource.ResetButton %></span>
    </div>
</div>
<div class="settings-help-block">
    <p><%= String.Format(Resource.DocServiceUrlHelp2.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
</div>
