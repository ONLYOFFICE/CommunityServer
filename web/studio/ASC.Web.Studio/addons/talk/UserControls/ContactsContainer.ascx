<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactsContainer.ascx.cs" Inherits="ASC.Web.Talk.UserControls.ContactsContainer" %>

<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<div id="talkSidebarContainer" class="contactlist">
    <div id="talkContactToolbarContainer">
        <div class="toolbar" unselectable="on">
           <!-- <div class="button-container toggle-filter my" title="<%=TalkResource.HintFilter%>" unselectable="on">
                <div class="button-talk toggle-filter my" unselectable="on"></div>
            </div>-->
            <div id="talkFilterContainer">
                <div class="helper">
                    <input id="filterValue" type="text" placeholder="<%=TalkResource.Search%>" />
                    <div class="button-talk clear-filter hidden" unselectable="on" style="font-size: 20px;" >×</div>  
                </div>
            </div>
            <div class="button-container settings" unselectable="on">
                <div class="button-talk create-conference" id="button-create-conference" title="<%=TalkResource.HintCreateConference%>" unselectable="on"></div>

                <div id="button_settings" class="button-talk settings"></div>
                <div class="helper popupContainerClass container <%= HasFiles ? "has-files" : "" %>" id='pop'>
                    <div class="subs-only-button">
                        <span  class="sub-button"><a id="button-event-signal" class="on_off_button off"  ></a></span>
                        <div class="settingname"><%=TalkOverviewResource.VoicesOfEvents%></div>
                    </div>
                    <div class="subs-only-button settingsitem">
                        <span  class="sub-button"><a id="button-browser-notification" class="on_off_button off"  ></a></span>
                        <div class="settingname"><%=TalkOverviewResource.BrowsersNotification%></div>
                    </div> 
                    <div class="subs-only-button settingsitem">
                        <span  class="sub-button"><a id="button-list-contacts-groups" class="on_off_button off" ></a></span>
                        <div class="settingname"><%=TalkOverviewResource.GroupListOfContact%></div>
                    </div>
                    <div class="subs-only-button settingsitem">
                        <span  class="sub-button"><a id="button-offlineusers" class="on_off_button off"  ></a></span>
                        <div class="settingname"><%=TalkOverviewResource.HideOfflineContacts%></div>
                    </div>
                    <div class="subs-only-button settingsitem">
                        <span class="sub-button"><a id="button-send-ctrl-enter" class="on_off_button off" ></a></span>
                        <div class="settingname"><%=TalkOverviewResource.SendMessageHotButton%></div>
                    </div>
                    <div class="subs-only-button settingsitem delete-files-link">
                        <a id="button-clear-files" class="link dotline"><%=TalkOverviewResource.DeleteSentFiles%></a>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="talkContactsContainer">
        <ul class="contactlist webkit-scrollbar" unselectable="on">
            <li class="contact offline default" unselectable="on">
                <div class="state" unselectable="on"></div>
                <div class="toolbar" unselectable="on">
                    <div class="button-talk send-invite" title="<%=TalkResource.BtnSendInvite%>"></div>
                    <div class="button-talk add-to-mailing" title="<%=TalkResource.BtnAddToMailing%>" unselectable="on"></div>
                </div>
                <div class="title" unselectable="on"></div>
            </li>
        </ul>
        <ul class="grouplist webkit-scrollbar" unselectable="on">
            <li class="group default" unselectable="on">
                <div class="separator"></div>
                <div class="head" unselectable="on">
                    <div class="expander" unselectable="on"></div>
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
        <div id="talkContactToolbarContainer">
            <div class="unread-messages" title="<%=TalkResource.HintUnreadMessages%>" unselectable="on">
                <div class="button-container" unselectable="on">
                    <div class="left-side" unselectable="on"></div>
                    <div class="right-side" unselectable="on"></div>
                    <div class="button-talk read-new-message" unselectable="on">0</div>
                </div>
            </div>
            <div id="talkFilterContainers">
            </div>
            <div class="toolbar" unselectable="on" >
                <div id="talkStatusMenu" class="offline" unselectable="on">
                    <div id="talkConnectionStatus" unselectable="on">
                        <div class="icon" unselectable="on"></div>
                        <div class="title" unselectable="on"><%=TalkResource.Connection%></div>
                    </div>
                    <!--<div id="talkCurrentStatus" class="offline" unselectable="on">
                        <div class="state" unselectable="on"></div>
                        <div class="title" unselectable="on"><%=TalkResource.StatusOffline%></div>
                    </div>
                    <div class="helper" unselectable="on">
                        <div class="container popupContainerClass" unselectable="on">
                            <!-- <div class="top-border" unselectable="on"></div>
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
                    </div> -->
                </div>

            </div>
            <div class="button-container toggle-group my" title="<%=TalkResource.HintGroups%>" unselectable="on">
                <div class="button-talk toggle-group my" unselectable="on"></div>
            </div>
        </div>
    </div>
    <!--  <div class="label" unselectable="on"><%=TalkResource.LabelCurrentStatus%>:</div>--->
</div>

