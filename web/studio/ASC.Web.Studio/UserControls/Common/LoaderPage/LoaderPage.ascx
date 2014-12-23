<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoaderPage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.LoaderPage.LoaderPage" %>
<%@ Import Namespace="Resources" %>

 <div class="loader-page">
        <div class="romb blue"></div>
        <div class="romb green"></div>
        <div class="romb red"></div>
        <div class="text"><%= Resource.LoadingProcessing %></div>
 </div>