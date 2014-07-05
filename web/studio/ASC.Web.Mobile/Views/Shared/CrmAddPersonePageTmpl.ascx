<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-crm-addpersone" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-crm-addperson ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-tasks ui-btn-left ui-btn-row target-back" href="#crm"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content">
      <div class = "fields-container item-container" data = "name">                     
            <input class="persone-firstname" type="text" maxlength = 50 {{if contact != null}} value = "${contact.firstName}"{{else}} placeholder = "<%=Resources.MobileResource.CrmPersonFirsrName%>"{{/if}}/>        
            <input class="persone-secondname" type="text" maxlength = 50 {{if contact != null}} value = "${contact.lastName}"{{else}}placeholder = "<%=Resources.MobileResource.CrmPersonLastName%>"{{/if}}/>              
      </div>
      <div class = "fields-container item-container" data = "company">
            <!--<select class="company-name" type="text">
                <option value="-1"><%=Resources.MobileResource.LblSelectCompany%></option>
                {{each companies}}
                    {{if isCompany}}
                        <option value="${id}" >${displayName}</option>
                    {{/if}}
                {{/each}}                
            </select>                         
            <input class="company-name" type="text" maxlength = 100 placeholder = "Company"/>
            <button class="delete-field"></button>--> 
            {{if contact != null && contact.title != ""}}
                <input class="persone-position" type="text" maxlength = 100 value = "${contact.title}"/> 
            {{else}}             
                <input class="persone-position" type="text" maxlength = 100 placeholder = "<%=Resources.MobileResource.LblInputPosition%>"/> 
            {{/if}}                 
      </div>
      <div class = "item-container fields-container" data = "phone" id = "${contactTypes.phone.id}"> 
            {{if contact != null}}
            {{each contact.commonData}}
                {{if infoType == 0}}                    
                    <div class = "info"><select class="phone-type" type="text">
                    {{tmpl({categories: contactTypes.phone.categories, commonDataCategoryId: $value.category}) '#template-crm-datacategory'}}
                    </select>
                    <input class="contact-phone" type="text" maxlength = 50 value = "${data}"/>
                    <button class="delete-field"></button>
                    </div>                    
                {{/if}}
            {{/each}}
          {{/if}}                     
            <div class = "info"><select class="phone-type" type="text">
                {{each contactTypes.phone.categories}}
                    <option value="${id}">${title}</option>
                {{/each}}                 
            </select>
            <input class="wke contact-phone" type="text" maxlength = 50 placeholder = "<%=Resources.MobileResource.LblInputPhone%>"/>            
            </div>
                                 
      </div>
      <div class = "fields-container item-container" data = "email" id = "${contactTypes.email.id}"> 
            {{if contact != null}}
                {{each contact.commonData}}
                    {{if infoType == 1}}
                    <div class = "info"><select class="email-type" type="text">
                        {{tmpl({categories: contactTypes.email.categories, commonDataCategoryId: $value.category}) '#template-crm-datacategory'}}
                        </select>
                        <input class="contact-email" type="text" maxlength = 100 value = "${data}"/>
                        <button class="delete-field"></button>
                        </div>
                    {{/if}}
                {{/each}}
            {{/if}}                  
            <div class = "info"><select class="email-type" type="text">
                {{each contactTypes.email.categories}}
                    <option value="${id}">${title}</option>
                {{/each}}              
            </select>
            <input class="wke contact-email" type="text" maxlength = 100 placeholder = "<%=Resources.MobileResource.LblInputEmail%>"/></div>                
      </div>
      <div class = "fields-container item-container" data = "address"> 
            {{if contact != null && contact.addresses.length > 0}}
                {{each contact.addresses}}                    
                    <div class = "info">
                        <select class="address-type" type="text">
                           {{tmpl({categories: contactTypes.address.categories, commonDataCategoryId: $value.category}) '#template-crm-datacategory'}}           
                        </select>
                        <input class="contact-address detail" type="text" maxlength = 100 value = "${country}"/>
                        <button class="delete-field"></button>
                        <input class="contact-address city" type="text" maxlength = 100 value = "${city}"/>
                        <input class="contact-address state" type="text" maxlength = 100 value = "${state}"/>
                        <input class="contact-address street" type="text" maxlength = 100 value = "${street}"/>
                        <input class="contact-address zipcode" type="text" maxlength = 100 value = "${zip}"/>
                       
                    </div>
                    <div class = "separator"></div>
                {{/each}}
            {{/if}}                  
            <div class = "info">
            <select class="address-type" type="text">
                {{each contactTypes.address.categories}}
                    <option value="${id}">${title}</option>
                {{/each}}              
            </select>
            <input class="wke contact-address detail" type="text" maxlength = 100 placeholder = "<%=Resources.MobileResource.CrmAddressCountry%>"/>
            <input class="contact-address city" type="text" maxlength = 100 placeholder = "<%=Resources.MobileResource.CrmAddressCity%>"/>
            <input class="contact-address state" type="text" maxlength = 100 placeholder = "<%=Resources.MobileResource.CrmAddressState%>"/>
            <input class="contact-address street" type="text" maxlength = 100 placeholder = "<%=Resources.MobileResource.CrmAddressStreet%>"/>
            <input class="contact-address zipcode" type="text" maxlength = 100 placeholder = "<%=Resources.MobileResource.CrmAddressZipcode%>"/></div>               
      </div>
      <div class = "fields-container item-container" data = "site" id = "${contactTypes.website.id}"> 
            {{if contact != null}}
                {{each contact.commonData}}
                    {{if infoType == 2}}
                        <div class = "info"><input class="contact-site" type="text" value = "${data}"/>
                        <button class="delete-field"></button>
                        </div>
                    {{/if}}
                {{/each}}
            {{/if}}                       
            <div class = "info"><input class="wke contact-site" type="text" placeholder = "<%=Resources.MobileResource.LblInputWebSite%>"/></div>            
      </div>
      <div class="item-container persone-item-container add-crm-persone">
      {{if contact != null}}
        <button class="create-item add-crm-persone edit" data-id = ${contact.id} {{if contact.company != null}}data-contactid = ${contact.company.id}{{/if}}><%=Resources.MobileResource.BtnCrmEditPerson%></button>
      {{else}}
        <button class="create-item add-crm-persone" {{if id != null && id != undefined}}data-id = "${id}"{{/if}}><%=Resources.MobileResource.BtnCrmAddPerson%></button>
      {{/if}}             
        <!--<div><button class="create-item cansel-crm-contact"><%=Resources.MobileResource.BtnCancel%></button></div>-->
      </div>
    </div>
  </div>
</div>
</script>