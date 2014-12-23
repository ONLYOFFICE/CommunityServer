<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<%-- Empty screen control --%>
<script id="emptyScrTmpl" type="text/x-jquery-tmpl">
    <div id="${ID}" class="noContentBlock emptyScrCtrl{{if typeof(CssClass)!="undefined"}} ${CssClass}{{/if}}" >
        {{if typeof(ImgSrc)!="undefined" && ImgSrc != null && ImgSrc != ""}}
        <table>
            <tr>
                <td>
                    <img src="${ImgSrc}" alt="" class="emptyScrImage" />
                </td>
                <td>
                    <div class="emptyScrTd">
        {{/if}}
                    {{if typeof(Header)!="undefined" && Header != null && Header != ""}}
                        <div class="header-base-big">${Header}</div>
                    {{/if}}
                    {{if typeof(HeaderDescribe)!="undefined" && HeaderDescribe != null && HeaderDescribe != ""}}
                        <div class="emptyScrHeadDscr">${HeaderDescribe}</div>
                    {{/if}}
                    {{if typeof(Describe)!="undefined" && Describe != null && Describe != ""}}
                        <div class="emptyScrDscr">{{html Describe}}</div>
                    {{/if}}
                    {{if typeof(ButtonHTML)!="undefined" && ButtonHTML != null && ButtonHTML != ""}}
                        <div class="emptyScrBttnPnl">{{html ButtonHTML}}</div>
                    {{/if}}
        {{if typeof(ImgSrc)!="undefined" && ImgSrc != null && ImgSrc != ""}}
                    </div>
                </td>
            </tr>
        </table>
        {{/if}}
    </div>
</script>

<%-- Tag view control --%>
<script id="tagViewTmpl" type="text/x-jquery-tmpl">
    <div id="tagContainer">
        <span>
            <span id="addNewTag" class="baseLinkAction crm-withArrowDown" title="<%= CRMCommonResource.AddNewTag %>"><%= CRMCommonResource.AddNewTag %></span>

            <div id="addTagDialog" class="studio-action-panel addTagDialog">
                <ul class="dropdown-content mobile-overflow">
                    {{each availableTags}}
                        {{tmpl({"tagLabel": $value}) 'tagInAllTagsTmpl'}}
                    {{/each}}
                </ul>
                <div class="h_line">&nbsp;</div>
                <div style="margin-bottom:5px;"><%= CRMCommonResource.CreateNewTag%>:</div>
                <input type="text" class="textEdit" maxlength="50"/>
                <a id="addThisTag" class="button blue" title="<%= CRMCommonResource.OK %>"><%= CRMCommonResource.OK %></a>
            </div>
        </span>
        <div style="display:inline;">
            {{each tags}}
                {{tmpl({"tagLabel": $value}) 'taqTmpl'}}
            {{/each}}
        </div>

        <img class="adding_tag_loading" alt="<%= CRMSettingResource.AddTagInProgressing %>"
                title="<%= CRMSettingResource.AddTagInProgressing %>"
                src="<%= WebImageSupplier.GetAbsoluteWebPath("loader_16.gif") %>" />
    </div>
</script>

<script id="taqTmpl" type="text/x-jquery-tmpl">
    <span class="tag_item">
        <span class="tag_title" data-value="${jQuery.base64.encode(tagLabel)}">${ASC.CRM.Common.convertText(tagLabel, true)}</span>
        <a class="delete_tag" alt="<%= CRMCommonResource.DeleteTag %>" title="<%= CRMCommonResource.DeleteTag %>"
            onclick="ASC.CRM.TagView.deleteTag(jq(this).parent())"></a>
     </span>
</script>

<script id="tagInAllTagsTmpl" type="text/x-jquery-tmpl">
    <li><a class="dropdown-item" onclick="ASC.CRM.TagView.addExistingTag(this);" data-value="${jQuery.base64.encode(tagLabel)}">${ASC.CRM.Common.convertText(tagLabel, true)}</a></li>
