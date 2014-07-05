<%@ Control Language="C#" AutoEventWireup="true" %>

<script id="accountsPanelTmpl" type="text/x-jquery-tmpl">
  <li class="menu-item none-sub-list">
    <a id="${id}"{{if marked}} class="tag tagArrow"{{/if}}><span class="link dotted">${email}</span></a>
  </li>
</script>