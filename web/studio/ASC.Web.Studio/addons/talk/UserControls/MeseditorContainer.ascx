<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MeseditorContainer.ascx.cs" Inherits="ASC.Web.Talk.UserControls.MeseditorContainer" %>

<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<div id="talkMeseditorContainer" class="disabled">
    <div class="overflow-layer"></div>
    <div id="talkTextareaContainer">
        <ul class="textareas">
            <li class="textarea default">
                <iframe src="about:blank"></iframe>
                <div class="meseditorContainerPlaceholder"><%=TalkResource.WriteMessage%></div>
            </li>
        </ul>
    </div>
    <%-- <iframe id="messageContainer" src="<%=VirtualPathUtility.ToAbsolute("~/addons/talk/blank.html")%>"></iframe> --%>
    <div id="talkMeseditorToolbarContainer">





        <div class="toolbar" unselectable="on">
            <div class="button-container emotions my" title="<%=TalkResource.HintEmotions%>" unselectable="on">

                <div class="button-talk emotions" unselectable="on"></div>

                <div class="helper" unselectable="on">
                    <div class="popupContainerClass container" unselectable="on">
                        <div class="top-border" unselectable="on"></div>
                        <div class="left-top-side" unselectable="on"></div>
                        <div class="right-top-side" unselectable="on"></div>
                        <ul class="smiles" unselectable="on">
                            <li class="smile smile01" title=":-)" unselectable="on"></li>
                            <li class="smile smile02" title=";-)" unselectable="on"></li>
                            <li class="smile smile03" title=":-\" unselectable="on"></li>
                            <li class="smile smile04" title=":-D" unselectable="on"></li>
                            <li class="smile smile05" title=":-(" unselectable="on"></li>
                            <li class="smile smile06" title="8-)" unselectable="on"></li>
                            <li class="smile smile07" title="*DANCE*" unselectable="on"></li>
                            <li class="smile smile08" title="[:-}" unselectable="on"></li>
                            <li class="smile smile09" title="%-)" unselectable="on"></li>
                            <li class="smile smile10" title="=-O" unselectable="on"></li>
                            <li class="smile smile11" title=":-P" unselectable="on"></li>
                            <li class="smile smile12" title=":'(" unselectable="on"></li>
                            <li class="smile smile13" title=":-!" unselectable="on"></li>
                            <li class="smile smile14" title="*THUMBS UP*" unselectable="on"></li>
                            <li class="smile smile15" title="*SORRY*" unselectable="on"></li>
                            <li class="smile smile16" title="*YAHOO*" unselectable="on"></li>
                            <li class="smile smile17" title="*OK*" unselectable="on"></li>
                            <li class="smile smile18" title="]:->" unselectable="on"></li>
                            <li class="smile smile19" title="*HELP*" unselectable="on"></li>
                            <li class="smile smile20" title="*DRINK*" unselectable="on"></li>
                        </ul>
                        <div class="bottom-border" unselectable="on"></div>
                        <div class="left-bottom-side" unselectable="on"></div>
                        <div class="right-bottom-side" unselectable="on"></div>
                    </div>
                </div>
            </div>
            <div class="button-container send-file my" id="talkFileUploaderFake" title="<%=TalkResource.HintSendFile%>" unselectable="on">
            </div>
            <div class="button-container send-file my display-none" id="talkFileUploader" title="<%=TalkResource.HintSendFile%>" unselectable="on">
            </div>
            <!--<div class="button-container searchmessage my" title="">
                    <div class="button-talk searchmessage my"></div>
             </div>
            <asp:PlaceHolder ID="talkHistoryButton" runat="server">
                <div class="button-container history my" title="<%=TalkResource.HintHistory%>" unselectable="on">
                    <div class="button-talk history my" unselectable="on"></div>
                </div>
            </asp:PlaceHolder>-->
            <!--  <asp:PlaceHolder ID="talkToggleSendButton" runat="server">
        <div class="button-container toggle-sendbutton" title="<%=TalkResource.HintCtrlEnterSender%>" unselectable="on"><div class="button-talk toggle-sendbutton" unselectable="on"></div></div>
      </asp:PlaceHolder>
      <asp:PlaceHolder ID="talkMassendButton" runat="server">
        <div class="button-container create-massend" title="<%=TalkResource.HintCreateMassend%>" unselectable="on"><div class="button-talk create-massend" unselectable="on"></div></div>
      </asp:PlaceHolder>
      <asp:PlaceHolder ID="talkConferenceButton" runat="server">
        <div class="button-container create-conference" title="<%=TalkResource.HintCreateConference%>" unselectable="on"><div class="button-talk create-conference" unselectable="on"></div></div>
      </asp:PlaceHolder>   -->
            
            <!-- <div class="button-container remove-mailing">
                <div class="left-side"></div>
                <div class="right-side"></div>
                <div class="button-state remove-mailing"></div>
                <div class="button-label remove-mailing" title="<%=TalkResource.BtnRemoveMailing%>" unselectable="on"><%=TalkResource.BtnRemoveMailing%></div>
                <div class="button-talk remove-mailing" title="<%=TalkResource.BtnRemoveMailing%>" unselectable="on"></div>
            </div> -->

           <!-- <div class="button-container remove-conference">
                <div class="left-side"></div>
                <div class="right-side"></div>
                <div class="button-state remove-conference"></div>
                <div class="button-label remove-conference" title="<%=TalkResource.BtnRemoveConference%>" unselectable="on"><%=TalkResource.BtnRemoveConference%></div>
                <div class="button-talk remove-conference" title="<%=TalkResource.BtnRemoveConference%>" unselectable="on"></div>
            </div> -->
        </div>
        <div id="talkSendMenu">
            <div class="button-container history" title="<%=TalkResource.HintHistory%>" unselectable="on">
                <div class="button-talk history" unselectable="on"></div>
            </div>
            <div class="button-container toggle-sendbutton" title="<%=TalkResource.HintCtrlEnterSender%>" unselectable="on">
                <div class="button-talk toggle-sendbutton" unselectable="on"></div>
            </div>
            <div class="button-container create-massend" title="<%=TalkResource.HintCreateMassend%>" unselectable="on">
                <div class="button-talk create-massend" unselectable="on"></div>
            </div>
            <div class="button-container create-conference my" title="<%=TalkResource.HintCreateConference%>" unselectable="on">
                <div class="button-talk create-conference" unselectable="on"></div>
            </div>
            <div class="left-side"></div>
            <div class="right-side"></div>
            
            <div class="button blue" ><%=TalkResource.LabelSend%>             
            <div class="button-talk send-message" unselectable="on"></div></div>
        </div>

    </div>
    <div style="clear: both;"></div>
</div>
