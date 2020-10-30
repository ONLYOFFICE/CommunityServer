<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportUsers.ascx.cs" Inherits="ASC.Web.People.UserControls.ImportUsers" %>
<%@ Import Namespace="ASC.Core.Billing" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.ThirdParty.ImportContacts" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Management" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id='importAreaBlock' class='importAreaBlock'>
    <div id="wizardSteps"></div><br /><br />
    <div id="file-option">
        <div class="file-name-select">
        <div class="file-name">
        <span><%= Resource.ImportFileName %></span><span id="file-name"></span>
        </div>
        <div id="importFrom">
            <span class="link dotline dArrow"><%= Resource.ImportSelectAnotherSource %></span>
        </div>
        </div>
        <div id="source">
            <span>
                <%= Resource.ImportEncoding %>:
            </span>
            <span class="link dotline dArrow"></span>
        </div>
        <div id="separator">
            <span>
                <%= Resource.ImportDelimiter %>:
            </span>
            <span class="link dotline dArrow"></span>
        </div>
        <div id="delimiter">
            <span>
                <%= Resource.ImportTextDelimiter %>:
            </span>
            <span class="link dotline dArrow"></span>
        </div><br /><br />
        <div class="chekCustomHeader">
            <input type="checkbox" id="addDefaultHeader" />
            <label><%= Resource.NameInFirstLine %></label>
        </div>
    </div>

    <div class="source-text"><%= Resource.ImportWizardSecondStep %></div>
    <div id="importUsers">
        <div class="blockUI blockMsg blockElement" id="upload"><img /></div>
        <div class="clearFix importUsers" id="panel">
            <div class="selectSource"><span><%= Resource.SelectDataSource %></span></div>
            <div class="import-from-file" id="import_flatUploader">
                <div class="filetemp">
                    <%= Resource.ImportContactsFromFile %>
                </div>
                <p><%= string.Format(Resource.ImportFileCSV.HtmlEncode(), "<span class=\"swgIcons outlook\"><span>","<span class=\"swgIcons google\"><span>","<span class=\"swgIcons thunderbird\"><span>")%></p>
            </div>
            <div id="fromHand" class="file" title="<%= Resource.ImportContactsFromFile %>">
                <p><%= Resource.ImportButtonManually %></p>
            </div>
            <div style="z-index: 1;" class="frame">
                <% if (Import.Enable)
                    { %>
                <iframe src="<%= Import.Location %>" style="border: none; height: 64px; width: 382px; overflow: hidden; filter: alpha(opacity=100); float: left;" frameborder="0" scrolling="no" id="ifr"></iframe>
                <% } %>
            </div>
        </div>

    <div id="wizard_users">
        <div id="deleteUserBlock">
            <div class="checkAll">
                <input type="checkbox" id="checkAll" />
            </div>
            <input type="button" class="button gray" id="deleteUserButton" value="   <%= Resource.DeleteButton %>" />
        </div>
        <div class="clearFix" id="addUserBlock">
            <div class="nameBox">
                <div class="error" id="fnError">
                    <%= Resource.ImportContactsErrorFirstName %>
                </div>
                <input type="text" id="firstName" autocomplete="off" class="textEdit first" placeholder="<%= Resource.FirstName %>" maxlength="64" />
            </div>
            <div class="lastnameBox">
                <div class="error" id="lnError">
                   <%= Resource.ImportContactsErrorLastName %>
                </div>
                <input type="text" id="lastName" autocomplete="off" class="textEdit" placeholder="<%= Resource.LastName %>" maxlength="64" />
            </div>
            <div class="emailBox">
                <div class="error" id="eaError">
                    <%= Resource.ImportContactsErrorEmail %>
                </div>
                <input type="text" id="email" autocomplete="off" class="textEdit last" placeholder="<%= Resource.Email %>" maxlength="64" />
            </div>
            <div class="btn">
                <div class="btnBox">
                    <input type="button" class="button gray" id="saveSettingsBtn" />
                </div>
            </div>
        </div>
        <table class="tableHeader">
        </table>
        <div class="restr">
            <table id="userList">
            </table>
            <div class="importErrorBox">
                <div class="errorBubble"></div>
            </div>
        </div>
    </div>
    <div class="desc" style="display:none;">
        <label>
            <input type="checkbox" id="importAsCollaborators"
            <%= EnableInviteLink ? "" : "disabled=\"disabled\" checked=\"checked\"" %> />
            <%= CustomNamingPeople.Substitute<Resource>("InviteUsersAsCollaborators").HtmlEncode() %>
        </label>
        <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'answerForHelpInviteGuests1'});"></div>
        <div class="popup_helper" id="answerForHelpInviteGuests1">
            <p>
                <%= string.Format(CustomNamingPeople.Substitute<Resource>("NoteInviteCollaborator").HtmlEncode(), "<b>","</b>")%>
                <% if (TenantExtra.EnableTarrifSettings && TenantExtra.GetTenantQuota().ActiveUsers != LicenseReader.MaxUserCount)
                   { %>
                <%= Resource.NotePriceCollaborator %>
                <% } %>
                <% if (!string.IsNullOrEmpty(HelpLink))
                           { %>
                <a href="<%= HelpLink %>" target="_blank">
                    <%= Resource.LearnMore %></a>
                <% } %>
            </p>
        </div>
    </div>
    <div class="middle-button-container" style="display:none;">
        <input type="button" id="next-step" class="button blue big impBtn" value="<%=Resource.GoToContactListBtn %>" />
        <span class="splitter-buttons"></span>
        <input type="button" id="import-btn" class="button blue big impBtn" value="<%=Resource.AddToPortalBtn %>" />
        <span class="splitter-buttons"></span>
        <input type="button" id="last-step" class="button blue big" value="<%=Resource.ReturnToImportSettingsBtn%>" />
    </div>
