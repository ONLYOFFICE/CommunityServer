<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master" CodeBehind="Calls.aspx.cs" Inherits="ASC.Web.CRM.Calls" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ MasterType TypeName="ASC.Web.CRM.BasicTemplate" %>

<asp:Content ContentPlaceHolderID="FilterContent" runat="server">
    <div id="calls-filter"></div>
</asp:Content>

<asp:Content ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"/>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"/>
</asp:Content>

<asp:Content ContentPlaceHolderID="PagingContent" runat="server">
    <table id="calls-paging">
        <tbody>
            <tr>
                <td>
                    <div id="calls-paging-box"></div>
                </td>
                <td id="calls-paging-stat-box">
                    <span><%= CRMCommonResource.Total %>:&nbsp;</span>
                    <span id="calls-paging-items-count"></span>
                    <span><%= CRMCommonResource.ShowOnPage %>:&nbsp;</span>
                    <select id="calls-paging-page-count" class="top-align">
                        <option value="25">25</option>
                        <option value="50">50</option>
                        <option value="75">75</option>
                        <option value="100">100</option>
                    </select> 
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>
