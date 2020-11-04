<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoadPhotoControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.LoadPhotoControl" %>
<%@ Import Namespace="ASC.ActiveDirectory.Base.Settings" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace=" ASC.Web.Studio.Core" %>

<div id="userPhotoDialog" class="popupContainerClass display-none">
    <div class="containerHeaderBlock">
        <table>
            <tbody>
                <tr>
                    <td><%= Resource.EditPhoto %></td>
                    <td class="popupCancel">
                        <div class="cancelButton cancel-btn">×</div>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    <div class="containerBodyBlock">
        <div class="empty-block <%= HasAvatar ? "display-none" : "" %>">
            <div class="header-base gray-text"><%= Resource.PhotoNotSelected %></div>
			<a class="link dotline upload-btn"><%= Resource.UploadButton %></a>
            <span class="splitter-buttons"></span>
            <span class="gray-text"><%= FileSizeComment.GetFileImageSizeNote(Resource.PhotoMaxSize, false) %></span>
		</div>
        <div class="preview-block <%= HasAvatar ? "" : "display-none" %>">
            <div class="preview-image-container">
                <img src="<%= MainImgUrl %>" alt=""/>
                <div class="empty-selection display-none"><%= ResourceJS.EmptySelectedArea %></div>
            </div>
            <% if (!IsLdap || !LdapSettings.GetImportedFields.Contains(LdapSettings.MappingFields.AvatarAttribute))
            { %>
                <div class="preview-buttons-container clearFix">
                    <a class="link dotline delete-btn"><%= Resource.DeleteButton %></a>
                    <a class="link dotline upload-btn"><%= Resource.UploadButton %></a>
                    <span class="splitter-buttons"></span>
                    <span class="gray-text"><%= FileSizeComment.GetFileImageSizeNote(Resource.PhotoMaxSize, false) %></span>
                </div>
            <%} %>
        </div>
        <%--<div class="thumbnail-block">
        <% foreach (var item in ThumbnailsData.ThumbnailList.Where(x => x.Size != ASC.Web.Core.Users.UserPhotoManager.MaxFotoSize && x.Size != ASC.Web.Core.Users.UserPhotoManager.RetinaFotoSize))
            { %>
            <div class='thumbnail-preview-img' style='height:<%= item.Size.Height %>px; width:<%= item.Size.Width %>px;'>
                <img src='<%= item.ImgUrl %>' alt=""/>
            </div>
        <% } %>
        </div>--%>
        <div class="middle-button-container">
            <a class="button blue middle save-btn"><%= Resource.SaveButton %></a>
            <span class="splitter-buttons"></span>
            <a class="button gray middle cancel-btn"><%= Resource.CancelButton %></a>
        </div>
    </div>
</div>