<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<%--Settings: Custom fields --%>

<script id="customFieldSettingsRowTmpl" type="text/x-jquery-tmpl">
{{if fieldType ==  3}}
    <li class="with-entity-menu">
        <table class="field_row" cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td class="" style="width:6px; background: #FFFFFF">
                        <div class="sort_drag_handle borderBase">&nbsp;</div>
                    </td>
                    <td class="borderBase" style="width:200px;"></td>
                    <td class="borderBase">
                        <label>
                            {{tmpl "customFieldSettingsTmpl"}}
                            <span class="customFieldTitle">${label}</span>
                        </label>
                    </td>
                    <td class="borderBase count_link_contacts gray-text" style="width:100px;">
                        ${relativeItemsString}
                    </td>
                    <td class="borderBase" style="width:30px;">
                        <div id="fieldMenu_${id}" fieldid="${id}" class="entity-menu" data-relativeitemscount="${relativeItemsCount}" title="<%= CRMCommonResource.Actions %>"></div>
                        <div class="ajax_loader loader-middle" title="">&nbsp;</div>
                    </td>
                </tr>
            </tbody>
        </table>
    </li>
{{else fieldType ==  4}}
      <li class="expand_collapce_element with-entity-menu">
         <table class="field_row" cellspacing="0" cellpadding="0" border="0" width="100%">
            <tbody>
                <tr>
                    <td class="" style="width:6px; background: #FFFFFF">
                        <div class="sort_drag_handle borderBase">&nbsp;</div>
                    </td>
                    <td class="borderBase" style="padding-left: 10px">
                        {{tmpl "customFieldSettingsTmpl"}}
                    </td>
                    <td class="borderBase count_link_contacts" style="width:100px;"></td>
                    <td class="borderBase" style="width:30px;">
                        <div id="fieldMenu_${id}" fieldid="${id}" class="entity-menu" data-relativeitemscount="${relativeItemsCount}" title="<%= CRMCommonResource.Actions %>"></div>
                        <div class="ajax_loader loader-middle" title="">&nbsp;</div>
                    </td>
                </tr>
             </tbody>
          </table>
       </li>
{{else}}
       <li class="with-entity-menu">
          <table class="field_row" cellspacing="0" cellpadding="0" border="0" width="100%">
            <tbody>
                <tr>
                    <td class="" style="width:6px; background: #FFFFFF">
                        <div class="sort_drag_handle borderBase">&nbsp;</div>
                    </td>
                    <td class="header-base-small borderBase customFieldTitle" style="width:200px; padding-left:10px;">
                        <div class="text-overflow" style="width:200px;overflow:hidden;" title="${label}">${label}</div>
                    </td>
                    <td class="borderBase">
                       {{tmpl "customFieldSettingsTmpl"}}
                    </td>
                    <td class="borderBase count_link_contacts gray-text" style="width:100px;">
                        ${relativeItemsString}
                    </td>
                    <td class="borderBase" style="width:30px;">
                        <div id="fieldMenu_${id}" fieldid="${id}" class="entity-menu" data-relativeitemscount="${relativeItemsCount}" title="<%= CRMCommonResource.Actions %>"></div>
                        <div class="ajax_loader loader-middle" title="">&nbsp;</div>
                    </td>
                </tr>
            </tbody>
        </table>
    </li>
{{/if}}
</script>

<script id="customFieldSettingsTmpl" type="text/x-jquery-tmpl">
    {{if fieldType ==  0}}
        <input id="custom_field_${id}" name="custom_field_${id}" size="${(maskObj.size > 120 ? 120 : maskObj.size)}" type="text" class="textEdit">
    {{else fieldType ==  1}}
        <textarea rows="${(maskObj.rows > 25 ? 25 : maskObj.rows)}" cols="${(maskObj.cols > 120 ? 120 : maskObj.cols)}" name="custom_field_${id}" id="custom_field_${id}"></textarea>
    {{else fieldType ==  2}}
        <select class="comboBox" name="custom_field_${id}" id="custom_field_${id}" disabled="disabled">
          {{if maskObj}}
            {{each maskObj}}
            <option value="${$value}">${$value}</option>
            {{/each}}
          {{/if}}
        </select>
    {{else fieldType ==  3}}
              <input name="custom_field_${id}" id="custom_field_${id}" type="checkbox" style="vertical-align: middle;" disabled="disabled"/>
    {{else fieldType ==  4}}
       <span id="custom_field_${id}" class="header-base headerExpand customFieldTitle" onclick="ASC.CRM.SettingsPage.toggleCollapceExpand(this)">${label}</span>
    {{else fieldType ==  5}}
      <input type="text" id="custom_field_${id}"  name="custom_field_${id}" class="textEdit textEditCalendar" />
    {{/if}}
