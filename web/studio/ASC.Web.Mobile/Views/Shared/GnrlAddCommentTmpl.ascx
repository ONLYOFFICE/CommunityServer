<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-addcomment-block" type="text/x-jquery-tmpl" async="true">
<div class="ui-item-content">
  <div class="addcomment-item-container item-textfield">
    <input class="comment-type" type="hidden" value="${item.type}" />
    <input class="comment-id" type="hidden" value="${item.id}" />
    <input class="comment-parentid" type="hidden" value="${parentid}" />
    <input class="comment-subject" type="hidden" value="${item.title}" />
    <textarea class="ui-text-area comment-content"></textarea>
  </div>
  <div class="addcomment-item-container create-comment">
    {{if $data.classname && $data.label}}
      <button class="create-item create-comment ${classname}">${label}</button>
    {{else}}
      {{if item.type == 'forum'}}
        <button class="create-item create-comment"><%=Resources.MobileResource.BtnAddPost%></button>
      {{else}}
        <button class="create-item create-comment"><%=Resources.MobileResource.BtnAddComment%></button>
      {{/if}}
    {{/if}}
  </div>
</div>
</script>