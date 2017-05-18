<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommonList.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.CommonList" %>
<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>

<div id="addTaskPanel" class="commonActionPanel"></div>
<div id="milestoneActionPanel" class="commonActionPanel"></div>
<div id="commonPopupContainer" style="display: none"></div>
<div id="moveTaskPanel" style="display: none"></div>
<div class="project-info-container display-none"></div>
<div id="filterContainer">
    <div id="ProjectsAdvansedFilter"></div>
</div>
<div id="emptyScrCtrlPrj" class="noContentBlock emptyScrCtrl"></div>

<div id="CommonListContainer">
    <div id="groupActionContainer">
        <div class="header-menu-spacer"> </div>
    </div>

    <div class="taskList"></div>

    <table id="tableListProjects" class="table-list">
        <tbody>
        </tbody>
    </table>

    <table id="milestonesList" class="table-list">
        <thead>
        </thead>
        <tbody>
        </tbody>
    </table>
    <div id="discussionsList">
    </div>
    <table id="timeSpendsList" class="listContainer pm-tablebase">
        <thead>
        </thead>
        <tbody>
        </tbody>
    </table>
    <table id="tableForNavigation" cellpadding="4" cellspacing="0" style="display: none">
        <tbody>
        <tr>
            <td>
                <div id="divForTaskPager" class="divPager">
                </div>
            </td>
            <td style="text-align:right;">
                <span class="gray-text"><%= ProjectsCommonResource.Total%> : </span>
                <span class="gray-text" style="margin-right: 20px;" id="totalCount"></span>
                <span class="gray-text"><%= ProjectsCommonResource.ShowOnPage%> : </span>
                <span id="countOfRows" class="top-align link dotline">25</span> 
            </td>
        </tr>
        </tbody>
    </table>
    
    <div id="descriptionTab" class="display-none">
        <div class="tab"></div>
        <div class="tab1"></div>
        <div id="subtaskContainer" class="display-none">    
            <div class="subtasks"></div>
        </div>
        <div id="filesContainer" class="display-none">
            <asp:PlaceHolder runat="server" ID="phAttachmentsControl" />
        </div>

        <div id="commonCommentsContainer" class="display-none">
            <scl:CommentsList ID="commonComments" runat="server" BehaviorID="commonComments">
            </scl:CommentsList>
        </div>  
    </div>
</div>
