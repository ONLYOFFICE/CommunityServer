<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="editMessageHeaderTmpl" type="text/x-jquery-tmpl">
    <div class="simpleWrapper">
        <div class="contentMenuWrapper">
            <ul class="clearFix contentMenu contentMenuDisplayAll" id="WriteMessageGroupButtons">
                <li class="menuAction blue btnSend unlockAction">
                    <span title="<%: MailResource.SendBtnLabel %>"><%: MailResource.SendBtnLabel %></span>
                </li>
                <li class="menuAction btnSave unlockAction">
                    <span title="<%: MailResource.SaveBtnLabel %>"><%: MailResource.SaveBtnLabel %></span>
                </li>
                <li class="menuAction btnDelete unlockAction">
                    <span title="<%: MailResource.DeleteBtnLabel %>"><%: MailResource.DeleteBtnLabel %></span>
                </li>
                <li class="menuAction btnAddTag unlockAction">
                    <span title="<%: MailResource.AddTag %>"><%: MailResource.AddTag %></span>
                    <div class="down_arrow"></div>
                </li>
                <li class="menu-action-on-top">
                    <a class="on-top-link" onclick="javascript:window.scrollTo(0, 0);">
                        <%: MailResource.OnTopLabel%>
                    </a>
                </li>
                <li>
                    <div class="savedtime pull-right" style="display: none; ">
                        <%: MailResource.SavedAt %>: <span class="savedtime-value"></span>
                    </div>
                </li>
            </ul>
            <div class="header-menu-spacer" style="display: block;">&nbsp;</div>
        </div>
        <div class="newMessageWrap">
            <div class="head" message_id="${id}">
                <div class="value-group">
                    <label>
                        <%: MailScriptResource.FromLabel %>:
                    </label>
                    <div class="value">
                        <span id="newmessageFromSelected" class="pointer">
                            <span class="baseLinkAction"></span>
                            <div class="arrow-down"></div>
                        </span>

                        <span id="newmessageFromWarning" style="display:none; margin-left: 16px;" class="red-text">
                            <%: MailResource.MessageFromWarning %>
                        </span>
                    </div>
                </div>
                <div class="value-group">
                    <label>
                        <%: MailScriptResource.ToLabel %>:
                    </label>
                    <div class="pull-right">
                        <a id="AddCopy" class="link dotline"><%: MailResource.AddCopy %></a>
                    </div>
                    <div class="value with-right">
                        <textarea id="newmessageTo" class="to" spellcheck="false" placeholder="<%= MailResource.SearchInToCcBccFieldsPlaceHolderLabel %>" style="resize: none;" tabindex="1">{{if $item.action!='forward'}}${to}{{/if}}</textarea>
                    </div>
                </div>
                <div class="value-group cc hidden">
                    <label>
                        <%: MailResource.CopyLabel %>:
                    </label>
                    <div class="value with-right">
                        <textarea id="newmessageCopy" class="cc" spellcheck="false" style="resize: none;" tabindex="2">${cc}</textarea>
                    </div>
                </div>
                <div class="value-group bcc hidden">
                    <label>
                        <%: MailResource.BCCLabel %>:
                    </label>
                    <div class="value with-right">
                        <textarea id="newmessageBCC" class="bcc" spellcheck="false" style="resize: none;" tabindex="3">${bcc}</textarea>
                    </div>
                </div>
                <div class="value-group">
                    <label>
                        <%: MailScriptResource.SubjectLabel %>:
                    </label>
                    <div class="pull-right">
                        <label for="newmessageImportance" class="checkbox">
                            <input type="checkbox" id="newmessageImportance" name="Importance"{{if important==true}} value="1" checked="true"{{else}} value="0"{{/if}}>
                            </input>
                            <span>
                                <%: MailResource.ImportanceLabel %>
                            </span>
                        </label>
                    </div>
                    <div class="value with-right">
                        <input id="newmessageSubject" class="subject" type="text" spellcheck="false" maxlength="250" style="resize: none;" tabindex="4" value="${subject}"/>
                    </div>
                </div>
                <div class="value-group tags hidden">
                    <label><%: MailScriptResource.Tags %>:</label>
                    <div class="value"><div class="itemTags"></div></div>
                </div>
            </div>
            <div id="id_block_errors_container" class="error-popup" style="display: none;"> 
                <span class="text"><%: MailResource.BlockedContentWarning %></span>
                <a class="close-info-popup" href="#" onclick="jq('#id_block_errors_container').hide(); return false;"></a>
            </div>
        </div>
    </div>
</script>

