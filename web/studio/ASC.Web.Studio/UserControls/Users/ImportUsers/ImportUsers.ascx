<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportUsers.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Users.ImportUsers" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Management" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<!--[if IE]>
<style>
#wizard_users .checkall
{
    border:0;
    margin-top:13px;
}
</style>
<![endif]-->

<!--[if lte IE 8]>
<style>
.fistable {
   width:540px;
   padding: 0 12% 0 22%;
   cellpadding:20%;
   display:block;
}
#wizard_users #userList .userItem .name {
    width: 436px;
    padding-left:6px;
}
.fistable .desc {
    width:450px;
}
#wizard_users {
    width:750px;
    padding-left:6px;
}
#wizard_users #userList .userItem .check {
    padding:0 0 0 2px;
}
</style>
<![endif]-->

<!--[if IE 9]>
<style>
#wizard_users #userList .userItem .check input {
    margin:0 3px 0 2px;
}

#wizard_users #userList .userItem .name {
    width:438px;
}

#wizard_users #userList .userItem .name .firstname,
#wizard_users #userList .userItem .name .lastname
{
    float:left;
    padding-right:14px;
    vertical-align:top;
}
</style>
<![endif]-->

<div id="importUsers">
    <div class="blockUI blockMsg blockElement" id="upload"><img/></div>
        <div class="desc">
            <%= String.Format(ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resource>("ImportContactsDescription"),"<span class=\"starStyle\">*</span>")%>
        </div>
        <div class="smallDesc"><span class="starStyle">*</span> <%= Resource.ImportContactsSmallDescription %></div>
        <div class="clearFix importUsers" id="panel">
            <div class="frame <%= MobileDetector.IsMobile ? "framePad" : "" %>">
                <iframe src="<%= SetupInfo.GetImportServiceUrl() %>" style="border: none; width: <%= MobileDetector.IsMobile ? "100%" : "505px" %>; height: 50px; overflow: hidden; filter: alpha(opacity=100);" frameborder="0" id="ifr"></iframe>
            </div>
        <div class="file" onclick="ImportUsersManager.ChangeVisionFileSelector();" title="<%= Resource.ImportContactsFromFile %>" style="display: <%= MobileDetector.IsMobile ? "none" : "block" %>;">
            <%= Resource.ImportContactsFromFile %>
            <div class="innerBox float-right">
            </div>
            <div class="fileSelector studio-action-panel" onclick="ImportUsersManager.ChangeVisionFileSelector();">
                <ul class="dropdown-content">
                    <li id="import_flatUploader"><a href="javascript:void(0);" class="dropdown-item"><%= Resource.ImportContactsFromFileCSV %></a></li>
                    <li id="import_msUploader"><a href="javascript:void(0);" class="dropdown-item"><%= Resource.ImportContactsFromFileMS %></a></li>
                </ul>
            </div>
        </div>
    </div>

    <div id="wizard_users">
        <div class="clearFix <%= MobileDetector.IsMobile ? "mob" : "" %>" id="addUserBlock">
            <div class="checkall">
                <input type="checkbox" id="checkAll" onclick="ImportUsersManager.ChangeAll()" />
            </div>
            <div class="nameBox">
                <div class="error" id="fnError">
                    <%= Resource.ImportContactsErrorFirstName %>
                </div>
                <input type="text" id="firstName" class="textEdit" placeholder="<%= Resource.FirstName %>" maxlength="64" />
            </div>
            <div class="lastnameBox">
                <div class="error" id="lnError">
                   <%= Resource.ImportContactsErrorLastName %>
                </div>
                <input type="text" id="lastName" class="textEdit" placeholder="<%= Resource.LastName %>" maxlength="64" />
            </div>
            <div class="emailBox">
                <div class="error" id="eaError">
                    <%= Resource.ImportContactsErrorEmail %>
                </div>
                <input type="text" id="email" class="textEdit" placeholder="<%= Resource.Email %>" maxlength="64" />
            </div>
            <div class="<%= MobileDetector.IsMobile ? "mobBtn" : "btn" %>">
                <div class="btnBox">
                    <input type="button" class="button gray" id="saveSettingsBtn" onclick="ImportUsersManager.AddUser();" 
                        value="<%= Resource.ImportContactsAddButton %>" />
                </div>
            </div>
        </div>
        <div class="restr  <%= MobileDetector.IsMobile ? "mob" : "" %>">
            <table id="userList">
            </table>
        </div>
    </div>
    <div class="desc">
        <label>
            <input type="checkbox" id="importAsCollaborators" onclick="ImportUsersManager.ChangeInviteLinkType();"
            <%= EnableInviteLink ? "" : "disabled=\"disabled\" checked=\"checked\"" %> />
            <%= Resource.InviteUsersAsCollaborators%>
        </label>
        <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'answerForHelpInviteGuests',position: 'fixed'});"></div>
    </div>
    <div class="middle-button-container ">
        <div id="import-users-progress">
            <div class="asc-progress-wrapper">
                <div class="asc-progress-value" style="width: 0%;"></div>
            </div>
            <div style="padding-top: 2px;" class="text-medium-describe">
                <%=Resource.ImportUsersProgressText%>
                <span id="backup_percent">0% </span>
            </div>
        </div>
        <a id="import-btn" class="button blue disable impBtn" onclick="ImportUsersManager.ImportList();">
            <%=Resource.ImportContactsSaveButton%>
        </a>
        <span class="splitter-buttons"></span>
        <a id="import-delete-btn" class="button gray disable buttonsImportContact cncBtn" onclick="ImportUsersManager.DeleteSelected();">
            <%= Resource.ImportContactsDeleteButton %>
        </a>
        <span class="splitter-buttons"></span>
        <a id="import-cancel-btn" class="button gray buttonsImportContact" onclick="ImportUsersManager.HideImportWindow();">
            <%= Resource.ImportContactsCancelButton %>
        </a>
        <div class="inviteLabel">
            <div class="invite-text">
                <%= Resource.ImportContactsInviteLinkLabel %>:
            </div>
            <div>
                <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'answerForHelpInv',position: 'fixed'});">
                </div>
                <div class="popup_helper" id="answerForHelpInv">
                    <p>
                        <%=Resource.ImportContactsInviteHint%></p>
                </div>
                <div class="popup_helper" id="answerForHelpInviteGuests">
                    <p>
                        <%=string.Format(Resource.NoteForInviteCollaborator, "<b>","</b>")%>
                         <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
                            { %>}
                        <a href="<%= CommonLinkUtility.GetHelpLink(true) %>" target="_blank">
                            <%= Resource.LearnMore %></a>
                        <% } %>
                    </p>
                </div>
            </div>

            <div id="inviteLinkPanel" class="inputBox">
                <a id="inviteLinkCopy" class="invite-copy-link link dotline small gray"><span><%= Resource.CopyToClipboard %></span></a>
                <input id="inviteUserLink" type="text" <% if (!MobileDetector.IsMobile) { %> readonly="readonly" <%} %> class="textEdit" value="<%= EnableInviteLink ? InviteLink.GenerateLink(EmployeeType.User): InviteLink.GenerateLink(EmployeeType.Visitor) %>"
                    <% if (EnableInviteLink) { %> data-invite-user-link="<%=InviteLink.GenerateLink(EmployeeType.User)%>"
                    data-invite-visitor-link="<%=InviteLink.GenerateLink(EmployeeType.Visitor) %>"
                    <%} %> />
            </div>
        </div>
        

    </div>

