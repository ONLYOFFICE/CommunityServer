<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MoreFeatures.ascx.cs" Inherits="ASC.Web.Files.Controls.MoreFeatures" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<div id="moreFeatures" class="more-features">
    <h2 class="more-features_title"><%: FilesUCResource.MoreFeaturesTitle %></h2>
    <div class="more-features_block">
        <hr />
        <p><%= String.Format(FilesUCResource.MoreFeaturesText.HtmlEncode(), "<br /><b>", "</b>") %></p>
        <hr />
     </div>
    <div class="more-features_add">
        <p class="more-features_add_text"><%: FilesUCResource.MoreFeaturesTextList %></p>
        <ul class="more-features_add_list">
            <li><%: FilesUCResource.MoreFeaturesItem1 %></li>
            <li><%: FilesUCResource.MoreFeaturesItem2 %></li>
            <li><%: FilesUCResource.MoreFeaturesItem3 %></li>
            <li><%: FilesUCResource.MoreFeaturesItem4 %></li>
        </ul>
    </div>
    <a class="button green huge" target="_blank" href="<%= "http://www.onlyoffice.com/" + Lng + "/registration.aspx?from=personal"%>"><%= FilesUCResource.GetGreeAccountBtn %></a>
</div>
