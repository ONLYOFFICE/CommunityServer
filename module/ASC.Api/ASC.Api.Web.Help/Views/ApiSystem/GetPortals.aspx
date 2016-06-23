<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Get portals
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {url = "portals"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/registration/getportals</span>
        <span class="auth">This function requires authentication</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">Get the list of all the portals registered to the user with the email address specified in the request.</p>

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
                <td>email<span class="required">*</span>
                    <div class="infotext">sent in Query</div>
                </td>
                <td>portal owner email address</td>
                <td>string</td>
                <td>test@example.com</td>
            </tr>
        </tbody>
    </table>
    <span class="required-descr"><span class="required">*</span><em> - required field</em></span>

    <div class="header-gray">
        Returns
        <span id="clipLink">Get link to this headline</span>
        <a id="returns"></a>
    </div>
    <p>Returns the list of all the portals registered to the user with the email address specified in the request.</p>

    <div class="header-gray">Example Response</div>
    <pre>
{
    "portals" : [ "http://example" ],
}
</pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