</script>


<script id="customFieldActionPanelBodyTmpl" type="text/x-jquery-tmpl">
<div>
    <dl>
        <dt></dt>
        <dd>
            <div class="requiredField">
                <span class="requiredErrorText"><%= CRMSettingResource.EmptyLabelError %></span>
                <div class="headerPanelSmall header-base-small" style="margin-bottom:5px;">
                    <%= CRMSettingResource.Label%>:
                </div>
                <input type="text" class="textEdit" maxlength="255"/>
            </div>
        </dd>

        <dt><%= CRMSettingResource.Type %>:</dt>
        <dd>
            <select onchange="ASC.CRM.SettingsPage.selectTypeEvent(this);" class="comboBox">
                <option value="0">
                    <%= CRMSettingResource.TextField %>
                </option>
                <option value="1">
                    <%= CRMSettingResource.TextArea %>
                </option>
                <option value="2">
                    <%= CRMSettingResource.SelectBox%>
                </option>
                <option value="3">
                    <%= CRMSettingResource.CheckBox%>
                </option>
                <option value="4">
                    <%= CRMSettingResource.Heading%>
                </option>
                <option value="5">
                    <%= CRMSettingResource.Date%>
                </option>
            </select>
        </dd>
        <dt class="field_mask text_field" style="display: block;">
            <%= CRMSettingResource.Size%>:
        </dt>
        <dd class="field_mask text_field" style="display: block;">
            <input id="text_field_size" class="textEdit" value="" />
        </dd>
        <dt class="field_mask textarea_field">
            <%= CRMSettingResource.Rows%>:
        </dt>
        <dd class="field_mask textarea_field">
            <input id="textarea_field_rows" class="textEdit" value="" />
        </dd>
        <dt class="field_mask textarea_field">
            <%= CRMSettingResource.Cols%>:
        </dt>
        <dd class="field_mask textarea_field">
            <input id="textarea_field_cols" class="textEdit" value="" />
        </dd>
        <dt class="field_mask select_options">
            <%= CRMSettingResource.SelectOptions%>:</dt>
        <dd class="field_mask select_options">
            <ul>
                <li style="display: none">
                    <input type="text" class="textEdit" maxlength="255"/>
                    <label class="deleteBtn"
                        alt="<%= CRMSettingResource.RemoveOption%>" title="<%= CRMSettingResource.RemoveOption%>"
                        onclick="jq(this).parent().remove()"></label>
                </li>
            </ul>
            <span onclick="ASC.CRM.SettingsPage.toSelectBox(this)" title="<%= CRMSettingResource.AddOption%>"
                id="addOptionButton" class="baseLinkAction">
                <%= CRMSettingResource.AddOption%></span>
        </dd>
    </dl>
</div>
</script>

<%--Settings: DealMilestoneView--%>

