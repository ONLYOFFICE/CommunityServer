<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HistoryView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Common.HistoryView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div id="historyBlock">
        <table class="details-menu" width="100%" cellpadding="0" cellspacing="0">
         <colgroup>
            <col />
            <col style="width: 150px;"/>
            <col/>
        </colgroup>
        <tbody>
        <tr>
            <td id="categorySelectorContainer"></td>
            <td style="white-space:nowrap;padding-right: 15px;">
                <%= CRMCommonResource.Date %>:
                <input type="text" class="textEditCalendar" autocomplete="off"/>
            </td>
            <td style="white-space:nowrap;">
                <div id="eventLinkToPanel" class="empty-select"></div>
            </td>
        </tr>
        </tbody>
    </table>
    <div style="padding-right: 5px; padding-top: 8px;">
        <textarea id="historyCKEditor"></textarea>
    </div>

    <div style="margin-top: 10px;">
        <table width="100%">
        <tr>
            <td style="white-space:nowrap;" class="historyViewUserSelectorCont">
                <%=CRMCommonResource.SelectUsersToNotify%>:
            </td>
            <td style="white-space:nowrap;" class="historyViewUserSelectorCont">
                <div id ="historyViewUserSelector"></div>
            </td>
            <td width="100%" align="right">
                <div style="float:right;">
                    <a id="attachShowButton" class="attachLink baseLinkAction linkMedium" onclick="ASC.CRM.HistoryView.showAttachmentPanel(true)" >
                        <%= CRMCommonResource.ShowAttachPanel%>
                    </a>
                    <a id="attachHideButton" class="attachLink baseLinkAction linkMedium" onclick="ASC.CRM.HistoryView.showAttachmentPanel(false)" style="display: none;" >
                        <%= CRMCommonResource.HideAttachPanel%>
                    </a>
                </div>
            </td>
        </tr>
        </table>
        <div id="selectedUsers_HistoryUserSelector_Container" class="clearFix" style="margin-top: 10px;"></div>
    </div>

    <div id="attachOptions" style="display:none;margin-top: 10px;">
        <asp:PlaceHolder ID="_phfileUploader" runat="server" />
    </div>

    <div class="middle-button-container">
        <a class="button blue middle disable" onclick="ASC.CRM.HistoryView.addEvent(this); return false;">
            <%= CRMCommonResource.AddThisNote %>
        </a>
        <span class="describe-text display-none lond-data-text" style="padding-left: 24px;"></span>
    </div>

    <br />
    <div class="clearFix">
        <div id="eventsFilterContainer">
            <div id="eventsAdvansedFilter"></div>
        </div>
        <br />

        <div id="eventsList">
            <table id="eventsTable" class="table-list padding10" cellpadding="0" cellspacing="0">
                <tbody>
                </tbody>
            </table>
            <div id="showMoreEventsButtons">
                <a class="crm-showMoreLink" style="display:none;">
                    <%= CRMJSResource.ShowMoreButtonText %>
                </a>
                <a class="loading-link" style="display:none;">
                    <%= CRMJSResource.LoadingProcessing %>
                </a>
            </div>
        </div>
    </div>
</div>