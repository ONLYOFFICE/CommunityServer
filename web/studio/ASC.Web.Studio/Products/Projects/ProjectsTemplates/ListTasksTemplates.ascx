<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script id="projects_taskListItemTmpl" type="text/x-jquery-tmpl">
            <div class="task with-entity-menu clearFix{{if !$item.data.subtasks.length}} noSubtasks{{/if}}{{if $item.data.canEdit}} canedit{{/if}}{{if $item.data.status == 2}} closed{{/if}}" taskid="${id}"{{if $item.data.milestone != null}}  milestoneid="${milestone.id}"{{/if}}>        
                
                {{if ($item.data.canEdit && !Teamlab.profile.isVisitor) || $item.data.canCreateTimeSpend || $item.data.canDelete}}
                    <div class="entity-menu" taskid="${id}" projectid="${projectOwner.id}" canDelete="${canDelete}" canEdit="${canEdit}" data-canCreateSubtask="${canCreateSubtask}" data-canCreateTimeSpend="${canCreateTimeSpend}" {{if responsible != null}}userid="${responsible.id}"{{/if}}></div>
                {{else}}
                    <div class="nomenupoint"></div>
                {{/if}}

                <div class="check">
                  <div taskid="${id}" class="changeStatusCombobox{{if $item.data.canEdit}} canEdit{{/if}}">
                      {{if $item.data.status == 2}}
                          <span title="<%= TaskResource.Closed%>" class="closed"></span>
                      {{else}}
                          <span title="<%= TaskResource.Open%>" class="open"></span>
                      {{/if}}   
                      {{if $item.data.canEdit}}<span class="arrow"></span> {{/if}}              
                  </div>
                </div>
                
                <div class="taskPlace">
                  <div class="taskName" taskid="${id}">
                    {{if $item.data.priority == 1}}
                      <span class="high_priority"></span>
                    {{/if}}
                        <a taskid="${id}" href="tasks.aspx?prjID=${projectOwner.id}&id=${id}" 
                            projectid = ${projectOwner.id}
                            description = "${description}"
                            {{if $item.data.milestone != null}}milestoneid="${milestone.id}" milestone="[${milestone.displayDateDeadline}] ${milestone.title}" milestoneurl="milestones.aspx?prjID=${projectOwner.id}&id=${milestone.id}"{{/if}}
                            {{if typeof $item.data.responsibles != 'undefined' && $item.data.responsibles.length}} responsible="${responsibles[0].displayName}"{{/if}}
                            {{if typeof $item.data.createdBy != 'undefined'}}createdBy="${createdBy.displayName}"{{/if}}
                            {{if typeof $item.data.updatedBy != 'undefined' && $item.data.updatedBy != null}}updatedBy="${updatedBy.displayName}"{{/if}}
                            project =  "${projectOwner.title}"
                            created="${displayDateCrtdate}" 
                            updated="${displayDateUptdate}"                         
                            status="${status}"
                            data-deadline="${deadline}"
                            data-start = "${displayDateStart}">
                            ${title}                    
                        </a>                    
					</div>
				    <div class="subtasksCount" data="<%=TaskResource.Subtask %>"> 
                        {{if ASC.Projects.TasksManager.openedCount($item.data.subtasks)}}
                            <span class="expand" taskid="${id}"><span class="dottedNumSubtask" title="<%=ProjectResource.TitleTaskOpenSubtasks %>">+{{html ASC.Projects.TasksManager.openedCount($item.data.subtasks)}}</span></span>
                        {{else}}
                            {{if $item.data.canCreateSubtask}}
                            <span class="add" taskid="${id}">+ <%=TaskResource.Subtask %></span>
                            {{/if}}                          
                        {{/if}} 
                    </div>   
                </div>  
                                                                                                                                                                                   
                {{if $item.data.responsibles.length > 1}}
                    <div taskid="${id}"  userId="${responsibles[0].id}">
                        <div class="otherMargin"><span taskid="${id}" class="other" title="<%=ProjectResource.TitleTaskResponsibleUsers %>">${responsibles.length} <%= TaskResource.Responsibles %></span></div>
                        <ul class="others" taskid="${id}">
                            {{each responsibles}}
                               {{if $index >= 0}}
                                    <li userId="${id}" class="user dropdown-item{{if id=='4a515a15-d4d6-4b8e-828e-e0586f18f3a3'}} not-action{{/if}}"><span  title="${displayName}">${displayName}</span></li>
                               {{/if}}                          
                            {{/each}}
                        </ul>                
                    </div>                        
                {{else}}
                
                    {{if $item.data.responsible == null}}
                        <div class="not user" taskid="${id}">
                            <span><%= TaskResource.NoResponsible%></span>
                        </div>
                    {{else}}
                        <div class="user" taskid="${id}" userId="${responsible.id}">
                                <span {{if responsible.id=='4a515a15-d4d6-4b8e-828e-e0586f18f3a3'}}class="not-action"{{else}} title="<%=ProjectResource.TitleTaskResponsibleUser %>"{{/if}}>${responsible.displayName}</span>                                                  
                        </div>            
                    {{/if}}                                
                {{/if}}
                {{if $item.data.displayDateDeadline.length}} 
                    <div class="deadline">
                      {{if ASC.Projects.TasksManager.compareDates($item.data.deadline)}}
                          <span id = "${id}" class="timeLeft red" deadline = ${displayDateDeadline}>${displayDateDeadline}</span>
                      {{else}}
                          <span id = "${id}" class="timeLeft" deadline = ${displayDateDeadline}>${displayDateDeadline}</span>
                      {{/if}}                                                            
                    </div>
                {{else}}
                    {{if $item.data.milestone != null}}                            
                        <div class='deadline'>
                            {{if !ASC.Projects.TasksManager.compareDates($item.data.milestone.deadline)}}
                                <span id = "${id}" class="timeLeft" deadline = ${milestone.displayDateDeadline}>${milestone.displayDateDeadline}</span>
                            {{else}}
                                <span id = "${id}" class="timeLeft red" deadline = ${milestone.displayDateDeadline}>${milestone.displayDateDeadline}</span>
                            {{/if}}                            
                        </div>
                    {{/if}} 
                {{/if}} 
            </div>
            <div taskid="${id}" class="subtasks"{{if !$item.data.subtasks.length || $item.data.status == 2}} style="display:none;"{{/if}} projectid="${projectOwner.id}">    
                {{tmpl 'projects_subtaskTemplate'}}
                {{if $item.data.canEdit || $item.data.canCreateSubtask}}
                    {{if ($item.data.status == 1 || $item.data.status == 4) && ($item.data.subtasks.length)}}
                        <div class="quickAddSubTaskLink icon-link plus" taskid="${id}" projectid="${projectOwner.id}" visible="true"> 
                    {{else}}
                        <div class="quickAddSubTaskLink icon-link plus" taskid="${id}" projectid="${projectOwner.id}" visible="false" style="display:none;">
                    {{/if}}                                                
                            <span class="dottedLink" taskid="${id}"><%= TaskResource.AddNewSubtask%></span>
                        </div>                           
                {{/if}}  
                <div class="st_separater" taskid="${id}"></div>       
             </div> 
    </script>

    <script id="projects_milestoneForMoveTaskPanelTmpl" type="text/x-jquery-tmpl">               
            <div class="ms">
                <input id="ms_${id}" value="${id}" type="radio" name="milestones"/>
                <label for="ms_${id}">[${displayDateDeadline}] ${title}</label>
            </div>             
    </script>