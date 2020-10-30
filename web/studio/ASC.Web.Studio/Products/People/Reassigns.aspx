<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" CodeBehind="Reassigns.aspx.cs" Inherits="ASC.Web.People.Reassigns" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="Resources" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <div class="clearFix profile-title header-with-menu">
        <span class="header text-overflow" title="<%= PageTitle %>"><%= PageTitle %></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="PeoplePageContent" runat="server">

    <% if(RemoveData) {%>

    <div class="action-container display-none">
        <div class="list-container">
            <div class="headerPanelSmall header-base-small"><%= PeopleResource.RemovingListHdr %></div>
            <ul>
                <li><%= PeopleResource.RemovingListItem1 %></li>
                <li><%= PeopleResource.RemovingListItem2 %></li>
                <li><%= PeopleResource.RemovingListItem3 %></li>
                <li><%= PeopleResource.RemovingListItem4 %></li>
            </ul>
        </div>
        <p><%= UserControlsCommonResource.NotBeUndone %></p>
        <% if (!String.IsNullOrEmpty(HelpLink)) %>
        <% { %>
        <div>
            <a class="link underline" href="<%= HelpLink + "/gettingstarted/people.aspx#DeletingProfile_block" %>" target="_blank">
                <%= PeopleResource.RemovingReadMore %>
            </a>
        </div>
        <% } %>
        <div class="warning-header red-text"><%= Resource.Warning %></div>
        <div><%= Resource.DeleteUserDataConfirmation %></div>
        <div class="big-button-container">
            <a class="start-btn button blue big"><%= Resource.DeleteButton %></a>
            <span class="splitter-buttons"></span>
            <a class="button gray big" href="Reassigns.aspx?user=<%= HttpUtility.UrlEncode(UserInfo.UserName) %>"><%= Resource.ReassignData %></a>
            <span class="splitter-buttons"></span>
            <a class="button gray big" href="<%= ProfileLink %>"><%= Resource.CancelButton %></a>
        </div>
    </div>

    <div class="progress-container display-none">
        <p><%= String.Format(
           PeopleResource.RemovingProgressUserInfo,
           "<a class=\"from-user-link link underline\" href=\"\"></a>") %></p>

        <p><%= PeopleResource.ReassignsProgressNotifyInfo %></p>

        <div class="progress-block">
            <div class="progress-row clearFix">
                <div class="progress-title header-base-small"><%= PeopleResource.ReassignDocumentsModule %></div>
                <div class="progress-desc" data-step="25"></div>
            </div>
            <div class="progress-row clearFix">
                <div class="progress-title header-base-small"><%= PeopleResource.ReassignCrmModule %></div>
                <div class="progress-desc" data-step="50"></div>
            </div>
            <div class="progress-row clearFix">
                <div class="progress-title header-base-small"><%= PeopleResource.ReassignMailModule %></div>
                <div class="progress-desc" data-step="75"></div>
            </div>
            <div class="progress-row clearFix">
                <div class="progress-title header-base-small"><%= PeopleResource.ReassignTalkModule %></div>
                <div class="progress-desc" data-step="99"></div>
            </div>
        </div>

        <div class="big-button-container">
            <a class="abort-btn button gray big display-none"><%= PeopleResource.RemovingAbortButton %></a>
            <a class="restart-btn button gray big display-none"><%= PeopleResource.RemovingRestartButton %></a>
            <a class="ok-btn button gray big display-none" href="/Products/People/"><%= Resource.OKButton %></a>
        </div>
    </div>

    <% } else { %>

    <div class="action-container display-none">
        <div class="user-selector-container">
            <%= PeopleResource.ReassignsToUser %>
            <span  id="userSelector" class="link dotline"><%= PeopleJSResource.ChooseUser %></span>
        </div>
        <div class="list-container">
            <div class="headerPanelSmall header-base-small"><%= PeopleResource.ReassignsTransferedListHdr %></div>
            <ul>
                <li><%= PeopleResource.ReassignsTransferedListItem1 %></li>
                <li><%= PeopleResource.ReassignsTransferedListItem2 %></li>
                <li><%= PeopleResource.ReassignsTransferedListItem3 %></li>
            </ul>
        </div>
        <p><%= UserControlsCommonResource.NotBeUndone %></p>
        <% if (!String.IsNullOrEmpty(HelpLink)) %>
        <% { %>
        <div>
            <a class="link underline" href="<%= HelpLink + "/gettingstarted/people.aspx#DeletingProfile_block" %>" target="_blank">
                <%= PeopleResource.ReassignsReadMore %>
            </a>
        </div>
        <% } %>
        <div class="delete-profile-container">
            <label>
                <input type="checkbox" <%= DeleteProfile ? "checked" : "" %>/><%= PeopleResource.DeleteProfileAfterReassignment %>
            </label>
        </div>
        <div class="big-button-container">
            <a class="start-btn button blue big disable"><%= PeopleResource.ReassignButton %></a>
            <span class="splitter-buttons"></span>
            <a class="button gray big" href="<%= ProfileLink %>"><%= PeopleResource.CancelButton %></a>
        </div>
    </div>

    <div class="progress-container display-none">
        <p><%= String.Format(
           PeopleResource.ReassignsProgressUserInfo,
           "<a class=\"from-user-link link underline\" href=\"\"></a>",
           "<a class=\"to-user-link link underline\" href=\"\"></a>") %></p>

        <p><%= PeopleResource.ReassignsProgressNotifyInfo %></p>

        <div class="progress-block">
            <div class="progress-row clearFix">
                <div class="progress-title header-base-small"><%= PeopleResource.ReassignDocumentsModule %></div>
                <div class="progress-desc" data-step="33"></div>
            </div>
            <div class="progress-row clearFix">
                <div class="progress-title header-base-small"><%= PeopleResource.ReassignProjectsModule %></div>
                <div class="progress-desc" data-step="66"></div>
            </div>
            <div class="progress-row clearFix">
                <div class="progress-title header-base-small"><%= PeopleResource.ReassignCrmModule %></div>
                <div class="progress-desc" data-step="99"></div>
            </div>
        </div>

        <div class="big-button-container">
            <a class="abort-btn button gray big display-none"><%= PeopleResource.ReassignAbortButton %></a>
            <a class="restart-btn button gray big display-none"><%= PeopleResource.ReassignRestartButton %></a>
            <a class="ok-btn button gray big display-none" href="/Products/People/"><%= Resource.OKButton %></a>
        </div>
    </div>

    <% }%>

</asp:Content>