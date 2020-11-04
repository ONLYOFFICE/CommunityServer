<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmallChat.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.SmallChat.SmallChat" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>

<div class="small_chat_main_window" data-error="<%= ChatResource.ServerError %>">
    <div class="icon_ch_size display-none"></div>
    <table class="small_chat_top_panel display-none">
        <tbody>
            <tr>
                <td class="small_chat_search_field_container">
                    <input class="small_chat_search_field textEdit" type="text" placeholder="<%= ChatResource.Search %>"/>
                    <div class="search_icon search_icon_image" title="<%= ChatResource.Search %>"></div>
                </td>
               <!-- <td class="small_chat_status_menu_container">
                    <div class="small_chat_status_menu" title="<%= ChatResource.StatusOnline %>">
                        <div class="small_chat_text_status unselect_text image_online"><%= ChatResource.StatusOnline %></div>
                        <div class="studio-action-panel" id="smallChatPopupID">
                            <ul class="dropdown-content">
                                <li class="dropdown-item user_status online disable" title="<%= ChatResource.StatusOnline %>">
                                    <div class="small_chat_image_state image_online"></div><%= ChatResource.StatusOnline %></li>
                                <li class="dropdown-item user_status away" title="<%= ChatResource.StatusAway %>">
                                    <div class="small_chat_image_state image_away"></div><%= ChatResource.StatusAway %></li>
                                <li class="dropdown-item user_status not_available" title="<%= ChatResource.StatusNA %>">
                                    <div class="small_chat_image_state image_not_available"></div><%= ChatResource.StatusNA %></li>
                                <li class="dropdown-item user_status offline" title="<%= ChatResource.StatusOffline %>">
                                    <div class="small_chat_image_state image_offline"></div><%= ChatResource.StatusOffline %></li>
                            </ul>
                        </div>
                    </div>
                </td> -->
            </tr>
        </tbody>
    </table>
    <div class="contact_container display-none webkit-scrollbar">
        <div class="small_chat_contact_not_found_record display-none"><%= ChatResource.UserNotFound %></div>
        <div class="chat_contact_loader display-none"></div>
    </div>
    <div class="small_chat_down_panel">
        <div class="show_small_chat_icon small_chat_icon_white down_panel_icon" title="<%= ChatResource.ShowHideChatAltTitle %>"></div>
        <div class="extend_chat_icon down_panel_icon" title="<%= ChatResource.ExtendChatAltTitle %>"></div>
        <div class="small_chat_option_icon down_panel_right_icon" title="<%= ChatResource.SmallChatOptionsAltTitle %>"></div>
    </div>
</div>
<div class="studio-action-panel" id="smallChatOptionsPopupID">
    <ul class="dropdown-content">
        <li class="dropdown-item small_chat_en_dis_sounds" data-path="<%= SoundPath %>">
            <div class="small_chat_checkbox small_chat_checkbox_enabled"></div>
            <%= ChatResource.HintSounds %>
        </li>
        <li class="dropdown-item small_chat_en_dis_ctrl_enter_sender">
            <div class="small_chat_checkbox small_chat_checkbox_disabled"></div>
            <%= ChatResource.HintCtrlEnterSender %>
        </li>
        <li class="dropdown-item small_chat_minimize_all_windows_if_lose_focus">
            <div class="small_chat_checkbox small_chat_checkbox_disabled"></div>
            <%= ChatResource.MinimizeAllWindowsIfLoseFocus %>
        </li>
        <li class="dropdown-item small_chat_minimize_all_windows disable">
            <%= ChatResource.MinimizeAllWindows %>
        </li>
        <li class="dropdown-item small_chat_close_all_windows disable">
            <%= ChatResource.CloseAllWindows %>
        </li>
    </ul>
