<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportUsersTemplate.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.ImportUsersTemplate" %>

<script id="userRecord" type="text/x-jquery-tmpl">
<tr class="userItem clearFix">
    <td class="check">
        <input type="checkbox" onclick="ImportUsersManager.CheckState();" checked="checked" />
    </td>
    <td class="name">
        <div class="clearFix">
            <div class="firstname">
                <input type="text" class="studioEditableInput firstName" value="${FirstName}" maxlength="64" />
            </div>
            <div class="lastname">
                <input type="text" class="studioEditableInput lastName" value="${LastName}" maxlength="64" />
            </div>
        </div>
    </td>
    <td class="email">
        <input type="text" class="studioEditableInput email" value="${Email}" maxlength="64" onchange="ImportUsersManager.EraseError(this);" />
    </td>
    <td class="remove {{if jq.browser.mobile === true}}mob{{/if}}">
        <div onclick="ImportUsersManager.RemoveItem(this);"></div>
    </td>
    <td class="errors"></td>
</tr>
</script>