<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Mail.MailPage" MasterPageFile="~/Masters/BaseTemplate.master" %>

<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeaderContent" runat="server">
    <script>
        var service_ckeck_time = "<%=GetServiceCheckTimeout()%>";
        var FilterByGroupLocalize = "<asp:Localize runat=server ID="PeopleGroupLocalize"></asp:Localize>";
        var MailFaqUri = "<%=GetMailFaqUri()%>";
        var MailSupportUrl = "<%=GetMailSupportUri()%>";
        var crm_available = "<%=IsCrmAvailable()%>" === "True" ? true : false;
        var tl_available = "<%=IsPeopleAvailable()%>" === "True" ? true : false;
        var MailDownloadHandlerUri = "<%=GetMailDownloadHandlerUri()%>";
        var MailDownloadAllHandlerUri = "<%=GetMailDownloadAllHandlerUri()%>";
        var MailViewDocumentHandlerUri = "<%=GetMailViewDocumentHandlerUri()%>";
        var MailEditDocumentHandlerUri = "<%=GetMailEditDocumentHandlerUri()%>";
    </script>

</asp:Content>

<asp:Content ID="MailSideContent" ContentPlaceHolderID="SidePanel" runat="server">
    <div id="loading-mask" class="loadmask">
        <div class="body" align="center">
            <div class="logo">
                <div class="text"><%= MailResource.LoadingMail %></div>
            </div>
        </div>
    </div>

    <div id="manageWindow" style="display: none">
        <sc:Container ID="_manageFieldPopup" runat="server">
            <Header>
            </Header>
            <Body>
            </Body>
        </sc:Container>
    </div>
    <div class="hidden" id="commonPopup">
        <sc:Container ID="_commonPopup" runat="server">
            <Header>
            </Header>
            <Body>
            </Body>
        </sc:Container>
    </div>
    <div class="page-menu">
      <ul class="menu-actions">
        <li class="menu-main-button middle create-new" id="createNewMailBtn">
            <span class="main-button-text"><%= MailResource.WriteNewLetterBtnLabel %></span>
        </li>
        <li id="check_email_btn" title="<%= MailResource.RefreshBtnHint %>" class="menu-gray-button">
            <span class="mail"></span>
        </li>
      </ul>
      <ul class="menu-list" id="foldersContainer">
      </ul>
      <asp:PlaceHolder ID="MailSidePanelContainer" runat="server" />
      
      <div id="accountsPanel" class="expandable top-margin-menu hidden">
        <div class="content"><ul class="menu-list accounts"></ul></div>
        <div class="more hidden left-margin">
          <div class="shadow"></div>
          <div class="text"><%= MailResource.ShowMoreAccounts %></div>
        </div>
    </div>

      <ul class="menu-list top-margin-menu" id="customContactPanel">
        <li class="menu-item none-sub-list" runat="server" id="tlContactsContainer">
          <span class="menu-item-icon group"></span>
          <a class="menu-item-label outer-text text-overflow" id="teamlab" href="#tlcontact">
            <span class="menu-item-label inner-text"><%= MailScriptResource.TeamLabContactsLabel %></span>
          </a>
        </li>
        <li class="menu-item none-sub-list" runat="server" id="crmContactsContainer">
          <span class="menu-item-icon company"></span>
          <a class="menu-item-label outer-text text-overflow" id="crm" href="#crmcontact">
            <span class="menu-item-label inner-text"><%= MailScriptResource.CRMContactsLabel %></span>
          </a>
        </li>
      </ul>
      <ul class="menu-list with-expander">
        <li class="menu-item sub-list add-block">
          <div class="category-wrapper">
            <span class="expander"></span>
            <a class="menu-item-label outer-text text-overflow" id="settingsLabel" href="javascript:void(0);">
                <span class="menu-item-icon settings"></span>
                <span class="menu-item-label inner-text gray-text settings" ><%= MailResource.Settings %></span>
            </a>
          </div>
          <ul class="menu-sub-list" id="settingsContainer">
            <li class="menu-sub-item">
              <a class="menu-item-label outer-text text-overflow" id="accountsSettings" href="#accounts">
                <span class="menu-item-label inner-text"><%= MailResource.AccountsSettingsLabel %></span>
              </a>
            </li>
            <li class="menu-sub-item">
              <a class="menu-item-label outer-text text-overflow" id="tagsSettings" href="#tags">
                <span class="menu-item-label inner-text"><%= MailResource.TagsSettingsLabel %></span>
              </a>
            </li>
            <% if (IsAdministrator){%>
            <li class="menu-sub-item">
              <a class="menu-item-label outer-text text-overflow" href="<%= VirtualPathUtility.ToAbsolute("~/management.aspx") + "?type=" + (int)ASC.Web.Studio.Utility.ManagementType.AccessRights + "#mail" %>">
                <span class="menu-item-label inner-text"><%= MailResource.AccessRightsSettings %></span>
              </a>
            </li>
            <% }%>
          </ul>
        </li>
        <asp:PlaceHolder runat="server" ID="sideHelpCenter"></asp:PlaceHolder>
        <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
      </ul>
    </div>
</asp:Content>

<asp:Content ID="MailPageContent" ContentPlaceHolderID="PageContent" runat="server">
  <asp:PlaceHolder ID="BlankModalPH" runat="server" />
  <div class="mainContainerClass">
    <asp:PlaceHolder ID="MailControlContainer" runat="server" />
    <div id="blankPage" class="hidden page_content"></div>
    <div id="tagsColorsPanel" class="actionPanel">
        <div class="popup-corner colors"></div>
        <div id="tagsColorsContent">
          <div class="tag1" colorstyle="1"></div><div class="tag2" colorstyle="2"></div><div class="tag3" colorstyle="3"></div><div class="tag4" colorstyle="4"></div><div class="tag5" colorstyle="5"></div><div class="tag6" colorstyle="6"></div><div class="tag7" colorstyle="7"></div><div class="tag8" colorstyle="8"></div>
          <div class="tag9" colorstyle="9"></div><div class="tag10" colorstyle="10"></div><div class="tag11" colorstyle="11"></div><div class="tag12" colorstyle="12"></div><div class="tag13" colorstyle="13"></div><div class="tag14" colorstyle="14"></div><div class="tag15" colorstyle="15"></div><div class="tag16" colorstyle="16"></div>
        </div>
    </div>
  </div>
</asp:Content>

<asp:Content ID="clientTemplatesResourcesPlaceHolder" ContentPlaceHolderID="clientTemplatesResourcesPlaceHolder" runat="server">
</asp:Content>