</script>

<%--HistoryView--%>

<script id="historyRowTmpl" type="text/x-jquery-tmpl">
    <tr id="event_${id}">
        <td class="borderBase entityPlace">
            <a name="${id}"></a>
            {{if typeof(category) != "undefined"}}
            <label title="${category.title}" alt="${category.title}" class="event_category ${category.cssClass}"></label>
            {{/if}}
        </td>
        <td class="borderBase title">
            {{if contact != null && contact.id != ASC.CRM.HistoryView.ContactID }}
                <a class="link underline gray" href="default.aspx?id=${contact.id}">${contact.displayName}</a>
                {{if entity != null && entity.entityId != ASC.CRM.HistoryView.EntityID}}
                    &nbsp;/&nbsp;
                {{/if}}
            {{/if}}
            {{if entity != null && entity.entityId != ASC.CRM.HistoryView.EntityID }}
                ${entityType}: <a class="link underline gray" href="${entityURL}">${entity.entityTitle}</a>
            {{/if}}


            {{if typeof message != "undefined" && message != null}}
            <div>
                <%= CRMCommonResource.TheEmail %>
                "<a class="link historyEventMailSubj" href="mailviewer.aspx?id=${id}" target="_blank">${content}</a>"
                {{if message.is_sended === true}}
                <%= CRMCommonResource.HistoryEventOutboxMail %>
                {{else}}
                <%= CRMCommonResource.HistoryEventInboxMail %>
                {{/if}}
            </div>
            {{else}}
            <div>
                {{html htmlUtility.getFull(Encoder.htmlDecode(ASC.CRM.Common.convertText(content, true))) }}
            </div>
            {{/if}}

            <span class="text-medium-describe">${createdDate} <%= CRMCommonResource.Author %>: ${createdBy.displayName}</span>
        </td>
        <td class="borderBase activityData describe-text">
            {{if files != null }}
                <label class="event_attached_file" align="absmiddle"></label>
                <a id="eventAttachSwither_${id}" class="baseLinkAction linkMedium">
                    <%= CRMCommonResource.ViewFiles %>
                </a>
                <div id="eventAttach_${id}" class="studio-action-panel eventAttachPanel">
                    <ul class="dropdown-content">
                        {{each(i, file) files}}
                            <li id="fileContent_${file.id}">
                                <a target="_blank" href="${file.webUrl}" class="dropdown-item">
                                    ${file.title} 
                                </a>
                                {{if $data.canEdit == true }}
                                <img class="deleteFileBtn" align="absmiddle" title="<%= CRMCommonResource.Delete %>"
                                    onclick="ASC.CRM.HistoryView.deleteFile(${file.id}, ${$data.id})"
                                    src="<%=WebImageSupplier.GetAbsoluteWebPath("remove_12.png", ProductEntryPoint.ID)%>" />
                                {{/if}}
                            </li>
                        {{/each}}
                    </ul>
                </div>
            {{/if}}
        </td>
        <td class="borderBase" style="width: 20px;">
          {{if canEdit == true }}
            <a id="eventTrashImg_${id}" class="crm-deleteLink" title="<%= CRMCommonResource.DeleteHistoryEvent %>"
               onclick="ASC.CRM.HistoryView.deleteEvent(${id})"></a>
            <div id="eventLoaderImg_${id}" class="loader-middle baseList_loaderImg"></div>
          {{/if}}
      </td>
    </tr>
</script>


<%--ImportFromCSVView--%>

<script id="previewImportDataTemplate" type="text/x-jquery-tmpl">
    <tr>
        <td class="borderBase">
          <input type="checkbox" checked="checked" />
        </td>
        <td class="borderBase">
          <img src="${default_image}" />
        </td>
        <td style="width:100%" class="borderBase">
              ${contact_title}
        </td>
    </tr>
</script>

<script id="columnSelectorTemplate" type="text/x-jquery-tmpl">
   {{if isHeader}}
       <option name="is_header">${title}</option>
   {{else !isHeader}}
        <option name="${name}" >${title}</option>
   {{/if}}
