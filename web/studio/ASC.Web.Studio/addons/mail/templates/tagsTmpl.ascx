<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="tagsTmpl" type="text/x-jquery-tmpl">
    <table class="tag_list">
        <tbody>
            {{tmpl(tags, { htmlEncode : $item.htmlEncode }) "tagItemTmpl"}}
        </tbody>
    </table>
</script>

<script id="tagItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${id}" class="tag_item row {{if id<0 }}inactive{{/if}}">
        <td class="label">
            <span class="tag tagArrow tag${style}" title="${$item.htmlEncode(name)}" style="margin-top:0;">${$item.htmlEncode(name)}</span>
        </td>
        <td class="addresses"></td>
        {{if id<0 }}
            <td class="notify_column">
                <span class="notification" title="<%: MailScriptResource.TagNotificationText %>"><%: MailScriptResource.TagNotificationText %></span>
            </td>
        {{else}}
            <td class="menu_column">
                <div class="menu" title="<%: MailScriptResource.Actions %>" data_id="${id}"></div>
            </td>
        {{/if}}
    </tr>
</script>

<script id="tagInPanelTmpl" type="text/x-jquery-tmpl">
    <div>
        <span class="tag {{if used == false}} inactive{{else}} tagArrow tag${style}{{/if}}" tag_id="${id}" title="${$item.htmlEncode(name)}">
            <span class="square tag${style}"></span>
            <div class="name">${$item.htmlEncode(name)}</div>
        </span>
    </div>
</script>

<script id="tagInLeftPanelTmpl" type="text/x-jquery-tmpl">
    <div class="tag {{if used == false}} inactive{{else}} tagArrow tag${style}{{/if}}" labelid="${id}" title="${$item.htmlEncode(name)}">
        <span class="square tag${style}"></span>
        <div class="name link dotted">${$item.htmlEncode(short_name)}</div>
    </div>
</script>

<script id="tagInMessageTmpl" type="text/x-jquery-tmpl">
    <span class="tag tagArrow tag${style}" title="${$item.htmlEncode(name)}">
        <span>${$item.htmlEncode(short_name)}</span>
        <a class="tagDelete" tagid="${id}"></a>
    </span>
</script>

<script id="tagEmailInEditPopupTmpl" type="text/x-jquery-tmpl">
    <tr>
        <td>
            <span class="linked_address" title="${address}">${address}</span>
            <div class="delete_tag_address" />
        </td>
    </tr>
</script>