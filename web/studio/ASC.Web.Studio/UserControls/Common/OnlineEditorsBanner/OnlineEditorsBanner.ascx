<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OnlineEditorsBanner.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.OnlineEditorsBanner.OnlineEditorsBanner" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>


<div class="backdrop" blank-page=""></div>
<div class="files-banner">
    <div class="files-banner_bookmark"><span><%= Resource.FilesBannerGoodNews %></span></div>
    <h3 class="files-banner_title"><%= Resource.FilesBannerHeader1 %></h3>
    <p class="files-banner_title-list"><%= Resource.FilesBannerHeader2 %></p>
    <ul class="files-banner_list clearFix">
        <li><%= String.Format(Resource.FilesBannerItem1, "<br />") %></li>        
        <li><%= String.Format(Resource.FilesBannerItem2, "<br />") %></li>
        <li><%= String.Format(Resource.FilesBannerItem3, "<br />") %></li>
        <li><%= String.Format(Resource.FilesBannerItem4, "<br />") %></li>
    </ul>
    <p class="files-banner_more"><%= Resource.FilesBannerItemMore %></p>
    <p class="files-banner_text"><strong><%= Resource.FilesBannerText %></strong></p>
    <div class="files-banner_btns">
        <a class="button blue huge files-banner_btn __ok"><%= Resource.FilesBannerBtnOk %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray huge files-banner_btn __cancel"><%= Resource.FilesBannerBtnCancel %></a>
    </div>
    <p class="files-banner_note">
        <strong><%= Resource.Note %>: </strong>
        <%= CoreContext.Configuration.Personal ? Resource.FilesBannerNotePersonal : Resource.FilesBannerNote %>
    </p>

</div>
