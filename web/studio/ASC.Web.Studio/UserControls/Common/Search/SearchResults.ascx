<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchResults.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Search.SearchResults" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Search" %>

<asp:Repeater ID="results" runat="server">
    <ItemTemplate>
        <div class="search-results-block clearFix">
            <div class="header-base header-search-results" onclick="SearchResults.Toggle('<%# Container.FindControl("resultItems").ClientID %>','btnToggleNavigator_<%# Container.ItemIndex %>')">
                <img class="logoUrl" align="absmiddle" alt="<%# ((ASC.Web.Studio.Core.Search.SearchResult)Container.DataItem).Name %>" src="<%# ((ASC.Web.Studio.Core.Search.SearchResult)Container.DataItem).LogoURL %>" />
                <%# ((ASC.Web.Studio.Core.Search.SearchResult)Container.DataItem).Name.HtmlEncode()%>
                <span id="btnToggleNavigator_<%# Container.ItemIndex %>" class="controlButton"></span>
            </div>

            <div id="oper_<%# Container.ItemIndex %>" style="float: right; padding-top: 10px; display: <%# ((ASC.Web.Studio.Core.Search.SearchResult)Container.DataItem).Items.Count > ((ASC.Web.Studio.Core.Search.SearchResult)Container.DataItem).PresentationControl.MaxCount?"block":"none" %>">
                <%=Resources.Resource.TotalFinded %>: <%# ((ASC.Web.Studio.Core.Search.SearchResult)Container.DataItem).Items.Count%><span>&nbsp;&nbsp;|&nbsp;&nbsp;<span class="showAllLink"
                    onclick="SearchResults.ShowAll(this,'<%# Container.FindControl("resultItems").ClientID %>','<%#((ASC.Web.Studio.Core.Search.SearchResult)Container.DataItem).ProductID %>','<%# Container.ItemIndex %>');"><%= Resources.Resource.ShowAllSearchResult %></span></span>
            </div>
        </div>

        <div id="resultItems" runat="server" class="search-results"></div>
    </ItemTemplate>
</asp:Repeater>