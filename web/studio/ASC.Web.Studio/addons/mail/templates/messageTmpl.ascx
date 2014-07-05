<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="messageTmpl" type="text/x-jquery-tmpl">
    <div class="itemWrapper">
        <div class="head-subject">
            <button type="button" id="menuActionBack" class="header-back-link button gray"><span></span></button>
            <div class="importance pull-left">
                <i class="icon-{{if $item.important==false}}un{{/if}}important" 
                    title="{{if $item.important}}<%: MailScriptResource.ImportantLabel %>{{else}}<%: MailScriptResource.NotImportantLabel %>{{/if}}"></i>
            </div>
            <div class="viewTitle">
                {{if typeof($item.last_message.subject)==='undefined' || $item.last_message.subject == null || $item.last_message.subject ==''}}
                    <%: MailResource.NoSubject %>
                {{else}}
                    {{html $item.wordWrap($item.last_message.subject) }}
                {{/if}}
                {{if crm_available == true}}
                <div class="header-crm-link" style="display: none"></div>
                {{/if}}
            </div>

        </div>

        {{if $item.messages.length == 1}}
            {{tmpl($item.messages[0], {
                fileSizeToStr   : $item.fileSizeToStr,
                cutFileName: $item.cutFileName,
                getFileNameWithoutExt: $item.getFileNameWithoutExt,
                getFileExtension: $item.getFileExtension,
                htmlEncode      : $item.htmlEncode,
                isSingleMessage : true
            }) $item.messages[0].template_name}}
        {{else}}
            {{tmpl({}, {
                needBottomBorder : false,
                needSortButton   : true
            }) "messageTopButtons"}}

            {{each $item.messages}}
                {{if $value.hidden_count > 0 }}
                    {{tmpl($value, {
                        count       : $value.hidden_count
                    }) "collapsedMessagesTmpl"}}
                {{/if}}
                {{tmpl($value, {
                    fileSizeToStr   : $item.fileSizeToStr,
                    cutFileName: $item.cutFileName,
                    getFileNameWithoutExt: $item.getFileNameWithoutExt,
                    getFileExtension: $item.getFileExtension,
                    htmlEncode      : $item.htmlEncode,
                    isSingleMessage : false
                }) $value.template_name}}
            {{/each}}
        {{/if}}
    </div>
</script>

<script id="collapsedMessagesTmpl" type="text/x-jquery-tmpl">
    <div class="collapsed-messages" style="border-top: 1px solid #D7D8DC;">
        <label>${$item.count} <%: MailResource.MoreLabel %></label>
    </div>
</script>

