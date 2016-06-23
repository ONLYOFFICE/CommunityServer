<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="actionPanelTmpl" type="text/x-jquery-tmpl">
    <div class="actionPanel{{if typeof(css) !=='undefined'}} ${css}{{/if}}">
        <div class="actionPanelContent">
        </div>
    </div>
</script>

<script id="actionPanelItemTmpl" type="text/x-jquery-tmpl">
    <div class="action{{if typeof(css_class) !=='undefined' && css_class !== ''}} ${css_class}{{/if}}{{if typeof(disabled) === 'undefined' || disabled === false}} active{{/if}}"
        isActive="{{if typeof(disabled) !=='undefined'}}${disabled === false}{{else}}true{{/if}}"
        title="${text}">
        ${text}{{if typeof(explanation) !=='undefined'}}<span class="explanation">${explanation}</span>{{/if}}
    </div>
</script>

<script id="moreLinkTmpl" type="text/x-jquery-tmpl">
    <div class="more_lnk">
        <span class="gray">${moreText}</span>
    </div>
</script>

<script id="popupNotificationTmpl" type="text/x-jquery-tmpl">
    <div class="toast-popup-container">
        <div class="toast ${cssClass}">
            <div class="toast-message">${text}</div>
        </div>
    </div>
</script>

<script id="mapLinkTmpl" type="text/x-jquery-tmpl">
    <a href="${mapUrl}" target="_blank" class="link underline">(<%= MailScriptResource.CalendarMapLabel %>)</a>
</script>

<script id="calendarDateHelperTmpl" type="text/x-jquery-tmpl">
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'CalendarDateHelperBlock_${messageId}'});"></span>
    <div class="popup_helper" id="CalendarDateHelperBlock_${messageId}">
        <p><%=MailResource.CalendarDateInformationText%></p>
        <div class="cornerHelpBlock pos_top"></div>
    </div>
</script>