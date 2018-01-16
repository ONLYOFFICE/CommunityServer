<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/products/people/Masters/PeopleBaseTemplate.Master" CodeBehind="Reassigns.aspx.cs" Inherits="ASC.Web.People.Reassigns" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="Resources" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PeoplePageContent" runat="server">
    
    <div class="clearFix profile-title header-with-menu">
        <span class="header text-overflow" title="<%= PageTitle %>"><%= PageTitle %></span>
    </div>
    <div id="reassignActionContainer" class="display-none">
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
        <div><%= UserControlsCommonResource.NotBeUndone %></div>
        <%--<% if (!String.IsNullOrEmpty(HelpLink)) %>
        <% { %>
        <div>
            <a class="link underline" href="<%= HelpLink %>" target="_blank"><%= PeopleResource.ReassignsReadMore %></a>
        </div>
        <% } %>--%>
        <div class="big-button-container">
            <a id="reassignBtn" class="button blue big disable"><%= PeopleResource.ReassignButton %></a>
            <span class="splitter-buttons"></span>
            <a id="сancelBtn" class="button gray big" href="<%= ProfileLink %>"><%= PeopleResource.CancelButton %></a>
        </div>
    </div>
    <div id="reassignProgressContainer" class="display-none">
        <%= String.Format(PeopleResource.ReassignsProgressText, "<br/>") %>
        <%--<div class="big-button-container">
            <a id="terminateBtn" class="button gray big" href="<%= ProfileLink %>"><%= PeopleResource.TerminateButton %></a>
        </div>--%>
    </div>

</asp:Content>