<script id="messageShortTmpl" type="text/x-jquery-tmpl">
    <div class="message-wrap{{if visible != true}} hidden{{/if}}" message_id="${id}" folder="${folder}" restore_folder_id="${restoreFolderId}">
        <table class="short-view" message_id="${id}">
            <tbody>
                <tr class="message_short" data_id="${id}">
                    <td class="from_label">
                        <span><%: MailScriptResource.FromLabel %>:</span>
                    </td>
                    <td class="from">
                        <span title="${fromName}">${fromName}</span>
                    </td>
                    <td class="content">
                        <span title="${$item.htmlEncode(introduction)}">${$item.htmlEncode(introduction)}</span>
                    </td>
                    <td class="load_box">
                        <div class="loader hidden"></div>
                    </td>
                    <td class="icon">
                        {{if attachments.length > 0}}
                            <i class="icon-attachment"></i>
                        {{/if}}
                    </td>
                    <td class="date-time">
                        <span>
                            {{if isToday}}
                                <%: MailResource.TodayLabel %>
                            {{else}}
                                {{if isYesterday}}
                                    <%: MailResource.YesterdayLabel %>
                                {{else}}
                                    ${displayDate}
                                {{/if}}
                            {{/if}}
                        </span>
                    </td>
                    <td class="time">
                        <span>${displayTime}</span>
                    </td>
                </tr>
            </tbody>
        </table>
        <div class="full-view hidden" message_id="${id}" content_blocked="${contentIsBlocked}">
            {{if $item.isSingleMessage == true}}
                {{tmpl($item.data, {
                    needBottomBorder : true,
                    needSortButton   : false
                }) "messageTopButtons"}}
            {{/if}}
            <div class="head" message_id="${id}">
                <div class="row" data_id="${id}">
                    <div class="menu" data_id="${id}" title="<%: MailScriptResource.Actions %>"></div>
                    <label><%: MailScriptResource.FromLabel %>:</label>
                    <div class="value">
                        <a class="from" href="javascript:void(0);">${from}</a>
                        {{if crm_available && isFromCRM != true && folder == 1 && from != 'mail-daemon@teamlab.com'}}
                            <span class="AddToCRMContacts addUserLink">
                                <a class="link dotline"><%: MailResource.AddToCRMContacts %></a>
                                <span class="sort-down-black down_arrow"></span>
                            </span>
                        {{/if}}
                    </div>
                </div>
                <div class="row" data_id="${id}">
                    <label><%: MailScriptResource.ToLabel %>:</label>
                    <div class="value to-addresses">${to}</div>
                </div>
                {{if cc }}
                    <div class="row" data_id="${id}">
                        <label><%: MailResource.CopyLabel %>:</label>
                        <div class="value cc-addresses">${cc}</div>
                    </div>
                {{/if}}
                {{if bcc }}
                    <div class="row" data_id="${id}">
                        <label><%: MailResource.BCCLabel %>:</label>
                        <div class="value bcc-addresses">${bcc}</div>
                    </div>
                {{/if}}
                <div class="row" data_id="${id}">
                    <label><%: MailScriptResource.DateLabel %>:</label>
                    <div class="value">
                        <span>${displayDate}</span>
                        <span style="margin-left: 5px">${displayTime}</span>
                    </div>
                </div>
                <div class="row tags hidden" data_id="${id}">
                    <label><%: MailScriptResource.Tags %>:</label>
                    <div class="value"><div class="itemTags"></div></div>
                </div>
            </div>

            {{if contentIsBlocked == true}}
                {{tmpl($item.data, {}) "messageBlockContent"}}
            {{/if}}

            <div message_id="${id}" class="body"></div>

            {{if hasAttachments == true}}
                <div class="attachments" message_id="${id}">
                    {{if attachments.length > 0}}
                        <div class="title-attachments">
                            <div class="icon"><i class="icon-attachment"></i></div>
                            <div class="attachment-message has-attachment"></div><%: MailResource.Attachments %> (${attachments.length}):
                            <span class="fullSizeLabel">
                                <%: MailResource.FullSize %>: ${$item.fileSizeToStr(full_size)}
                            </span>
                        </div>
                         <% if (MailPage.IsTurnOnAttachmentsGroupOperations()) 
                            { %>
                                {{if attachments.length > 1}}
                                    <div class="attachments-buttons">
                                        <i class="icon-download-all" />
                                        <a href="${download_all_url}" target="_blank" class="baseLinkAction link dotline"><%: MailResource.AttachDownloadAll %></a>
                                        <i class="icon-save-all-to-teamlab" />
                                        <a href="javascript:void(0);" class="exportAttachemntsToMyDocs baseLinkAction link dotline"><%: MailResource.ExportAttachmentsToMyDocuments %></a>
                                    </div>
                                {{/if}}
                        <% } %>
                    {{/if}}
                    <table class="attachments_list" 
                        save_to_docs_attachment="<%: MailResource.SaveAttachToMyDocs %>"
                        save_to_projects_docs_attachment="<%: MailResource.SaveAttachToProjDocs %>"
                        attach_to_crm_attachment="<%: MailResource.AttacToCRMContact %>">
                        <tbody>
                            {{each attachments}}
                                <tr class="row" data_id="${$value.fileId}">
                                    <td class="file_icon">
                                        <div class="attachmentImage ${$value.iconCls}"/>
                                    </td>
                                    <td class="file_info">
                                        <a target="_blank" href="${$value.handlerUrl}" title="${$value.fileName}" 
                                            {{if $value.isImage == true}}
                                                class="screenzoom" 
                                            {{else}} 
                                                    {{if $value.canView == false}} download="${$value.fileName}" {{/if}} 
                                            {{/if}}>
                                            <span class="file-name">
                                                ${$item.cutFileName($item.getFileNameWithoutExt($value.fileName))}
                                                <span class="file-extension">${$item.getFileExtension($value.fileName)}</span>
                                            </span>
                                        </a>
                                        <span class="fullSizeLabel">(${$item.fileSizeToStr($value.size)})</span>
                                        {{if $value.canEdit == true }}
                                            <div class="icon_edit">
                                                <div class="pencil" title="<%:MailResource.EditAttachment%>" onclick="messagePage.editDocumentAttachment(${$value.fileId});"/>
                                            </div>
                                        {{/if}}
                                    </td>
                                    <td class="menu_column">
                                        <div class="menu" data_id="${$value.fileId}" name="${$value.fileName}" title="<%: MailScriptResource.Actions %>" />
                                    </td>
                                </tr>
                            {{/each}}
                        </tbody>
                    </table>
                </div>
            {{/if}}
        </div>
    </div>
