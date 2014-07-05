<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-person" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-people-item ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-left ui-btn-row target-back" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <%-- <div class="ui-page-title">
      <div class="text-field-wrapper"><label class="search-label" for="txtSearchValue"></label><input class="input-text search-field top-search-field" type="text" value="<%Resources.MobileResource.LblSearch%>" title="<%Resources.MobileResource.LblSearch%>" onblur="if(this.value=='')this.value = this.title" onfocus="if(this.value==this.title)this.value=''" /></div>
      <span class="title-text"><img class="product-icon" src="<%=Url.Content("~/content/images/icon-people.svg")%>" alt="${title}" /><span>${title}</span></span>
    </div> --%>
    <div class="ui-item-title">
      <img class="ui-person-avatar" src="${$data.item.avatar}" alt="${$data.item.displayName}" />
      <span class="item-title persone-title">
        <span class="item-title-field persone-displayname">${$data.item.displayName}</span>
        {{if $data.item.groups.length > 0}}
          <span class="item-title-field persone-group">${$data.item.groups[0].name}</span>
        {{/if}}
        <span class="item-title-field persone-title">${$data.item.title}</span>
      </span>
    </div>
    <div class="ui-item-content ui-person-content">
      <div class="ui-person-information">
        <table class="person-info">
          {{if item.contacts.mailboxes.length > 0}}
            <tr class="item-title item-title-mailboxes">
              <td class="ui-title" colspan="2"><span><%=Resources.MobileResource.LblEmailAddresses%></span></td>
            </tr>
            {{each item.contacts.mailboxes}}
              <tr class="item-field item-field-mailboxes">
                <td class="ui-field field-${$value.name}">
                  <div class="field-container">
                    <span>${$value.label}:</span>
                  </div>
                </td>
                <td class="ui-value value-${$value.name}">
                  <div class="field-container">
                    <a class="target-top change-page-none" href="mailto:${$value.val}">${$value.val}</a>
                  </div>
                </td>
              </tr>
            {{/each}}
          {{/if}}
          {{if item.contacts.telephones.length > 0}}
            <tr class="item-title item-title-telephones">
              <td class="ui-title" colspan="2"><span><%=Resources.MobileResource.LblPhoneNumbers%></span></td>
            </tr>
            {{each item.contacts.telephones}}
              <tr class="item-field item-field-telephones">
                <td class="ui-field field-${$value.name}">
                  <div class="field-container">
                    <span>${$value.label}</span>
                  </div>
                </td>
                <td class="ui-value value-${$value.name}">
                  <div class="field-container">
                    <a class="target-top change-page-none" href="tel:${$value.val}">${$value.val}</a>
                  </div>
                </td>
              </tr>
            {{/each}}
          {{/if}}
          {{if item.contacts.links.length > 0}}
            <tr class="item-title item-title-links">
              <td class="ui-title" colspan="2"><span><%=Resources.MobileResource.LblInformation%></span></td>
            </tr>
            {{each item.contacts.links}}
              <tr class="item-field item-field-links">
                <td class="ui-field field-${$value.name}">
                  <div class="field-container">
                    <div class="link-type link-type-${$value.name}"></div>
                    <span>${$value.label}</span>
                  </div>
                </td>
                <td class="ui-value value-${$value.name}">
                  <div class="field-container">
                    {{if $value.val}}
                      <a class="{{if $value.istop}}target-top{{else}}target-blank{{/if}} change-page-none" href="${$value.val}">${$value.title}</a>
                    {{else}}
                      <span>${$value.title}</span>
                    {{/if}}
                  </div>
                </td>
              </tr>
            {{/each}}
          {{/if}}
        </table>
      </div>
    </div>
  </div>
</div>
</script>