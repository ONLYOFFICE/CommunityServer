<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="groupTableTmpl" type="text/x-jquery-tmpl">
    <div class="group_table_container" group_id="${id}">
        {{tmpl({email: address.email, id: id}) "groupTableRowTmpl"}}
        <div class="group_content">
            {{tmpl({mailboxes: mailboxes}) "mailboxTableTmpl"}}
        </div>
    </div>
</script>


<script id="groupTableRowTmpl" type="text/x-jquery-tmpl">
    <table class="group_menu">
        <tr data_id="${id}" class='row'>
            <td class="name_column">
                <div class="group_name">
                    <span class="group_icon"></span>
                    <span class="name bold" title="<%: MailAdministrationResource.GroupLabel %>: ${email}">${email}</span>
                    <span class="show_group gray link dotline open" onclick="javascript:administrationPage.showGroupContent('${id}');">
                            <%: MailScriptResource.HidePasswordLinkLabel %>
                    </span>
                </div>
            </td>
            <td class="menu_column">
                <div class="menu" title="<%: MailScriptResource.Actions %>" data_id="${id}"></div>
            </td>
        </tr>
    </table>
</script>