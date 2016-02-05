<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoomsContainer.ascx.cs" Inherits="ASC.Web.Talk.UserControls.RoomsContainer" %>

<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Talk.Resources" %>


<div id="talkRoomsContainer">
  <ul class="rooms">
    <li class="room default">
      <div class="filtering-panel" unselectable="on">
        <div class="textfield filtering-field" unselectable="on"><input class="search-value" /></div>
        <div class="button-container search-start" title="<%=TalkResource.HintSearch%>" unselectable="on">
          <div class="button-talk search-start" unselectable="on"></div>
        </div>
        <div class="button-container search-prev-message" title="<%=TalkResource.HintSearchPrevMessage%>" unselectable="on">
          <div class="button-talk search-prev-message" unselectable="on"></div>
        </div>
        <div class="button-container search-next-message" title="<%=TalkResource.HintSearchNextMessage%>" unselectable="on">
          <div class="button-talk search-next-message" unselectable="on"></div>
        </div>
        <div class="custom-select filtering-menu" data-value="2" unselectable="on">
          <div class="title filter-value" title="<%=TalkResource.HistoryFilterLastMonth%>" unselectable="on"><%=TalkResource.HistoryFilterLastMonth%></div>
          <div class="helper" unselectable="on">
            <ul class="options" unselectable="on">
              <li class="option filter-option" data-id="0" title="<%=TalkResource.HistoryFilterLastDay%>" unselectable="on"><%=TalkResource.HistoryFilterLastDay%></li>
              <li class="option filter-option" data-id="1" title="<%=TalkResource.HistoryFilterLastWeek%>" unselectable="on"><%=TalkResource.HistoryFilterLastWeek%></li>
              <li class="option filter-option" data-id="2" title="<%=TalkResource.HistoryFilterLastMonth%>" unselectable="on"><%=TalkResource.HistoryFilterLastMonth%></li>
              <li class="option filter-option" data-id="3" title="<%=TalkResource.HistoryFilterAll%>" unselectable="on"><%=TalkResource.HistoryFilterAll%></li>
            </ul>
          </div>
        </div>
        <div class="button-container close-history">
          <div class="left-side"></div>
          <div class="right-side"></div>
          <div class="button-state close-history"></div>
          <div class="button-label close-history" title="<%=TalkResource.BtnCloseHistory%>" unselectable="on"><%=TalkResource.BtnCloseHistory%></div>
          <div class="button-talk close-history" title="<%=TalkResource.BtnCloseHistory%>" unselectable="on"></div>
        </div>
      </div>
      <div class="room-title" unselectable="on">
        <div class="size" unselectable="on">(<span class="all" unselectable="on">0</span>)</div>
      </div>
      <div class="room-separator" unselectable="on">
        <div class="button-talk toggle-minimizing" unselectable="on"></div>
      </div>
      <div class="sub-panel" unselectable="on">
        <div class="splash-contactlist" unselectable="on">
          <span class="label" unselectable="on">
            <span class="state" unselectable="on"></span>
            <span class="title" unselectable="on"><%=TalkResource.LabelConferenceSplash%></span>
          </span>
        </div>
        <ul class="contactlist" unselectable="on">
          <li class="contact default" unselectable="on">
            <div class="state" unselectable="on"></div>
            <div class="toolbar" unselectable="on">
              <div class="button-talk remove-member" title="<%=TalkResource.BtnRemoveMember%>" unselectable="on"></div>
            </div>
            <div class="title" unselectable="on"></div>
          </li>
        </ul>
      </div>
      <div class="messages">
        <ul class="messages">
          <li class="message default">
            <div class="head">
              <span class="title"></span>
              <span class="date">(<span class="value"></span>)</span>
            </div>
            <div class="body"></div>
          </li>
        </ul>
      </div>
      <div class="history">
        <ul class="messages">
          <li class="message default">
            <div class="head">
              <span class="title"></span>
              <span class="date">(<span class="value"></span>)</span>
            </div>
            <div class="body"></div>
          </li>
        </ul>
      </div>
    </li>
  </ul>
</div>
