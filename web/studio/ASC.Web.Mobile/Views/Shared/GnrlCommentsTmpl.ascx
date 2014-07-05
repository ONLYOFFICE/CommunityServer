<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-comments" type="text/x-jquery-tmpl" async="true">
{{each comments}}
  <li class="item-comment{{if $value.isMine === true}} is-mine{{/if}}" data-commentid="${$value.id}">
    {{if !$value.inactive}}
        <div class="comment-body">
          {{if $value.createdBy}}
            <img class="item-avatar" src="${$value.createdBy.avatar}" alt="${$value.createdBy.displayName}" />
          {{/if}}
          <div class="ui-item-content">
            <span class="inner-text">{{html $value.text}}</span>
          </div>
          <div class="sub-info">
            <span class="timestamp">
              <span class="date">${$value.displayDatetimeCrtdate}</span>
            </span>
            {{if $value.createdBy}}
              <a class="author" href="#people/${$value.createdBy.id}">${$value.createdBy.displayName}</a>
            {{/if}}
          </div>
        </div>                
    {{else}}
        <div style="padding:10px;"><%=Resources.MobileResource.CommentWasRemoved%></div>
    {{/if}}
    <ul class="inline-comments">{{if $value.comments && $value.comments.length > 0}}{{tmpl($value) '#template-comments'}}{{/if}}</ul>
  </li>
{{/each}}
</script>