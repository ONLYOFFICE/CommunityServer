<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Validate portal name
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {url = "portals"}, new {@class = "up"}) %>
        <span class="hdr">POST /api/registration/validateportalname</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">Checks if the name is available to create a portal.</p>

    <div class="header-gray">Parameters</div>
    <table class="table">
        <colgroup>
            <col style="width: 20%" />
            <col />
            <col style="width: 120px" />
            <col style="width: 20%" />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Name</td>
                <td>Description</td>
                <td>Type</td>
                <td>Example</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>portalName<span class="required">*</span>
                    <div class="infotext">sent in Query</div>
                </td>
                <td>the name of a portal</td>
                <td>string</td>
                <td>example</td>
            </tr>
        </tbody>
    </table>
    <span class="required-descr"><span class="required">*</span><em> - required field</em></span>

    <div class="header-gray">
        Returns
        <span id="clipLink">Get link to this headline</span>
        <a id="returns"></a>
    </div>
    <p>The response will contain the following information.</p>

    <div class="header-gray">Example Response</div>
    <table class="table">
        <colgroup>
            <col style="width: 20%" />
            <col />
            <col style="width: 120px" />
            <col style="width: 20%" />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Name</td>
                <td>Description</td>
                <td>Type</td>
                <td>Example</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>message</td>
                <td>reply "portalNameReadyToRegister" if the portal name is available</td>
                <td>string</td>
                <td>portalNameReadyToRegister</td>
            </tr>
            <tr class="tablerow">
                <td>variants</td>
                <td>list of all the existing potal names starting with the name in request</td>
                <td>array of string</td>
                <td>["example"]</td>
            </tr>
        </tbody>
    </table>
    <pre>
{
    "errors" : [ "portalNameExist" ],
    "variants" : [ "example", "example2016" ],
}
</pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
