<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactsContainer.ascx.cs" Inherits="ASC.Web.Talk.UserControls.ContactsContainer" %>

<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<div id="talkSidebarContainer" class="grouplist">
  <div id="talkContactToolbarContainer">
    <div class="unread-messages" title="<%=TalkResource.HintUnreadMessages%>" unselectable="on">
      <div class="button-container" unselectable="on">
        <div class="left-side" unselectable="on"></div>
        <div class="right-side" unselectable="on"></div>
        <div class="button-talk read-new-message" unselectable="on">0</div>
      </div>
    </div>
    <div class="toolbar" unselectable="on">
      <div class="button-container toggle-offlineusers" title="<%=TalkResource.HintOfflineContacts%>" unselectable="on"><div class="button-talk toggle-offlineusers" unselectable="on"></div></div>
      <div class="button-container toggle-group" title="<%=TalkResource.HintGroups%>" unselectable="on"><div class="button-talk toggle-group" unselectable="on"></div></div>
      <div class="button-container toggle-sounds" title="<%=TalkResource.HintSounds%>" unselectable="on"><div class="button-talk toggle-sounds" unselectable="on"></div></div>
      <div class="button-container toggle-notifications disabled" title="<%=TalkResource.HintNotifications%>" unselectable="on"><div class="button-talk toggle-notifications" unselectable="on"></div></div>
      <div class="button-container toggle-filter" title="<%=TalkResource.HintFilter%>" unselectable="on"><div class="button-talk toggle-filter" unselectable="on"></div></div>
    </div>
    <div id="talkFilterContainer">
      <div class="helper">
        <input id="filterValue" type="text" />
      </div>
    </div>
  </div>
  <div id="talkContactsContainer">
    <ul class="contactlist" unselectable="on">
      <li class="contact offline default" unselectable="on">
        <div class="state" unselectable="on"></div>
        <div class="toolbar" unselectable="on">
          <div class="button-talk send-invite" title="<%=TalkResource.BtnSendInvite%>"></div>
          <div class="button-talk add-to-mailing" title="<%=TalkResource.BtnAddToMailing%>" unselectable="on"></div>
        </div>
        <div class="title" unselectable="on"></div>
      </li>
    </ul>
    <ul class="grouplist" unselectable="on">
      <li class="group default" unselectable="on">
        <div class="separator"></div>
        <div class="head" unselectable="on">
          <div class="state" unselectable="on"></div>
          <div class="size" unselectable="on">(<span class="online" unselectable="on"></span><span class="separator">/</span><span class="all" unselectable="on"></span>)</div>
          <div class="title" unselectable="on"></div>
        </div>
        <ul class="contactlist" unselectable="on">
          <li class="contact offline default" unselectable="on">
            <div class="state" unselectable="on"></div>
            <div class="toolbar" unselectable="on">
              <div class="button-talk send-invite" title="<%=TalkResource.BtnSendInvite%>" unselectable="on"></div>
              <div class="button-talk add-to-mailing" title="<%=TalkResource.BtnAddToMailing%>" unselectable="on"></div>
            </div>
            <div class="title" unselectable="on"></div>
          </li>
        </ul>
      </li>
    </ul>
  </div>
  <div id="talkStatusContainer" unselectable="on">
    <div id="talkStatusMenu" class="offline" unselectable="on">
      <div class="left-side" unselectable="on"></div>
      <div class="right-side" unselectable="on"></div>
      <div id="talkCurrentStatus" class="offline" unselectable="on">
        <div class="state" unselectable="on"></div>
        <div class="title" unselectable="on"><%=TalkResource.StatusOffline%></div>
      </div>
      <div class="helper" unselectable="on">
        <div class="container" unselectable="on">
          <div class="left-side" unselectable="on"></div>
          <div class="right-side" unselectable="on"></div>
          <div class="top-border" unselectable="on"></div>
          <ul class="statuses" unselectable="on">
            <li class="status offline current" title="<%=TalkResource.StatusOffline%>" unselectable="on">
              <div class="state" unselectable="on"></div>
              <div class="title" unselectable="on"><%=TalkResource.StatusOffline%></div>
            </li>
            <li class="status online" title="<%=TalkResource.StatusOnline%>" unselectable="on">
              <div class="state" unselectable="on"></div>
              <div class="title" unselectable="on"><%=TalkResource.StatusOnline%></div>
            </li>
            <li class="status away" title="<%=TalkResource.StatusAway%>" unselectable="on">
              <div class="state" unselectable="on"></div>
              <div class="title" unselectable="on"><%=TalkResource.StatusAway%></div>
            </li>
            <li class="status xa" title="<%=TalkResource.StatusNA%>" unselectable="on">
              <div class="state" unselectable="on"></div>
              <div class="title" unselectable="on"><%=TalkResource.StatusNA%></div>
            </li>
          </ul>
        </div>
      </div>
    </div>
    <div class="label" unselectable="on"><%=TalkResource.LabelCurrentStatus%>:</div>
  </div>
</div>
