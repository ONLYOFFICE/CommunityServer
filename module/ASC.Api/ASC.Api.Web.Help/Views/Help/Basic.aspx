<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Basic concepts
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="PreMainContent">
    <!--<div class="banner" style="text-align: center;padding-top: 58px; background-color: #222e3e">
        <img src="<%=Url.Content("~/Content/images/banner.jpg")%>" alt="TeamLab Api"/>
    </div>-->
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="content">
        <h1>
            Introduction</h1>
        <p>
            The Teamlab API is implemented as REST over HTTP using GET/POST/PUT/DELETE. All the resources, like posts or comments, have their own URLs and are designed to be manipulated in isolation.
        </p>
        <h2>
            Authentication</h2>
        <p>
            Authentication in the TeamLab API is managed via the HTTP authentication, i.e. every request must include the Authorization HTTP header.
        </p>
        <div class="alert-message block-message">
            For information and examples please visit the <a href="<%=Url.Action("Authentication") %>">Authentication</a> section.
        </div>
        
        <h2>
            Making requests</h2>
        <p>
            To identify the request and response format please make sure that both the 'Content-Type' and 'Accept' headers are set to 'application/json'.
        </p>
        <p>
            Any API method can be called stating the format for the response (json or xml).
            <br/><br />
            Example:
            <br/>
            api/2.0/people/@self can be called both as api/2.0/people/@self.json - then the format of the returned media is = JSON
            and api/2.0/people/@self.xml - then the format of the returned media is = XML.
            <br /><br />By default the XML format is used for the response if no format is specified in the request (e.g. api/2.0/people/@self will return XML media).
        </p>
        
        <h2>
            Responses</h2>
        <p>
            <strong>If a request succeeds, it will return a status code in the 200 range</strong> and, in case no format was specified in the request, an XML-formatted response. Note that, in general, if a request causes a new record to be created (like a new post, or comment, etc.), the response will use the "201 Created" status. Any other successful operation (like a successful query, delete, or update) will return a 200 status code.
        </p>
        <p>
            <strong>If a request fails, a non-200 status code will be returned</strong>, possibly with error information in XML format as the response content. For instance, if a requested record could not be found, the HTTP response might look something like:
        </p>
        <pre style="margin-bottom: 15px;"><code>HTTP/1.1 404 Not Found</code></pre>
        <h2>
            Rate limiting</h2>
        <p>
            You can perform up to 500 requests per 10 second period from the same IP address for the same account. If you exceed this limit, a 503 response for the subsequent requests will be received. Check the Retry-After header to see how many seconds to wait before you try again.
        </p>
        <h2>
            Conventions used in this documentation</h2>
        <p>
            The following notation is used in the documentation:
        </p>
        <ul>
            <li><code>{text}</code>: states for the text that should be replaced by your own data (ID, search query, etc.)</li>
        </ul>
         <h2>
            Support</h2>
        <p>
            You can ask our developers at <a href="https://developers.teamlab.com/" target="_blank">developers.teamlab.com</a> (registration required)
        </p>

    </div>
</asp:Content>
