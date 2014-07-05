<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Teamlab API
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row show-grid">
        <div class="span8">           
            <h2>Create Web widgets</h2>
            <img src="<%=Url.Content("~/Content/img/widgets.png")%>" />
            <p class="describe">Read this section to learn about the main terms and concepts used in the TeamLab API, find answers to the most frequently asked questions and more.</p>
        </div>
        <div class="span8">
            <h2>Build Mobile app</h2>
            <img src="<%=Url.Content("~/Content/img/mobile.png")%>" />
            <p class="describe">If you are not sure what to look for or do not know the exact method name, you can browse the whole list of methods to find a necessary one.</p>
            
        </div>
        <div class="span8">
            <h2>Integrate TeamLab in Web site</h2>
            <img src="<%=Url.Content("~/Content/img/website.png")%>" />
            <p class="describe">If you know the exact method name or a part of its name or description, you can try and find it using the site search.</p>
        </div>
        <div class="span8">
            <h2>Export/Import TeamLab data</h2>
            <img src="<%=Url.Content("~/Content/img/data.png")%>" />            
            <p class="describe">If you know the exact method name or a part of its name or description, you can try and find it using the site search.</p>
        </div>  
          <div class="span16">And do not forget the portal developers <a href="http://www.developers.teamlab.com" target="_blank">www.developers.teamlab.com</a></div>

    </div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder">
    <script type="text/javascript">$(".dashboard").equalHeights(".describe");</script>
</asp:Content>