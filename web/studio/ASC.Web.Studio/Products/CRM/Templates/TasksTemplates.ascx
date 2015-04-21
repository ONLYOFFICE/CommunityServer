<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<%--Tasks List--%>
<script id="tasksListBaseTmpl" type="text/x-jquery-tmpl">
    <div id="taskFilterContainer">
        <div id="tasksAdvansedFilter"></div>
    </div>

    <div id="hiddenBlockForTaskContactSelector" style="display:none;">
         <span id="taskContactSelectorForFilter" class="custom-value">
            <span class="inner-text">
                <span class="value"><%= CRMCommonResource.Select %></span>
            </span>
        </span>
    </div>
</script>

<script id="taskExtendedListTmpl" type="text/x-jquery-tmpl">
<div id="taskList"  style="display:none;{{if IsTab === false}}min-height: 400px;{{else}}min-height: 200px;{{/if}}">
    <table id="taskTable" class="tableBase" cellpadding="4" cellspacing="0">
        <colgroup>
            <col style="width: 55px;" />
            <col style="width: 30px;" />
            <col />
            <col style="width: 150px;" />
            <col style="width: 120px;" />
            <col style="width: 40px;" />
        </colgroup>
        <tbody>
        </tbody>
    </table>

    <table id="tableForTaskNavigation" class="crm-navigationPanel" cellpadding="4" cellspacing="0" border="0">
        <tbody>
        <tr>
            <td>
                <div id="divForTaskPager"></div>
            </td>
            <td style="text-align:right;">
                <span class="gray-text"><%= CRMTaskResource.TotalTasks %>:</span>
                <span class="gray-text" id="totalTasksOnPage"></span>

                <span class="gray-text"><%= CRMCommonResource.ShowOnPage %>:&nbsp;</span>
                <select class="top-align">
                    <option value="25">25</option>
                    <option value="50">50</option>
                    <option value="75">75</option>
                    <option value="100">100</option>
                </select>
            </td>
        </tr>
        </tbody>
    </table>
</div>

<div id="taskStatusListContainer" class="studio-action-panel" taskid="">
    <ul class="dropdown-content">
        <li class="open">
            <a class="dropdown-item"><%= CRMTaskResource.TaskStatus_Open %></a>
        </li>
        <li class="closed">
            <a class="dropdown-item"><%= CRMTaskResource.TaskStatus_Closed %></a>
        </li>
    </ul>
</div>

<div id="taskActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="dropdown-item display-none" id="sendEmailLink" target="_blank"><%= CRMCommonResource.SendEmail %></a></li>
        <li><a class="dropdown-item" id="editTaskLink"><%= CRMTaskResource.EditTask %></a></li>
        <li><a class="dropdown-item" id="deleteTaskLink"><%= CRMTaskResource.DeleteTask %></a></li>
    </ul>
</div>

<div id="confirmationDeleteOneTaskPanel" style="display: none;">
    <div class="popupContainerClass">
        <div class="containerHeaderBlock">
            <table cellspacing="0" cellpadding="0" border="0" style="width:100%; height:0px;">
                <tbody>
                    <tr valign="top">
                        <td>
                            <%= CRMCommonResource.Confirmation %>
                        </td>
                        <td class="popupCancel">
                            <div class="cancelButton" onclick="PopupKeyUpActionProvider.CloseDialog();">&times</div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div class="containerBodyBlock">
            <div class="confirmationAction">
                <b><%= CRMJSResource.ConfirmDeleteTask %></b>
            </div>
            <div class="confirmationNote"><%= CRMJSResource.DeleteConfirmNote %></div>
            <div class="middle-button-container">
                <a class="button blue middle"><%= CRMCommonResource.OK %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();" ><%= CRMCommonResource.Cancel %></a>
            </div>
        </div>
    </div>
</div>

