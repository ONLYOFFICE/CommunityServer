<%@ Assembly Name="ASC.Web.People" %>
<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>

 <script id="userActionMenuTemplate" type="text/x-jquery-tmpl">
        <ul class="dropdown-content">
            {{if ($data.canEdit === "true") && ($data.isAdmin === true || $data.isMe === true ) && (user.status !== "blocked")}}
              <li><a class="edit-profile dropdown-item"><%= PeopleResource.LblEdit %></a></li>
            {{/if}}
            {{if ($data.canDel === "true") && ($data.isAdmin === true) && (user.status === "blocked")}}
              <li><a class="enable-profile dropdown-item"><%= PeopleResource.EnableUserButton%></a></li>
            {{/if}}
            {{if ($data.canEdit === "true") && ($data.isAdmin === true || $data.isMe === true) && (user.status !== "blocked") && (user.status !== "waited")}}
              <li><a class="change-password dropdown-item"><%= PeopleResource.LblChangePassword %></a></li>
            {{/if}}
            {{if ($data.isAdmin === true || $data.isMe === true) && (user.status !== "blocked") && (user.status !== "waited")}}
              <li><a class="change-email dropdown-item"><%= PeopleResource.LblChangeEmail %></a></li>
            {{/if}}
            {{if ($data.isAdmin === true || $data.isMe === true) && (user.status === "waited")}}
              <li><a class="email-activation dropdown-item"><%= PeopleResource.LblSendActivation%></a></li>
            {{/if}}
            {{if ($data.canDel === "true") && ($data.isAdmin === true) && ($data.isMe !== true) && (user.status !== "blocked")}}
              <li><a class="block-profile dropdown-item"><%= PeopleResource.DisableUserButton%></a></li>
            {{/if}}
            {{if (user.isOwner !== "true") && ((($data.canDel === "true") && ($data.isAdmin === true) && (user.status === "blocked")) || ($data.isMe === true)) }}
              <li><a class="delete-profile dropdown-item"><%= PeopleResource.LblDeleteProfile %></a></li>
            {{/if}}

        </ul>
</script>

