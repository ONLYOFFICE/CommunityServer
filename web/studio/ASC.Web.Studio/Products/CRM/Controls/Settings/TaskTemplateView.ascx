<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskTemplateView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.TaskTemplateView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="header-base settingsHeader"><%= CRMSettingResource.TaskTemplates %></div>

<div id="TaskTemplateViewTabs"></div>

<div class="clearFix settingsNewItemBlock">
    <a class="link dotline plus" onclick="ASC.CRM.TaskTemplateView.showTemplateConatainerPanel();" >
        <%= CRMSettingResource.AddNewTaskTemplateContainer%>
    </a>
</div>

<div id="templateConatainerPanel" style="display: none">
    <sc:Container ID="_templateConatainerPanel" runat="server">
        <Header>
        </Header>
        <Body>
            <div class="requiredField">
                <span class="requiredErrorText"><%= CRMSettingResource.EmptyLabelError %></span>
                <div class="headerPanelSmall header-base-small" style="margin-bottom:5px;">
                    <%= CRMSettingResource.TitleItem %>:
                </div>
                <input id="templateConatainerTitle" type="text" class="textEdit" style="width:100%" maxlength="255"/>
            </div>

            <div class="h_line"><!--– –--></div>

            <div class="action_block">
                <a class="button blue"></a>
                <span class="splitter"></span>
                <a class="button gray" onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">
                     <%= CRMCommonResource.Cancel %>
                </a>
            </div>

            <div style="display: none;" class="ajax_info_block">
                <span class="loader-text-block"> <%= CRMSettingResource.AddTaskTemplateContainerInProgressing%> </span>              
            </div>
        </Body>
    </sc:Container>
</div>

<div id="templatePanel" style="display: none;">
    <sc:Container id="_templatePanel" runat="server">
        <header>
        </header>
        <body>
            <div class="templateHeader-splitter requiredField">
                <span class="requiredErrorText"><%= CRMJSResource.EmptyTaskTitle%></span>
                <div class="headerPanelSmall templateHeaderSmall-splitter bold"><%= CRMTaskResource.TaskTitle%>:</div>
                <input class="textEdit" id="tbxTemplateTitle" style="width:100%" type="text" maxlength="255"/>
            </div>

            <div class="templateHeader-splitter">
                <div class="templateHeaderSmall-splitter bold"><%= CRMTaskResource.TaskCategory%>:</div>
                <asp:PlaceHolder ID="phCategorySelector" runat="server"></asp:PlaceHolder>
            </div>

            <div class="templateHeader-splitter requiredField" id="taskDeadlineContainer">
                <div class="templateHeaderSmall-splitter bold"><%= CRMTaskResource.DueDate%>:</div>
                <div class="templateHeaderSmall-splitter">
                    <input type="radio" id="deadLine_fixed" name="duedate_fixed" value="true"/>
                    <label for="deadLine_fixed"><%= CRMSettingResource.OffsetFromTheStartOfContainer%></label>
                    <br/>
                    <input type="radio" id="deadLine_not_fixed" name="duedate_fixed" value="false" />
                    <label for="deadLine_not_fixed"><%= CRMSettingResource.OffsetFromThePreviousTask%></label>
                </div>
                <%= CRMSettingResource.DisplacementInDays%>:
                <input type="text" class="textEdit" id="tbxTemplateDisplacement" style="width:50px" maxlength="5"/>
                <span class="splitter"></span>
                <%= CRMTaskResource.Time%>:
                <span class="splitter"></span>
                <select class="comboBox" id="templateDeadlineHours" style="width:45px;">
                    <%=InitHoursSelect()%>
                </select>
                <b style="padding: 0 3px;">:</b>
                <select class="comboBox" id="templateDeadlineMinutes" style="width:45px;">
                    <%=InitMinutesSelect()%>
                </select>
            </div>

            <div class="templateHeader-splitter requiredField">
                <span class="requiredErrorText"><%= CRMJSResource.EmptyTaskResponsible%></span>
                <div class="headerPanelSmall templateHeaderSmall-splitter bold">
                    <%= CRMTaskResource.TaskResponsible%>:
                </div>
                <div>
                    <div style="float:right;">
                        <input type="checkbox" id="notifyResponsible" style="float:left">
                        <label for="notifyResponsible" style="float:left;padding: 2px 0 0 4px;">
                            <%= CRMCommonResource.Notify%>
                        </label>
                    </div>
                    <div id="taskTemplateViewAdvUsrSrContainer"></div>
                </div>
            </div>

            <div class="templateHeader-splitter">
                <div class="templateHeaderSmall-splitter bold">
                    <%= CRMTaskResource.TaskDescription%>:
                </div>
                <textarea id="tbxTemplateDescribe" style="width:100%;resize:none;" rows="3" cols="70"></textarea>
            </div>

            <div class="h_line"><!--– –--></div>

            <div class="action_block">
                <a class="button blue"></a>
                <span class="splitter"></span>
                <a class="button gray" onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">
                    <%= CRMTaskResource.Cancel%>
                </a>
            </div>

            <div class='ajax_info_block' style="display: none;">
                <span class="loader-text-block"> <%= CRMSettingResource.AddTaskTemplateInProgressing%> </span>                
            </div>
        </body>
    </sc:Container>
</div>


<ul id="templateConatainerContent" class="clearFix"></ul>

<div id="emptyContent" style="display: none">
    <asp:PlaceHolder ID="_phEmptyContent" runat="server"></asp:PlaceHolder>
</div>