<script id="editMessageFooterTmpl" type="text/x-jquery-tmpl">
    <div class="simpleWrapper">
        <div class="newMessageWrap" id="newMessage" streamId="${streamId}">
            <div id="attachments_count_container" class="attachments-counter-position">
                <div class="title-attachments">
                    <div class="icon" style="padding-left: 0;"><i class="icon-attachment"></i></div>
                    <span id="attachments_count_label"><%: MailResource.Attachments %>:</span>
                    <span id="full-size-label" class="fullSizeLabel"></span>
                </div>
            </div>
            <div id="attachment_upload_pnl" style="margin-left: 2px;margin-bottom: 42px;">
               <div class="containerAction" style="display: block;">
                    <span id="attachments_browse_btn" class="addUserLink">
                        <a class="link dotline"><%: MailResource.AttachFileLabel %></a>
                    </span>
                    <span id="documents_browse_btn" class="addUserLink" onclick="javascript:DocumentsPopup.showPortalDocUploader();return false;">
                        <a class="link dotline" ><%: MailResource.AttachFilesFromDocsLabel %></a>
                    </span>
                    <span id="attachments_clear_btn" class="addUserLink deleteAttachmentsIcon" onclick="AttachmentManager.RemoveAll();">
                        <a class="link dotline"><%: MailResource.AttachDeleteAllLabel %></a>
                    </span>
                    <span id="attachments_limit_txt"></span>
                    <div id="switcher" class="pull-right" style="display: none;">
                        <%= MailResource.SwitchLabel %>
                        <a class="link dotline" href="javascript:;" onclick="javascript:AttachmentManager.SwitchMode();return false;">
                            <%= MailResource.FlashUploaderLabel %>:
                        </a>
                    </div>
                </div>
            </div>
            <div class="attachments">
                <table id="mail_attachments" class="attachments_list"
                    save_to_docs_attachment="<%: MailResource.SaveAttachToMyDocs %>"
                    save_to_projects_docs_attachment="<%: MailResource.SaveAttachToProjDocs %>"
                    attach_to_crm_attachment="<%: MailResource.AttacToCRMContact %>">
                    <tbody>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</script>

<script id="attachmentTmpl" type="text/x-jquery-tmpl">
    <tr class="row" data_id="${orderNumber}">
        <td class="file_icon">
            <div class="attachmentImage ${iconCls}"/>
        </td>
        <td class="file_info">
            <a {{if handlerUrl == ''}} 
                    href="javascript: void(0)" onclick="return false" class="unloded" 
               {{else}} 
                    href="${handlerUrl}" target="_blank"
               {{/if}} 
                    title="${fileName}"
               {{if isImage == true}}
                    class="screenzoom" 
               {{else}} 
                    {{if canView == false}} download="${fileName}" {{/if}} 
               {{/if}}>
               <span class="file-name">
                    ${$item.cutFileName($item.getFileNameWithoutExt(fileName))}
                    <span class="file-extension">${$item.getFileExtension(fileName)}</span>
               </span>
            </a>
            <span class="fullSizeLabel" {{if size == 0}} style="display:none;" {{/if}}>(${$item.fileSizeToStr(size)})</span>
        </td>
        <td class="delete_icon">
            <div class="delete_attachment" onclick="AttachmentManager.RemoveAttachment(${orderNumber});" />
        </td>
        <td class="load_result">
            {{if error}}
                {{if error != ''}}
                    <span class="file-load-result red-text" title="${error}">${error}</span>
                {{else}}
                {{if warn}}
                    {{if fileId > 0 && warn != ''}}
                            <span class="file-load-result uploaded-text" title="<%: MailAttachmentsResource.UploadedLabel %> (<%: MailAttachmentsResource.WarningLabel %>: ${warn})"><%: MailAttachmentsResource.UploadedLabel %> (<%: MailAttachmentsResource.WarningLabel %>: ${warn})</span>
                        {{/if}}
                    {{/if}}
                {{/if}}
            {{else}}
                {{if warn}}
                    {{if fileId > 0 && warn != ''}}
                        <span class="file-load-result uploaded-text" title="<%: MailAttachmentsResource.UploadedLabel %> (<%: MailAttachmentsResource.WarningLabel %>: ${warn})"><%: MailAttachmentsResource.UploadedLabel %> (<%: MailAttachmentsResource.WarningLabel %>: ${warn})</span>
                    {{/if}}
                {{else}}
                    {{if fileId > 0}}
                        <span class="file-load-result uploaded-text" title="<%: MailAttachmentsResource.UploadedLabel %>"><%: MailAttachmentsResource.UploadedLabel %></span>
                    {{/if}}
                {{/if}}
            {{/if}}
            <div id="item_progress_${orderNumber}" class="attachment-progress" {{if fileId > 0 || error != ''}} style="display:none;" {{/if}}>
                <div class="fu-progress-cell">
                    <div class="upload-progress">
                        <span class="progress-slider progress-color" style="width:0;">&nbsp;</span>
                        <span class="progress-label"><%: MailAttachmentsResource.UploadingLabel %></span>
                    </div>
                </div>
            </div>
        </td>
        <td class="menu_column">
            <div class="menu" data_id="${orderNumber}" name="${fileName}" title="<%: MailScriptResource.Actions %>" />
        </td>
    </tr>
</script>

<script id="writeMessageTmpl" type="text/x-jquery-tmpl">
    <div id="editMessagePage">
        <div id="editMessagePageHeader">{{tmpl($item.data, { fileSizeToStr: $item.GetSizeString }) "editMessageHeaderTmpl"}}</div>
        <div id="WYSIWYGEditor" class="mail_wysiwyg_editor">
            <textarea id="ckMailEditor"></textarea>
        </div>
        <div id="editMessagePageFooter">{{tmpl($item.data, { fileSizeToStr: $item.GetSizeString }) "editMessageFooterTmpl"}}</div>
    </div>
</script>