<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Set tariff
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {url = "billing"}, new {@class = "up"}) %>
        <span class="hdr">PUT /api/registration/tariff</span>
        <span class="auth">This function requires authentication</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">Portal pricing plan change.</p>

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
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal name</td>
                <td>string</td>
                <td>example</td>
            </tr>
            <tr class="tablerow">
                <td>dueDate
                    <div class="infotext">sent in Body</div>
                </td>
                <td>portal pricing plan end date</td>
                <td>Date</td>
                <td>2016-07-13</td>
            </tr>
            <tr class="tablerow">
                <td>ActiveUsers
                    <div class="infotext">sent in Body</div>
                </td>
                <td>number of active users</td>
                <td>integer</td>
                <td>50</td>
            </tr>
            <tr class="tablerow">
                <td>maxTotalSize
                    <div class="infotext">sent in Body</div>
                </td>
                <td>maximal size of available disk space measured in bytes**</td>
                <td>long</td>
                <td>1073741824</td>
            </tr>
            <tr class="tablerow">
                <td>maxFileSize
                    <div class="infotext">sent in Body</div>
                </td>
                <td>maximal size of the uploaded files measured in bytes**</td>
                <td>long</td>
                <td>104857600</td>
            </tr>
            <tr class="tablerow">
                <td>features
                    <div class="infotext">sent in Body</div>
                </td>
                <td>available functions list</td>
                <td>string</td>
                <td>whitelabel</td>
            </tr>
        </tbody>
    </table>
    <span class="required-descr"><span class="required">*</span><em> - required field</em></span>
    <span class="required-descr">**<em> - please keep in mind that the sent value will be converted into megabytes and stored that way, so it is not recommended to use values not multiple of 1048576 (1 megabyte) as they will be truncated to the integer resulting from the division by 1048576, or values less than 1048576 bytes as zero value will be returned in that case both in response to this and <a class="underline" href="<%= Url.Action("gettariff") %>"><em>get tariff</em></a> requests.</em></span>

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