</script>

<script id="columnMappingTemplate" type="text/x-jquery-tmpl">
    {{each firstContactFields}}
        <tr>
            <td class="borderBase">
              ${headerColumns[$index]}
            </td>
            <td class="borderBase">
             {{html $item.renderSelector($index)}}
            </td>
            <td class="borderBase">
               ${$value}
            </td>
        </tr>
    {{/each}}
</script>

<%--ConfirmationPanel--%>

<script id="blockUIPanelTemplate" type="text/x-jquery-tmpl">
    <div style="display:none;" id="${id}">
        <div class="popupContainerClass">
            <div class="containerHeaderBlock">
                <table cellspacing="0" cellpadding="0" border="0" style="width:100%; height:0px;">
                    <tbody>
                        <tr valign="top">
                            <td>${headerTest}</td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" title="<%= CRMCommonResource.Close %>">&times</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="infoPanel" style="display:none;">
                <div></div>
            </div>
            <div class="containerBodyBlock">
                {{if typeof(questionText) != "undefined" && questionText != ""}}
                <div>
                    <b>${questionText}</b>
                </div>
                {{/if}}

                {{html innerHtmlText}}

            {{if typeof(OKBtn) != 'undefined' && OKBtn != "" || typeof(OtherBtnHtml) != 'undefined' || typeof(CancelBtn) != 'undefined' && CancelBtn != ""}}
            <div class="middle-button-container">
                {{if typeof(OKBtn) != 'undefined' && OKBtn != ""}}
                <a class="button blue middle{{if typeof(OKBtnClass) != 'undefined'}} ${OKBtnClass}{{/if}}"
                    {{if typeof(OKBtnHref) != 'undefined' && OKBtnHref != ""}} href="${OKBtnHref}"{{/if}}>
                    ${OKBtn}
                </a>
                <span class="splitter-buttons"></span>
                {{/if}}
                {{if typeof(OtherBtnHtml) != 'undefined'}}
                    {{html OtherBtnHtml}}
                    <span class="splitter-buttons"></span>
                {{/if}}
                {{if typeof(CancelBtn) != 'undefined' && CancelBtn != ""}}
                <a class="button gray middle{{if typeof(CancelBtnClass) != 'undefined'}} ${CancelBtnClass}{{/if}}" onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">${CancelBtn}</a>
                {{/if}}
            </div>
            {{/if}}
        </div>
        </div>
    </div>
</script>

<%--MakePublicPanel--%>
<script id="makePublicPanelTemplate" type="text/x-jquery-tmpl">
    <div class="makePublicPanel">
        <div class="bold" style="margin: 16px 0 10px;">${Title}</div>
        <p>${Description}</p>

        <div>
            <table class="border-panel" cellpadding="5" cellspacing="0">
                <tr>
                    <td>
                        <input style="margin: 0" type="checkbox" id="isPublic" {{if IsPublicItem == true}}checked="checked"{{/if}} />
                    </td>
                    <td style="padding-left:0;">
                        <label class="makePublicPanelLabel" for="isPublic">
                            ${CheckBoxLabel}
                        </label>
                    </td>
                    <td style="padding-left:0;">
                        <span class="makePublicPanelOptions">
                            <select class="makePublicPanelSelector"></select>
                        </span>
                    </td>
                </tr>
            </table>
        </div>
    </div>
</script>


<%--PrivatePanel--%>
<script id="privatePanelTmpl" type="text/x-jquery-tmpl">
    <div>
        <span class="header-base"><%= CRMCommonResource.PrivatePanelHeader %></span>
        <p>${description}</p>
        <div>
            <table class="border-panel" cellpadding="5" cellspacing="0">
                <tr>
                    <td>
                        <input style="margin: 0" type="checkbox" id="isPrivate" {{if isPrivate==true}}checked="checked"{{/if}}
                            onclick="ASC.CRM.PrivatePanel.changeIsPrivateCheckBox();" />
                    </td>
                    <td style="padding-left:0">
                        <label for="isPrivate">
                            ${checkBoxLabel}
                        </label>
                    </td>
                </tr>
            </table>
        </div>

        <div id="privateSettingsBlock" {{if isPrivate==false}}style="display:none;"{{/if}}>
            <br />
            <b>${accessListLable}:</b>
        </div>
    </div>
