<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>

<%-- Empty screen control --%>
<script id="emptyScrTmpl" type="text/x-jquery-tmpl">
    <div class="noContentBlock emptyScrCtrl" >
        {{if typeof(ImgCss)!="undefined" && ImgCss != null && ImgCss != ""}}
        <table>
            <tr>
                <td>
                    <div class="emptyScrImage img ${ImgCss}"></div>
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
        {{if typeof(ImgCss)!="undefined" && ImgCss != null && ImgCss != ""}}
                    </div>
                </td>
            </tr>
        </table>
        {{/if}}
    </div>
</script>

<script id="emptyScrButtonTmpl" type="text/x-jquery-tmpl">
    <a {{if typeof(ButtonHref)!="undefined" && ButtonHref != null && ButtonHref != ""}}href="${ButtonHref}" {{/if}} class="${ButtonClass} link dotline">${ButtonText}</a>
</script>