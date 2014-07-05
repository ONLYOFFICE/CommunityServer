<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>

<script id="sharingListTemplate" type="text/x-jquery-tmpl">
    {{each(i, item) items}}     
        <div id="sharing_item_${item.id}" class="sharingItem borderBase clearFix {{if i%2 == 0}}tintMedium{{/if}}">        
            
        {{if item.isGroup}}
            <div class="name" title="${item.name}">
                ${item.name}
            </div>
        {{else}}
            <div class="name">
                <span class="userLink" title="${item.name}">${item.name}</span>   
            </div>             
        {{/if}}
            
            <div class="remove">
                {{if item.canEdit & !item.hideRemove}}
                    <img class="removeItem" data="${item.id}" border="0" align="absmiddle"
                        src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("trash_16.png")%>" alt="<%=Resources.Resource.DeleteButton%>"/>
                {{else}}
                    &nbsp;
                {{/if}}
            </div>
            
            <div class="action">
                {{if item.canEdit}}
                    <select data="${item.id}" id="select_${item.id}">
                        {{each(j, action) actions}}
                            {{if !action.disabled | action.id == item.selectedAction.id}}
                                <option value="${action.id}"
                                    {{if action.id == item.selectedAction.id}}selected="selected"{{/if}} 
                                    {{if action.disabled}}disabled="disabled"{{/if}} >${action.name}</option>
                            {{/if}}
                        {{/each}}
                    </select>
                {{else}}
                    ${item.selectedAction.name}
                {{/if}}
            </div>
        </div>
    {{/each}}
</script>