</script>

<script id="messageBlockContent" type="text/x-jquery-tmpl">
    <div id="id_block_content_popup_${id}" class="error-popup hidden">
            <span class="text"><%: MailResource.BlockedContentWarning %></span>
            <a id="id-btn-block-content-${id}" class="link dotline" href="#" onclick="return false;"><%: MailResource.DisplayImagesLabel %></a>
            <a id="id-btn-always-block-content-${id}" class="link dotline" href="#" onclick="return false;" style="margin-left: 8px;"><%: MailResource.AlwaysDisplayImagesLabel %> "${sender_address}"</a>
            <a class="close-info-popup" href="#" onclick="jq('#id_block_content_popup_${id}').hide(); return false;"></a>
    </div>
</script>

<script id="messageTopButtons" type="text/x-jquery-tmpl">
    <div class="messageHeader" {{if $item.needBottomBorder == false}}style="border-bottom:none;"{{/if}}>
        <div class="contentMenuWrapper">
            <ul class="clearFix contentMenu contentMenuDisplayAll" id="MessageGroupButtons">
                <li class="menuAction btnReply unlockAction">
                    <span title="<%: MailResource.ReplyBtnLabel %>"><%: MailResource.ReplyBtnLabel %></span>
                </li>
                <li class="menuAction btnReplyAll unlockAction">
                    <span title="<%: MailResource.ReplyAllBtnLabel %>"><%: MailResource.ReplyAllBtnLabel %></span>
                </li>
                <li class="menuAction btnForward unlockAction">
                    <span title="<%: MailResource.ForwardLabel %>"><%: MailResource.ForwardLabel %></span>
                </li>
                <li class="menuAction btnDelete unlockAction">
                    <span title="<%: MailResource.DeleteBtnLabel %>"><%: MailResource.DeleteBtnLabel %></span>
                </li>
                <li class="menuAction btnAddTag unlockAction">
                    <span title="<%: MailResource.AddTag %>"><%: MailResource.AddTag %></span>
                    <div class="down_arrow"></div>
                </li>
                <li class="menuAction btnMore unlockAction">
                    <span title="<%: MailResource.MoreMenuButton %>"><%: MailResource.MoreMenuButton %></span>
                    <div class="down_arrow"></div>
                </li>
                <li class="menu-action-simple-pagenav">
                    <div>
                        <a class="pagerPrevButtonCSSClass" href=""><%: MailResource.GoToPrevMessage %></a>
                        <a class="pagerNextButtonCSSClass" href=""><%: MailResource.GoToNextMessage %></a>
                    </div>
                </li>
                {{if typeof($item.needSortButton)!=='undefined' && $item.needSortButton}}
                <li>
                    <div id="sort-conversation" class="sort-icon hidden-min"></div>
                    <div id="collapse-conversation" class="collapse-conversation hidden-min">Expand all</div>
                </li>
                {{/if}}
                <li class="menu-action-on-top">
                    <a class="on-top-link" onclick="javascript:window.scrollTo(0, 0);">
                        <%: MailResource.OnTopLabel%>
                    </a>
                </li>
            </ul>
            <div class="header-menu-spacer">&nbsp;</div>
        </div>
    </div>
</script>
