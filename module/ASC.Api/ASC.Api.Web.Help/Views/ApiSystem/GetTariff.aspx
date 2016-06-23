<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Get tariff
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {url = "billing"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/registration/tariff</span>
        <span class="auth">This function requires authentication</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">Get the portal pricing plan.</p>

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
                <td>portal name</td>
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
    <p>Returns the description of the portal and the portal pricing plan.</p>

    <div class="header-gray">Example Response</div>
    <pre>
{
    "tariff" : 
    {
        "ActiveUsers" : 50,
        "DueDate" : "2016-07-13",
        "Features" : "whitelabel",
        "MaxFileSize" : 104857600,
        "MaxTotalSize" : 1073741824
    },
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