<script id="userListTemplate" type="text/x-jquery-tmpl">
{{each(i, user) users}}
  <tr id="user_${user.id}" class="item profile {{if ($data.isAdmin === true || user.isMe === true) && (user.isMe === true || user.isPortalOwner !== true)}} with-entity-menu {{/if}} {{if user.isTerminated}} blocked{{else user.isActivated === false}} waited{{/if}} {{if user.isChecked == true}} selected{{/if}}"
        data-id="${user.id}"
        data-username="${user.userName}"
        data-email="${user.email}"
        data-displayname="${user.displayName}"
        data-status="{{if user.isTerminated}}blocked{{else user.isActivated === false}}waited{{/if}}"
        data-isAdmin="${user.isAdmin}"
        data-isOwner="${user.isPortalOwner}"
        data-isVisitor="${user.isVisitor}"
      >
       
    {{if ($data.isAdmin === true)}}
    <td class="check-list" id="checkRow_${user.id}">
        <input type="checkbox" id="checkUser_${user.id}" class="checkbox-user"
            {{if user.isChecked == true}}checked="checked"{{/if}} />
    </td>
    {{/if}}
    <td class="icon-list">
      <a class="ava" href="${user.link}">
        <img src="${user.avatarSmall}" title="${user.displayName}" /> 
        
        {{if ($data.isAdmin === true && user.isVisitor)}}<span class="role collaborator" title="<%= ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("Guest").HtmlEncode() %>"></span>{{/if}}
        {{if (user.isAdmin || listAdminModules.length) && (!user.isPortalOwner)}}<span class="role admin" title="<%= Resources.Resource.Administrator %>"></span>{{/if}}
        {{if (user.isPortalOwner)}}<span class="role owner" title="<%= Resources.Resource.Owner %>"></span>{{/if}}

        {{if (user.isTerminated || user.isActivated === false)}}<span class="status"></span> {{/if}}
      </a>
    </td>
    <td class="name">
        <a class="link bold" href="${user.link}">${user.displayName}</a>
      {{if user.bithdayDaysLeft != null}}
        <div class="birthday">
        {{if user.bithdayDaysLeft == 0}}<%= Resources.Resource.DrnToday %>
        {{else user.bithdayDaysLeft==1}}<%= Resources.Resource.DrnTomorrow %>
        {{else user.bithdayDaysLeft==2}}<%= Resources.Resource.In %> <% = DateTimeExtension.Yet(2) %>
        {{else user.bithdayDaysLeft==3}}<%= Resources.Resource.In %> <% = DateTimeExtension.Yet(3) %>
        {{/if}}
        </div>
      {{/if}}
        <div class="title">
            {{if user.isGroupManager && user.title.length}}
            <span class="inner-text manager">${user.title}</span>
            {{else}}
            <span class="inner-text">${user.title}</span>
            {{/if}}
        </div>
    </td>
    <td class="group">
        {{if user.groups.length === 1 && user.group != null}}
        <a class="link dotted text-overflow" title="${user.group.name}" href="#group=${user.group.id}">
            ${user.group.name}
        </a>
        {{else user.groups.length > 1}}
            <span id="peopleGroupsSwitcher_${user.id}" class="withHoverArrowDown">
                <span class="link dotted">${user.groups.length} <%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("Departments").HtmlEncode()%></span>
            </span>
            <div id="peopleGroups_${user.id}" class="studio-action-panel">
                <ul class="dropdown-content">
                    {{each(i, gr) user.groups}}
                    <li><a class="dropdown-item" href="#group=${gr.id}">${gr.name}</a></li>
                    {{/each}}
                </ul>
            </div>
        {{/if}}
    </td>
    <td class="location">
        <span class="inner-text">${user.location}</span>
    </td>
    <td class="info">
    {{if user.isTerminated}}
        <span class="red-text">
           <%= PeopleResource.BlockedMessage %>
        </span>
     {{else}}
          {{if user.isActivated === false && user.email}}
            <span class="email gray-text"><%= PeopleResource.WaitingForConfirmation %></span>
          {{/if}}
       {{/if}}
      {{if user.isActivated && !user.isTerminated && user.email}}
        <div id="peopleEmail_${id}" class="btn email">
            <span class="link dotted">${user.email}</span>
        </div>
        <div id="peopleEmailSwitcher_${id}" class="studio-action-panel">
            <ul class="dropdown-content">
                <li>
                    <a class="send-email dropdown-item"><%= PeopleResource.LblSendEmail %></a>
                </li>
                {{if !user.isMe && !user.isTerminated}}
         <li>
             <a class="open-dialog dropdown-item"><%= PeopleResource.LblSendMessage %></a>
         </li>
                {{/if}}
            </ul>
        </div>

      {{else}}
        &nbsp;
      {{/if}}
    </td>
    <td class="menu-action-list">
        {{if ($data.isAdmin === true || user.isMe === true) && (user.isMe === true || user.isPortalOwner !== true)}}
        <div id="peopleMenu_${id}" class="entity-menu" title="<%= PeopleResource.Actions %>"></div>
        {{/if}}
    </td>
  </tr>
{{/each}}
</script>


<script id="depAddPopupBodyTemplate" type="text/x-jquery-tmpl">
     <div class="clearFix block-cnt-splitter requiredField" >
        <span class="requiredErrorText"><%= PeopleResource.ErrorEmptyName %></span>
        <div class="headerPanelSmall header-base-small">
            <%= PeopleResource.Title %>:
        </div>
        <input type="text" id="studio_newDepName" class="textEdit" style="width: 99%;" maxlength="100" />
    </div>
    <div class="clearFix block-cnt-splitter">
        <div class="headerPanelSmall header-base-small">
            ${ASC.People.Resources.DepartmentMaster}:
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
        <a class="button blue middle"><%= PeopleResource.AddButton %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle"><%= PeopleResource.CancelButton %></a>
    </div>
</script>