</div>

<div id="importUserLimitPanel">
    <sc:Container ID="limitPanel" runat="server">
        <Header><%=Resource.ImportUserLimitTitle%></Header>
        <Body>
            <div class="tariff-limitexceed-users">
                <div id="importUserLimitHeader" class="header-base-medium">
                    <%=PeopleLimit > 0 ? String.Format(Resource.ImportUserLimitHeader, PeopleLimit) : Resource.ImportUserOverlimitHeader%>
                </div>
                <br/>
                <div>
                    <%=FreeTariff ?
                        string.Format(Resource.ImportUserOpenPortalLimitReason,
                            "<br/><a class='link underline' href='http://helpcenter.onlyoffice.com/gettingstarted/configuration.aspx#PublicPortals' target='_blank'>",
                            "</a>") :
                        Resource.ImportUserLimitReason%>
                </div>
            </div>
            <div class="middle-button-container">
                <a class="blue button medium" href="<%= TenantExtra.GetTariffPageLink() %>">
                    <%= UserControlsCommonResource.UpgradeButton %></a>
                <span class="splitter-buttons"></span>
                    <a id="import-limit-btn" class="gray button" onclick="ImportUsersManager.ConfirmationLimit();">
                        <%= Resource.AddUsersCaption %>
                    </a>
                    <span class="splitter-buttons"></span>
                    <a id="import-limit-cancel-btn" class="button gray" onclick="ImportUsersManager.HideImportUserLimitPanel();">
                        <%= Resource.ImportContactsCancelButton %>
                    </a>
            </div>
        </Body>
    </sc:Container>
</div>

<table id="donor" class="display-none">
    <tr>
        <td class="fistable">
            <div class="desc">
                <%= Resource.ImportContactsFirstable %>
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
            <a class="button blue" onclick="ImportUsersManager.HideInfoWindow('okcss');">
                <%= Resource.ImportContactsOkButton %>
            </a>
        </div>
    </Body>
</sc:Container>
<div class="blockUpload display-none" id="blockProcess"></div>