</div>

<div id="importUserLimitPanel">
    <sc:Container ID="limitPanel" runat="server">
        <Header><%=Resource.ImportUserLimitTitle%></Header>
        <Body>
            <div class="tariff-limitexceed-users">
                <div id="importUserLimitHeader" class="header-base-medium">
                    <%= PeopleLimit > 0 ? String.Format(Resource.ImportUserLimitHeader.HtmlEncode(), PeopleLimit) : Resource.ImportUserOverlimitHeader.HtmlEncode()%>
                </div>
                <br />
                <div>
                    <%= FreeTariff && !string.IsNullOrEmpty(HelpLink) ?
                        string.Format(Resource.ImportUserOpenPortalLimitReason.HtmlEncode(),
                            "<br /><a class='link underline' href='" + HelpLink + "/gettingstarted/configuration.aspx#PublicPortals' target='_blank'>",
                            "</a>") :
                        Resource.ImportUserLimitReason.HtmlEncode() %>
                </div>
            </div>
            <div class="middle-button-container">
                <% if (TenantExtra.EnableTarrifSettings)
                   { %>
                <a class="blue button medium" href="<%= TenantExtra.GetTariffPageLink() %>">
                    <%= UserControlsCommonResource.UpgradeButton %></a>
                <span class="splitter-buttons"></span>
                <% } %>
                    <a id="import-limit-btn" class="<%= TenantExtra.EnableTarrifSettings ? "gray" : "blue" %> button">
                        <%= CustomNamingPeople.Substitute<Resource>("AddUsersCaption").HtmlEncode() %>
                    </a>
                    <span class="splitter-buttons"></span>
                    <a id="import-limit-cancel-btn" class="button gray">
                        <%= Resource.ImportContactsCancelButton %>
                    </a>
            </div>
        </Body>
    </sc:Container>
</div>

<table id="donor" class="display-none">
    <tr>
        <td class="fistable">
            <div class="desc holder">
                <%= CustomNamingPeople.Substitute<Resource>("ImportContactsFirstable").HtmlEncode() %>
            </div>
        </td>
    </tr>
</table>

<sc:Container ID="icon" runat="server">
    <Header><%= Resource.ImportContactsErrorHeader %></Header>
    <Body>
        <div id="infoMessage">
        </div>
        <div class="clearFix okImportUsers">
            <a class="button blue">
                <%= Resource.ImportContactsOkButton %>
            </a>
        </div>
    </Body>
</sc:Container>
<div class="blockUpload display-none" id="blockProcess"></div>

    <asp:PlaceHolder ID="Tariff" runat="server"></asp:PlaceHolder>
</div>