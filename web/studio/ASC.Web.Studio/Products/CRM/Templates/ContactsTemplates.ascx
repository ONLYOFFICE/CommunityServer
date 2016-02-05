<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<%--Contacts List--%>

<script id="contactsListBaseTmpl" type="text/x-jquery-tmpl">
<div id="mainContactList">
    <div class="clearFix">
        <div id="contactsFilterContainer">
            <div id="contactsAdvansedFilter"></div>
        </div>
        <div id="companyListBox" style="display: none">
            <ul id="contactsHeaderMenu" class="clearFix contentMenu contentMenuDisplayAll">
                <li class="menuAction menuActionSelectAll menuActionSelectLonely">
                    <div class="menuActionSelect">
                        <input type="checkbox" id="mainSelectAll" title="<%=CRMCommonResource.SelectAll%>" onclick="ASC.CRM.ListContactView.selectAll(this);" />
                    </div>
                </li>
                {{if IsCRMAdmin === true}}
                <li class="menuAction menuActionSendEmail" title="<%= CRMCommonResource.SendEmail %>">
                    <span><%= CRMCommonResource.SendEmail%></span>
                    <div class="down_arrow"></div>
                </li>
                {{/if}}
                <li class="menuAction menuActionAddTag" title="<%= CRMCommonResource.AddNewTag %>">
                    <span><%=CRMCommonResource.AddNewTag%></span>
                    <div class="down_arrow"></div>
                </li>
                <%--<li class="menuAction menuActionPermissions" title="<%= CRMCommonResource.SetPermissions %>">
                    <span><%=CRMCommonResource.SetPermissions%></span>
                </li>--%>
                <li class="menuAction menuActionAddTask" title="<%= CRMTaskResource.AddNewTaskButtonText %>">
                    <span><%=CRMTaskResource.AddNewTaskButtonText%></span>
                </li>
                <li class="menuAction menuActionDelete" title="<%= CRMCommonResource.Delete %>">
                    <span><%= CRMCommonResource.Delete%></span>
                </li>
                <li class="menu-action-simple-pagenav">
                </li>
                <li class="menu-action-checked-count">
                    <span></span>
                    <a class="linkDescribe baseLinkAction" style="margin-left:10px;" onclick="ASC.CRM.ListContactView.deselectAll();">
                        <%= CRMCommonResource.DeselectAll%>
                    </a>
                </li>
                <li class="menu-action-on-top">
                    <a class="on-top-link" onclick="javascript:window.scrollTo(0, 0);">
                        <%= CRMCommonResource.OnTop%>
                    </a>
                </li>
            </ul>
            <div class="header-menu-spacer">&nbsp;</div>

            <table id="companyTable" class="tableBase" cellpadding="4" cellspacing="0">
                <colgroup>
                    <col style="width: 26px;"/>
                    <col style="width: 40px;"/>
                    <col/>
                    <col style="width: 200px;"/>
                    <col style="width: 200px;"/>
                    <col style="width: 200px;"/>
                    <col style="width: 40px;"/>
                </colgroup>
                <tbody>
                </tbody>
            </table>

            <table id="tableForContactNavigation" class="crm-navigationPanel" cellpadding="4" cellspacing="0" border="0">
                <tbody>
                <tr>
                    <td>
                        <div id="divForContactPager">
                        </div>
                    </td>
                    <td style="text-align:right;">
                        <span class="gray-text"><%= CRMContactResource.TotalContacts%>:</span>
                        <span class="gray-text" id="totalContactsOnPage"></span>
                        <span class="page-nav-info">
                            <span class="gray-text"><%= CRMCommonResource.ShowOnPage%>:&nbsp;</span>
                            <select class="top-align">
                                <option value="25">25</option>
                                <option value="50">50</option>
                                <option value="75">75</option>
                                <option value="100">100</option>
                            </select>
                        </span>
                    </td>
                </tr>
                </tbody>
            </table>
        </div>
    </div>
</div>

<div id="permissionsContactsPanelInnerHtml" class="display-none">
    {{if IsCRMAdmin !== true}}
    <div style="margin-top:10px">
        <b><%= CRMCommonResource.AccessRightsLimit%></b>
    </div>
    {{/if}}
</div>


