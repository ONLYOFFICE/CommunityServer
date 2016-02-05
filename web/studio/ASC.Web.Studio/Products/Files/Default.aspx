<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Files._Default" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Utils" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTSidePanel">
    <div class="page-menu">
        <asp:PlaceHolder ID="CommonSideHolder" runat="server"></asp:PlaceHolder>
    </div>

    <% if (CoreContext.Configuration.Personal)
       { %>
    <a href="#more" class="morefeatures-link banner-link gray-text"><%= string.Format(FilesUCResource.MoreFeatures, "<br>", "<span>", "</span>") %></a>
    <% } %>

    <% if (DisplayAppsBanner)
       { %>
    <a href="https://itunes.apple.com/app/onlyoffice-documents/id944896972?mt=8" target="_blank"
        class="mobile-app-banner banner-link gray-text"><%= string.Format(FilesUCResource.AppStore, "<br>", "<span>", "</span>") %></a>
    <% } %>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>

    <%--Panels--%>

    <div id="settingCommon">
        <% if (Global.IsAdministrator && !CoreContext.Configuration.Personal && ImportConfiguration.SupportInclusion && !Desktop)%>
        <% { %>
        <span class="header-base"><%= FilesUCResource.ThirdPartyAccounts %></span>
        <br />
        <br />
        <label>
            <input type="checkbox" id="cbxEnableSettings" class="checkbox" <%= FilesSettings.EnableThirdParty ? "checked='checked'" : "" %> />
            <%= FilesUCResource.ThirdPartyEnableSettings %>
        </label>
        <br />
        <br />
        <br />
        <% } %>

        <span class="header-base"><%= FilesUCResource.SettingUpdateIfExist %></span>
        <br />
        <br />
        <label>
            <input type="checkbox" class="update-if-exist float-left checkbox" <%= FilesSettings.UpdateIfExist ? "checked=\"checked\"" : string.Empty %> />
            <%= string.Format(FilesUCResource.ConfirmUpdateIfExist, "<br/><span class=\"text-medium-describe\">", "</span>")%>
        </label>
        <% if (FileConverter.EnableAsUploaded)
           { %>
        <br />
        <br />
        <label>
            <input type="checkbox" class="store-original checkbox" <%= FilesSettings.StoreOriginalFiles ? "checked=\"checked\"" : string.Empty %> />
            <%= FilesUCResource.ConfirmStoreOriginalUploadCbxLabelText %>
        </label>
        <% } %>
    </div>

    <div id="settingThirdPartyPanel">
        <asp:PlaceHolder runat="server" ID="SettingPanelHolder"></asp:PlaceHolder>
    </div>

    <div id="helpPanel"></div>

    <asp:PlaceHolder ID="ThirdPartyScriptsPlaceHolder" runat="server"></asp:PlaceHolder>

</asp:Content>