<script id="dealMilestoneTmpl" type="text/x-jquery-tmpl">
    <li id="deal_milestone_id_${id}" class="with-entity-menu">
        <table class="deal_milestone_row" cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td class="" style="width:6px; background: #FFFFFF">
                        <div class="sort_drag_handle borderBase">&nbsp;</div>
                    </td>
                    <td class="borderBase" style="width:25px;">
                        <div class="currentColor" style="background:${color}" onclick="ASC.CRM.DealMilestoneView.showColorsPanel(this);"></div>
                    </td>
                    <td class="header-base-small borderBase deal_milestone_title" style="width:200px;">
                        ${title}
                    </td>
                    <td class="borderBase">
                        {{if stageType == 1}}
                        <%= CRMSettingResource.DealMilestoneStatusDescription_ClosedAndWon %>
                        {{else stageType == 2}}
                        <%= CRMSettingResource.DealMilestoneStatusDescription_ClosedAndLost %>
                        {{/if}}
                        {{html jq.htmlEncodeLight(description).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;") }}
                    <td class="borderBase" style="width:60px; text-align: center;">
                          ${successProbability}%
                    </td>
                    <td class="borderBase count_link_items" style="width:100px;">
                        <a class="gray-text" href="${relativeItemsUrl}">${relativeItemsString}</a>
                    </td>
                    <td class="borderBase" style="width:30px;">
                    {{if relativeItemsCount == 0 }}
                        <div id="deal_milestone_menu_${id}" dealmilestoneid="${id}" class="entity-menu" title="<%= CRMCommonResource.Actions %>"></div>
                        <div class="ajax_loader loader-middle" title="">&nbsp;</div>
                    {{/if}}
                    </td>
                </tr>
            </tbody>
        </table>
    </li>
</script>



<script id="dealMilestoneActionPanelBodyTmpl" type="text/x-jquery-tmpl">
 <div>
    <dl>
        <dt class="selectedColor">&nbsp;</dt>
        <dd>
            <span class="baseLinkAction crm-withArrowDown change_color" onclick="ASC.CRM.DealMilestoneView.showColorsPanelToSelect();">
                <%= CRMSettingResource.ChangeColor %>
            </span>
            <div id="popup_colorsPanel" class="studio-action-panel colorsPanelSettings">
                <span class="style1" colorstyle="1"></span><span class="style2" colorstyle="2"></span><span class="style3" colorstyle="3"></span><span class="style4" colorstyle="4"></span><span class="style5" colorstyle="5"></span><span class="style6" colorstyle="6"></span><span class="style7" colorstyle="7"></span><span class="style8" colorstyle="8"></span>
                <span class="style9" colorstyle="9"></span><span class="style10" colorstyle="10"></span><span class="style11" colorstyle="11"></span><span class="style12" colorstyle="12"></span><span class="style13" colorstyle="13"></span><span class="style14" colorstyle="14"></span><span class="style15" colorstyle="15"></span><span class="style16" colorstyle="16"></span>
            </div>
        </dd>

        <dt></dt>
        <dd>
            <div class="requiredField">
                <span class="requiredErrorText"><%= CRMSettingResource.EmptyTitleError %></span>
                <div class="headerPanelSmall header-base-small" style="margin-bottom:5px;">
                    <%= CRMSettingResource.TitleItem %>:
                </div>
                <input type="text" class="textEdit title" maxlength="255"/>
            </div>
        </dd>

        <dt><%= CRMSettingResource.Description %>:</dt>
        <dd>
            <textarea rows="4" style="resize: none;"></textarea>
        </dd>

        <dt><%= CRMSettingResource.Likelihood %>:</dt>
        <dd>
            <input type="text" class="textEdit probability" style="width: 30px;"  /> %
        </dd>

        <dt><%= CRMDealResource.DealMilestoneType%>:</dt>
        <dd>
            <ul>
                <li>
                    <input type="radio" id="dealMilestoneStatusOpen" name="deal_milestone_status" value="<%= (Int32)ASC.CRM.Core.DealMilestoneStatus.Open %>" />
                    <label for="dealMilestoneStatusOpen"><%=ASC.CRM.Core.DealMilestoneStatus.Open.ToLocalizedString() %></label>
                </li>
                <li>
                    <input type="radio" id="dealMilestoneStatusClosedAndWon" name="deal_milestone_status" value="<%= (Int32)ASC.CRM.Core.DealMilestoneStatus.ClosedAndWon %>"
                        onclick="javascript:jq('#manageDealMilestone .probability').val('100');"/>
                    <label for="dealMilestoneStatusClosedAndWon"><%=ASC.CRM.Core.DealMilestoneStatus.ClosedAndWon.ToLocalizedString()%></label>
                </li>
                <li>
                    <input type="radio" id="dealMilestoneStatusClosedAndLost" name="deal_milestone_status" value="<%= (Int32)ASC.CRM.Core.DealMilestoneStatus.ClosedAndLost %>"
                        onclick="javascript:jq('#manageDealMilestone .probability').val('0');"/>
                    <label for="dealMilestoneStatusClosedAndLost"><%=ASC.CRM.Core.DealMilestoneStatus.ClosedAndLost.ToLocalizedString()%></label>
                </li>
            </ul>
        </dd>
    </dl>
</div>
</script>

<%--Settings: ListItemView--%>

<script id="listItemsTmpl" type="text/x-jquery-tmpl">
    <li id="list_item_id_${id}" class="with-entity-menu">
        <table cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td class="" style="width:6px; background: #FFFFFF">
                        <div class="sort_drag_handle borderBase">&nbsp;</div>
                    </td>

                    {{if ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3}}
                        <td class="borderBase" style="width:36px;">
                            <label data-imgName="${imageName}" title="${imageTitle}"
                                class="currentIcon {{if ASC.CRM.ListItemView.CurrentType == 2}}task_category{{else}}event_category{{/if}} ${cssClass}"
                                onclick="ASC.CRM.ListItemView.showIconsPanel(this);"></label>
                            <div class="ajax_change_icon loader-big" title="">&nbsp;</div>
                        </td>
                    {{else ASC.CRM.ListItemView.CurrentType === 1}}
                        <td class="borderBase" style="width:25px;">
                            <div class="currentColor" style="background:${color}" onclick="ASC.CRM.ListItemView.showColorsPanel(this);"
                                title="<%= CRMSettingResource.ChangeColor %>"></div>
                        </td>
                    {{/if}}

                    <td class="header-base-small borderBase item_title" style="width:230px;">
                        ${title}
                    </td>

                    <td class="borderBase item_description">
                        {{if ASC.CRM.ListItemView.CurrentType !== 4}}
                            {{html jq.htmlEncodeLight(description).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;") }}
                        {{/if}}
                    </td>

                    {{if ASC.CRM.ListItemView.CurrentType === 3}}
                    <td class="borderBase count_link_items gray-text" style="width:100px;">
                        ${relativeItemsString}
                    </td>
                    {{else}}
                    <td class="borderBase count_link_items" style="width:100px;">
                        <a class="gray-text" href="${relativeItemsUrl}">${relativeItemsString}</a>
                    </td>
                    {{/if}}

                    <td class="borderBase" style="width:30px;">
                        {{if relativeItemsCount === 0 || ASC.CRM.ListItemView.CurrentType === 2}}
                        <div id="list_item_menu_${id}" class="entity-menu"
                            data-listitemid="${id}"
                            data-relativecount="${relativeItemsCount}"
                            title="<%= CRMCommonResource.Actions %>"></div>
                        <div class="ajax_loader loader-middle" title="">&nbsp;</div>
                        {{/if}}
                   </td>
                </tr>
            </tbody>
        </table>
    </li>
</script>



<script id="listViewActionPanelBodyTmpl" type="text/x-jquery-tmpl">
<div>
    <div class="clearFix" style="margin-bottom:10px;">
        {{if (currentType == 2 || currentType == 3)}}
            <div style="float: left;">
                <label class="selectedIcon {{if currentType == 2}}task_category{{else}}event_category{{/if}}" title="" data-imgName=""></label>
            </div>
            <div style="padding-top: 6px">
                <span class="baseLinkAction crm-withArrowDown change_icon" onclick="ASC.CRM.ListItemView.showIconsPanelToSelect();">
                    <%= CRMSettingResource.ChangeIcon %>
                </span>
            </div>

            {{if currentType == 2}}
            <div id="popup_iconsPanel_2" class="iconsPanelSettings studio-action-panel" style="width: 148px;height: 112px;">
                <label class="task_category task_category_call" title="<%= CRMTaskResource.TaskCategory_Call %>" data-imgName="task_category_call.png"></label>
                <label class="task_category task_category_deal" title="<%= CRMTaskResource.TaskCategory_Deal %>" data-imgName="task_category_deal.png"></label>
                <label class="task_category task_category_demo" title="<%= CRMTaskResource.TaskCategory_Demo %>" data-imgName="task_category_demo.png"></label>
                <label class="task_category task_category_email" title="<%= CRMTaskResource.TaskCategory_Email %>" data-imgName="task_category_email.png"></label>
                <label class="task_category task_category_fax" title="<%= CRMTaskResource.TaskCategory_Fax %>" data-imgName="task_category_fax.png"></label>
                <label class="task_category task_category_follow_up" title="<%= CRMTaskResource.TaskCategory_FollowUP %>" data-imgName="task_category_follow_up.png"></label>
                <label class="task_category task_category_lunch" title="<%= CRMTaskResource.TaskCategory_Lunch %>" data-imgName="task_category_lunch.png"></label>
                <label class="task_category task_category_meeting" title="<%= CRMTaskResource.TaskCategory_Meeting %>" data-imgName="task_category_meeting.png"></label>
                <label class="task_category task_category_note" title="<%= CRMTaskResource.TaskCategory_Note %>" data-imgName="task_category_note.png"></label>
                <label class="task_category task_category_ship" title="<%= CRMTaskResource.TaskCategory_Ship %>" data-imgName="task_category_ship.png"></label>
                <label class="task_category task_category_social_networks" title="<%= CRMTaskResource.TaskCategory_SocialNetworks %>" data-imgName="task_category_social_networks.png"></label>
                <label class="task_category task_category_thank_you" title="<%= CRMTaskResource.TaskCategory_ThankYou %>" data-imgName="task_category_thank_you.png"></label>
            </div>
            {{else}}
            <div id="popup_iconsPanel_3" class="iconsPanelSettings studio-action-panel" style="width: 74px; height: 74px;">
                <label class="event_category event_category_note" title="<%= CRMCommonResource.HistoryCategory_Note %>" data-imgName="event_category_note.png"></label>
                <label class="event_category event_category_email" title="<%= CRMCommonResource.HistoryCategory_Email %>" data-imgName="event_category_email.png"></label>
                <label class="event_category event_category_call" title="<%= CRMCommonResource.HistoryCategory_Call %>" data-imgName="event_category_call.png"></label>
                <label class="event_category event_category_meeting" title="<%= CRMCommonResource.HistoryCategory_Meeting %>" data-imgName="event_category_meeting.png"></label>
            </div>
            {{/if}}

        {{else currentType == 1}}
            <div class="selectedColor">&nbsp;</div>
            <span class="baseLinkAction crm-withArrowDown change_color" onclick="ASC.CRM.ListItemView.showColorsPanelToSelect();">
                <%= CRMSettingResource.ChangeColor %>
            </span>
            <div id="popup_colorsPanel" class="studio-action-panel colorsPanelSettings">
                <span class="style1" colorstyle="1"></span><span class="style2" colorstyle="2"></span><span class="style3" colorstyle="3"></span><span class="style4" colorstyle="4"></span><span class="style5" colorstyle="5"></span><span class="style6" colorstyle="6"></span><span class="style7" colorstyle="7"></span><span class="style8" colorstyle="8"></span>
                <span class="style9" colorstyle="9"></span><span class="style10" colorstyle="10"></span><span class="style11" colorstyle="11"></span><span class="style12" colorstyle="12"></span><span class="style13" colorstyle="13"></span><span class="style14" colorstyle="14"></span><span class="style15" colorstyle="15"></span><span class="style16" colorstyle="16"></span>
            </div>
        {{/if}}
    </div>

    <div class="requiredField" style="margin-bottom:10px;">
        <span class="requiredErrorText"><%= CRMSettingResource.EmptyTitleError %></span>
        <div style="margin-bottom:5px;" class="headerPanelSmall header-base-small">
            <%= CRMSettingResource.TitleItem %>:
        </div>
        <input type="text" class="textEdit" maxlength="255" />
    </div>

    {{if currentType != 4}}
    <div>
        <div style="margin-bottom:5px;">
            <b><%= CRMSettingResource.Description %>:</b>
        </div>
        <textarea rows="4" style="resize: none;"></textarea>
    </div>
    {{/if}}
</div>
</script>


<%--Settings: TagSettingsView--%>

<script id="deleteUnusedTagsButtonTmpl" type="text/x-jquery-tmpl">
    <li>
        <a id="deleteUnusedTagsButton" class="dropdown-item" title="<%= CRMSettingResource.DeleteUnusedTags %>">
            <%= CRMSettingResource.DeleteUnusedTags %>
        </a>
    </li>
</script>

<script id="tagRowTemplate" type="text/x-jquery-tmpl">
    <li>
        <table class="table-list" cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td class="header-base-small">
                        <div class="title">${ASC.CRM.Common.convertText(title,true)}</div>
                    </td>
                    <td class="count_link_items" style="width:150px;">
                        <a class="gray-text" href="${relativeItemsUrl}">${relativeItemsString}</a>
                    </td>
                    <td style="width:40px;">
                        <a class="crm-deleteLink" title="<%= CRMSettingResource.DeleteTag %>" alt="<%= CRMSettingResource.DeleteTag %>"
                            onclick='ASC.CRM.TagSettingsView.deleteTag(this);'></a>
                        <div class="ajax_loader loader-middle" title="">&nbsp;</div>
                    </td>
                </tr>
            </tbody>
        </table>
    </li>
</script>

<script id="tagSettingsActionPanelBodyTmpl" type="text/x-jquery-tmpl">
<div>
    <div class="requiredField">
        <span class="requiredErrorText"></span>
        <div class="headerPanelSmall header-base-small" style="margin-bottom:5px;">
            <%= CRMSettingResource.Label%>:
        </div>
        <input id="tagTitle" type="text" class="textEdit" style="width:100%" maxlength="50"/>
    </div>
</div>
</script>

<%--Settings: TaskTemplateView--%>

<script id="templateContainerRow" type="text/x-jquery-tmpl">
    <li id="templateContainerHeader_${id}">
        <table class="templateContainer_row" cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td class="borderBase">
                        <span onclick="ASC.CRM.TaskTemplateView.toggleCollapceExpand(this)" class="header-base headerExpand">
                            ${title}
                        </span>
                    </td>
                    <td class="borderBase" style="width:70px;text-align: right;padding-right: 10px;">
                        <a class="crm-addNewLink" align="absmiddle"
                            title="<%= CRMSettingResource.AddNewTaskTemplate %>"
                            onclick="ASC.CRM.TaskTemplateView.showTemplatePanel(${id})"></a>
                        <a class="crm-editLink" align="absmiddle"
                            title="<%= CRMSettingResource.EditTaskTemplateContainer %>"
                            onclick="ASC.CRM.TaskTemplateView.showTemplateConatainerPanel(${id})"></a>
                        <a class="crm-deleteLink" align="absmiddle"
                            title="<%= CRMSettingResource.DeleteTaskTemplateContainer %>"
                            onclick="ASC.CRM.TaskTemplateView.deleteTemplateConatainer(${id})"></a>
                        <img class="loaderImg" align="absmiddle" style="display: none;"
                            src="<%= WebImageSupplier.GetAbsoluteWebPath("loader_16.gif") %>"/>
                    </td>
                </tr>
            </tbody>
        </table>
    </li>
    <li id="templateContainerBody_${id}" style="{{if typeof(items)=="undefined"}}display:none;{{/if}}">
        {{if typeof(items)!="undefined"}}
        {{tmpl(items) "templateRow"}}
        {{/if}}
    </li>
</script>

<script id="templateRow" type="text/x-jquery-tmpl">
    <table cellspacing="0" cellpadding="0"  id="templateRow_${id}" class="templateContainer_row" style="margin-bottom: -1px;">
        <tbody>
            <tr>
                <td class="borderBase" style="width:30px;">
                    <img title="${category.title}" alt="${category.title}" src="${category.imagePath}" />
                </td>
                <td class="borderBase">
                    <div class="divForTemplateTitle">
                        <span id="templateTitle_${id}" class="templateTitle" title="${description}">${title}</span>
                    </div>
                    <div style="padding-top: 5px; display: inline-block;">
                        ${ASC.CRM.TaskTemplateView.getDeadlineDisplacement(offsetTicks, deadLineIsFixed)}
                    </div>
                </td>
                <td class="borderBase" style="width:200px;">
                    <span class="userLink">${responsible.displayName}</span>
                </td>
                <td class="borderBase" style="width:70px;text-align: right;padding-right: 10px;">
                    <a class="crm-editLink" align="absmiddle"
                            title="<%= CRMSettingResource.EditTaskTemplate %>"
                            onclick="ASC.CRM.TaskTemplateView.showTemplatePanel(${containerID}, ${id})"></a>
                    <a class="crm-deleteLink" align="absmiddle"
                         title="<%= CRMSettingResource.DeleteTaskTemplate %>"
                         onclick="ASC.CRM.TaskTemplateView.deleteTemplate(${id})"></a>
                    <img class="loaderImg" align="absmiddle" style="display: none;"
                         src="<%= WebImageSupplier.GetAbsoluteWebPath("loader_16.gif") %>"/>
                </td>
            </tr>
        </tbody>
    </table>
</script>


<%--Settings: WebToLeadFormView--%>

<script id="sampleFormTmpl" type="text/x-jquery-tmpl">
  <form name='sampleForm' method='POST' action='${webtoleadfromhandlerPath}' accept-charset='UTF-8'>
    <meta content='text/html;charset=UTF-8' http-equiv='content-type'>
    <style type="text/css">
        #sampleFormPanel {
           padding: 10px;
        }
        #sampleFormPanel dt {
           float: left;
           text-align: right;
           width: 40%;
        }
        #sampleFormPanel dd:not(:first-child) {
             margin-bottom: 5px;
             margin-left: 40%;
             padding-left: 10px;
        }
        #sampleFormPanel input[type=text] {
             border: solid 1px #C7C7C7;
        }
        #sampleFormPanel input[type=checkbox] {
             margin-left: 0;
        }
         #sampleFormPanel .requiredField:after {
            color: #c00;
            content: " *";
            font-size: 12px;
            vertical-align: text-top;
        }
    </style>

    <dl id="sampleFormPanel">
        <dt><input type="hidden" name="is_company" value="${isCompany}"/></dt>
        <dd>
         {{each fieldListInfo}}
           <dt>
               {{if type == 3}}
               <label for="cb_${name}">${title}:</label>
               {{else type == -1 && (isCompany == true && title == ASC.CRM.Resources.CRMContactResource.CompanyName || isCompany == false && title == ASC.CRM.Resources.CRMContactResource.FirstName)}}
               <div class="requiredField">${title}:</div>
               {{else}}
               ${title}:
               {{/if}}
           </dt>
           <dd>
                {{if type == -1 || type == 0}}
                <input name="${name}" type="text" {{if mask != null && mask != ""}}size="${mask.size}"{{/if}}/>
                {{else type == 1}}
                <textarea name="${name}" {{if mask != null && mask != ""}}cols="${mask.cols}" rows="${mask.rows}"{{/if}}></textarea>
                {{else type == 2}}
                <select name="${name}">
                    {{if mask != null && mask != ""}}
                    <option value=""></option>
                    {{each mask}}
                        <option value="${$value}">${$value}</option>
                    {{/each}}
                    {{/if}}
                </select>
                {{else type == 3}}
                <input id="cb_${name}" name="${name}" type="checkbox" />
                {{/if}}
           </dd>
        {{/each}}
        <dt>
        </dt>
        <dd>
            <input name="${name}" value="<%= CRMSettingResource.SubmitFormData %>" type="submit"
                onclick="javascript:
                            var isValid = true,
                                form = document.getElementById('sampleFormPanel'),
                                childs = form.getElementsByTagName('input'),
                                isCompany = null,
                                firstName = '',
                                lastName = '',
                                companyName = '',
                                fieldName = '';
                            for (var i = 0, n = childs.length; i < n; i++) {
                               fieldName = childs[i].getAttribute('name');

                                switch (fieldName) {
                                    case 'is_company':
                                        isCompany = childs[i].value.trim();
                                        break;
                                    case 'firstName':
                                        firstName = childs[i].value.trim();
                                        break;
                                    case 'lastName':
                                        lastName = childs[i].value.trim();
                                        break;
                                    case 'companyName':
                                        companyName = childs[i].value.trim();
                                        break;
                                }
                            }
                            if (isCompany == 'false') {
                                if (firstName == ''){
                                    if(typeof (toastr) === 'object' && typeof (toastr.error) === 'function') toastr.error('<%= CRMContactResource.ErrorEmptyContactFirstName %>');
                                    else alert('<%= CRMContactResource.ErrorEmptyContactFirstName %>');
                                    isValid = false;
                                }
                            }
                            else if (isCompany == 'true') {
                                if(companyName == '') {
                                    if(typeof (toastr) === 'object' && typeof (toastr.error) === 'function') toastr.error('<%= CRMContactResource.ErrorEmptyCompanyName %>');
                                    else alert('<%= CRMContactResource.ErrorEmptyCompanyName %>');
                                    isValid = false;
                                }
                            } else {
                                isValid = false;
                            }
                            return isValid;"/>
        </dd>
    </dl>
    {{each tagListInfo}}
        <input type="hidden" name="tag" value="${title}" />
    {{/each}}
    <input type="hidden" name="return_url" value="${returnURL}" />
    <input type="hidden" name="web_form_key"  value="${webFormKey}"/>
    <input type="hidden" name="notify_list" value="${notifyList}"/>
    <input type="hidden" name="managers_list" value="${managersList}"/>
    <input type="hidden" name="share_type" value="${shareType}"/>
  </form>