</script>


<%--CategorySelector--%>
<script id="categorySelectorTemplate" type="text/x-jquery-tmpl">
    <div id="${jsObjName}">
        {{if typeof helpInfoText != "undefined" && helpInfoText != ""}}
        <div style="float: left;">
        {{/if}}

        {{if jq.browser.mobile === true}}
            {{tmpl "categorySelectorMobileTemplate"}}
        {{else}}
            {{tmpl "categorySelectorDesktopTemplate"}}
        {{/if}}

        {{if typeof helpInfoText != "undefined" && helpInfoText != ""}}
        </div>
        <div>
            <div style="height: 20px;margin: 0 0 0 4px;" class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: '${jsObjName}_helpInfo'});">
            </div>
            <div class="popup_helper" id="${jsObjName}_helpInfo">
                {{html helpInfoText}}
            </div>
        </div>
        {{/if}}
    </div>
</script>

<script id="categorySelectorMobileTemplate" type="text/x-jquery-tmpl">
    <select id="${jsObjName}_select" style="width:${maxWidth}px;" onchange="javascript:${jsObjName}.changeContact(jq(this).find('option:selected'));">
        {{each categories}}
            <option id="${jsObjName}_category_${$value.id}" value="${$value.id}">
                ${$value.title}
            </option>
        {{/each}}
    </select>
</script>

<script id="categorySelectorDesktopTemplate" type="text/x-jquery-tmpl">
    <div style="width:${maxWidth}px;" class="categorySelector-selector-container clearFix" id="${jsObjName}_selectorContainer">
        <input type="text" id="${jsObjName}_categoryTitle" value="${selectedCategory.title}" style="width:${maxWidth - 25}px; height:16px; border:none; padding:2px; float: left;" class="textEdit" readonly="readonly" />
        <div class="categorySelector-selector"></div>
        <input type="hidden" id="${jsObjName}_categoryID" value="${selectedCategory.id}" />
    </div>
    <div class="categorySelector-categoriesContainer" id="${jsObjName}_categoriesContainer">
        <div class="categorySelector-categories">

        {{each categories}}
            <div id="${jsObjName}_category_${$value.id}" class="categorySelector-category" onclick="javascript:${jsObjName}.changeContact(this);" >
                {{if typeof($value.cssClass) != "undefined"}}
                <label class="${$value.cssClass}"></label>
                {{/if}}    
                <div style="padding: 9px 0 0 35px;">${$value.title}</div>
            </div>
        {{/each}}

        </div>
    </div>
</script>



<%--History mail event--%>

<script id="historyMailTemplate" type="text/x-jquery-tmpl">
    <div class="headTitle clearFix">
        <div class="importance float-left">
            <i class="{{if important === true}}icon-important{{else}}icon-unimportant{{/if}}"></i>
        </div>
        <span class="header-base">${subject}</span>
    </div>
    <div class="head">
        <div class="row">
            <label><%= CRMCommonResource.MailFrom%>:</label>
            <div class="value">
                <a href="${fromHref}" target="_blank" class="from">${from}</a>
            </div>
        </div>
        <div class="row">
            <label><%= CRMCommonResource.MailTo %>:</label>
            <div class="value to-addresses">
                <a href="${toHref}" target="_blank" class="to">${to}</a>
            </div>
        </div>

        {{if typeof cc_text != "undefined" && cc_text != ""}}
        <div class="row"> 
            <label><%= CRMCommonResource.MailCopyTo %>:</label>
            <div class="value cc-addresses">
                ${cc_text}
            </div> 
        </div>
        {{/if}}

        <div class="row">
            <label><%= CRMCommonResource.Date %>:</label>  
            <div class="value">
               <span>${date_created}</span>
            </div>
        </div>
    </div>
