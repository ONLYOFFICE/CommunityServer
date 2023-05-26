<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StorageSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.StorageSettings.StorageSettings" %>

<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

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

    {{if id == "S3"}}
    {{tmpl($data) "consumerItemS3Tmpl"}}
    {{else}}
    {{tmpl($data) "consumerItemBaseTmpl"}}
    {{/if}}

    <div class="small-button-container">
        {{if !current}}
            <a id="saveBtn${id}" class="button blue {{if !isSet}} disable {{/if}} middle saveButton">${ASC.Resources.Master.ResourceJS.StorageButtonEnable}</a>
        {{else}}
            <a id="setDefault${id}" class="button blue middle saveButton">${ASC.Resources.Master.ResourceJS.StorageButtonResetToDefault}</a>
        {{/if}}
    </div>
</div>

</script>

<div id="storageContainer">
    <div class="header-base"><%= Resource.StorageTitle %></div>

    <p class="auth-service-text">
        <%: Resource.StorageText %>
        <br />
    </p>
    <div class="storageBlock clearFix">
    </div>
</div>
