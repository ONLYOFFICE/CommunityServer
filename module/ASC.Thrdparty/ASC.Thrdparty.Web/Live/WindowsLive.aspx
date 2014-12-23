<%@ Page Title="" Language="C#" MasterPageFile="~/Popup.Master" AutoEventWireup="true" CodeBehind="WindowsLive.aspx.cs" Inherits="ASC.Thrdparty.Web.Live.WindowsLive" %>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <% if (Token == null) { %>
    <p>
        Please <a href="<%=ConsentUrl%>">click here</a> to grant consent for this application to access your Windows Live data.
    </p>
    <% }%> 
</asp:Content>
