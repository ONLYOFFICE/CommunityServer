<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>

<script id="simpleContactTmpl" type="text/x-jquery-tmpl">
    <tr id="contactItem_${id}" {{if typeof showActionMenu !== "undefined" && showActionMenu === true}}class="with-entity-menu"{{/if}}>
        <td class="borderBase" style="width:40px;">
            <div class="contactItemPhotoImgContainer{{if isShared === true}} sharedContact{{/if}}">
                {{if isCompany == true}}
                <img class="contactItemPhotoImg" src="<%=ContactPhotoManager.GetSmallSizePhoto(0, true) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${smallFotoUrl}');" />
                {{else}}
                <img class="contactItemPhotoImg" src="<%=ContactPhotoManager.GetSmallSizePhoto(0, false) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${smallFotoUrl}');" />
                {{/if}}
                <img class="contactItemPhotoImg" style="display:none;" alt="${displayName}" title="${displayName}"/>
            </div>
        </td>
        <td class="borderBase main-info-contact" style="width:50%;">
            <div class="contactTitle">
                {{if typeof(id)=="number"}}
                <a class="linkHeaderMedium" href="${contactLink}">${displayName}</a>
                {{else}}
                <span class="header-base-small">${displayName}</span>
                {{/if}}
            </div>
            {{if isCompany == false && typeof (company) != "undefined" && company != null && showCompanyLink}}
                <div class="contactTitle">
                    <%=CRMContactResource.Company%>:
                    <a href="${company.contactLink}" data-id="${company.id}" id="contact_${id}_company_${company.id}" class="linkMedium crm-companyInfoCardLink">
                        ${company.displayName}
                    </a>
                </div>
            {{/if}}
        </td>

         <td class="borderBase contact-info-contact">
            <div class="primaryDataContainer">
                {{each(i, item) commonData}}
                    {{if item.infoType == 0 && item.isPrimary}}
                        <span title="${item.data}">${item.data}</span>
                    {{/if}}
                {{/each}}
            </div>
            <div class="primaryDataContainer">
                {{each(i, item) commonData}}
                    {{if item.infoType == 1 && item.isPrimary}}
                        <a title="${item.data}" href="mailto:${item.data}" class="linkMedium">${item.data}</a>
                    {{/if}}
                {{/each}}
            </div>
        </td>
        <td class="borderBase" style="width:25px;">
            {{if typeof showActionMenu !== "undefined" && showActionMenu === true}}
                <div title="<%= CRMCommonResource.Actions %>" class="entity-menu" id="simpleContactMenu_${id}"
                    data-displayName="${displayName}" 
                    data-email="{{if primaryEmail !== null}}${primaryEmail.data}{{/if}}"
                ></div>
            {{else typeof showUnlinkBtn !== "undefined" && showUnlinkBtn === true}}
            <img src="<%=WebImageSupplier.GetAbsoluteWebPath("unlink_16.png")%>"
               class="contact-info-link-img"
               title="<%= CRMCommonResource.UnlinkContact %>"
               onclick='ASC.CRM.ListContactView.removeMember({{if typeof(id)=="number"}}${id}{{else}}"${id}"{{/if}});'
               id="trashImg_${id}" />
            <div id="loaderImg_${id}" class="loader-middle baseList_loaderImg"></div>
            {{/if}}
        </td>
    </tr>
</script>

<script id="simpleContactActionMenuTmpl" type="text/x-jquery-tmpl">
    <div id="simpleContactActionMenu" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a class="unlinkContact dropdown-item"><%= CRMCommonResource.UnlinkContact %></a></li>
            <%--<li><a class="writeEmail dropdown-item"><%= CRMContactResource.WriteEmail %></a></li>
            <li><a class="viewMailingHistory dropdown-item"><%= CRMCommonResource.ViewMailingHistoryWithParticipant %></a></li>--%>
        </ul>
    </div>
</script>