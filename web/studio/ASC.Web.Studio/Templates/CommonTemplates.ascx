<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Import Namespace="Resources" %>

<%-- Empty screen control --%>
<script id="template-emptyScreen" type="text/x-jquery-tmpl">
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


<%-- BlockUI screen control --%>

<script id="template-blockUIPanel" type="text/x-jquery-tmpl">
    <div style="display:none;" id="${id}">
        <div class="popupContainerClass">
            <div class="containerHeaderBlock">
                <table cellspacing="0" cellpadding="0" border="0" style="width:100%; height:0px;">
                    <tbody>
                        <tr valign="top">
                            <td {{if typeof(headerClass) != "undefined" && headerClass != ""}}class="${headerClass}"{{/if}}>${headerTest}</td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" title="<%= Resources.Resource.CloseButton %>">&times</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="infoPanel{{if typeof(infoType) != "undefined" && infoType != ""}} ${infoType}{{/if}}"
                    {{if typeof(infoMessageText) == "undefined" || infoMessageText == ""}}style="display:none;"{{/if}}>
                <div>{{if typeof(infoMessageText) != "undefined" && infoMessageText != ""}}${infoMessageText}{{/if}}</div>
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
                    {{if typeof(OKBtnID) != 'undefined'}}id="${OKBtnID}"{{/if}}
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
                <a class="button gray middle{{if typeof(CancelBtnClass) != 'undefined'}} ${CancelBtnClass}{{/if}}"
                    {{if typeof(CancelBtnID) != 'undefined'}}id="${CancelBtnID}"{{/if}}
                    onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">
                    ${CancelBtn}
                </a>
                {{/if}}
            </div>
            {{/if}}
        </div>
        </div>
    </div>
</script>


<%-- Attachments control --%>

<script id="template-newFile" type="text/x-jquery-tmpl">
    <tr class="newDoc">
        <td class="${tdclass}" colspan="2">
            <input id="newDocTitle" type="text" class="textEdit" data="<%= UserControlsCommonResource.NewDocument%>" maxlength="165" value="<%= UserControlsCommonResource.NewDocument%>"/>
            <span id="${type}" onclick="${onCreateFile}" class="button gray btn-action __apply createFile" title="<%= Resource.AddButton%>"></span>
            <span onclick="${onRemoveNewDocument}" title="<%= UserControlsCommonResource.QuickLinksDeleteLink%>" class="button gray btn-action __reset remove"></span>
        </td>        
    </tr>
</script>

<script id="template-fileAttach" type="text/x-jquery-tmpl">
    <tr>
        <td id="af_${id}">
            {{if type=="image"}}
                <a href="${viewUrl}" rel="imageGalery" class="screenzoom ${exttype}" title="${title}">
                    <div class="attachmentsTitle">${title}</div>
                    {{if versionGroup > 1}}
                        <span class="version"><%= UserControlsCommonResource.Version%>${versionGroup}</span>
                    {{/if}}
                </a>
            {{else}}
                {{if type == "editedFile" || type == "viewedFile"}}
                    <a href="${docViewUrl}" class="${exttype}" title="${title}" target="_blank">
                        <div class="attachmentsTitle">${title}</div>
                        {{if versionGroup > 1}}
                            <span class="version"><%= UserControlsCommonResource.Version%>${versionGroup}</span>
                        {{/if}}
                    </a>
                {{else}}
                    <a href="${downloadUrl}" class="${exttype} noEdit" title="${title}" target="_blank">
                        <div class="attachmentsTitle">${title}</div>
                        {{if versionGroup > 1}}
                            <span class="version"><%= UserControlsCommonResource.Version%>${versionGroup}</span>
                        {{/if}}
                    </a>
                {{/if}}
            
            {{/if}}
        </td>
    
        <td class="editFile">
            {{if (access==0 || access==1)}}
                <a class="{{if trashAction == "delete"}}deleteDoc{{else}}unlinkDoc{{/if}}" title="{{if trashAction == "delete"}}<%= UserControlsCommonResource.DeleteFile%>{{else}}<%= UserControlsCommonResource.RemoveFromList%>{{/if}}" data-fileId="${id}"></a>
            {{/if}}
            {{if (!jq.browser.mobile)}}
            <a class="downloadLink" title="<%= UserControlsCommonResource.DownloadFile%>" href="${downloadUrl}"></a>
            {{/if}}
            {{if (type == "editedFile")&&(access==0 || access==1)}}
                <a id="editDoc_${id}" title="<%= UserControlsCommonResource.EditFile%>" target="_blank" href="${editUrl}"></a>
            {{/if}}
        </td>
    </tr>
</script>



<%-- AccountLinkControl control --%>

<script id="template-accountLinkCtrl" type="text/x-jquery-tmpl">
    <div class="account-links tabs-content">
        <ul class="clearFix">
            {{each(i, acc) infos}}
            <li class="${acc.Provider}{{if acc.Linked == true}} connected{{/if}}">
                <span class="label"></span>
                <span {{if acc.Linked == true}} class="linked"{{/if}}>
                    {{if acc.Linked == true}}
                        <%= Resources.Resource.AssociateAccountConnected %>
                    {{else}}
                        <%= Resources.Resource.AssociateAccountNotConnected %>
                    {{/if}}
                </span> 
                <a href="${acc.Url}" class="popup{{if acc.Linked == true}} linked{{/if}}" id="${acc.Provider}">
                    {{if acc.Linked == true}}
                        <%= Resources.Resource.AssociateAccountDisconnect %>
                    {{else}}
                        <%= Resources.Resource.AssociateAccountConnect %>
                    {{/if}}
                </a>
            </li>
            {{/each}}
        </ul>
    </div>
</script>