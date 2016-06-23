<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>


<script id="contactInfoCardResourcesTmpl" type="text/x-jquery-tmpl">
    <div id="contactInfoCardResources" class="display-none"
        data-one="<%= CRMJSResource.OneContact %>"
        data-no="<%= CRMJSResource.NoContacts %>"
        data-many="<%= CRMJSResource.ManyContacts %>"></div>
</script>


<script id="contactInfoCardTmpl" type="text/x-jquery-tmpl">
<div class="crm-contactInfoCard" id="contactInfoCard_${id}">
    {{if isPrivateForMe == false}}
        <a href="${contactLink}">
            {{if isCompany == true}}
            <img class="crm-contactInfoCardImg" border="0" style="margin: 0 auto;" src="<%=ContactPhotoManager.GetMediumSizePhoto(0, true) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${mediumFotoUrl}');" />
            {{else}}
            <img class="crm-contactInfoCardImg" border="0" style="margin: 0 auto;" src="<%=ContactPhotoManager.GetMediumSizePhoto(0, false) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${mediumFotoUrl}');" />
            {{/if}}
            <img class="crm-contactInfoCardImg" border="0" style="margin: 0 auto;display:none;" alt="${displayName}" title="${displayName}"/>
        </a>
    {{else}}
        {{if isCompany == true}}
        <img class="crm-contactInfoCardImg" border="0" style="margin: 0 auto 10px; float: left;" src="<%=ContactPhotoManager.GetMediumSizePhoto(0, true) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${mediumFotoUrl}');" />
        {{else}}
        <img class="crm-contactInfoCardImg" border="0" style="margin: 0 auto 10px; float: left;" src="<%=ContactPhotoManager.GetMediumSizePhoto(0, false) %>" alt="${displayName}" title="${displayName}" onload="ASC.CRM.Common.loadContactFoto(jq(this), jq(this).next(), '${mediumFotoUrl}');" />
        {{/if}}
        <img class="crm-contactInfoCardImg" border="0" style="margin: 0 auto 10px; float: left;display:none;" alt="${displayName}" title="${displayName}"/>
    {{/if}}

    <div class="infoCardContent">
        {{if isPrivateForMe == true}}
            <label class="crm-private-lock"></label>
            <span class="crm-task-title header-base-medium">${displayName}</span>
        {{else}}
            <a title="${displayName}" class="linkHeader" style="display: inline-block;
                margin-bottom: 6px;text-decoration: underline;" href="${contactLink}">
                ${displayName}
            </a>

        {{if isCompany == true && typeof(personsCount) != "undefined"}}
            <div class="personsCountString" style="margin-bottom: 6px;" data-one="<%= CRMJSResource.OneContact %>" data-no="<%= CRMJSResource.NoContacts %>" data-many="<%= CRMJSResource.ManyContacts %>">
                {{if personsCount == 0}}
                    ${jq("#contactInfoCardResources").attr("data-no")}
                {{else personsCount == 1}}
                    1 ${jq("#contactInfoCardResources").attr("data-one")}
                {{else}}
                    ${personsCount} ${jq("#contactInfoCardResources").attr("data-many")}
                {{/if}}
            </div>
        {{else}}
            <div style="margin-bottom: 6px;">${title}</div>
        {{/if}}

            <ul>
                {{each(i, item) commonData}}
                    {{if item.infoType == 1 && item.isPrimary}}
                        <li>
                            <div class="crm-email">
                                <a class="linkMedium" href="mailto:${item.data}">
                                    ${item.data}
                                </a>&nbsp;<span class="text-medium-describe"> (${item.categoryName})</span>
                            </div>
                        </li>
                    {{/if}}
                    {{if item.infoType == 0 && item.isPrimary}}
                        <li>
                            <div class="crm-phone">
                                ${item.data}&nbsp;<span class="text-medium-describe"> (${item.categoryName})</span>
                            </div>
                        </li>
                    {{/if}}
                {{/each}}

                {{if typeof (primaryAddress) != "undefined" && primaryAddress != null}}
                    <li>
                        <div class="crm-address">{{html primaryAddress.Data}}<span class="text-medium-describe"> (${primaryAddress.categoryName})</span>
                            <br/>
                            <a class="linkMedium" style="text-decoration: underline;" href="${primaryAddress.Href}" target="_blank">
                                <%= CRMContactResource.ShowOnMap %>
                            </a>
                        </div>
                    </li>
                {{/if}}
            </ul>
        {{/if}}
    </div>
</div>
</script>