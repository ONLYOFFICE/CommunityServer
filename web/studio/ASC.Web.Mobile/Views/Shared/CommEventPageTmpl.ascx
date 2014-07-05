<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-community-event" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-community-item page-community-event ui-header{{if item.comments}} loaded-comments{{/if}}">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-event ui-btn-left ui-btn-row" href="#{{if $data.back}}${$data.back}{{else}}community/events{{/if}}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    <%-- <a class="ui-btn ui-btn-additem ui-btn-addevent ui-btn-right ui-btn-no-text target-self" href="#community/events/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a> --%>
  </div>
  <div class="ui-content">
    <div class="ui-item-title ui-event-title">
      <img src="${item.createdBy.avatar}" alt="${item.createdBy.displayName}" />
      <span class="item-title event-title">
        <span class="inner-text">${item.title}</span>
      </span>
      <div class="sub-info">
        <span class="timestamp">
          <span class="date">${item.displayDatetimeCrtdate}</span>
        </span>
        {{if item.createdBy}}
          <a class="author" href="#people/${item.createdBy.id}">${item.createdBy.displayName}</a>
        {{/if}}
      </div>
    </div>
    {{if item.text}}
      <div class="ui-item-content ui-event-content">{{html item.text}}</div>
    {{/if}}
    {{if item.poll}}
      <div class="poll-content">
        {{each item.poll.votes}}
          <div class="poll-item{{if $value.leader === true}} leader{{/if}}">
            <span class="item-title">${$value.title}</span>
            <span class="item-progress" style="width:{{if $value.percent === 0}}2px{{else}}${$value.percent * .8}%{{/if}}"><span class="progress-line"></span></span>
            <span class="item-value">${$value.percent}%</span>
          </div>
        {{/each}}
      </div>
    {{/if}}
    {{tmpl({item : item, classname : 'add-community-event-comment', label : '<%=Resources.MobileResource.BtnAddComment%>'}) '#template-addcomment-block'}}
    <ul class="ui-item-comments">
      {{if item.comments}}
        {{tmpl({comments : item.comments}) '#template-comments'}}
      {{/if}}
    </ul>
    <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
    <span class="ui-btn load-comments target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnLoadComments%></span></span></span>
    <a class="ui-btn add-comment target-self" href="#community/event/${item.id}/comment/add"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnAddComment%></span></span></a>
  </div>
</div>
</script>