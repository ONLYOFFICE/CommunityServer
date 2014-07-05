<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchResults.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Search.SearchResults" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Search" %>

<asp:Repeater ID="results" runat="server" ItemType="ASC.Web.Studio.Core.Search.SearchResult">
    <ItemTemplate>
        <div class="search-results-block clearFix">
            <div class="header-base header-search-results" onclick="SearchResults.Toggle('<%# Container.FindControl("resultItems").ClientID %>','btnToggleNavigator_<%# Container.ItemIndex %>')">
                <img class="logoUrl" align="absmiddle" alt="<%# Item.Name %>" src="<%# Item.LogoURL %>" />
                <%# Item.Name.HtmlEncode()%>
                <img id="btnToggleNavigator_<%# Container.ItemIndex %>" class="controlButton" src="<%= WebImageSupplier.GetAbsoluteWebPath("collapse_down_dark.png") %>" alt=""/>
            </div>

            <div id="oper_<%# Container.ItemIndex %>" style="float: right; padding-top: 10px; display: <%# Item.Items.Count > Item.PresentationControl.MaxCount?"block":"none" %>">
                <%=Resources.Resource.TotalFinded %>: <%# Item.Items.Count%><span>&nbsp;&nbsp;|&nbsp;&nbsp;<span class="showAllLink"
                    onclick="SearchResults.ShowAll(this,'<%# Container.FindControl("resultItems").ClientID %>','<%#Item.ProductID %>','<%# Container.ItemIndex %>');"><%= Resources.Resource.ShowAllSearchResult %></span></span>
            </div>
        </div>

        <div id="resultItems" runat="server" class="search-results"></div>
    </ItemTemplate>
</asp:Repeater>