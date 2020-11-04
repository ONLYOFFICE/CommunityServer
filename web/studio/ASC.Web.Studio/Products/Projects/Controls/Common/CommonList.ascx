<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommonList.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.CommonList" %>
<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>

<div id="addTaskPanel" class="commonActionPanel"></div>
<div id="milestoneActionPanel" class="commonActionPanel"></div>
<div id="commonPopupContainer" style="display: none"></div>
<div id="moveTaskPanel" style="display: none"></div>

<div id="emptyScrCtrlPrj" class="noContentBlock emptyScrCtrl"></div>

<div id="CommonListContainer">

    <div class="taskList with-checkbox"></div>

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

    <table id="timeSpendsList" class="listContainer pm-tablebase" cellpadding="0">
        <thead>
        </thead>
        <tbody>
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
