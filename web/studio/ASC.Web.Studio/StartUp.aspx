<%@ Page MasterPageFile="~/Masters/basetemplate.master" Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="StartUp.aspx.cs" Inherits="ASC.Web.Studio.StartUp" %>

<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="Resources" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="server">
    <div class="startup-loading-cnt">
        <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
        <div class="startup-loading-cnt_logo"><strong><%= TenantName %></strong></div>
        <p class="startup-loading-cnt_text"><%= Resource.StartUpWaitText %></p>
        <div id="progressbar_container" class="startup-loading-cnt_progress">
            <div id="progress-line">
                <div class="asc-progress-wrapper">
                    <div class="asc-progress-value"></div>
                </div>
                <span class="asc-progress_percent"></span>
            </div>
            <div id="progress-error"></div>
        </div>
    </div>
</asp:Content>
