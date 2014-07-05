<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>


<script id="projects_taskDescriptionTmpl" type="text/x-jquery-tmpl">
        {{if description!=""}}
            <p class="task-description">
                {{html jq.linksParser(window.ASC.Projects.TaskDescroptionPage.formatDescription(description))}}
            </p>
        {{/if}}            
        <div class="task-desc-block">
            <span class="title"><%= ProjectResource.Project%>:</span>
            <span class="value"><a href="tasks.aspx?prjID=${projId}">${project}</a></span>
        </div>
        {{if milestone != ""}}
            <div class="milestone task-desc-block">
                <span class="title"><%= MilestoneResource.Milestone%>:</span>
                <span class="value">${milestone}</span>
            </div>
        {{/if}}
        {{if displayDateStart !=""}}
            <div class="task-desc-block">
                <span class="title"><%=TaskResource.TaskStartDate%>:</span>                      
                    <span class="value">${displayDateStart}</span>
            </div>
        {{/if}}  
        {{if displayDateDeadline !=""}}
            <div class="task-desc-block">
                <span class="title"><%=TaskResource.EndDate%>:</span>                      
                {{if window.ASC.Projects.TaskDescroptionPage.compareDates(deadline)}}
                    <span class="deadlineLate value">${displayDateDeadline}</span>
                {{else}}
                    <span class="value">${displayDateDeadline}</span>
                {{/if}}  
            </div>
        {{/if}}       
        {{if priority == 1 }}
            <div class="priority task-desc-block">
                <span class="title"><%= TaskResource.Priority%>:</span>
                <span class="value"><span class="colorPriority high"></span><%=TaskResource.HighPriority %></span>
            </div>
        {{/if}} 
        <div class="responsible task-desc-block">
            <span class="title"><%= TaskResource.AssignedTo%>:</span>
            <span class="value">
                {{if responsibles.length == 0}}
                    <%=TaskResource.WithoutResponsible %>
                {{else}}
                    {{each(i, resp) responsibles}}
                        ${resp.displayName}
                        {{if i < responsibles.length - 1}},{{/if}}
                    {{/each}}
                {{/if}}
            </span>
        </div> 
        
        {{if canCreateTimeSpend}}        
        <div class="timeSpend task-desc-block">
            <span class="title"><%= ProjectsCommonResource.SpentTotally%>:</span>
            <span id="timeSpent" class="value">
                <a href="timetracking.aspx?prjID=${projId}&ID=${taskId}">
                    ${timeSpend.hours}<%=TimeTrackingResource.ShortHours %> ${timeSpend.minutes}<%=TimeTrackingResource.ShortMinutes %>
                    </a>
            </span>            
        </div>          
        {{/if}}
        
        <div class="timeInfo task-desc-block">
          {{if createdDate!=""}}
            <span class="title"><%= TaskResource.CreatingDate %>:</span>
            <span id="startDate" class="value">${createdDate}</span>
          {{/if}}
            <span class="title"><%= TaskResource.TaskProducer %>:</span>
            <span class="value">${createdBy}</span>
        </div>
               
        {{if status==2}}
            <div class="timeInfo task-desc-block">
                <span class="title"><%= TaskResource.ClosingDate %>:</span>
                <span id="endDate" class="value">${closedDate}</span>
                <span class="title"><%= TaskResource.ClosedBy %>:</span>
                <span class="value">${closedBy}</span>
            </div>        
        {{/if}} 
        <div class="buttonContainer">
            <span>
                {{if responsibles.length == 0}}
                    <a id="acceptButton" class="button blue big"><%=TaskResource.Accept%></a> 
                {{else}}
                    {{if status != 2}}
                        <a id="closeButton" class="button blue big"><%=TaskResource.CompleteTask %></a>
                    {{/if}}
                    {{if status == 2}}
                        <a id="resumeButton" class="button blue big"><%=TaskResource.TaskReopen%></a>
                    {{/if}}             
                {{/if}}
            </span>
        </div>
</script>

<script id="projects_taskDescriptionSubtasksContainerTmpl" type="text/x-jquery-tmpl">
        {{tmpl "projects_subtaskTemplate"}}
        {{if canEdit || canCreateSubtask}}           
            <div class="quickAddSubTaskLink icon-link plus" taskid="${id}" projectid="${projectOwner.id}" {{if status == 1 || status == 4}} visible="true" {{else}}visible="false" style="display:none;"{{/if}}>
                <span class="dottedLink" taskid="${id}" data-first="<%=TaskResource.CreateFirstSubtask%>"><%= TaskResource.AddNewSubtask%></span>
            </div>                 
        {{/if}}  
        <div class="st_separater" taskid="${id}"></div>       
</script>

<script id="projects_taskLinks" type="text/x-jquery-tmpl">
  <tr data-taskid="${id}" class="linked-task with-entity-menu{{if relatedTaskObject.invalidLink}} invalid-link{{/if}} {{if status==2}} gray-text{{/if}}">
        <td class="title stretch">
        {{if relatedTaskObject.invalidLink}}<span class="attantion">&nbsp;</span>{{/if}}
        <a class="task-name {{if status==2}} gray-text{{/if}}" href="tasks.aspx?prjID=${projectId}&id=${id}" target="_blank">${title}</a>
        </td>
        <td class="start-date" title="<%=TaskResource.TaskStartDate %>">{{if displayDateStart != ""}}${displayDateStart}{{/if}}</td>
        <td class="end-date" title="<%=TaskResource.EndDate %>">{{if displayDateDeadline != ""}}${displayDateDeadline}{{/if}}</td>
        <td class="link-type" title="<%= ProjectResource.GanttLinkType %>" data-type="${relatedTaskObject.linkType}">${relatedTaskObject.linkTypeText}</td>
        <td class="duration{{if relatedTaskObject.invalidLink}} red-text{{/if}}" title="<%=TaskResource.RelatedTaskIntervalDesc %>"> {{if relatedTaskObject.invalidLink}}-{{/if}}{{if relatedTaskObject.delay!=0}}${relatedTaskObject.delay}{{/if}}</td>
        <td class="actions">{{if canEdit}}<div class="entity-menu" data-taskid="${id}"></div>{{/if}}</td>
  </tr>
</script>