<div id="files_hintCategoriesPanel" class="hintDescriptionPanel">
    <%= CRMTaskResource.TooltipCategories %>
    <a href="http://www.onlyoffice.com/help/tipstricks/tasks-categories.aspx" target="_blank"><%= CRMCommonResource.ButtonLearnMore %></a>
</div>
</script>

<script id="taskListTmpl" type="text/x-jquery-tmpl">
    <tbody>
        {{tmpl(tasks) "taskTmpl"}}
    </tbody>
</script>

<script id="taskTmpl" type="text/x-jquery-tmpl">
    <tr id="task_${id}" class="with-entity-menu">
        <td class="borderBase">
            <div class="check">
                <div class="changeStatusCombobox{{if canEdit == true}} canEdit{{/if}}" taskid="${id}">
                    {{if isClosed == true}}
                        <span class="closed" title="<%= CRMTaskResource.TaskStatus_Closed %>"></span>
                    {{else}}
                        <span class="open" title="<%= CRMTaskResource.TaskStatus_Open %>"></span>
                    {{/if}}
                </div>
            </div>
            <div class="ajax_edit_task loader-big" title=""></div>
        </td>

        <td class="borderBase">
            <label class="task_category ${category.cssClass}" title="${category.title}" alt="${category.title}"></label>
        </td>
        <td class="borderBase">
            <div class="divForTaskTitle">
                <span id="taskTitle_${id}" class="${classForTitle}"
                    dscr_label="<%=CRMCommonResource.Description%>" dscr_value="${description}">
                    ${title}
                </span>
            </div>
            <div style="padding-top: 5px; display: inline-block;">
                <span class="${classForTaskDeadline}">
                    ${deadLineString}
                </span>
            </div>
        </td>

        <td class="borderBase" style="white-space:nowrap; padding-right:15px;">
            {{if entity != null && ASC.CRM.ListTaskView.EntityID == 0}}
            <div class="divForEntity">
                ${entityType}: <a class="linkMedium" href="${entityURL}">${entity.entityTitle}</a>
            </div>
            {{/if}}

            {{if contact != null && ASC.CRM.ListTaskView.ContactID != contact.id}}
                <div class="divForEntity" {{if entity != null}} style="padding-top: 5px;" {{/if}}>
                {{if contact.isCompany == true}}
                    <a href="default.aspx?id=${contact.id}" data-id="${contact.id}" id="task_${id}_company_${contact.id}" class="linkMedium crm-companyInfoCardLink">
                        ${contact.displayName}
                    </a>
                {{else}}
                    <a href="default.aspx?id=${contact.id}&type=people" data-id="${contact.id}" id="task_${id}_person_${contact.id}" class="linkMedium crm-peopleInfoCardLink">
                        ${contact.displayName}
                    </a>
                {{/if}}
                </div>
            {{/if}}
        </td>

        <td class="borderBase">
            <div class="divForUser{{if responsible.activationStatus == 2}} removedUser{{/if}}">
                <span title="${responsible.displayName}" resp_id="${responsible.id}">
                    ${responsible.displayName}
                </span>
            </div>
        </td>

        <td class="borderBase">
        {{if canEdit == true}}
            <div id="taskMenu_${id}" class="entity-menu" title="<%= CRMCommonResource.Actions %>"
                 onclick="ASC.CRM.ListTaskView.showActionMenu(${id});" ></div>
        {{/if}}
        </td>
    </tr>
</script>

<%--TaskActionView--%>

