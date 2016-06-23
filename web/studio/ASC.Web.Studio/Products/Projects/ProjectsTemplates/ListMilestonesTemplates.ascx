<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script id="projects_milestoneTemplate" type="text/x-jquery-tmpl">
    <tr id = "${id}" class="with-entity-menu ${status}" isKey="${isKey}" isNotify="${isNotify}">
        <td class="status">
            {{if canEdit && !Teamlab.profile.isVisitor}}
                <div class="changeStatusCombobox canEdit" milestoneId="${id}">
                {{if status == 'closed'}}
                    <span class="${status}" title="<%= MilestoneResource.StatusClosed %>"></span>
                {{else}}
                    <span class="${status}" title="<%= MilestoneResource.StatusOpen %>"></span>
                {{/if}}
                    <span class="arrow"></span>
                </div>
            {{else}}
                <div class="changeStatusCombobox noEdit">
                {{if status == 'closed'}}
                    <span class="${status}" title="<%= MilestoneResource.StatusClosed %>"></span>
                {{else}}
                    <span class="${status}" title="<%= MilestoneResource.StatusOpen %>"></span>
                {{/if}}
                </div>
            {{/if}}
        </td>
        <td class="title stretch">
            <div>
                {{if isKey == true}}
                    <span class="key" title="<%= MilestoneResource.RootMilestone %>"></span>
                {{/if}}
                <a href="javascript:void(0)" projectId="${projectId}" projectTitle="${projectTitle}" createdById="${createdById}" createdBy="${createdBy}"
                    created="${created}" description="${description}">${title}</a>
            </div>
        </td>
        <td class="activeTasksCount">
            {{if activeTasksCount != 0 || closedTasksCount != 0}}
                {{if status != 'closed'}}
                    <a href="${activeTasksLink}" title="<%=ProjectResource.TitleMilestoneOpenTasks %>">${activeTasksCount}</a>
                {{else}}
                    ${activeTasksCount}
                {{/if}}
            {{/if}}
        </td>
        <td class="slash">
            {{if activeTasksCount != 0 || closedTasksCount != 0}}
                /
            {{/if}}
        </td>
        <td class="closedTasksCount">
            {{if activeTasksCount != 0 || closedTasksCount != 0}}
                {{if status != 'closed'}}
                    <a href="${closedTasksLink}" title="<%=ProjectResource.TitleMilestoneClosedTasks %>">${closedTasksCount}</a>
                {{else}}
                    ${closedTasksCount}
                {{/if}}
            {{/if}}
        </td>
        <td class="deadline">
            {{if status == "overdue"}}  
                <span class="overdue">${deadline}</span>
            {{else}}
                <span>${deadline}</span>
            {{/if}}
        </td>
        <td class="responsible">
            <div>
                <span responsibleId='${responsibleId}' {{if responsibleId=='4a515a15-d4d6-4b8e-828e-e0586f18f3a3'}} class="not-action"{{/if}} title="<%=ProjectResource.TitleMilestoneResponsibleUser %>">${responsible}</span>
            </div>
        </td>
        <td class="actions">
            {{if (canEdit && !Teamlab.profile.isVisitor) || canDelete}}
                <div class="entity-menu" milestoneId="${id}" projectId="${projectId}" status="${status}" canDelete="${canDelete}"></div>
            {{/if}}
        </td>
    </tr>
</script>

