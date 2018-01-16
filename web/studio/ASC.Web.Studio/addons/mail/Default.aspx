<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Assembly Name="ASC.Api.Mail" %>

<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Mail.MailPage" MasterPageFile="~/Masters/BaseTemplate.master" %>

<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeaderContent" runat="server">
    <% if (IsBlank)
       { %>
        <style type="text/css">
            #studioPageContent .studio-top-panel.mainPageLayout,
            #studioPageContent .mainPageLayout {
                display: none;
            }
        </style>
    <% } %>
    
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content ID="MailSideContent" ContentPlaceHolderID="SidePanel" runat="server">
    <div id="manageWindow" style="display: none">
        <sc:Container ID="_manageFieldPopup" runat="server">
            <header>
            </header>
            <body>
            </body>
        </sc:Container>
    </div>

    <div class="hidden" id="commonPopup">
        <sc:Container ID="_commonPopup" runat="server">
            <header>
            </header>
            <body>
            </body>
        </sc:Container>
    </div>
    
    <div class="page-menu">
        <ul class="menu-actions">
            <li class="menu-main-button middle create-new" id="createNewMailBtn">
                <span class="main-button-text"><%: MailResource.WriteNewLetterBtnLabel %></span>
            </li>
            <li id="check_email_btn" title="<%: MailResource.RefreshBtnHint %>" class="menu-gray-button">
                <span class="mail"></span>
            </li>
        </ul>
        <ul class="menu-list" id="foldersContainer">
            <li class="menu-item none-sub-list" folderid="1" unread="0">
                <table>
                    <tr>
                        <td width="100%">
                            <a class="menu-item-label outer-text text-overflow" href="#inbox" folderid="1">
                                <span class="menu-item-icon inbox"></span>
                                <span class="menu-item-label inner-text"><%= MailResource.FolderNameInbox %></span>
                            </a>
                        </td>
                        <td>
                            <div class="lattersCount counter"></div>
                        </td>
                    </tr>
                </table>
            </li>
            <li class="menu-item none-sub-list" folderid="2" unread="0">
                <table>
                    <tr>
                        <td width="100%">
                            <a class="menu-item-label outer-text text-overflow" href="#sent" folderid="2">
                                <span class="menu-item-icon sent"></span>
                                <span class="menu-item-label inner-text"><%= MailResource.FolderNameSent %></span>
                            </a>
                        </td>
                        <td>
                            <div class="lattersCount counter"></div>
                        </td>
                    </tr>
                </table>
            </li>
            <li class="menu-item none-sub-list" folderid="3" unread="0">
                <table>
                    <tr>
                        <td width="100%">
                            <a class="menu-item-label outer-text text-overflow" href="#drafts" folderid="3">
                                <span class="menu-item-icon drafts"></span>
                                <span class="menu-item-label inner-text"><%= MailResource.FolderNameDrafts %></span>
                            </a>
                        </td>
                        <td>
                            <div class="lattersCount counter"></div>
                        </td>
                    </tr>
                </table>
            </li>
            <li class="menu-item none-sub-list" folderid="4" unread="0">
                <table>
                    <tr>
                        <td width="100%">
                            <a class="menu-item-label outer-text text-overflow" href="#trash" folderid="4">
                                <span class="menu-item-icon trash"></span>
                                <span class="menu-item-label inner-text"><%= MailResource.FolderNameTrash %></span>
                            </a>
                        </td>
                        <td>
                            <div class="lattersCount counter"></div>
                        </td>
                    </tr>
                </table>
            </li>
            <li class="menu-item none-sub-list" folderid="5" unread="0">
                <table>
                    <tr>
                        <td width="100%">
                            <a class="menu-item-label outer-text text-overflow" href="#spam" folderid="5">
                                <span class="menu-item-icon spam"></span>
                                <span class="menu-item-label inner-text"><%= MailResource.FolderNameSpam %></span>
                            </a>
                        </td>
                        <td>
                            <div class="lattersCount counter"></div>
                        </td>
                    </tr>
                </table>
            </li>
        </ul>
        <asp:PlaceHolder ID="MailSidePanelContainer" runat="server" />
        

        <div id="accountsPanel" class="expandable top-margin-menu hidden" <% if (Accounts.Count > 1)
                                                                             { %> style="display: block;" <% } %>>
            <div class="content" style="max-height: 250px;">
                <ul class="menu-list accounts">
                </ul>
            </div>
            <div class="more hidden left-margin">
                <div class="shadow"></div>
                <div class="text"><%: MailResource.ShowMoreAccounts %></div>
            </div>
        </div>

        <ul id="menuContactsSettings" class="menu-list with-expander">
            <li  class="menu-item sub-list add-block">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" id="addressBookLabel" href="javascript:void(0);">
                     <span class="menu-item-icon group"></span><span class="menu-item-label inner-text"><%= MailResource.AddressBook %></span>
                </a>
            </div>
            <ul id="customContactPanel" class="menu-sub-list">
                <li class="menu-sub-item" runat="server" id="customContactsContainer">
                    <a class="menu-item-label outer-text text-overflow" id="custom" href="#customcontact">
                        <span class="menu-item-label inner-text"><%: MailScriptResource.PersonalContactsLabel %></span>
                    </a>
                </li>

                <li class="menu-sub-item" runat="server" id="tlContactsContainer">
                    <a class="menu-item-label outer-text text-overflow" id="teamlab" href="#tlcontact">
                        <span class="menu-item-label inner-text"><%: MailScriptResource.TeamLabContactsLabel %></span>
                    </a>
                </li>
                <li class="menu-sub-item" runat="server" id="crmContactsContainer">
                    <a class="menu-item-label outer-text text-overflow" id="crm" href="#crmcontact">
                        <span class="menu-item-label inner-text"><%: MailScriptResource.CRMContactsLabel %></span>
                    </a>
                </li>
            </ul>
            </li>
        </ul>


        <ul class="menu-list with-expander">
            <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>
            <li class="menu-item sub-list add-block open-by-default">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow" id="settingsLabel" href="javascript:void(0);">
                        <span class="menu-item-icon settings"></span>
                        <span class="menu-item-label inner-text gray-text settings"><%: MailResource.Settings %></span>
                    </a>
                </div>
                <ul class="menu-sub-list" id="settingsContainer">
                    <li class="menu-sub-item">
                        <a class="menu-item-label outer-text text-overflow" id="commonSettings" href="#common">
                            <span class="menu-item-label inner-text"><%: MailResource.CommonSettingsLabel %></span>
                        </a>
                    </li>
                    <% if (IsAdministrator && !IsPersonal && IsTurnOnServer())
                       { %>
                        <li class="menu-sub-item">
                            <a class="menu-item-label outer-text text-overflow" id="adminSettings" href="#administration">
                                <span class="menu-item-label inner-text"><%: MailScriptResource.AdministrationLabel %></span>
                            </a>
                        </li>
                    <% } %>
                    <li class="menu-sub-item">
                        <a class="menu-item-label outer-text text-overflow" id="accountsSettings" href="#accounts">
                            <span class="menu-item-label inner-text"><%: MailResource.AccountsSettingsLabel %></span>
                        </a>
                    </li>
                    <li class="menu-sub-item">
                        <a class="menu-item-label outer-text text-overflow" id="tagsSettings" href="#tags">
                            <span class="menu-item-label inner-text"><%: MailResource.TagsSettingsLabel %></span>
                        </a>
                    </li>
                    <% if (IsAdministrator && !IsPersonal)
                       { %>
                        <li class="menu-sub-item">
                            <a class="menu-item-label outer-text text-overflow" href="<%= VirtualPathUtility.ToAbsolute("~/management.aspx") + "?type=" + (int)ASC.Web.Studio.Utility.ManagementType.AccessRights + "#mail" %>">
                                <span class="menu-item-label inner-text"><%: MailResource.AccessRightsSettings %></span>
                            </a>
                        </li>
                    <% } %>
                </ul>
            </li>
            <% if (!IsPersonal)
               { %>
                <asp:PlaceHolder runat="server" ID="sideHelpCenter"></asp:PlaceHolder>
                <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
            <% } %>
        </ul>
    </div>
