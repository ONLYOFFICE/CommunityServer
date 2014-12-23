<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script id="projects_discussionTemplate" type="text/x-jquery-tmpl">
    <div class="container-list">
      <div class="header-list clearFix">
          <div class="avatar-list">
              <img src="${authorAvatar}" alt="${authorName}"/>
          </div>
          <div class="describe-list">
              <div class="title-list">
                  <a href="${discussionUrl}">
                      ${title}
                  </a>
                  {{if status == 1}}
                    <span class="status"><%= MessageResource.ArchiveDiscussionStatus %></span>
                  {{/if}}
              </div>
              <div class="info-list">
                    <span class="caption-list"><%=MessageResource.AuthorTitle%>:</span>
                    <span class="name-list author{{if authorId=='4a515a15-d4d6-4b8e-828e-e0586f18f3a3'}} not-action{{/if}}" data-authorId="${authorId}">${authorName}</span>
              </div>
              {{if projectTitle}}
                  <div class="info-list">          
                        <span class="caption-list"><%=ProjectResource.Project%>:</span>
                        <span class="name-list project" data-projectId="${projectId}">${projectTitle}</span>
                  </div>
              {{/if}}
              <div class="date-list">
                    ${createdDate}<span class="time-list">${createdTime}</span>
              </div>
          </div>
        </div>
        <div class="content-list">
                {{html htmlUtility.getFull(text)}}
                
                {{if hasPreview}}
                   <div><a class="read-more" target="_blank" href="${discussionUrl}"><font><%=MessageResource.ReadMore%></font></a></div>
                {{/if}}
                
                <div class="comment-list">
                    <a href="${commentsUrl}"><%=MessageResource.Comments%>: ${commentsCount}</a>
                    {{if canComment}}
                    <a href="${writeCommentUrl}"><%=ProjectsCommonResource.WriteComment%></a>
                    {{/if}}
                </div>
        </div>
    </div>
</script>

<script id="projects_discussionActionTemplate" type="text/x-jquery-tmpl">
    <div class="discussionContainer">
        <div class="discussionHeaderContainer">
            <span class="discussionHeader">${title}</span>
        </div>
        <div class="discussionAuthorAvatar">
            <img src="${authorAvatarUrl}" alt="${authorName}"/>
        </div>
        <div class="discussionDescriptionContainer">
            <div>
                <a class="link bold" href="${authorPageUrl}">
                    ${authorName}
                </a>
                <span class="discussionCreatedDate">
                    ${createOn}
                </span>
            </div>
            <div class="discussionAuthorTitle">
                ${authorTitle}
            </div>
            <div class="discussionText">
                {{html content}}
            </div>
            <div style="clear: both;"></div>
        </div>
    </div>
</script>

<script id="projects_subscribedUser" type="text/x-jquery-tmpl">
    <li class="items-display-list_i" guid='${id}'>
        <span class="item-name">
            {{if hidden}}
            <a class="link gray" href="${profileUrl}" target="_blank">${displayName}</a>
            {{else}}
            <a class="link" href="${profileUrl}" target="_blank">${displayName}</a>
            {{/if}}
        </span>
        {{if !descriptionFlag}}<div class="reset-action"></div>{{/if}}
    </li>
</script>