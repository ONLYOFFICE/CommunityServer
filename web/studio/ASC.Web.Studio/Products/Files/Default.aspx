<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Files._Default" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Helpers" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Utils" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTHeaderContent">
    <% if (!CoreContext.Configuration.CustomMode)
       { %>
    <% var uri = new UriBuilder(Request.GetUrlRewriter())
       {
           Path = "",
           Query = "email=" + HttpUtility.UrlEncode(CurrentUser.Email),
       };
    %>
    <meta name="apple-itunes-app" content="app-id=944896972, app-argument=<%= HttpUtility.HtmlEncode(uri) %>" />
    <% } %>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="CreateButtonContent">
    <div class="page-menu">
        <asp:PlaceHolder ID="CreateButtonHolder" runat="server"></asp:PlaceHolder>
    </div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="BTSidePanel">
    <div class="page-menu">
        <asp:PlaceHolder ID="CommonSideHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="AppBannerHolder" runat="server" />
    </div>

    <% if (!Desktop && CoreContext.Configuration.Personal && false)
       { %>
    <a href="#more" class="morefeatures-link banner-link gray-text"><%= string.Format(FilesUCResource.MoreFeatures, "<br>", "<span>", "</span>") %></a>
    <% } %>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="FilterContent">
    <asp:PlaceHolder ID="FilterHolder" runat="server" />
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>

    <%--Panels--%>

    <div id="settingCommon">
        <span class="header-base"><%= FilesUCResource.SettingUpdateIfExist %></span>
        <% if (FileConverter.EnableAsUploaded)
           { %>
        <br />
        <br />
        <input type="checkbox" id="cbxStoreOriginal" class="store-original on-off-checkbox" <%= FilesSettings.StoreOriginalFiles ? "checked=\"checked\"" : string.Empty %> />
        <label for="cbxStoreOriginal">
            <%= FilesUCResource.ConfirmStoreOriginalUploadCbxLabelText %>
        </label>
        <% } %>
        <br />
        <br />
        <input type="checkbox" id="cbxDeleteConfirm" class="on-off-checkbox" <%= FilesSettings.ConfirmDelete ? "checked=\"checked\"" : string.Empty %> />
        <label for="cbxDeleteConfirm">
            <%= FilesUCResource.ConfirmDelete %>
        </label>

        <% if (!CurrentUser.IsVisitor())
           { %>
        <br />
        <br />
        <br />
        <span class="header-base"><%= FilesUCResource.SettingVersions %></span>
        <br />
        <br />
        <input type="checkbox" id="cbxUpdateIfExist" class="update-if-exist on-off-checkbox" <%= FilesSettings.UpdateIfExist ? "checked=\"checked\"" : string.Empty %> />
        <label for="cbxUpdateIfExist">
            <%= string.Format(FilesUCResource.ConfirmUpdateIfExist, "<br/><span class=\"text-medium-describe\">", "</span>") %>
        </label>
        <br />
        <br />
        <input type="checkbox" id="cbxForcesave" class="on-off-checkbox" <%= FilesSettings.Forcesave ? "checked='checked'" : "" %> />
        <label for="cbxForcesave">
            <%= FilesUCResource.SettingForcesave %>
        </label>
        <br />
        <br />
        <br />
        <span class="header-base"><%= FilesUCResource.SettingSection %></span>
        <br />
        <br />
        <input type="checkbox" id="cbxFavorites" class="on-off-checkbox" <%= FilesSettings.FavoritesSection ? "checked=\"checked\"" : string.Empty %> />
        <label for="cbxFavorites">
            <%= FilesUCResource.SettingFavorite %>
        </label>
        <br />
        <br />
        <input type="checkbox" id="cbxRecent" class="on-off-checkbox" <%= FilesSettings.RecentSection ? "checked=\"checked\"" : string.Empty %> />
        <label for="cbxRecent">
            <%= FilesUCResource.SettingRecent %>
        </label>
        <% if (FileUtility.ExtsWebTemplate.Any())
           { %>
        <br />
        <br />
        <input type="checkbox" id="cbxTemplates" class="on-off-checkbox" <%= FilesSettings.TemplatesSection ? "checked=\"checked\"" : string.Empty %> />
        <label for="cbxTemplates">
            <%= FilesUCResource.SettingTemplates %>
        </label>
        <% } %>
        <% } %>
    </div>

    <% if (Global.IsAdministrator) 
       { %>
    <div id="settingAdmin">
        <span class="header-base"><%= FilesUCResource.SettingVersions %></span>
        <br />
        <br />
        <input type="checkbox" id="cbxStoreForcesave" class="on-off-checkbox" <%= FilesSettings.StoreForcesave ? "checked='checked'" : "" %> />
        <label for="cbxStoreForcesave">
            <%= FilesUCResource.SettingStoreForcesave %>
        </label>

        <% if (!CoreContext.Configuration.Personal && ThirdpartyConfiguration.SupportInclusion && !Desktop) 
           { %>
        <br />
        <br />
        <br />
        <span class="header-base"><%= FilesUCResource.ThirdPartyAccounts %></span>
        <br />
        <br />
        <input type="checkbox" id="cbxEnableSettings" class="on-off-checkbox" <%= FilesSettings.EnableThirdParty ? "checked='checked'" : "" %> />
        <label for="cbxEnableSettings">
            <%= FilesUCResource.ThirdPartyEnableSettings %>
        </label>
        <% } %>
    </div>
    <% } %>

    <div id="settingThirdPartyPanel">
        <asp:PlaceHolder runat="server" ID="SettingPanelHolder"></asp:PlaceHolder>
    </div>

    <div id="helpPanel"></div>

    <asp:PlaceHolder ID="ThirdPartyScriptsPlaceHolder" runat="server"></asp:PlaceHolder>

</asp:Content>
