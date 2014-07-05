<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script id="projects_subtaskTemplate" type="text/x-jquery-tmpl">               
  {{each subtasks}}
        <div id="subtask_${id}" status="${status}" class="with-entity-menu subtask clearFix {{if status == 2}} closed{{/if}}{{if $item.data.canEdit}} canedit{{/if}}" subtaskid="${id}" taskid="${$item.parent.data.id}" projectid="${$item.parent.data.projectOwner.id}">        
            {{if $item.parent.data.canEdit}}<div class="entity-menu" subtaskid="${id}" taskid="${$item.parent.data.id}"></div>
            {{else}}<div class="nomenupoint"></div>{{/if}}
            <div class="check"><input type="checkbox"{{if status == 2}} checked="checked"{{/if}}{{if !canEdit || $item.data.status == 2}} disabled="true"{{/if}} taskid="${$item.parent.data.id}" subtaskid="${id}"/></div>    
            <div class="taskName{{if $item.data.canEdit}} canedit{{/if}}" 
                subtaskid="${id}" 
                created="${displayDateCrtdate}"   
                {{if typeof $item.data.createdBy != 'undefined'}}createdBy="${createdBy.displayName}"{{/if}}
                 {{if updatedBy != null}} updatedBy="${updatedBy.displayName}" {{/if}}
                status ="${status}"
                updated="${displayDateUptdate}"><span>{{html jq.linksParser(jq.htmlEncodeLight($item.data.subtasks[$index].title))}}</span></div>            
            {{if $item.data.subtasks[$index].responsible == null}}
                <div class="not user" me="<%= ProjectResource.My%>" taskid="${id}" subtaskid="${id}">
                        <span><%= TaskResource.NoResponsible%></span>
                </div>
            {{else}}                        
                <div class="user" subtaskid="${$item.data.subtasks[$index].id}" data-userId="${$item.data.subtasks[$index].responsible.id}" title="${$item.data.subtasks[$index].responsible.displayName}">
                        ${$item.data.subtasks[$index].responsible.displayName}                        
                </div>            
            {{/if}}
        </div>      
   {{/each}}  
</script>

<script id="projects_newSubtaskTemplate" type="text/x-jquery-tmpl">
    <div id="subtask_${id}" status="${status}" class="with-entity-menu subtask clearFix canedit" subtaskid="${id}" taskid="${taskid}" projectid="${projectid}">        
        <div class="entity-menu" subtaskid="${id}" taskid="${taskid}"></div>
        <div class="check"><input type="checkbox" taskid="${taskid}" subtaskid="${id}"/></div>    
        <div class="taskName canedit" 
            subtaskid="${id}" 
            created="${displayDateCrtdate}"   
            {{if typeof createdBy != 'undefined'}}createdBy="${createdBy.displayName}"{{/if}}
            status ="${status}"
            updated="${displayDateUptdate}"><span>{{html jq.linksParser(jq.htmlEncodeLight(title))}}</span></div>            
        {{if responsible == null}}
            <div class="not user" taskid="${id}" subtaskid="${id}">
                    <span><%= TaskResource.NoResponsible%></span>
            </div>
        {{else}}                        
            <div class="user" subtaskid="${id}" data-userId="${responsible.id}">
                    ${responsible.displayName}                          
            </div>            
        {{/if}}
    </div>
</script>

<script id="projects_fieldForAddSubtask" type="text/x-jquery-tmpl">
    <table id="quickAddSubTaskField" class="add-subtask-field-wrapper display-none" >
        <tr>    
            <td class="subtask-name border-style">
                <input class="subtask-name-input" type="text" value="${title}" data-subtaskid="${subtaskid}" data-taskid="${taskid}" data-projectid="${projectid}" maxlength="255"/>
            </td>
            <td class="subtask-responsible border-style">
                <select class="subtask-responsible-selector"  data-userid="{{if responsible != null}}${responsible.id}{{/if}}" data-username="{{if responsible != null}}${responsible.displayName}{{/if}}">
                    <option value="-1" class="hidden"><%= TaskResource.ChooseResponsible%></option>
                    <option value="<%=Guid.Empty %>"><%= TaskResource.NoResponsible %></option>
                </select>
            </td>                    
            <td class="subtask-add-button">
                <a class="button gray"><%=ProjectResource.OkButton %></a>
            </td>
        </tr>
    </table>
</script>

<script id="projects_subtaskActionPanelTmpl" type="text/x-jquery-tmpl">
    <div id="subtaskActionPanel" class="studio-action-panel">
	    <div class="corner-top right"></div>        
        <ul class="dropdown-content">
            <li id="sta_edit" class="dropdown-item"><span><%= TaskResource.Edit%></span></li>
            <li id="sta_accept" class="dropdown-item"><span><%= TaskResource.AcceptSubtask%></span></li>
            <li id="sta_remove" class="dropdown-item"><span><%= ProjectsCommonResource.Delete%></span></li>
        </ul>
    </div>
</script>
