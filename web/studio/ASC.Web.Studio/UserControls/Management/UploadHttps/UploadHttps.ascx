<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UploadHttps.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.UploadHttps" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="Resources" %>
<div class="clearFix">
  <div class="settings-block">
    <div class="header-base clearFix">
        <div class="title">
            <%= Resource.UploadHttpsSettingsTitle %>
        </div>
    </div>
    <div>
        <div class="clearFix">
        </div>
        <div class="requiredField">
            <span class="requiredErrorText"><%= Resource.ErrorPasswordEmpty %></span>
            <div class="headerPanelSmall header-base-small headertitle"> <%= Resource.Password + ":" %></div>
            <input class="timeandlangText" type="password" id="certPwd"/>
        </div>
        <div id="statusContainer" class="display-none"></div>
        <div class="middle-button-container">
            <a id="certUploader" class="link dotline plus" ><%= Resource.UploadButton %></a>
            <div id="certFile">
                <span class="name"></span>
                <span class="menu-item-icon trash delete-btn"></span>
            </div>

        </div>
        <div class="middle-button-container">
            <a id="save" class="button blue middle disable" ><%= Resource.SaveButton %></a>
        </div>
    </div>
  </div>
  <div class="settings-help-block">
      <p><%= String.Format(Resource.UploadHttpsHelp.HtmlEncode(), "<b>", "</b>") %></p>
 </div>  
</div>

<div id="checkAttachmentContainer" style="display: none">
    <sc:Container ID="_checkAttachmentContainer" runat="server">
        <Header>
            <%= Resource.UploadHttpsSettingsHeader %>
        </Header>
        <Body>
            <div>
                <p>
                    <%= Resource.UploadHttpsSettingsError %>
                </p>
            </div>
            <div class="middle-button-container">
                <a class="button blue middle" id="okButton">
                    <%= Resource.OKButton%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle"  id="cancelButton">
                    <%= Resource.CancelButton%>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>