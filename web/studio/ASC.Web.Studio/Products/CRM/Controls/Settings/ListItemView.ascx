<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListItemView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.ListItemView" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<ul id="listView">
</ul>

<div id="listItemActionMenu" class="studio-action-panel" data-listitemid="" data-relativecount="">
    <ul class="dropdown-content">
        <li class="editItem"><a class="dropdown-item with-icon edit" onclick="ASC.CRM.ListItemView.showEditItemPanel();"><%= EditText %></a></li>
        <li class="deleteItem"><a class="dropdown-item with-icon delete" onclick="ASC.CRM.ListItemView.deleteItem();"><%= DeleteText %></a></li>
    </ul>
</div>

<% if (CurrentTypeValue == ListType.ContactStatus) %>
<% { %>
<div id="colorsPanel" class="studio-action-panel colorsPanelSettings">
    <span class="style1" colorstyle="1"></span><span class="style2" colorstyle="2"></span><span class="style3" colorstyle="3"></span><span class="style4" colorstyle="4"></span><span class="style5" colorstyle="5"></span><span class="style6" colorstyle="6"></span><span class="style7" colorstyle="7"></span><span class="style8" colorstyle="8"></span>
    <span class="style9" colorstyle="9"></span><span class="style10" colorstyle="10"></span><span class="style11" colorstyle="11"></span><span class="style12" colorstyle="12"></span><span class="style13" colorstyle="13"></span><span class="style14" colorstyle="14"></span><span class="style15" colorstyle="15"></span><span class="style16" colorstyle="16"></span>
</div>
<% } %>

<% if (CurrentTypeValue == ListType.TaskCategory) %>
<% { %>
<div id="selectItemForReplacePopUpBody" class="display-none">
    <div id="itemForReplaceSelectorContainer" class="headerPanelSmall-splitter">
        <p><%= CRMSettingResource.SelectTaskCategoryForReplace%></p>
    </div>
    <div class="big-button-container">
        <a id="deleteItemPopupOK" class="button blue middle"><%= CRMCommonResource.OK %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle" onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">
            <%= CRMCommonResource.Cancel%>
        </a>
    </div>
</div>
<div id="iconsPanel_<%= (int)CurrentTypeValue %>" class="iconsPanelSettings studio-action-panel" style="width: 148px;height: 112px;">
    <label class="task_category task_category_call" title="<%= CRMTaskResource.TaskCategory_Call %>" data-imgName="task_category_call.png"></label>
    <label class="task_category task_category_deal" title="<%= CRMTaskResource.TaskCategory_Deal %>" data-imgName="task_category_deal.png"></label>
    <label class="task_category task_category_demo" title="<%= CRMTaskResource.TaskCategory_Demo %>" data-imgName="task_category_demo.png"></label>
    <label class="task_category task_category_email" title="<%= CRMTaskResource.TaskCategory_Email %>" data-imgName="task_category_email.png"></label>
    <label class="task_category task_category_fax" title="<%= CRMTaskResource.TaskCategory_Fax %>" data-imgName="task_category_fax.png"></label>
    <label class="task_category task_category_follow_up" title="<%= CRMTaskResource.TaskCategory_FollowUP %>" data-imgName="task_category_follow_up.png"></label>
    <label class="task_category task_category_lunch" title="<%= CRMTaskResource.TaskCategory_Lunch %>" data-imgName="task_category_lunch.png"></label>
    <label class="task_category task_category_meeting" title="<%= CRMTaskResource.TaskCategory_Meeting %>" data-imgName="task_category_meeting.png"></label>
    <label class="task_category task_category_note" title="<%= CRMTaskResource.TaskCategory_Note %>" data-imgName="task_category_note.png"></label>
    <label class="task_category task_category_ship" title="<%= CRMTaskResource.TaskCategory_Ship %>" data-imgName="task_category_ship.png"></label>
    <label class="task_category task_category_social_networks" title="<%= CRMTaskResource.TaskCategory_SocialNetworks %>" data-imgName="task_category_social_networks.png"></label>
    <label class="task_category task_category_thank_you" title="<%= CRMTaskResource.TaskCategory_ThankYou %>" data-imgName="task_category_thank_you.png"></label>
</div>
<% } else if (CurrentTypeValue == ListType.HistoryCategory) { %>
<div id="iconsPanel_<%= (int)CurrentTypeValue %>" class="iconsPanelSettings studio-action-panel" style="width: 74px;height: 74px;">
    <label class="event_category event_category_note" title="<%= CRMCommonResource.HistoryCategory_Note %>" data-imgName="event_category_note.png"></label>
    <label class="event_category event_category_email" title="<%= CRMCommonResource.HistoryCategory_Email %>" data-imgName="event_category_email.png"></label>
    <label class="event_category event_category_call" title="<%= CRMCommonResource.HistoryCategory_Call %>" data-imgName="event_category_call.png"></label>
    <label class="event_category event_category_meeting" title="<%= CRMCommonResource.HistoryCategory_Meeting %>" data-imgName="event_category_meeting.png"></label>
</div>
<% } %>