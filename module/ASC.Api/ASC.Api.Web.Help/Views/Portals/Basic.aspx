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
        <h2>
            Introduction
        </h2>
        <p>
            The ONLYOFFICE™ API is implemented as REST over HTTP using GET/POST/PUT/DELETE.
            All the resources, like posts or comments, have their own URLs and are designed to be manipulated in isolation.
        </p>
        <h2>
            Authentication
        </h2>
        <p>
            Authentication in the ONLYOFFICE™ API is managed via the HTTP authentication, i.e. every request must include the Authorization HTTP header.
            For information and examples please visit the <a href="<%=Url.Action("auth") %>" class="underline">Authentication</a> section.
        </p>
        <h2>
            Making requests
        </h2>
        <p>
            To identify the request and response format please make sure that both the 'Content-Type' and 'Accept' headers are set to 'application/json'.
            Any API method can be called stating the format for the response (json or xml).
        </p>
        <p>
            <b>Example:</b><br/>
            api/2.0/people/@self can be called both as api/2.0/people/@self.json - then the format of the returned media is = JSON
            and api/2.0/people/@self.xml - then the format of the returned media is = XML.
        </p>
        <p>
            By default the XML format is used for the response if no format is specified in the request (e.g. api/2.0/people/@self will return XML media).
        </p>
        <h2>
            Responses
        </h2>
        <p>
            <b>If a request succeeds, it will return a status code in the 200 range</b> and, in case no format was specified in the request, an XML-formatted response.
            Note that, in general, if a request causes a new record to be created (like a new post, or comment, etc.), the response will use the "201 Created" status.
            Any other successful operation (like a successful query, delete, or update) will return a 200 status code.
        </p>
        <p>
            <b>If a request fails, a non-200 status code will be returned</b>, possibly with error information in XML format as the response content.
            For instance, if a requested record could not be found, the HTTP response might look something like:
        </p>
        <pre>HTTP/1.1 404 Not Found</pre>
        <h2>
            Rate limiting
        </h2>
        <p>
            You can perform up to 500 requests per 10 second period from the same IP address for the same account.
            If you exceed this limit, a 503 response for the subsequent requests will be received.
            Check the Retry-After header to see how many seconds to wait before you try again.
        </p>
        <h2>
            Conventions used in this documentation
        </h2>
        <p>
            The following notation is used in the documentation:<br/>
            <b>{text}</b>: states for the text that should be replaced by your own data (ID, search query, etc.)
        </p>
        <h2>
            Support
        </h2>
        <p>
            You can ask our developers at <a href="http://dev.onlyoffice.org/" target="_blank" class="underline">dev.onlyoffice.org</a> (registration required)
        </p>
    </div>
</asp:Content>
