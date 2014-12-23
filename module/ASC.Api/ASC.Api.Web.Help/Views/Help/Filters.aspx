<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Filters
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Request filtering</h2>
    <p>Every request to the API supports a certain number of parameters sent in the URL</p>
    <p>
        I.e. the request <a class="underline" href="<%=Url.DocUrl("people", null, "get", "api/2.0/people", "portals")%>">api/2.0/people</a> can be appended with several parameters, 
        for example <a class="underline" href="<%=Url.DocUrl("people", null, "get", "api/2.0/people", "portals")%>">api/2.0/people?startIndex=10&amp;count=25</a>
    </p>
    <table class="table">
        <colgroup>
            <col style="width: 25%"/>
            <col/>
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Parameter</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>count</td>
                <td>Number of the elements returned.</td>
            </tr>
            <tr class="tablerow">
                <td>startIndex</td>
                <td>The number of elements to be skipped in the beginning. Used for response data pagination.</td>
            </tr>
            <tr class="tablerow">
                <td>sortBy</td>
                <td>Sort by field name.</td>
            </tr>
            <tr class="tablerow">
                <td>sortOrder</td>
                <td>
                    Sorting direction. Can be "descending" or "ascending". For example, used together with sortBy:<br/>
                    <a class="underline" href="<%=Url.DocUrl("people", null, "get", "api/2.0/people", "portals")%>">api/2.0/people?sortBy=userName&amp;sortOrder=descending</a>                 
                </td>
            </tr>
            <tr class="tablerow">
                <td>filterBy</td>
                <td>Filter results by field name.</td>
            </tr>
            <tr class="tablerow">
                <td>filterOp</td>
                <td>Filtering operation. Can be one of the following: "contains","equals","startsWith","present"</td>
            </tr>
            <tr class="tablerow">
                <td>filterValue</td>
                <td>
                    Filter value. For example, used together with filterBy and filterOp:<br/>
                    <a class="underline" href="<%=Url.DocUrl("people", null, "get", "api/2.0/people", "portals")%>">api/2.0/people?filterBy=userName&amp;filterOp=startsWith&amp;filterValue=Alex</a>                 
                </td>
            </tr>
            <tr class="tablerow">
                <td>updatedSince</td>
                <td>Returns the values updated or created since a certain period of time.</td>
            </tr>
        </tbody>
    </table>
</asp:Content>