</div>
<script id="contactListTmpl" type="text/x-jquery-tmpl">
    <ul class="user_list online_user_list">
    {{each UsersOnline}}
        <li class="contact_block" data-username="${UserName}">
            <span class="contact_record unselect_text">${Encoder.htmlEncode(DisplayUserName)}</span>
            <div class="chat_user_state ${State}"/>
        </li>
    {{/each}}
    </ul>
    <ul class="user_list away_user_list">
    {{each UsersAway}}
        <li class="contact_block" data-username="${UserName}">
            <span class="contact_record unselect_text">${Encoder.htmlEncode(DisplayUserName)}</span>
            <div class="chat_user_state ${State}"/>
        </li>
    {{/each}}
    </ul>
    <ul class="user_list not_available_user_list">
    {{each UsersNotAvailable}}
        <li class="contact_block" data-username="${UserName}">
            <span class="contact_record unselect_text">${Encoder.htmlEncode(DisplayUserName)}</span>
            <div class="chat_user_state ${State}"/>
        </li>
    {{/each}}
    </ul>
    <ul class="user_list offline_user_list">
    {{each UsersOffline}}
        <li class="contact_block" data-username="${UserName}">
            <span class="contact_record unselect_text">${Encoder.htmlEncode(DisplayUserName)}</span>
            <div class="chat_user_state ${State}"/>
        </li>
    {{/each}}
    </ul>
    <ul class="user_list tenant_user_list"/>
</script>

<script id="contactTmpl" type="text/x-jquery-tmpl">
    <li class="contact_block" data-username="${UserName}">
        <span class="contact_record unselect_text">${Encoder.htmlEncode(ShowUserName)}</span>
        <div class="chat_user_state ${StateClass}"/>
    </li>
</script>

<script id="tenantBlockTmpl" type="text/x-jquery-tmpl">
    <li class="contact_block" data-username="${TenantGuid}">
        <span class="contact_record unselect_text">${Encoder.htmlEncode(TenantName)}</span>
    </li>
</script>

<script id="detailUserListTmpl" type="text/x-jquery-tmpl">
    <div class="detail_user_list display-none">
        <img class="small_chat_contact_photo" src="${PhotoURL}" border="0" width="82" height="82"/>
        <div class="small_chat_card">
            <div class="small_chat_character">
                <span class="characteristic"><%= Resource.Name%>:</span>
                <a class="small_chat_user link" href="/Products/People/Profile.aspx?user=${UserName}" title="${ShowUserName}">${ShowUserName}</a>
            </div>
            {{if Title != "" && Title != undefined}}
            <div class="small_chat_character">
                <span class="characteristic"><%= CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode()%>:</span>
                <span class="small_chat_post" title="${Title}">${Title}</span>
            </div>
            {{/if}}
            <div class="small_chat_character">
                <span class="characteristic"><%= Resource.UserType%>:</span>
                <span class="small_chat_post">${UserType}</span>
            </div>
            <div class="small_chat_character">
                <span class="characteristic"><%= Resource.Email%>:</span>
                <a class="small_chat_mail mail link" target="_blank" href="/addons/mail/#composeto/email=${Email}" title="${Email}">${Email}</a>
            </div>
            {{if Departments != undefined}}
            <div class="small_chat_character">
                <span class="characteristic"><%= CustomNamingPeople.Substitute<Resource>("Department").HtmlEncode()%>:</span>
                {{each(ID, DepartmentName) Departments}}
                <a class="small_chat_deps link" href="/Products/People/#group=${ID}">${Encoder.htmlEncode(DepartmentName)}</a>
                {{/each}}
            </div>
            {{/if}}
        </div>
    </div>
</script>

<script id="messageDialogTmpl" type="text/x-jquery-tmpl">
    <div class="conversation_block" data-username="${UserName}">
        <div class="header_of_conversation_block main-header-color">
            {{if ImageStatus}}
                <div class="conversation_block_user_state ${ImageStatus}" title="${ImageTitle}"/>
            {{/if}}
            <div class="text_of_header_of_conversation_block unselect_text">${Encoder.htmlEncode(HeaderText)}</div>
            <div class="close_conversation_block" title="<%= ChatResource.CloseMessageWindowAltTitle %>"/>
            <div class="extend_conversation_block" title="<%= ChatResource.ExtendChatAltTitle %>"/>
            <div class="minimize_conversation_block minimize_restore_conversation_block" title="<%= ChatResource.MinimizeMessageWindowAltTitle %>"/>
        </div>
        <div class="message_bus_container webkit-scrollbar">
             <div class="chat_messages_loading display-none">
                 <div class="chat_messages_loader"/>
                  <%= Resource.LoadingProcessing %>
             </div>
             <div class="typing_message_notification display-none"><%= ChatResource.TypingMessage %></div>
        </div>
        <div class="smile_icon" title="<%= ChatResource.SmileysAltTitle %>"/>
        <textarea class="message_input_area webkit-scrollbar" placeholder="<%= ChatResource.InputNotificationMessage %>" data-height="29" rows="1"/>
    </div>
