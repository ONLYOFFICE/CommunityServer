<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" CodeBehind="JabberClient.aspx.cs" Inherits="ASC.Web.Talk.JabberClient" Title="Untitled Page" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<%@ Register Src="~/addons/talk/UserControls/TabsContainerPart.ascx" TagName="TabsContainerPart" TagPrefix="asct" %>
<%@ Register Src="~/addons/talk/UserControls/RoomsContainer.ascx" TagName="RoomsContainer" TagPrefix="asct" %>
<%@ Register Src="~/addons/talk/UserControls/MeseditorContainer.ascx" TagName="MeseditorContainer" TagPrefix="asct" %>
<%@ Register Src="~/addons/talk/UserControls/ContactsContainer.ascx" TagName="ContactsContainer" TagPrefix="asct" %>

<asp:Content ID="TalkHeaderContent" ContentPlaceHolderID="HeaderContent" runat="server">
  <!--[if IE 9]>
    <meta http-equiv="X-UA-Compatible" content="IE=EmulateIE9" />
  <![endif]-->
</asp:Content>

<asp:Content ID="TalkContent" ContentPlaceHolderID="PageContent" runat="server">
    <!--hack for artifact css-->
    <style type="text/css">
        header{ margin: 0; }
        main{ height: 100vh; padding: 0; }
        .page-content { margin: 0; padding: 0; overflow: auto; }
        .paging-content { padding: 0; }
        div#talkWrapper.hide {
            display:none
        }
    </style>
  <div id="talkWrapper" class="hide">
    <div class="left-side"></div>
    <div class="right-side"></div>
    <div class="container">
    <asct:TabsContainerPart ID="TalkTabsContainer" runat="server" style ="width:100% " /> 

    
  <!--   <hr /> -->        
      <div id="talkMainContainer">
        
        <div id="talkContentContainer" class="disabled">
          <div id="talkStartSplash" unselectable="on">
            <div class="background" unselectable="on"></div>
            <div class="container" unselectable="on">
             <!-- <div class="right-side" unselectable="on"></div> -->
              <div class="chat_image"></div>
              <div class="label" unselectable="on"><%=string.Format(TalkResource.LabelFirstSplash,"<br/>")%></div>
            </div>
          </div>
          <asct:RoomsContainer ID="TalkRoomsContainer" runat="server" />
          <div id="talkVertSlider" unselectable="on"></div>
          <asct:MeseditorContainer ID="TalkMeseditorContainer" runat="server" /> 
        </div>
          <div id="talkDialogsContainer">
          <div class="background"></div>

          <div class="dialog browser-notifications">
              <div class="toolbar" unselectable="on">
                  <div class="container" unselectable="on">
                    <div class="checkbox-container toggle-browser-notifications-dialog" unselectable="on">
                      <table unselectable="on">
                        <tr unselectable="on">
                          <td unselectable="on"><input id="cbxToggleNotificationsDialog" name="cbxToggleNotificationsDialog" class="checkbox toggle-notifications-dialog" type="checkbox" checked="checked" /></td>
                          <td unselectable="on"><label for="cbxToggleNotificationsDialog" unselectable="on"><%=TalkResource.LabelDoNotAsk%></label></td>
                        </tr>
                      </table>
                    </div>
                  </div>
                </div>
            <div class="head" unselectable="on">
       <!--       <div class="left-side"></div>-->
             <!-- <div class="right-side"></div>-->
              <div class="title" unselectable="on"><%=TalkResource.TitleBrowserNotificationsDialog%></div>
              <div class="button-talk close-dialog" unselectable="on">X</div>
            </div>
            <div class="content" unselectable="on">
              <div class="in" unselectable="on">
                <div class="body" unselectable="on">
                  <div id="cbxToggleNotifications" class="block toggle-notifications disabled" unselectable="on">
                    <div class="button-talk notifications-allow" unselectable="on"></div>
                    <div class="button-talk notifications-delay" unselectable="on"></div>
                  </div>
                </div>
                
              </div>
            </div>
          </div>


        <div class="popupContainerClass dialog kick-occupant">
            <div class="containerHeaderBlock">
                <table style="width: 100%; height: 0px;" cellspacing="0" cellpadding="0" border="0">
                    <tbody>
                        <tr valign="middle">
                            <td>
                                <input id="hdnKickJId" type="hidden" />
                                <input id="hdnKickInvited" type="hidden" />
                                <input id="hdnKickRoomCid" type="hidden" />
                                <div class="title" style="background:none; height:auto" unselectable="on"><%=TalkResource.TitleKickOccupantDialog%>&nbsp;<div class="dialogNameRoom"><span class="value"></span></div></div>
                            </td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" title="<%=TalkResource.Close%>">×</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="containerBodyBlock">
                <div class="singlefield"><%=TalkResource.LabelKickOccupant%></div>
                
                <div class="middle-button-container" style="margin-top:20px">
                    
                    <a class="button blue middle button-talk kick-occupant"><%=TalkResource.BtnKick%></a> 
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%=TalkResource.BtnCancel%></a> 
                   
                </div>
            </div>
        </div> 

        <div class="popupContainerClass dialog create-room">
            <div class="containerHeaderBlock">
                <table style="width: 100%; height: 0px;" cellspacing="0" cellpadding="0" border="0">
                    <tbody>
                        <tr valign="middle">
                            <td><%=TalkResource.TitleCreateRoom%></td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" title="<%=TalkResource.Close%>">×</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="containerBodyBlock">
                <label class="createchat" style="top:35px;display:inline-block;margin-right: 10px;"><%=TalkOverviewResource.Type%></label>
                <div class="chatOrSpam">
                    <select id="chatOrSpam" style="display:inline-block"> <option value="Chat room"><%=TalkOverviewResource.ChatRoom%></option><option value="Mailing"><%=TalkOverviewResource.Mailing%></option></select>
                </div>
                <table style="width: 100%; height: 0px; margin-top:10px" cellspacing="0" cellpadding="0" border="0">
                    <tr>
                        <td width="10%">
                            <label for="txtRoomName" style="margin-right: 10px;"><%=TalkResource.LabelName%>:</label>
                        </td>
                        <td width="90%">
                            <div class="roomName" title="<%=TalkResource.LabelValidSymbols%>:&nbsp;<%=TalkResource.ValidSymbols%>">
                                <input class="textEdit" id="txtRoomName" type="text" maxlength="255" required/> 
                                <label class="hint" for="txtRoomName" unselectable="on"><%=TalkResource.LabelValidSymbols%>:&nbsp;<%=TalkResource.ValidSymbols%></label> 
                            </div>
                        </td>
                    </tr>
                </table>
                <table style="margin-top:10px">
                    <tr>
                        <td style="vertical-align: top">
                            <input style="margin-left:0" id="cbxTemporaryRoom" name="cbxTemporaryRoom" class="checkbox temporar-room" type="checkbox" checked="checked" />
                        </td>
                        <td>
                            <label for="cbxTemporaryRoom"><%=TalkResource.LabelTemporaryRoom%></label>
                        </td>
                    </tr>
                </table>
                
                <div class="middle-button-container" style="margin-top:20px">
                    <a class="button blue middle button-talk create-room"><%=TalkResource.BtnCreate%></a> 
                    <span class="splitter-buttons"></span>
                    <!-- <a class="button gray middle" onclick="jq.unblockUI();"><%=TalkResource.BtnCancel%></a> -->
                    <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%=TalkResource.BtnCancel%></a> 
                   
                </div>
            </div>
          </div>

          <div class="dialog create-mailing">
            <div class="head" unselectable="on">
              <div class="left-side"></div>
              <div class="right-side"></div>
              <div class="title" unselectable="on"><%=TalkResource.TitleCreateMailing%></div>
              <div class="button-talk close-dialog" unselectable="on"></div>
            </div>
            <div class="content" unselectable="on">
              <div class="in" unselectable="on">
                <div class="body" unselectable="on">
                  <div class="field hint" unselectable="on">
                    <label for="txtMailingName" unselectable="on"><%=TalkResource.LabelName%>:</label>
                    <div class="textfield"><input  id="txtMailingName" type="text" maxlength="255"  /></div>
                      <input class="advansed-filter advansed-filter-input advansed-filter-complete" type="text"  maxlength="255" id="626575"/>
                    <label class="hint" for="txtRoomName"  unselectable="on" ><%=TalkResource.LabelValidSymbols%>:&nbsp;<%=TalkResource.ValidSymbols%></label>
                  </div>
                </div>
                <div class="toolbar" unselectable="on">
                  <div class="container" unselectable="on">
                    <div class="button-container" unselectable="on">
                      <div class="left-side" unselectable="on"></div>
                      <div class="right-side" unselectable="on"></div>
                      <div class="button-talk create-mailing" unselectable="on"><%=TalkResource.BtnCreate%></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>


        <div class="popupContainerClass dialog remove-room">
            <div class="containerHeaderBlock">
                <table style="width: 100%; height: 0px;" cellspacing="0" cellpadding="0" border="0">
                    <tbody>
                        <tr valign="middle">
                            <td>
                                <input id="hdnRemoveJid" type="hidden" />
                                <div class="title" style="background:none; height:auto" unselectable="on"><%=TalkResource.TitleRemoveRoomDialog%>&nbsp;<div class="dialogNameRoom"><span class="value"></span></div></div>
                            </td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" style="user-select:none" title="<%=TalkResource.Close%>">×</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            
            <div class="containerBodyBlock">
                <div class="singlefield" unselectable="on"><%=TalkResource.LabelRemoveRoom%></div>
                <div class="middle-button-container" style="margin-top:20px">
                    <a class="button blue middle button-talk remove-room"><%=TalkResource.BtnRemove%></a> 
                    <span class="splitter-buttons"></span>
                    <!-- <a class="button gray middle" onclick="jq.unblockUI();"><%=TalkResource.BtnCancel%></a> -->
                    <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%=TalkResource.BtnCancel%></a> 
                   
                </div>
            </div>
        </div>

        <div class="popupContainerClass dialog recv-invite">
            <div class="containerHeaderBlock">
                <table style="width: 100%; height: 0px;" cellspacing="0" cellpadding="0" border="0">
                    <tbody>
                        <tr valign="middle">
                            <td>
                                <div class="title" style="background:none; height:auto"><%=TalkResource.TitleRecvInvite%>&nbsp;<div class="dialogNameRoom"><span class="value"></span></div></div>
                            </td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" style="user-select:none" title="<%=TalkResource.Close%>">×</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="containerBodyBlock">
                <div class="singlefield" unselectable="on"><span id="lblInviterName" class="label" unselectable="on"><%=TalkOverviewResource.ChatRoom%></span>&nbsp;<%=TalkResource.LabelRecvInvite%></div>
                <input id="hdnInvitationRoom" type="hidden" />
                <input id="hdnInvitationContact" type="hidden" />
                <div class="middle-button-container" style="margin-top:20px">
                    <a class="button blue middle button-talk accept-invite "><%=TalkResource.BtnAccept%></a> 
                    <span class="splitter-buttons"></span>
                    <div class="button gray middle button-talk decline-invite"><%=TalkResource.BtnDecline%></div>
                </div>
            </div>
        </div>
     
              
        <div class="popupContainerClass dialog delete-files">
            <div class="containerHeaderBlock">
                <table style="width: 100%; height: 0px;" cellspacing="0" cellpadding="0" border="0">
                    <tbody>
                        <tr valign="middle">
                            <td><%=TalkResource.DeleteSentFiles%></td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" title="<%=TalkResource.Close%>">×</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="containerBodyBlock">
                <div><%=TalkResource.DeleteSentFilesInfo%></div>
                <div style="margin-top:10px">
                    <label>
                        <input type="radio" name="clearType" value="0" checked="checked"/>
                        <%=TalkResource.DeleteSentFilesAll%>
                    </label>
                </div>
                <div style="margin-top:10px">
                    <label>
                        <input type="radio" name="clearType" value="1"/>
                        <%=TalkResource.DeleteSentFilesMonth%>
                    </label>
                </div>
                <div style="margin-top:10px">
                    <label>
                        <input type="radio" name="clearType" value="2"/>
                        <%=TalkResource.DeleteSentFilesYear%>
                    </label>
                </div>
                <div class="middle-button-container" style="margin-top:20px">
                    <a class="button blue middle button-talk delete-files-btn"><%=TalkResource.BtnDelete%></a> 
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%=TalkResource.BtnCancel%></a> 
                </div>
            </div>
          </div>

     </div>
        <div id="talkHorSlider" unselectable="on"></div>
        <asct:ContactsContainer ID="TalkContactsContainer" runat="server" />
      </div>
    </div>
  </div>
</asp:Content>