{{if IsCRMAdmin === true}}
<div id="sendEmailDialog" class="studio-action-panel group-actions">
    <ul class="dropdown-content">
        <li>
            <a class="dropdown-item" onclick="ASC.CRM.ListContactView.showCreateLinkPanel()">
                <%=CRMSettingResource.ExternalClient%>
            </a>
        </li>
        <li>
            <a class="dropdown-item" onclick="ASC.CRM.ListContactView.showSenderPage()">
                <%=String.Format(CRMSettingResource.InternalSMTP, MailSender.GetQuotas())%>
            </a>
        </li>
    </ul>
</div>
{{/if}}

<div id="addTagDialog" class="studio-action-panel group-actions addTagDialog">
    <ul class="dropdown-content mobile-overflow"></ul>
    <div class="h_line">&nbsp;</div>
    <div style="padding: 0 12px;">
        <div style="margin-bottom: 5px;" ><%= CRMCommonResource.CreateNewTag%>:</div>
        <input type="text" maxlength="50" class="textEdit" />
        <a onclick="ASC.CRM.ListContactView.addNewTag();" class="button blue" id="addThisTag">
            <%= CRMCommonResource.OK%>
        </a>
    </div>
</div>

<div id="contactActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="showProfileLink dropdown-item"><%= CRMContactResource.ShowContactProfile%></a></li>
        <li><a class="showProfileLinkNewTab dropdown-item"><%= CRMContactResource.ShowContactProfileNewTab%></a></li>
        <li><a class="addPhoneLink dropdown-item"><%= CRMJSResource.AddNewPhone%></a></li>
        <li><a class="addEmailLink dropdown-item"><%= CRMJSResource.AddNewEmail%></a></li>
        <li><a class="sendEmailLink dropdown-item" target="_blank"><%= CRMContactResource.WriteEmail%></a></li>
        <li><a class="addTaskLink dropdown-item"><%= CRMTaskResource.AddNewTask%></a></li>
        <li><a class="addDealLink dropdown-item"><%= CRMDealResource.CreateNewDeal %></a></li>
        <li><a class="addCaseLink dropdown-item"><%= CRMCasesResource.CreateNewCase %></a></li>
        <%--<li><a class="setPermissionsLink dropdown-item"><%= CRMCommonResource.SetPermissions%></a></li>--%>
        <li><a class="editContactLink dropdown-item"><%= CRMContactResource.EditContact%></a></li>
        <li><a class="deleteContactLink dropdown-item"><%= CRMContactResource.DeleteContact%></a></li>
    </ul>
</div>
</script>

<script id="contactListTmpl" type="text/x-jquery-tmpl">
    <tbody>
        {{tmpl(contacts) "contactTmpl"}}
    </tbody>
</script>

<script id="contactTmpl" type="text/x-jquery-tmpl">
    <tr id="contactItem_${id}" class="with-entity-menu">
        <td class="borderBase" style="padding-left: 6px;">
            <input type="checkbox" id="check_contact_${id}" onclick="ASC.CRM.ListContactView.selectItem(this);" style="margin-left: 2px;" {{if isChecked == true}}checked="checked"{{/if}} />
            <div id="loaderImg_${id}" class="loader-middle baseList_loaderImg"></div>
        </td>


        <td class="borderBase">
            <div class="contactItemPhotoImgContainer{{if isShared === true}} sharedContact{{/if}}">
                {{if isCompany == true}}
                <img class="contactItemPhotoImg" src="<%=ContactPhotoManager.GetSmallSizePhoto(0, true) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${smallFotoUrl}');" />
                {{else}}
                <img class="contactItemPhotoImg" src="<%=ContactPhotoManager.GetSmallSizePhoto(0, false) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${smallFotoUrl}');" />
                {{/if}}
                <img class="contactItemPhotoImg" style="display:none;" alt="${displayName}" title="${displayName}"/>
            </div>
        </td>

        <td class="borderBase">
            <div class="contactTitle">
                <a class="linkHeaderMedium" href="default.aspx?id=${id}">
                    ${displayName}
                </a>
            </div>
            {{if isCompany == false && company != null}}
                <div class="contactTitle">
                    <%=CRMContactResource.Company%>:
                    <a href="default.aspx?id=${company.id}" data-id="${company.id}" id="contact_${id}_company_${company.id}" class="linkMedium crm-companyInfoCardLink">
                        ${company.displayName}
                    </a>
                </div>
            {{/if}}
        </td>

        <td class="borderBase">
            <div class="primaryDataContainer">
                <input type="text" id="addPrimaryPhone_${id}" class="textEdit addPrimaryDataInput" autocomplete="off" maxlength="100"/>
            {{if primaryPhone != null}}
                <span class="primaryPhone" title="${primaryPhone.data}">${primaryPhone.data}</span>
            {{/if}}
            </div>
        </td>

        <td class="borderBase">
            <div class="primaryDataContainer">
                <input type="text" id="addPrimaryEmail_${id}" class="textEdit addPrimaryDataInput" autocomplete="off" maxlength="100"/>
            {{if primaryEmail != null}}
                <a class="primaryEmail linkMedium" title="${primaryEmail.data}" href="${primaryEmail.emailHref}" target="_blank">
                    ${primaryEmail.data}
                </a>
            {{/if}}
            </div>
        </td>

        <td class="borderBase">
            {{if nearTask != null}}
                <span id="taskTitle_${nearTask.id}" class="header-base-small nearestTask"
                    ttl_label="<%=CRMCommonResource.Title%>" ttl_value="${nearTask.title}"
                    dscr_label="<%=CRMCommonResource.Description%>" dscr_value="${nearTask.description}"
                    resp_label="<%=CRMCommonResource.Responsible%>" resp_value="${nearTask.responsible.displayName}">
                        ${nearTask.category.title} ${nearTask.deadLineString}
                </span>
            {{/if}}
        </td>
        <td class="borderBase">
            <div id="contactMenu_${id}" class="entity-menu" title="<%= CRMCommonResource.Actions %>"></div>
        </td>
    </tr>
