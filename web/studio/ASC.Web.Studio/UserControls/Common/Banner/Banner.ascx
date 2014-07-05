<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Banner.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Banner.Banner" %>

<% foreach (var banner in banners)
   {%>
<a id="<%= banner.Id %>" class="banner-registration" href="<%= banner.Url %>" title="<%= banner.Title %>" target="blank">
    <img alt="<%= banner.Title %>" src="<%= banner.ImgUrl %>" />
</a>
<span id="errorAffilliateBanner" class="errorText"></span>
<%} %>