</script>


<%--Settings: SMTPSettingsForm--%>

<script id="SMTPSettingsFormTemplate" type="text/x-jquery-tmpl">
    <table cellpadding="5" cellspacing="0">
        <tr>
            <td>
                <div class="header-base-small headerTitle"><%=CRMSettingResource.Host%>:</div>
                <input type="text" class="textEdit" id="tbxHost"/>
            </td>
            <td>
                <div class="header-base-small headerTitle"><%=CRMSettingResource.Port%>:</div>
                <div>
                    <input type="text" class="textEdit" id="tbxPort" maxlength="5"/>
                    <input id="cbxAuthentication" type="checkbox" />
                    <label for="cbxAuthentication" class="header-base-small" style="line-height: 21px;"><%=CRMSettingResource.Authentication%></label>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div class="header-base-small headerTitle"><%=CRMSettingResource.HostLogin%>:</div>
                <input type="text" class="textEdit" id="tbxHostLogin"/>
            </td>
            <td>
                <div class="header-base-small headerTitle"><%=CRMSettingResource.HostPassword%>:</div>
                <input type="password" class="textEdit" id="tbxHostPassword"/>
            </td>
        </tr>
        <tr>
            <td>
                <div class="header-base-small headerTitle"><%=CRMSettingResource.SenderDisplayName%>:</div>
                <input type="text" class="textEdit" id="tbxSenderDisplayName"/>
            </td>
            <td>
                <div class="header-base-small headerTitle"><%=CRMSettingResource.SenderEmailAddress%>:</div>
                <input type="text" class="textEdit" id="tbxSenderEmailAddress"/>
            </td>
        </tr>
        <tr>
            <td>
                <input id="cbxEnableSSL" type="checkbox" />
                <label for="cbxEnableSSL" class="header-base-small" style="float: left; line-height: 20px;">
                    <%=CRMSettingResource.EnableSSL%>
                </label>
            </td>
            <td></td>
        </tr>
    </table>
