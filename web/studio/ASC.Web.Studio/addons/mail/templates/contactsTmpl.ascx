<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="contactsTmpl" type="text/x-jquery-tmpl">
    <div class="contentMenuWrapper">
      <ul class="clearFix contentMenu contentMenuDisplayAll" id="ContactsListGroupButtons" style="display: block;">
        <li class="menuAction menuActionSelectAll">
          <div class="menuActionSelect">
            <input id="SelectAllContactsCB" type="checkbox" title="<%: MailResource.SelectAll %>" />
          </div>
          <div id="SelectAllContactsDropdown" class="down_arrow" title="<%: MailResource.Select %>">
          </div>
        </li>
        <li class="menuAction menuActionSendEmail">
          <span title="<%: MailResource.WriteLetter %>"><%: MailResource.WriteLetter %></span>
        </li>
        <li class="menu-action-simple-pagenav"></li>
        <li class="menu-action-on-top">
            <a class="on-top-link" onclick="javascript:window.scrollTo(0, 0);">
                <%: MailResource.OnTopLabel%>
            </a>
        </li>
      </ul>
      <div class="header-menu-spacer">&nbsp;</div>
    </div>
    <table class="contacts_list" id="ContactsList">
        <tbody>
            {{tmpl(contacts, { htmlEncode : $item.htmlEncode }) "contactItemTmpl"}}
        </tbody>
    </table>
</script>

<script id="contactItemTmpl" type="text/x-jquery-tmpl">
    <tr class="row" data_id="{{if typeof(userName)!=='undefined'}}${userName}{{else}}${id}{{/if}}">
        <td class="checkbox {{if emails.length==0}}disable{{/if}}">
            <input type="checkbox" {{if emails.length>0}}title="<%: MailResource.Select %>"{{else}}disabled="disabled" class="disable"{{/if}} data_id="{{if typeof(userName)!=='undefined'}}${userName}{{else}}${id}{{/if}}" />
        </td>
        <td class="info">
            <span class="name" title="${$item.htmlEncode(displayName)}">
                {{if displayName == ' '}}&#160;{{else}}${$item.htmlEncode(displayName)}{{/if}}
            </span>
        </td>
        <td class="emails_list">
            <div class="emails">
                {{each emails}}
                    <div class="email" isprimary="${$value.isPrimary}" title="${$value.email}" style="display:none;">
                        <span class="contactEmail" contact_name="${displayName}">${$value.email}</span>
                    </div>
                {{/each}}
                <div class="more_lnk">
                    {{if emails.length>1}}
                        <span class="gray">${"<%: MailScriptResource.More %>".replace('%1', emails.length-1)}</span>
                    {{/if}}
                </div>
            </div>
        </td>
        <td class="title" title="${$item.htmlEncode(title)}">${$item.htmlEncode(title)}</td>
        {{if type == "contact" }}
            <td class="tags_info">
                <div class="labels">
                    {{each tags}}
                        <span class="tag tagArrow">${$item.htmlEncode($value)}</span>
                    {{/each}}
                </div>
            </td>
        {{/if}}
        <td class="menu_column">
            <div class="menu menu-small" title="<%: MailScriptResource.Actions %>" data_id="{{if typeof(userName)!=='undefined'}}${userName}{{else}}${id}{{/if}}"></div>
        </td>
    </tr>
</script>

<script id="contactTagTmpl" type="text/x-jquery-tmpl">
    <span labelid="${id}" class="tag inactive tagArrow tag${style}" title="${$item.htmlEncode(name)}">${$item.htmlEncode(name)}</span>
</script>

