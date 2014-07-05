<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-files" type="text/x-jquery-tmpl" async="true">
{{each items}}
  <li class="item ${$value.classname}" data-itemid="${$value.id}">
    <div class="item-state"><div class="item-icon"></div></div>
    <a class="title{{if $value.target == 'none'}} target-none{{else $value.target == 'top'}} ui-item-link target-top{{else $value.target == 'blank'}} ui-item-link-none-active-item target-blank{{else}} ui-item-link target-self{{/if}}" href="${$value.href}">
      {{if $value.type === 'file'}}
        <span class="inner-text">
          <span class="itemtitle-helper filetitle-helper">
            <span class="itemtitle filetitle">
              <span class="itemname filename">${$value.filename}</span>
              <span class="fileextension-wrapper"><span class="fileextension">${$value.extension}</span></span>
            </span>
          </span>
          <span class="item-info">
            <span class="item-size">${$value.contentLength}</span>
            <span class="item-separator">|</span>
            <span class="item-version"><%=Resources.MobileResource.LblFileVersion%>&nbsp;${$value.version}</span>
          </span>
        </span>
      {{else $value.type === 'folder'}}
        <span class="inner-text">
          <span class="itemtitle-helper folderstitle-helper">
            <span class="itemtitle folderstitle">
              <span class="itemname foldername">${$value.title}</span>
            </span>
          </span>
          <span class="item-info">
            <span class="item-version"><%=Resources.MobileResource.LblFolders%>&nbsp;${$value.foldersCount}</span>
            <span class="item-separator">|</span>
            <span class="item-size"><%=Resources.MobileResource.LblFiles%>&nbsp;${$value.filesCount}</span>
          </span>
        </span>
      {{else}}
        <span class="inner-text">${$value.title}</span>
      {{/if}}
    </a>
    <div class="sub-info">
      <span class="timestamp">
        <span class="date">${$value.displayUptdate}</span>
      </span>
      <span class="author">${$value.createdBy.displayName}</span>
    </div>
  </li>
{{/each}}
</script>