</script>

<script id="smileMenuTmpl" type="text/x-jquery-tmpl">
    <div class="studio-action-panel smile_menu">
        <ul class="dropdown-content">
            <li>
                <div class="smile smile_first_line smile_first_row">:-)</div>
                <div class="smile smile_first_line">:)</div>
                <div class="smile smile_first_line">:(</div>
                <div class="smile smile_first_line">;-)</div>
                <div class="smile smile_first_line">:'(</div>
                <div class="smile smile_first_line">:-(</div>
                <div class="smile smile_first_line smile_last_row">:]</div>
                <div style="clear:left"/>
            </li>
            <li>
                <div class="smile smile_second_line smile_first_row">:-b</div>
                <div class="smile smile_second_line">:-p</div>
                <div class="smile smile_second_line">:-#</div>
                <div class="smile smile_second_line">:[</div>
                <div class="smile smile_second_line">;)</div>
                <div class="smile smile_second_line">:^)</div>
                <div class="smile smile_second_line smile_last_row">:-9</div>
                <div style="clear:left"/>
            </li>
            <li>
                <div class="smile smile_last_line smile_first_row">:S</div>
                <div class="smile smile_last_line">:-*</div>
                <div class="smile smile_last_line">o_o</div>
                <div class="smile smile_last_line">?-)</div>
                <div class="smile smile_last_line">:O</div>
                <div class="smile smile_last_line">:c)</div>
                <div class="smile smile_last_line smile_last_row">;(</div>
                <div style="clear:left"/>
            </li>
        </ul>
    </div>
</script>

<script id="messageDialogItemTmpl" type="text/x-jquery-tmpl">
    <li class="message_dialog_item dropdown-item" data-username="${UserName}">
        <div class="message_dialog_close_item_icon" title="<%= ChatResource.CloseItem %>"/>
        <div class="message_dialog_item_text">${Encoder.htmlEncode(ShowUserName)}</div>
    </li>
</script>

<script id="messageDialogMenuTmpl" type="text/x-jquery-tmpl">
    <div class="message_dialog_btn" title="<%= ChatResource.ShowInvisibleWindows %>">
        <div class="message_dialog_menu_icon"/>
        <span class="message_dialog_text">1</span>
    </div>
    <div class="studio-action-panel" id="messageDialogPopupID">
        <ul class="dropdown-content webkit-scrollbar">
            {{tmpl($data) '#messageDialogItemTmpl'}}
        </ul>
    </div>
</script>

<script id="messageTmpl" type="text/x-jquery-tmpl">
    {{if NotRead}}
    <div class="small_chat_message not_read_message">
    {{else}}
    <div class="small_chat_message">
    {{/if}}
        {{if IsMessageOfCurrentUser}}
            <span class="header_of_message_of_current_user">${Encoder.htmlEncode(Name)}</span>
            {{if Emoticon}}
            <div class="message_of_current_user message_of_user emoticon_message">{{html Message}}
            {{else}}
            <div class="message_of_current_user message_of_user">{{html Message}}
            {{/if}}
            </div>
            <div class="current_user_interval user_interval">${DateTime}</div>
        {{else}}
            <span class="header_of_message_of_other_user">${Encoder.htmlEncode(Name)}</span>
            {{if Emoticon}}
            <div class="message_of_other_user message_of_user emoticon_message">{{html Message}}
            {{else}}
            <div class="message_of_other_user message_of_user">{{html Message}}
            {{/if}}
            </div>
            <div class="other_user_interval user_interval">${DateTime}</div>
        {{/if}}
        <div style="clear: both;"/>
    </div>
</script>

<script id="messagesTmpl" type="text/x-jquery-tmpl">
    {{each Messages}}
    {{tmpl($value) '#messageTmpl'}}
    {{/each}}
</script>

<script id="messageNotificationTmpl" type="text/x-jquery-tmpl">
    <div class="notification_username" data-username="${UserName}">${Encoder.htmlEncode(ShowUserName)}</div>
    <div class="notification_text">${Encoder.htmlEncode(Message)}</div>
    <div class="notification_close"/>
</script>