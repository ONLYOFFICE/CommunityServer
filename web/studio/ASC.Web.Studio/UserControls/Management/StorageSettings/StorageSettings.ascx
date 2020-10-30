<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StorageSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.StorageSettings.StorageSettings" %>

<script id="storageSettingsBlockTemplate" type="text/x-jquery-tmpl">
    <div id="${id}">
        <div class="auth-service display-none">
            <div class="header-base">${title}</div>
            <br />
            <span class="link dotline"></span>
        </div>
        <div class="auth-data"></div>
    </div>
</script>

<script id="storageSettingsTemplate" type="text/x-jquery-tmpl">
<div class="storageItem">
    <div id="prop${title}">
        {{each(i, prop) properties}} 
            <div class="bold headerPanelSmall">${prop.title}:</div>
            {{if !isSet || current}}
                <input id="${prop.name}" type="text" class="storageKey disabled textEdit" value="${prop.value}" />
            {{else}}
                <input id="${prop.name}" type="text" class="storageKey textEdit" value="${prop.value}" />
            {{/if}} 
        {{/each}}
    </div>
    <div class="small-button-container">
        {{if !current}}
            <a id="saveBtn${title}" class="button blue {{if !isSet}} disable {{/if}} middle saveButton">${ASC.Resources.Master.Resource.StorageButtonEnable}</a>
        {{else}}
            <a id="setDefault${title}" class="button blue middle saveButton">${ASC.Resources.Master.Resource.StorageButtonResetToDefault}</a>
        {{/if}}
    </div>
</div>

</script>

<div id="storageContainer">
    <div class="header-base"><%= Resources.Resource.StorageTitle %></div>

    <p class="auth-service-text">
        <%: Resources.Resource.StorageText %>
        <br />
    </p>
    <div class="storageBlock clearFix">
    </div>
</div>