</script>

<%--Invoice Items List--%>

<script id="invoiceItemsListTmpl" type="text/x-jquery-tmpl">
    <tbody>
        {{tmpl(invoiceItems) "invoiceItemTmpl"}}
    </tbody>
</script>

<script id="invoiceItemTmpl" type="text/x-jquery-tmpl">
    <tr id="invoiceItem_${id}" class="with-entity-menu">
       <%-- <td class="borderBase" style="padding: 0 0 0 6px;">
            <input type="checkbox" id="checkInvoiceItem_${id}" onclick="ASC.CRM.InvoiceItemsView.selectItem(this);"
                 {{if isChecked == true}}checked="checked"{{/if}} />
            <img id="loaderImg_${id}" style="display:none;" alt="" src="<%=WebImageSupplier.GetAbsoluteWebPath("loader_16.gif")%>" />
        </td>--%>
        <td>
            <div class="invoiceItemSKU">
                ${stockKeepingUnit}
            </div>
        </td>
        <td>
            <div id="invoiceItemTitle_${id}" class="invoiceItemTitle" dscr_label="<%=CRMCommonResource.Description%>" dscr_value="${description}">
                ${title}
            </div>
        </td>

        <td class="invoiceTaxes invoiceTax1">
            {{if typeof(invoiceTax1) != 'undefined' && invoiceTax1 != null}}
            <div id="invoiceItemTax1_${id}" dscr_label="${invoiceTax1.name}" dscr_value="${invoiceTax1.rate} %">
                ${invoiceTax1.name}
            </div>
            {{/if}}
        </td>

        <td class="invoiceTaxes invoiceTax2">
            {{if typeof(invoiceTax2) != 'undefined' && invoiceTax2 != null}}
            <div id="invoiceItemTax2_${id}" dscr_label ="${invoiceTax2.name}" dscr_value="${invoiceTax2.rate} %">
                ${invoiceTax2.name}
            </div>
            {{/if}}
        </td>

        <td class="invoiceItemPrice">
            <div>
                {{html priceFormat}}
            </div>
        </td>

        <td class="invoiceItemQuantity">
            <div>
                {{if trackInvenory === true }}
                    ${stockQuantity}
                {{else quantity != 0}}
                    ${quantity}
                {{/if}}
             </div>
        </td>

        <td style="padding:5px;">
            <div id="invoiceItemMenu_${id}" class="entity-menu" title="<%= CRMCommonResource.Actions %>"></div>
        </td>
    </tr>