</script>


<%--ContactActionView and ContactDetailsView: social networks --%>

<script id="FacebookProfileTmpl" type="text/x-jquery-tmpl">
    <tr>
        <td class="sm_tbl_UserList_clmnBtRelate">
            <a class="button gray plus" onclick="ASC.CRM.SocialMedia.AddFacebookProfileToContact('${userID}'); return false;"><%=CRMCommonResource.Add%></a>
        </td>
        <td class="sm_tbl_UserList_clmnAvatar">
            <div style="min-height: 40px;">
                <img src="${smallImageUrl}" alt="${userName}" width="40"/>
            </div>
        </td>
        <td class="sm_tbl_UserList_clmnUserName" style="padding:5px;">
            <span class="header-base-small sn_userName" style="color: Black !important;">${userName}</span>
        </td>
    </tr>
</script>

<script id="TwitterProfileTmpl" type="text/x-jquery-tmpl">
    <tr>
        <td class="sm_tbl_UserList_clmnBtRelate">
            <a class="button gray plus"
                onclick="ASC.CRM.SocialMedia.AddTwitterProfileToContact('${screenName}'); return false;">
                <%= CRMCommonResource.Add %>
            </a>
        </td>
        <td class="sm_tbl_UserList_clmnAvatar">
            <div style="min-height: 40px;">
                <img src="${smallImageUrl}" alt="${userName}" width="40"/>
            </div>
        </td>
        <td class="sm_tbl_UserList_clmnUserName" style="padding:5px;">
            <span class="header-base-small sn_userName" style="color: Black !important;">${userName}</span>
            <br />
            ${description}
        </td>
    </tr>
</script>

<script id="TwitterProfileTabTmpl" type="text/x-jquery-tmpl">
    <tr>
        <td class="sm_tbl_UserList_clmnBtRelate">
            <a class="button gray plus"
                onclick="ASC.CRM.SocialMedia.AddAndSaveTwitterProfileToContact('${screenName}', jq.getURLParam('id')); return false;">
                <%= CRMCommonResource.Choose %>
            </a>
        </td>
        <td class="sm_tbl_UserList_clmnAvatar">
            <div style="min-height: 40px;">
                <img src="${smallImageUrl}" width="40" alt=""/>
            </div>
        </td>
        <td class="sm_tbl_UserList_clmnUserName" style="padding:5px;">
            <span class="header-base-small sn_userName" style="color: Black !important;">${userName}</span>
            <br />
            ${description}
        </td>
    </tr>
</script>

<script id="socialMediaAvatarTmpl" type="text/x-jquery-tmpl">
    <div class="ImageHolderOuter" onclick="ASC.CRM.SocialMedia.UploadUserAvatar(event,'${socialNetwork}','${identity}');">
        <img src="${imageUrl}" alt="Avatar" class="AvatarImage" />
    </div>
</script>


<%--ContactFullCardView: contactInfo list --%>

<script id="collectionContainerTmpl" type="text/x-jquery-tmpl">
    <tr>
        <td class="describe-text" style="white-space:nowrap;">${Type}:</td>
        <td></td>
        <td class="collectionItemsTD"></td>
    </tr>
