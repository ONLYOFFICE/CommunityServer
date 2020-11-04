<%@ Assembly Name="ASC.Web.Community" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Community/Master/Community.Master" AutoEventWireup="true"
 CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Birthdays.Default" %>

<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Community.Birthdays.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>


<asp:Content ContentPlaceHolderID="CommunityPageContent"  runat="server">

    <% if (todayBirthdays != null && todayBirthdays.Count > 0) %>
    <% { %>
    <div class="birthday-header birthday-content-splitter">
        <%= BirthdaysResource.BirthdaysTodayTitle%>
    </div>
    <table class="birthday-content-splitter" cellspacing="0" cellpadding="0">
    <colgroup>
        <col width="166px"/>
        <col/>
    </colgroup>
    <tbody>
        <tr>
            <td class="birthday-date birthday-header">
                <%= DateTime.UtcNow.ToString("dd MMMM").ToLower()%>
            </td>
            <td>
                <div class="user-card-list">
                <% foreach (var usr in todayBirthdays) %>
                <% { %>
                    <div class="user-card clearFix">

                    <div class="user-avatar borderBase">
                        <img src="<%= usr.GetBigPhotoURL() %>">
                    </div>

                    <div class="user-info">
                        <a class="linkHeader" href="<%= CommonLinkUtility.GetUserProfile(usr.ID) %>" title="<%= usr.DisplayUserName()%>">
                            <%= usr.DisplayUserName()%>
                        </a>
                        <div class="user-info-title gray-text"><%= usr.Title%></div>
                        <a class="<%= usr.ID == SecurityContext.CurrentAccount.ID ? "display-none" : "button blue middle" %>"
                                onclick="ASC.Community.Birthdays.openContact(this);"
                                username="<%= usr.UserName.HtmlEncode() %>">
                            <%= BirthdaysResource.BirthdayCongratulateLinkTitle%>
                        </a>
                    </div>
                </div>
                <% } %>
                </div>
            </td>
        </tr>
    </tbody>
    </table>
    <% } %>



    <asp:PlaceHolder ID="upcomingEmptyContent" runat="server"/>


    <% if (upcomingBirthdays != null && upcomingBirthdays.Count > 0) %>
    <% { %>
    <div class="birthday-header birthday-content-splitter">
        <%= BirthdaysResource.BirthdaysUpcomingTitle%>
    </div>
    <table class="tableBase birthday-content-splitter" cellspacing="0" cellpadding="8">
        <colgroup>
            <col width="80px" />
            <col />
        </colgroup>
        <tbody>
        <% foreach (var bday in  upcomingBirthdays) %>
        <% { %>
            <tr>
                <td class="birthday-date birthday-header borderBase">
                    <div class="birthday-date">
                        <%= bday.Date.ToShortDayMonth()%>
                    </div>
                </td>
                <td class="borderBase">
                    <div class="user-card-list">
                        <% foreach (var usr in bday.Users) %>
                        <% { %>
                        <div class="small-user-card clearFix <%= IsInRemindList(usr.ID) ? "active" : "" %>">
                            <div class="user-avatar">
                                <img src="<%= UserPhotoManager.GetMediumPhotoURL(usr.ID) %>">
                            </div>
                            <div class="user-info">
                                <a class="link bold" href="<%= CommonLinkUtility.GetUserProfile(usr.ID) %>" title="<%= usr.DisplayUserName()%>">
                                    <%= usr.DisplayUserName()%>
                                </a>
                                <div class="user-info-title <%= usr.ID == SecurityContext.CurrentAccount.ID ? "display-none" : "" %>">
                                    <a class="baseLinkAction gray-text birthday-remind" onclick="ASC.Community.Birthdays.remind(this);">
                                        <%= BirthdaysResource.BirthdayRemindLinkTitle%>
                                    </a>
                                    <span class="birthday-remind-active">
                                        <%= BirthdaysResource.BirthdayRemindReadyLinkTitle%>
                                        <a onclick="ASC.Community.Birthdays.clearRemind(this);"></a>
                                    </span>
                                </div>
                                <input type="hidden" value="<%= usr.ID %>"/>
                            </div>
                        </div>
                        <% } %>
                    </div>
                </td>
            </tr>
        <% } %>
        </tbody>
    </table>
    <% } %>

</asp:Content>