</script>


<%--Invoice Taxes List--%>

<script id="invoiceTaxesListTmpl" type="text/x-jquery-tmpl">
    <tbody>
        {{tmpl(invoiceTaxes) "invoiceTaxTmpl"}}
    </tbody>
</script>

<script id="invoiceTaxTmpl" type="text/x-jquery-tmpl">
    <tr id="invoiceTax_${id}" class="with-entity-menu">
        <td>
            <div class="invoiceTaxName" title="${name}">
                ${name}
            </div>
        </td>
        <td>
            <div class="invoiceTaxRate">
                ${rate}
            </div>
        </td>

        <td>
            <div class="invoiceTaxDscr">
                {{html jq.htmlEncodeLight(description).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;") }}
            </div>
        </td>

        <td style="padding:5px;">
            {{if canEdit == true}}
            <div id="invoiceTaxMenu_${id}" class="entity-menu" title="<%= CRMCommonResource.Actions %>"></div>
            {{/if}}
        </td>
    </tr>
</script>


<script id="taxSettingsActionPanelBodyTmpl" type="text/x-jquery-tmpl">
<div>
    <div class="requiredField">
        <span class="requiredErrorText"><%= CRMInvoiceResource.EmptyTaxNameError %></span>
        <div class="headerPanelSmall header-base-small">
            <%= CRMInvoiceResource.InvoiceTaxName %>:
        </div>
        <input type="text" class="textEdit taxName" maxlength="255"/>
    </div>
    <div class="requiredField">
        <span class="requiredErrorText"><%= CRMInvoiceResource.EmptyTaxRateError %></span>
        <div class="headerPanelSmall header-base-small">
            <%= CRMInvoiceResource.InvoiceTaxRate %>:
        </div>
        <input type="text" class="textEdit taxRate" maxlength="4"/> %
    </div>
    <div>
        <div class="header-base-small">
            <%= CRMSettingResource.Description %>:
        </div>
        <textarea cols="4" rows="4"></textarea>
    </div>
</div>
</script>