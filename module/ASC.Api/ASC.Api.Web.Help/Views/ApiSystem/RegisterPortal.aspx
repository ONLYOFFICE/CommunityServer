<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Register portal
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {url = "portals"}, new {@class = "up"}) %>
        <span class="hdr">POST /api/registration/registerportal</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">New portal registration.</p>

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
                <td>FirstName<span class="required">*</span>
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal owner first name</td>
                <td>string</td>
                <td>John</td>
            </tr>
            <tr class="tablerow">
                <td>LastName<span class="required">*</span>
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal owner last name</td>
                <td>string</td>
                <td>Smith</td>
            </tr>
            <tr class="tablerow">
                <td>Email<span class="required">*</span>
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal owner email address</td>
                <td>string</td>
                <td>test@example.com</td>
            </tr>
            <tr class="tablerow">
                <td>portalName<span class="required">*</span>
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal name</td>
                <td>string</td>
                <td>example</td>
            </tr>
            <tr class="tablerow">
                <td>Phone
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal owner phone number</td>
                <td>string</td>
                <td>+123456789</td>
            </tr>
            <tr class="tablerow">
                <td>Language
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal language</td>
                <td>string</td>
                <td>en</td>
            </tr>
            <tr class="tablerow">
                <td>TimeZoneName
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal time zone</td>
                <td>string</td>
                <td>UTC</td>
            </tr>
            <tr class="tablerow">
                <td>Password
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal owner password</td>
                <td>string</td>
                <td>123456</td>
            </tr>
        </tbody>
    </table>
    <span class="required-descr"><span class="required">*</span><em> - required field</em></span>

    <div class="header-gray">
        Returns
        <span id="clipLink">Get link to this headline</span>
        <a id="returns"></a>
    </div>
    <p>Returns the link for portal activation and portal description.</p>

    <div class="header-gray">Example Response</div>
    <pre>
{
    "reference" : "http://example/confirm.aspx",
    "tenant" :
    {
        "ownerId" : "78e1e841-8314-48465-8fc0-e7d6451b6475",
        "status" : 1,
        "tenantDomain" : "example",
        "tenantId" : 1,
    },
}
</pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
