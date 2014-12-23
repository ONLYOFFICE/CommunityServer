<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script id="projects_templatesEditMilestoneTmpl" type="text/x-jquery-tmpl">
    <div class="milestone" id="m_${number}">
        <div class="mainInfo menuButtonContainer with-entity-menu">
            <span class="daysCount" value="${duration}"><span>${duration}</span></span>
            <div class="titleContainerEdit"><span class="title">${title}</span></div>
            <a class="addTask"> + <%=ProjectTemplatesResource.Task %></a>
            <span class="entity-menu"></span>
        </div>
        {{if displayTasks}}
        <div class="milestoneTasksContainer" style="display: block;">
        {{else}}
        <div class="milestoneTasksContainer">
        {{/if}}
            <div class="listTasks" milestone="m_${number}">
                {{each(i, task) tasks}}
                    <div id="${number}_${i+1}" class="task menuButtonContainer with-entity-menu">
                         <div class="titleContainer"><span class="title">${task.title}</span></div>
                        <span class="entity-menu"></span>
                    </div>
                {{/each}}
            </div>
        </div>
    </div>
</script>

<script id="projects_templatesEditTaskTmpl" type="text/x-jquery-tmpl">
    <div class="task menuButtonContainer with-entity-menu" id="t_${number}">
          <div class="titleContainer"><span class="title">${title}</span></div>
          <span class="entity-menu"></span>
    </div>
</script> 





<script id="projects_templatesCreateMilestoneTmpl" type="text/x-jquery-tmpl">
    <div class="milestone" id="m_${number}">
        <div class="mainInfo menuButtonContainer with-entity-menu">
            <span class="dueDate"><span>${date}</span></span>
            <div class="titleContainer"><span class="title">${title}</span></div>
            <a class="addTask"> + <%=ProjectTemplatesResource.Task %></a>
            <span class="entity-menu"></span>
            {{if chooseRep}}
            <span class="chooseResponsible">
                <span class="dottedLink" guid="${chooseRep.id}">${chooseRep.name}</span>
            </span>
            {{/if}}
        </div>
        {{if displayTasks}}
        <div class="milestoneTasksContainer" style="display: block;">
        {{else}}
        <div class="milestoneTasksContainer">
        {{/if}}
            <div class="listTasks" milestone="m_${number}">
                {{each(i, task) tasks}}
                    <div id="${number}_${i+1}" class="task menuButtonContainer with-entity-menu">
                        <div class="titleContainer"><span class="title">${task.title}</span></div>
                        <span class="entity-menu"></span>
                        {{if chooseRep}}
                        <span class="chooseResponsible nobody">
                            <span class="dottedLink"><%=ProjectsJSResource.ChooseResponsible %></span>
                        </span>
                        {{/if}}
                    </div>
                {{/each}}
            </div>
    </div>
</script>

<script id="projects_templatesCreateTaskTmpl" type="text/x-jquery-tmpl">
    <div class="task menuButtonContainer with-entity-menu" id="t_${number}">
          <div class="titleContainer"><span class="title">${title}</span></div>
          <span class="entity-menu"></span>
            {{if chooseRep}}
                    <span class="chooseResponsible">
                        <span class="dottedLink" guid="${chooseRep.id}">${chooseRep.name}</span>
                    </span>
            {{else}}
                {{if selectResp}}
                    <span class="chooseResponsible nobody">
                        <span class="dottedLink"><%=ProjectsJSResource.ChooseResponsible %></span>
                    </span>
                {{/if}}
            {{/if}}

    </div>
</script> 





<script id="projects_templateTmpl" type="text/x-jquery-tmpl">
        <tr class="with-entity-menu template menuButtonContainer" id="${id}">
              <td class="stretch">
              <a href="projectTemplates.aspx?id=${id}&action=edit" class="title">${title}</a>
              <span class="description">(${milestones} <%= ProjectTemplatesResource.Milestones %>, ${tasks} <%= ProjectTemplatesResource.Tasks %>)</span>
              </td>
              <td>
                <span class="entity-menu"></span>
              </td>
        </tr>
</script> 
