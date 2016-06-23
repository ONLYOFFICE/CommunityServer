<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<string>"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Partners Program API Methods
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% var section = Model as string; %>

    <% if (string.IsNullOrEmpty(section) || string.Equals(section, "portals"))
       { %>
    <h1>
        <span class="hdr">Portals</span>
    </h1>
    <table class="table hover">
        <colgroup>
            <col style="width: 25%" />
            <col style="width: 40%" />
            <col />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Method</td>
                <td>Resource</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td><%= Html.ActionLink("Validate portal name", "validateportalname", null, new {@class = "underline"}) %></td>
                <td><%= Html.ActionLink("POST /api/registration/validateportalname", "validateportalname", null, new {@class = "underline"}) %></td>
                <td>Checks if the name is available to create a portal.</td>
            </tr>
            <tr class="tablerow">
                <td><%= Html.ActionLink("Register portal", "registerportal", null, new {@class = "underline"}) %></td>
                <td><%= Html.ActionLink("POST /api/registration/registerportal", "registerportal", null, new {@class = "underline"}) %></td>
                <td>Checks if the name is available to create a portal.</td>
            </tr>
            <tr class="tablerow">
                <td><%= Html.ActionLink("Get portals", "getportals", null, new {@class = "underline"}) %></td>
                <td><%= Html.ActionLink("GET /api/registration/getportals", "getportals", null, new {@class = "underline"}) %></td>
                <td>Get the list of registered portals.</td>
            </tr>
            <tr class="tablerow">
                <td><%= Html.ActionLink("Portal deletion", "removeportal", null, new {@class = "underline"}) %></td>
                <td><%= Html.ActionLink("DELETE /api/registration/removeportal", "removeportal", null, new {@class = "underline"}) %></td>
                <td>Portal deletion.</td>
            </tr>
            <tr class="tablerow">
                <td><%= Html.ActionLink("Status portal", "statusportal", null, new {@class = "underline"}) %></td>
                <td><%= Html.ActionLink("PUT /api/registration/statusportal", "statusportal", null, new {@class = "underline"}) %></td>
                <td>Portal activation status change.</td>
            </tr>
        </tbody>
    </table>
    <% }
       else if (string.IsNullOrEmpty(section) || string.Equals(section, "billing"))
       { %>
    <h1>
        <span class="hdr">Billing</span>
    </h1>
    <table class="table hover">
        <colgroup>
            <col style="width: 25%" />
            <col style="width: 40%" />
            <col />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Method</td>
                <td>Resource</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td><%= Html.ActionLink("Set tariff", "settariff", null, new {@class = "underline"}) %></td>
                <td><%= Html.ActionLink("PUT /api/registration/tariff", "settariff", null, new {@class = "underline"}) %></td>
                <td>Portal pricing plan change.</td>
            </tr>
            <tr class="tablerow">
                <td><%= Html.ActionLink("Get tariff", "gettariff", null, new {@class = "underline"}) %></td>
                <td><%= Html.ActionLink("GET /api/registration/tariff", "gettariff", null, new {@class = "underline"}) %></td>
                <td>Portal pricing plan change.</td>
            </tr>
        </tbody>
    </table>
    <% } %>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
