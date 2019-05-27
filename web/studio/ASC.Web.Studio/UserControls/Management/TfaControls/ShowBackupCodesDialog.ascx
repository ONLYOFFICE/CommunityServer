<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ShowBackupCodesDialog.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ShowBackupCodesDialog" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_showBackupCodesDialog" style="display: none;">
    <sc:Container runat="server" ID="_showBackupCodesContainer">
        <Header>
            <%= Resource.TfaAppShowBackupCodesTitle %>
        </Header>
        <Body>
            <div id="showBackupCodesContent">
            </div>
        </Body>
    </sc:Container>
</div>