</script>


<%--UserSelectorListView--%>

<script id="userSelectorListViewTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="selectedUsers${objId}" class="clearFix" style="margin-top: 10px;">
            {{if typeof usersWhoHasAccess != "undefined" && usersWhoHasAccess != null && usersWhoHasAccess.length > 0 }}
                {{each(i, item) usersWhoHasAccess}}
                    <div class="selectedUser">
                        ${item}
                    </div>
                {{/each}}
            {{/if}}
            {{if typeof selectedUsers != "undefined" && selectedUsers != null && selectedUsers.length > 0 }}
                {{each(i, obj) selectedUsers}}
                    <div class="selectedUser" id="selectedUser_${obj.id}${objId}"
                        onmouseover="${objName}.SelectedItem_mouseOver(this);"
                        onmouseout="${objName}.SelectedItem_mouseOut(this);">
                        ${obj.displayName}
                        <img src="${deleteImgSrc}" id="deleteSelectedUserImg_${obj.id}${objId}"
                            onclick="${objName}.SelectedUser_deleteItem(this);"
                            title="<%=CRMCommonResource.DeleteUser%>" style="display: none;" alt="" />
                    </div>
                {{/each}}
            {{/if}}
        </div>
        <div class="clearFix">
            <div id="usrSrListViewAdvUsrSrContainer${objId}">
                <span class="addUserLink">
                    <a class="link dotline">${addEmployeeLabel}</a>
                    <span class="sort-down-black"></span>
                </span>
            </div>
            {{if showNotifyPanel === true}}
            <div id="notifyPanel${objId}">
                <input type="checkbox" id="cbxNotify${objId}">
                <label for="cbxNotify${objId}">${notifyLabel}</label>
            </div>
            {{/if}}
        </div>
    </div>
</script>

<%-- EventLinkToPanel (HistoryView) --%>

<script id="eventLinkToPanelTmpl" type="text/x-jquery-tmpl">
    <div id="eventLinkToPanel" class="empty-select">
        {{if contactID != 0}}
            <span><%= CRMCommonResource.LinkTo %>:</span>
            <select id="typeSwitcherSelect" class="none-helper">
                <option class="default-field" value="-2" disabled="disabled"><%= CRMJSResource.Choose%></option>
                <option class="hidden" value="-1"><%= CRMCommonResource.ClearFilter%></option>
                <option value="${entityTypeOpportunity}"><%= CRMDealResource.Deal%></option>
                <option value="${entityTypeCase}"><%= CRMCasesResource.Case%></option>
            </select>

            <select id="dealsSwitcherSelect" style="display:none;" class="none-helper">
                <option class="default-field" value="-2" disabled="disabled"><%= CRMJSResource.Choose%></option>
                {{each deals}}
                <option value="${this.id}">${this.title}</option>
                {{/each}}
            </select>

            <select id="casesSwitcherSelect" style="display:none;" class="none-helper">
                <option class="default-field" value="-2" disabled="disabled"><%= CRMJSResource.Choose%></option>
                {{each cases}}
                <option value="${this.id}">${this.title}</option>
                {{/each}}
            </select>

            <input type="hidden" id="typeID" value=""/>
            <input type="hidden" id="itemID" value="0"/>
        {{else}}
            <span><%= CRMCommonResource.AttachThisNoteToContact %>:</span>
            <select id="contactSwitcherSelect" class="none-helper">
                <option class="default-field" value="-2" disabled="disabled"><%= CRMJSResource.Choose%></option>
                <option class="hidden" value="-1"><%= CRMCommonResource.ClearFilter%></option>
                {{each contacts}}
                <option value="${this.id}">${this.displayName}</option>
                {{/each}}
            </select>
            <input type="hidden" id="contactID" value="0"/>
        {{/if}}
    </div>
</script>