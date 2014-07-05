<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-crm-company" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-crm-item page-crm-company ui-header ui-footer ui-fixed-footer">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-dialog" href="#crm/navigate"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-addperson ui-btn-right ui-btn-no-text target-dialog" href="#crm/contact/${id}/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-scroller">        
      <div class="ui-item-title ui-company-title">
        <div class="item-avatar"><img src=${item.smallFotoUrl}></img></div>
        <div class="item-content">
            <span class="item-name">${item.displayName}</span>
            {{if phone != null}}
                <a href="tel:${phone}"><span class = "item-icon phone"></span></a>
            {{else}}
                <a class="add-phone target-self" href = "#crm/person/${id}/edit"><span class = "item-icon add-phone"></span></a>
            {{/if}}
            {{if email != null}}
                <a href="mailto:${email}"><span class = "item-icon email"></span></a>
            {{else}}
                <a class="add-email target-self" href = "#crm/person/${id}/edit"><span class = "item-icon add-email"></span></a>
            {{/if}}
            <a href="#crm/contact/${id}/add/historyevent"><span class = "item-icon addhistory"></span></a>
            <a href="#crm/contact/${id}/add/task"><span class = "item-icon addtask"></span></a>       
        </div>      
      </div>
    <div class="ui-item-content ui-company-content">        
        {{if item.commonData.length > 0}}
            <div class="company-phone">  
                 {{each item.commonData}}                    
                         {{if infoType == 0}}
                            <div class="item-category {{if isPrimary}}primary{{/if}}"><div class = "category-text">${category_title}</div></div>
                            <div class="item-content contact-data">
                                <span class="item-phone">${data}</span>
                            </div>    
                         {{/if}}                
                 {{/each}}     
            </div>           
        {{/if}}
        {{if item.commonData.length > 0}}
            <div class="company-email"> 
                {{each item.commonData}}                    
                     {{if infoType == 1}}
                        <div class="item-category {{if isPrimary}}primary{{/if}}"><div class = "category-text">${category_title}</div></div>                        
                        <div class="item-content contact-data">                            
                            <span class="item-email">${data}</span>
                        </div>
                     {{/if}}            
                {{/each}}
             </div>   
        {{/if}}
        {{if item.addresses.length > 0}}
            <div class="company-addresses">
                {{each item.addresses}}
                     <div class="item-category {{if isPrimary}}primary{{/if}}"><div class = "category-text"t>${categoryName}</div></div>
                     <div class="item-content contact-data">
                         {{if country}}<span class="item-address"><%=Resources.MobileResource.CrmAddressCountry%>: ${country}</span>{{/if}}
                         {{if city}}<span class="item-address"><%=Resources.MobileResource.CrmAddressCity%>: ${city}</span>{{/if}}
                         {{if state}}<span class="item-address"><%=Resources.MobileResource.CrmAddressState%>: ${state}</span>{{/if}}
                         {{if street}}<span class="item-address"><%=Resources.MobileResource.CrmAddressStreet%>: ${street}</span>{{/if}}
                         {{if zip}}<span class="item-address"><%=Resources.MobileResource.CrmAddressZipcode%>: ${zip}</span>{{/if}}
                         <a class="add-map target-blank" href="http://maps.google.com/maps?q=${street}, ${city}, ${state}, ${zip}, ${country}"><span class = "item-icon add-map"></span></a>                                   
                     </div> 
                 {{/each}}
            </div>            
        {{/if}}
        {{if item.commonData.length > 0}}
            <div class="company-site"> 
                {{each item.commonData}}                    
                    {{if infoType == 2}}
                        <div class="item-category"><div class = "category-text">${category_title}</div></div>
                        <div class="item-content contact-data">
                            <span class="item-site">${data}</span>
                         </div>
                    {{/if}}             
                 {{/each}}
            </div>                   
        {{/if}}
        <div class="item-container crm-item-container edit-contact">
            <a class="ui-btn create-item edit-contact" href = "#crm/company/${id}/edit" data-id = "${item.id}"><%=Resources.MobileResource.BtnCrmEditContact%></a>
        </div>
    </div>
  </div>
  </div>
  <div class="ui-footer">
    <div class="ui-navbar">
           <ul class="ui-grid ui-grid-5 nav-menu main-menu">
            <li class="ui-block filter-item info current-filter">
              <a class="nav-menu-item target-self" href="#crm/contact/${id}">
                <span class="item-icon"></span>
                <span class="inner-text"><%=Resources.MobileResource.BtnInfo%></span>
              </a>
            </li>
            <li class="ui-block filter-item history">
              <a class="nav-menu-item target-self" href="#crm/contact/${id}/history" data-id="${id}">
                <span class="item-icon"></span>
                <span class="inner-text"><%=Resources.MobileResource.BtnHistory%></span>
              </a>
            </li>
            <li class="ui-block filter-item tasks">
              <a class="nav-menu-item target-self" href="#crm/contact/${id}/tasks" data-id="${id}">
                <span class="item-icon"></span>
                <span class="inner-text"><%=Resources.MobileResource.BtnTasks%></span>
              </a>
            </li>
            <li class="ui-block filter-item persons">
              <a class="nav-menu-item target-self" href="#crm/contact/${id}/persones" data-id="${id}">
                <span class="item-icon"></span>
                <span class="inner-text"><%=Resources.MobileResource.BtnPersons%></span>
              </a>
            </li>
            <li class="ui-block filter-item files">
              <a class="nav-menu-item target-self" href="#crm/contact/${id}/files" data-id="${id}">
                <span class="item-icon"></span>
                <span class="inner-text"><%=Resources.MobileResource.BtnFiles%></span>
              </a>
            </li>
          </ul>     
    </div>
  </div>
</div>
</script>