<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>

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
                            <td>${headerTest}</td>
                            <td class="popupCancel">
                                <div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton" title="<%= Resources.Resource.CloseButton %>">&times</div>
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