</asp:Content>

<asp:Content ID="MailPageContent" ContentPlaceHolderID="PageContent" runat="server">
    <asp:PlaceHolder ID="BlankModalPH" runat="server" />
    <div class="mainContainerClass">
        <asp:PlaceHolder ID="MailControlContainer" runat="server" />
        <div id="blankPage" class="hidden page_content"></div>
        <div id="tagsColorsPanel" class="actionPanel">
            <div id="tagsColorsContent">
                <div class="tag1" colorstyle="1"></div>
                <div class="tag2" colorstyle="2"></div>
                <div class="tag3" colorstyle="3"></div>
                <div class="tag4" colorstyle="4"></div>
                <div class="tag5" colorstyle="5"></div>
                <div class="tag6" colorstyle="6"></div>
                <div class="tag7" colorstyle="7"></div>
                <div class="tag8" colorstyle="8"></div>
                <div class="tag9" colorstyle="9"></div>
                <div class="tag10" colorstyle="10"></div>
                <div class="tag11" colorstyle="11"></div>
                <div class="tag12" colorstyle="12"></div>
                <div class="tag13" colorstyle="13"></div>
                <div class="tag14" colorstyle="14"></div>
                <div class="tag15" colorstyle="15"></div>
                <div class="tag16" colorstyle="16"></div>
            </div>
        </div>
    </div>
</asp:Content>