<script id="taskActionViewTmpl" type="text/x-jquery-tmpl">
        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall header-base-small"><%= CRMTaskResource.TaskTitle%>:</div>
            <input class="textEdit" id="tbxTitle" style="width:100%" type="text" maxlength="100"/>
        </div>

        <div id="taskCategorySelectorContainer" class="headerPanelSmall-splitter">
            <div class="headerPanelSmall header-base-small"><%= CRMTaskResource.TaskCategory%>:</div>
        </div>

        <div class="headerPanelSmall-splitter">
            <div class="headerPanelSmall header-base-small">
                <%= CRMTaskResource.ConnectWithAContact%>:
            </div>
            <div class="connectWithContactContainer">
            </div>
        </div>

        <div class="headerPanelSmall-splitter requiredField" id="taskDeadlineContainer">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall header-base-small"><%= CRMTaskResource.DueDate%>:</div>
            <a id="deadline_0" class="baseLinkAction linkMedium" onclick="ASC.CRM.TaskActionView.changeDeadline(this);">
                <%= CRMTaskResource.Today%>
            </a>
            <span class="splitter"></span>
            <a id="deadline_3" class="baseLinkAction linkMedium" onclick="ASC.CRM.TaskActionView.changeDeadline(this);">
                <%= CRMTaskResource.ThreeDays%>
            </a>
            <span class="splitter"></span>
            <a id="deadline_7" class="baseLinkAction linkMedium" onclick="ASC.CRM.TaskActionView.changeDeadline(this);">
                <%= CRMTaskResource.Week%>
            </a>
            <span class="splitter"></span>

            <input id="taskDeadline" type="text" onkeypress="ASC.CRM.TaskActionView.keyPress(event);" class="pm-ntextbox textEditCalendar" />

            <span class="splitter"></span>
            <span class="bold"><%= CRMTaskResource.Time%>:</span>
            <span class="splitter"></span>
            <select class="comboBox" id="taskDeadlineHours" style="width:45px;" onchange="ASC.CRM.TaskActionView.changeTime(this);">
            </select>
            <b style="padding: 0 3px;">:</b>
            <select class="comboBox" id="taskDeadlineMinutes" style="width:45px;" onchange="ASC.CRM.TaskActionView.changeTime(this);">
            </select>
        </div>

        <div class="headerPanelSmall-splitter">
            <div class="headerPanelSmall header-base-small">
                <%= CRMCommonResource.Alert %>:
            </div>
            <div>
                <select class="comboBox" id="taskAlertInterval">
                    <option value="0" id="optAlert_0"><%= CRMTaskResource.Never %></option>
                    <option value="5" id="optAlert_5"><%= CRMTaskResource.PerFiveMinutes %></option>
                    <option value="15" id="optAlert_15"><%= CRMTaskResource.PerFifteenMinutes %></option>
                    <option value="30" id="optAlert_30"><%= CRMTaskResource.PerHalfHour %></option>
                    <option value="60" id="optAlert_60"><%= CRMTaskResource.PerHour %></option>
                    <option value="120" id="optAlert_120"><%= CRMTaskResource.PerTwoHours %></option>
                    <option value="1440" id="optAlert_1440" selected="selected"><%= CRMTaskResource.PerNight %></option>
                </select>
            </div>
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall header-base-small">
                <%= CRMTaskResource.TaskResponsible%>:
            </div>
            <div>
                <div id="taskActionViewAdvUsrSrContainer" data-responsible-id="">
                    <span>
                        <a class="link dotline taskResponsibleLabel"></a>
                        <span class="sort-down-black"></span>
                    </span>
                </div>

                <div class="taskActionViewNotifyResponsibleContainer">
                    <input type="checkbox" id="notifyResponsible" />
                    <label for="notifyResponsible"><%= CRMCommonResource.Notify%></label>
                </div>
            </div>
        </div>

        <div>
            <div class="headerPanelSmall header-base-small">
                <%= CRMTaskResource.TaskDescription%>:
            </div>
            <textarea id="tbxDescribe" style="width:100%;resize:none;" rows="3" cols="70"></textarea>
        </div>

        <div class="error-popup display-none"></div>
        <div class="big-button-container">
            <a id="taskActionPopupOK" class="button blue middle"><%= CRMJSResource.AddThisTask%></a>
            <span class="splitter-buttons"></span>
            <a id="taskActionPopupCancel" class="button gray middle" onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">
                <%= CRMTaskResource.Cancel%>
            </a>
        </div>
</script>