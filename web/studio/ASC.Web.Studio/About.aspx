<%@ Page MasterPageFile="~/Masters/basetemplate.master" Language="C#" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="ASC.Web.Studio.About" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="server">
    <div class="personal-page-header"><h1><b><%= Resource.AboutTitle %></b></h1></div>
    <div class="personal-page-container __about">
        <h2><%: Resource.AboutPersonalWhatIsItHeader %></h2>
        <p><%: Resource.AboutPersonalWhatIsItText %></p>

        <h2><%: Resource.AboutPersonalTheBestHeader %></h2>
        <ul>
            <li><%= String.Format(Resource.AboutPersonalTheBestText1.HtmlEncode(), "<b>", "</b>") %></li>
            <li><%= String.Format(Resource.AboutPersonalTheBestText2.HtmlEncode(), "<b>", "</b>")%></li>
        </ul>

        <h2><%: Resource.AboutPersonalCompareText %></h2>
        <p>
            <iframe width="560" height="315" src="//www.youtube.com/embed/0S0Op2MbLvw" frameborder="0" allowfullscreen></iframe>
        </p>

        <h2><%: Resource.AboutPersonalEnableHeader %></h2>
        <ul><%= String.Format(Resource.AboutPersonalEnableText.HtmlEncode(), "<li>", "</li>") %></ul>

        <h2><%: Resource.AboutPersonalUseHeader %></h2>
        <p><%: Resource.AboutPersonalUseText %></p>

        <p><%= String.Format(Resource.AboutPersonalMoreInfoText.HtmlEncode(), "<a target=\"_blank\" href=\"http://www.onlyoffice.com/\">", "</a>") %></p>

        <h2><%: Resource.AboutPersonalQuestionHeader %></h2>
        <p><%= String.Format(Resource.AboutPersonalQuestionText.HtmlEncode(), 
           "<a href=\"mailto:personal@onlyoffice.com\">", "<a target=\"_blank\" href=\"https://www.facebook.com/OnlyOffice-833032526736775/\">","</a>") %></p>
    </div>
     <asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>
</asp:Content>