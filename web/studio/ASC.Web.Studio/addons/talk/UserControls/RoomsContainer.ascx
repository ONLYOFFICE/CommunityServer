<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoomsContainer.ascx.cs" Inherits="ASC.Web.Talk.UserControls.RoomsContainer" %>
<%@ Import Namespace="ASC.Web.Talk.Resources" %>


<div id="talkRoomsContainer">

    <div id="talkTabInfoBlock" class="info-block information">
    <div class="left-side"></div>
    <div class="right-side"></div>
    <div class="container">
      <div class="state"></div>
      <div class="title">

        <span class="information"></span>
        <span class="conference">
          <span class="separator"><%=TalkResource.LabelTopic%>:</span>
          <span class="conference-subject"></span>
        </span>
        <span class="chat">
          <span class="department"></span>
          <span class="separator"></span>
          <span class="show"></span>
         <!-- <span class="message"></span>  -->
        </span>
      </div>
    </div>
  </div>

  <ul class="rooms">
      
    <li class="room default">
     <div class="filtering-panel close" unselectable="on" >
            
            <div class="filtering-panel-tools">
                <div class="searchmessagecount">
                    <span class="currentfoundmessage">0</span>
                    <span><%=TalkResource.SeparatorOfFoundMessages%></span>
                    <span class="countfoundmessage">0</span>
                </div>
                <div class="button-talk clear-search"></div>  
                <div class="search_nextbutton_container">
                    <div class="button-container search-next-message" title="<%=TalkResource.HintSearchNextMessage%>" unselectable="on">
                        <div class="button-talk search-next-message" unselectable="on"></div>
                    </div>
                </div>
                <div class="search_prevbutton_container">
                    <div class="button-container search-prev-message my" title="<%=TalkResource.HintSearchPrevMessage%>" unselectable="on">
                        <div class="button-talk search-prev-message" unselectable="on"></div>
                    </div>
                </div>
                
            </div>
            <div class="filtering-container">
                <div class="textfield filtering-field" unselectable="on">
                    <input class="search-value" placeholder="<%=TalkResource.SearchOnHistory%>" style="margin-left: 6px" onfocus="ASC.TMTalk.roomsManager.focusHistorySearch()" onblur="ASC.TMTalk.roomsManager.blurHistorySearch()" onkeyup="ASC.TMTalk.roomsManager.keyupHistorySearch()" onkeypress="ASC.TMTalk.roomsManager.keydownHistorySearch()"/>
                </div>
               <div class="button-container search-start my" title="<%=TalkResource.HintSearch%>" unselectable="on">
                    <div class="button-talk search-start" unselectable="on"></div>
                </div> 
            </div>
        </div>
     
      <div class="room-title" unselectable="on">
        <div class="size with-entity-menu openUserList" unselectable="on"><%=TalkResource.TotalUsers%>: <span class="all" unselectable="on">0</span></div>
        <div class="with-entity-menu removeRoom"><%=TalkResource.BtnRemoveConference%></div>
        <div class="with-entity-menu removeMailing"><%=TalkResource.BtnRemoveMailing%></div>
        
      </div>

      <div class="sub-panel border webkit-scrollbar" unselectable="on">
        <div class="splash-contactlist" unselectable="on">
          <span class="label" unselectable="on">
            <span class="state" unselectable="on"></span>
            <span class="title" unselectable="on"><%=TalkResource.LabelConferenceSplash%></span>
          </span>
        </div>
          <!--<div class="with-entity-menu removeRoom"><%=TalkResource.BtnRemoveConference%></div>
        <div class="with-entity-menu removeMailing"><%=TalkResource.BtnRemoveMailing%></div>
        <div class="with-entity-menu openUserList"><%=TalkResource.ShowUserList%></div>
          <div class="entity-menu toggle-minimizing"></div> -->
        
        <div class="size" unselectable="on"><%=TalkResource.TotalUsers%>: <span class="all" unselectable="on">0</span></div>
        

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
        <ul class="messages webkit-scrollbar">
            <li class="message default">
                <span class="daysplit"><span class="value"></span></span>
                <div class="message-container">
                    <div class="date date-left">
                        <span class="value"></span>
                    </div>
                    <div class="message">
                        <div class="head"><span class="title"></span></div>
                        <div class="body"></div>
                    </div>
                    <div class="date date-right">
                        <span class="value"></span>
                    </div>
                </div>
            </li>
        </ul>
      </div>
      <div class="history">
        <ul class="messages">
          <li class="message default">
            <div class="head">
              <span class="title"></span>
              <span class="date"><span class="value"></span></span>
            </div>
            <div class="body"></div>
          </li>
        </ul>
      </div>
    </li>
  </ul>
  
   <!-- <hr id="fixedbottom"/> 
    <div class="close-history-container" >  
        <div class="button-talk close-history" id="closeHistory">   
            <div class="button gray button-label close-history"  title="<%=TalkResource.BtnCloseHistory%>" unselectable="on"><%=TalkResource.BtnCloseHistory%></div>          
        </div>
    </div>
    <div id="closeHistory" class="button-container">
        <div class="button gray button-talk close-history" ><%=TalkResource.BtnCloseHistory%>
        <div class="button-talk close-history"></div></div>
    </div>-->
</div>
