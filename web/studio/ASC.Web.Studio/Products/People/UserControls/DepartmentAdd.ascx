<%@ Assembly Name="ASC.Web.People" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DepartmentAdd.ascx.cs" Inherits="ASC.Web.People.UserControls.DepartmentAdd" %>

<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_departmentAddDialog" class="display-none">
    <sc:Container runat="server" ID="_departmentAddContainer">
        <Header>
        <div id="grouptitle">
            <%= CustomNamingPeople.Substitute<PeopleResource>("AddDepartmentDlgTitle").HtmlEncode() %>
        </div>
        </Header>
        <Body>
            <asp:HiddenField runat="server" ID="_depProductID" />
            <input type="hidden" id="addDepartment_infoID" value="<%= _departmentAddContainer.ClientID %>_InfoPanel" />
            <div class="clearFix block-cnt-splitter requiredField" >
                <span class="requiredErrorText"><%= PeopleResource.ErrorEmptyName %></span>
                <div class="headerPanelSmall header-base-small">
                    <%= PeopleResource.Title %>:
                </div>
                <input type="text" id="studio_newDepName" class="textEdit" style="width: 99%;" maxlength="100" />
            </div>
            <div class="clearFix block-cnt-splitter">
                <div class="headerPanelSmall header-base-small" >
                    <%= CustomNamingPeople.Substitute<PeopleResource>("DepartmentMaster").HtmlEncode() %>:
                </div>

                <span id="headAdvancedSelector" class="link dotline plus">
                    <%= PeopleResource.ChooseUser %>
                </span>
                <div id="departmentManager" class="advanced-selector-select-result display-none">
                    <span class="result-name" data-id=""></span>
                    <span class="reset-icon"></span>
                </div>

            </div>
            <div class="clearFix">
                <div class="headerPanelSmall header-base-small" >
                    <%= PeopleResource.Members %>:
                </div>
                <span id="membersAdvancedSelector" class="link dotline plus"> <%= PeopleResource.AddMembers %>
                </span>
                <div class="members-dep-list">
                    <ul id="membersDepartmentList" class="advanced-selector-list-results"></ul>
                </div>
            </div>
            <div id="depActionBtn" class="middle-button-container">
                <a class="button blue middle" onclick="DepartmentManagement.AddDepartmentCallback();">
                    <%= PeopleResource.AddButton %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="DepartmentManagement.CloseAddDepartmentDialog();">
                    <%= PeopleResource.CancelButton %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>