</script>

<script id="collectionTmpl" type="text/x-jquery-tmpl">
    {{if infoType == 0 }}
        <div class="collectionItem">
            {{if isPrimary == true}}
            <a href="callto:${data}?call" class="linkMedium">${data}</a>
            {{else}}
            <span>${data}</span>
            {{/if}}
            <span class="text-medium-describe"> (${categoryName})</span>
        </div>

    {{else infoType == 1}}
        <div class="collectionItem">
            <a href="mailto:${data}" class="linkMedium">${data}</a>
            <span class="text-medium-describe"> (${categoryName})</span>
            {{if isPrimary == true}}<a class="linkDescribe baseLinkAction writeEmail" data-email="${data}"><%= CRMContactResource.WriteEmail %></a>{{/if}}
        </div>

    {{else infoType == 3 || infoType == 15 || infoType == 16 }}
        <div class="collectionItem">
            <span>${data}</span>
            <span class="text-medium-describe"> (${categoryName})</span>
        </div>

    {{else infoType == 10  || infoType == 12 || infoType == 13}}
        <div class="collectionItem">
            <a href="mailto:${data}" class="linkMedium">${data}</a>
            <span class="text-medium-describe"> (${categoryName})</span>
        </div>
    {{else infoType == 2 || infoType == 4 || infoType == 5 || infoType == 6 ||infoType == 8 ||infoType == 9 || infoType == 11 || infoType == 14}}
        <div class="collectionItem">
            <a href="${href}" target="_blank" class="linkMedium">${data}</a>
            <span class="text-medium-describe"> (${categoryName})</span>
        </div>

    {{else infoType == 7}}
        <div class="collectionItem">
            {{html data}}
            <span class="text-medium-describe"> (${categoryName})</span><br/>
            <a style="text-decoration: underline;" href="${href}" target="_blank" class="linkMedium">
                <%= CRMContactResource.ShowOnMap %>
            </a>
        </div>
    {{/if}}
</script>

<%--ContactDetailsView: projects tab --%>

<script id="projectSelectorOptionTmpl" type="text/x-jquery-tmpl">
    <option value="${id}">${title}</option>
</script>

<script id="projectSelectorItemTmpl" type="text/x-jquery-tmpl">
   <li data-id="${id}"><div class="dropdown-item">${title}</div></li>
</script>

<%--ContactDetailsView: deals tab --%>

<script id="dealSelectorOptionTmpl" type="text/x-jquery-tmpl">
    <option value="${id}">${title}</option>
</script>

<script id="dealSelectorItemTmpl" type="text/x-jquery-tmpl">
   <li data-id="${id}"><div class="dropdown-item">${title}</div></li>
</script>

<%--ContactDetailsView: merge --%>

<script id="listContactsToMergeTmpl" type="text/x-jquery-tmpl">
    {{each contacts}}
    <li>
        <input type="radio" name="contactToMerge" value="${id}" />
        <span>${displayName}</span>
    </li>
    {{/each}}
    <li>
        <input type="radio" name="contactToMerge" value="0"{{if count == 0}} style="display: none;"{{/if}} />
        <div class="contactToMergeSelectorContainer"{{if count > 0}} style="margin-left: 0;"{{/if}}>
        </div>
        <input type="hidden" name="selectedContactToMergeID" value="0" />
    </li>
</script>

<script id="twitterMessageListTmpl" type="text/x-jquery-tmpl">
    <div class="sm_message_line clearFix">
        <div class="sn_user_icon">
            <img alt="" src="${userImageUrl}" width="32" />&nbsp;
        </div>
        <div class="sn_message_block longWordsBreak">
            <span class="header-base-small sn_userName">
                <img style="border-width:0px;" src="<%= WebImageSupplier.GetAbsoluteWebPath("twitter.png", ProductEntryPoint.ID) %>"" class="sn_small_img">
                ${userName}
            </span><br />
            <div class="sn_message_text">
                {{html htmlUtility.getFull(Encoder.htmlDecode(ASC.CRM.Common.convertText(text, true)))}}
            </div>
            <span class="describe-text">${postedOnDisplay}</span>
        </div>
    </div>
</script>

<script id="twitterMessageListPanelTmpl" type="text/x-jquery-tmpl">
    <div style="display:none;" class="infoPanel sm_messagesList_ErrorDescription" id="smErrorDescriptionContainer">
        <div id="smErrorDescription"></div>
    </div>
    <div class="clearFix"></div>
</script>