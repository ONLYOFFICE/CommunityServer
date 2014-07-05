<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Filters
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Request filtering</h2>
    <p>
        Every request to the API supports a certain number of parameters sent in the URL</p>
        <p>
        I.e. the request <a href="<%=Url.DocUrl("people", "get", "api/2.0/people")%>">api/2.0/people</a> can be appended with several parameters, 
        for example <a href="<%=Url.DocUrl("people", "get", "api/2.0/people")%>">api/2.0/people?startIndex=10&amp;count=25</a>
        </p>
    <table class="views-table cols-9 zebra-striped">
        <thead>
            <tr>
                <th class="views-field-title">
                    Parameter
                </th>
                <th class="views-field-body">
                    Description
                </th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td class="views-field-title">
                    count
                </td>
                <td class="views-field-body">
                    Number of the elements returned.
                </td>
            </tr>
            <tr>
                <td class="views-field-title">
                    startIndex
                </td>
                <td class="views-field-body">
                    The number of elements to be skipped in the beginning. Used for response data pagination.
                </td>
            </tr>
            <tr>
                <td class="views-field-title">
                    sortBy
                </td>
                <td class="views-field-body">
                    Sort by field name.
                </td>
            </tr>
            <tr>
                <td class="views-field-title">
                    sortOrder
                </td>
                <td class="views-field-body">
                    Sorting direction. Can be "descending" or "ascending". For example, used together with sortBy:<br/>
                    <a href="<%=Url.DocUrl("people", "get", "api/2.0/people")%>">api/2.0/people?sortBy=userName&amp;sortOrder=descending</a>                 
                </td>
            </tr>
            <tr>
                <td class="views-field-title">
                    filterBy
                </td>
                <td class="views-field-body">
                    Filter results by field name.
                </td>
            </tr>
            <tr>
                <td class="views-field-title">
                    filterOp
                </td>
                <td class="views-field-body">
                    Filtering operation. Can be one of the following: "contains","equals","startsWith","present"
                </td>
            </tr>
            <tr>
                <td class="views-field-title">
                    filterValue
                </td>
                <td class="views-field-body">
                    Filter value. For example, used together with filterBy and filterOp:<br/>
                    <a href="<%=Url.DocUrl("people", "get", "api/2.0/people")%>">api/2.0/people?filterBy=userName&amp;filterOp=startsWith&amp;filterValue=Alex</a>                 
                </td>
            </tr>
            <tr>
                <td class="views-field-title">
                    updatedSince
                </td>
                <td class="views-field-body">
                    Returns the values updated or created since a certain period of time.
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>
