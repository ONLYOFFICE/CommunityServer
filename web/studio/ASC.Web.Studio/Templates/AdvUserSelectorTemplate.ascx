<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>

<script id="advUserSelectorTemplate" type="text/x-jquery-tmpl">
     <div id="${jsObjName}">
        {{if jq.browser.mobile === true}}
            <select class="comboBox" style="width:${InputWidth}px;" onchange="javascript:${jsObjName}.SelectUser(this);">
            </select>
        {{else}}
                {{if IsLinkView === true}}
                <span class="addUserLink" onclick="${jsObjName}.OnInputClick(${jsObjName}, event);">
                    <a class="link dotline">${LinkText}</a>
                    <span class="sort-down-black"></span>
                </span>
                {{else}}
                <table cellspacing="0" cellpadding="1" class="borderBase adv-userselector-inputContainer" width="${InputWidth + 8}px">
                    <tbody>
                        <tr>
                            <td width="16px">
                                <img align="absmiddle" src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("user_12.png") %>"
                                    id="peopleImg" style="margin:2px;{{if SelectedUser.displayName == ""}}display:none;{{else}}display:block;{{/if}}" />               
                                <img align="absmiddle" src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("search.png") %>"
                                    id="searchImg" {{if SelectedUser.displayName == ""}}style="display:block;"{{else}}style="display:none;"{{/if}}/>
                            </td>
                            <td>
                                <input type="text" autocomplete="off"
                                    oninput="${jsObjName}.SuggestUser(event);"
                                    onpaste="${jsObjName}.SuggestUser(event);"
                                    onkeyup="${jsObjName}.SuggestUser(event);"
                                    onclick="${jsObjName}.OnInputClick(${jsObjName}, event);"
                                    onkeydown="${jsObjName}.ChangeSelection(event);"
                                    class="textEdit inputUserName" style="width:100%;" value="${SelectedUser.displayName}"/>
                                <input class="loginhidden" name="login" value="{SelectedUser.id}" type="hidden"/>
                            </td>
                            <td width="20px" onclick="${jsObjName}.OnInputClick(${jsObjName}, event);">
                                <img align="absmiddle" src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("collapse_down_dark.png") %>" style="cursor:pointer;"/>
                            </td>
                        </tr>
                    </tbody>
                </table>
            {{/if}}
            <div class="adv-userselector-DepsAndUsersContainer" {{if IsLinkView === true}}style="height:230px;"{{/if}}>
            {{if IsLinkView === true}}
            <div style="margin-bottom: 10px;">
                <div style="width:50%;">
                    <table cellspacing="0" cellpadding="1" class="borderBase adv-userselector-inputContainer" width="100%" style="height: 18px;">
                        <tbody>
                            <tr>
                                <td>
                                    <input type="text" autocomplete="off"
                                        oninput="javascript:${jsObjName}.SuggestUser(event);"
                                        onpaste="javascript:${jsObjName}.SuggestUser(event);"
                                        onkeyup="javascript:${jsObjName}.SuggestUser(event);"
                                        onclick="${jsObjName}.OnInputClick(${jsObjName}, event);"
                                        onkeydown="${jsObjName}.ChangeSelection(event);"
                                        class="textEdit inputUserName" style="width:100%;" value="${SelectedUser.displayName}"/>
                                </td>
                                <td width="16px">
                                    <img align="absmiddle" src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("close.png") %>"
                                        onclick="${jsObjName}.ClearFilter();"
                                        style="cursor:pointer;" title="<%= Resources.Resource.ClearFilterButton %>"/>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <input class="loginhidden" name="login" value="${SelectedUser.id}" type="hidden"/>
                </div>
                </div>
            {{/if}}
            <div class="adv-userselector-users"></div>
            <div class="adv-userselector-deps"></div>
        </div>
        {{/if}}
    </div>
</script>

<script id="advUSUserListTemplate" type="text/x-jquery-tmpl">
    {{each(i, usr) users}}
        <div id="User_${usr.ID}" class="adv-userselector-user" onclick="javascript:${jsObjName}.SelectUser(this);">
            <img alt="" src="${usr.PhotoUrl}"/>
            <div>
                <div class="userName">${usr.Name}</div>
                <div class="describe-text" style="padding-left:40px;">${usr.Title}</div>
            </div>
        </div>
    {{/each}}
</script>
