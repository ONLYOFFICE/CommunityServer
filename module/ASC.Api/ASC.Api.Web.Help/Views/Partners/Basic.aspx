<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Basic concepts
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="content">
        <h1>
            <span class="hdr">Basic concepts</span>
        </h1>
        <p class="dscr">
            The ONLYOFFICE™ Partners API is implemented as REST over HTTP using GET/POST. All the resources, like client creation, portal registration, etc, have their own URLs and are designed to be manipulated in isolation.
        </p>
        <h2>
            Authentication
        </h2>
        <p>
            Authentication in the ONLYOFFICE™ Partners API is managed via the HTTP authentication, i.e. every request must include the Authorization HTTP header. For information and examples please visit the <%=Html.ActionLink("Authentication", "authentication", "partners", new {@class = "underline"})%> section.
        </p>
        <h2>
            Making requests
        </h2>
        <p>
            To identify the request and response format please make sure that both the 'Content-Type' and 'Accept' headers are set to 'application/json'. The API methods are called without stating the format for the response and are returned in JSON format.
        </p>
        <h2>
            Responses
        </h2>
        <p>
            If a request succeeds, it will return a status code in the 200 range and a JSON-formatted response. Note that, in general, if a request causes a new record to be created, the response will use the "201 Created" status. Any other successful operation will return a 200 status code.
        </p>
        <h2>
            Support
        </h2>
        <p>
            You can ask our developers at <a href="http://dev.onlyoffice.org/" target="_blank" class="underline">dev.onlyoffice.org</a> (registration required)
        </p>
    </div>
</asp:Content>
