<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Batching requests
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Batching requests</h2>
    <p>
        The standard version of the <a href="./">TeamLab API</a> is designed to make it really
        easy to get data for an individual object and to browse connections between objects.
        It also includes a limited ability to retrieve data for several objects in a single request.</p>
    <p>
        If your application needs the ability to access significant amounts of data in a
        single go - or you need to make changes to several objects at once, it is often
        more efficient to batch your queries rather than make multiple individual HTTP requests.</p>
    <p>
        To enable this, the Teamlab API supports request batching which allows you to pass instructions
        for several operations in a single HTTP request. You can also specify dependencies
        between related operations (described in the <b>Batch requests containing multiple methods</b> section below). Teamlab will process
        each of your independent operations in parallel and will process your dependent
        operations sequentially. Once all operations have been completed, a consolidated
        response will be passed back to you and the HTTP connection will be closed.</p>
    <h2>
        Making a simple batched request</h2>
    <p>
        The batch API takes in an array of logical HTTP requests represented as JSON arrays
        - each request has a <code>method</code> (corresponding to HTTP method GET/PUT/POST/DELETE etc),
        a <code>relativeUrl</code> (the portion of the URL after <code>&lt;portalname&gt;.teamlab.com</code>), optional headers
        array (corresponding to HTTP headers) and an optional <code>body</code> (for POST and PUT requests).
        The batch API returns an array of logical HTTP responses represented as JSON arrays
        - each response has a status code, an optional headers array and an optional body
        (which is a JSON encoded string).</p>
    <p>
        To make batched requests, you build a JSON object which describes each individual
        operation you'd like to perform and <code>POST</code> this to the TeamLab API endpoint
        at <code>/api/2.0/batch</code>. The following example gets the current
        user's profile information and the user group in a single request:</p>
    <pre><code>batch=[
{"method": "GET", "relativeUrl":"/api/2.0/people/@self"}, 
{"method": "GET", "relativeUrl": "/api/2.0/group/@self"}
]
    </code>
</pre>
    <p>
        Once both operations have been completed, TeamLab sends a response which encapsulates
        the result of all the operations. For each operation, the response includes a status
        code, header information, and the body. These are equivalent to the response you
        could expect from each operation if performed as raw requests against the TeamLab
        API. The body field contains a string encoded JSON object:</p>
    <p>
        For the above request, the expected response would be of the form:</p>
    <pre><code>
{
    "count":2,
    "startIndex":0,
    "status":0,
    "statusCode":200,
    "response":[
        {
            "status":200,
            "headers":{
                        "x-AspNet-Version":"2.0.50727",
                        "access-Control-Allow-Origin":"*",
                        "cache-Control":"private, max-age=0",
                        "content-Type":"application/json; charset=UTF-8"
                       },
            "data":"{\"count\":1,\"startIndex\":0,\"status\":0,\"statusCode\":200,\"response\":{\"id\":\"293bb997-28d8-4be0-8547-6eb50add1f3c\",\"userName\":\"Mike.Zanyatski\",\"firstName\":\"Mike\",\"lastName\":\"Zanyatski\",\"email\":\"mike@gmail.com\",\"birthday\":\"1974-05-16T05:00:00.0000000+05:00\",\"sex\":\"male\",\"status\":1,\"terminated\":null,\"department\":\"Sample group\",\"workFrom\":\"2007-10-09T05:00:00.0000000+05:00\",\"location\":\"\",\"notes\":\"\",\"displayName\":\"Mike Zanyatski\",\"title\":\"Manager\",\"contacts\":[],\"groups\":[{\"id\":\"eeb47881-6330-4b6d-8a32-82366d4caf27\",\"name\":\"Sample group\",\"manager\":\"Jake.Zazhitski\"}],\"avatarMedium\":\"/data/0/userphotos/eeb47881-6330-4b6d-8a32-82366d4caf27_size_48-48.jpeg\",\"avatar\":\"/data/0/userphotos/eeb47881-6330-4b6d-8a32-82366d4caf27_size_82-82.jpeg\",\"avatarSmall\":\"/data/0/userphotos/eeb47881-6330-4b6d-8a32-82366d4caf27_size_32-32.jpeg\"}}"},
        {
            "status":200,
            "headers":{...}
               ]
}
</code></pre>
    <p>
        <a name="multiple_methods"></a>
    </p>
    <h2>
        Batch requests containing multiple methods</h2>
    <p>
        It is possible to combine operations that would normally use different HTTP methods
        into a single batch request. While <code>GET</code> and <code>DELETE</code> operations
        must only have a <code>relativeUrl</code> and a <code>method</code> field, <code>POST</code>
        and <code>PUT</code> operations may contain an optional <code>body</code> field.
        This should be formatted as a raw HTTP POST body string, similar to a URL query
        string. The following example gets information on the current contact and updates
        the contact information for the contact with the selected ID in a single operation:</p>
    <pre><code>batch=[
{"method": "GET", "relativeUrl":"/api/2.0/people/@self"}, 
{"method": "POST", "relativeUrl": "/api/2.0/people/{userid}/contacts", 
"body":"contacts[0].Type=skype&amp;contacts[0].Value=skypename&amp;contacts[1].Type=msn&amp;contacts[1].Value=msn_login"}
]
    </code></pre>
  
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptPlaceholder" runat="server">
</asp:Content>
