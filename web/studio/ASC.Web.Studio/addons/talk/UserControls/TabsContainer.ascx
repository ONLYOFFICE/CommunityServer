<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TabsContainer.ascx.cs" Inherits="ASC.Web.Talk.UserControls.TabsContainer" %>

<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<div id="talkTabContainer">
  <ul class="tabs">
    <li class="tab default" unselectable="on">
      <div class="background" unselectable="on"><div class="helper" unselectable="on"></div></div>
      <div class="left-side" unselectable="on"></div>
      <div class="right-side" unselectable="on"></div>
      <div class="container" unselectable="on">
        <div class="state" unselectable="on"></div>
        <div class="button-talk close" unselectable="on"></div>
        <div class="tab-title" unselectable="on"></div>
      </div>
    </li>
  </ul>
  <div class="navigation" unselectable="on">
    <div class="button-container move-to-left" title="<%=TalkResource.HintShowLeftTab%>" unselectable="on"><div class="button-talk move-to-left"></div></div>
    <div class="button-container move-to-right" title="<%=TalkResource.HintShowRightTab%>" unselectable="on"><div class="button-talk move-to-right"></div></div>
  </div>
  <div id="talkTabInfoBlock" class="info-block information">
    <div class="left-side"></div>
    <div class="right-side"></div>
    <div class="container">
      <div class="state"></div>
      <div class="title">
        <span class="hint"></span>
        <span class="information"></span>
        <span class="conference">
          <span class="separator"><%=TalkResource.LabelTopic%>:</span>
          <span class="conference-subject"></span>
        </span>
        <span class="chat">
          <span class="department"></span>
          <span class="separator"><%=TalkResource.LabelStatus%>:</span>
          <span class="show"></span>
          <span class="message"></span>
        </span>
      </div>
    </div>
  </div>
</div>
