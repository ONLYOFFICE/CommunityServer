<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FilesDashboardEmptyScreen.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.FilesDashboardEmptyScreen" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>


<div id="dashboardBackdrop" class="backdrop display-none" blank-page=""></div>

<div id="dashboardContent" blank-page="" class="dashboard-center-box files display-none">
    <a class="close">&times;</a>
    <div class="content">
        <div class="slick-carousel">
            <div class="module-block slick-carousel-item clearFix">
                <div class="img work-with-office-docs"></div>
                <div class="text">
                    <div class="title"><%= FilesCommonResource.DashboardWorkWithOfficeDocs %></div>
                    <p><%= FilesCommonResource.DashboardWorkWithOfficeDocsFirstLine %></p>
                    <p><%= FilesCommonResource.DashboardWorkWithOfficeDocsSecondLine %></p>
                    <p><%= FilesCommonResource.DashboardWorkWithOfficeDocsThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img share-files"></div>
                <div class="text">
                    <div class="title"><%= FilesCommonResource.DashboardShareFiles %></div>
                    <p><%= FilesCommonResource.DashboardShareFilesFirstLine %></p>
                    <p><%= FilesCommonResource.DashboardShareFilesSecondLine %></p>
                    <p><%= FilesCommonResource.DashboardShareFilesThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img make-co-editing-comfy"></div>
                <div class="text">
                    <div class="title"><%= FilesCommonResource.DashboardMakeCoEditingComfy %></div>
                    <p><%= FilesCommonResource.DashboardMakeCoEditingComfyFirstLine %></p>
                    <p><%= FilesCommonResource.DashboardMakeCoEditingComfySecondLine %></p>
                    <p><%= FilesCommonResource.DashboardMakeCoEditingComfyThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img use-more-collaboration-tools"></div>
                <div class="text">
                    <div class="title"><%= FilesCommonResource.DashboardUseMoreCollaborationTools %></div>
                    <p><%= FilesCommonResource.DashboardUseMoreCollaborationToolsFirstLine %></p>
                    <p><%= FilesCommonResource.DashboardUseMoreCollaborationToolsSecondLine %></p>
                    <p><%= FilesCommonResource.DashboardUseMoreCollaborationToolsThirdLine %></p>
                </div>
            </div>
        </div>
    </div>
    <div class="dashboard-buttons">
        <a class="button huge create-button" onclick="jq('[blank-page]').remove();" href="#<%= ASC.Web.Files.Classes.Global.FolderMy %>">
            <%= FilesCommonResource.DashboardGoToMyDocuments %>
        </a>
    </div>
</div>