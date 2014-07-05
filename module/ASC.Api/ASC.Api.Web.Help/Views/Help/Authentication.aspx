<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Authentication
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>How to pass authentication?</h2>
    <p>One needs to perform several easy steps to pass authentication:</p>
    <ol>
        <li>Send POST request, containing two parameters: <strong>userName</strong> and <strong>password</strong>, to the <a href="<%=Url.DocUrl("authentication", "post", "api/2.0/authentication")%>">api/2.0/authentication</a> address
   <h4>Request</h4>
<pre><code>POST /api/2.0/authentication.json HTTP/1.1
Host: portal.teamlab.com
Accept: application/json,application/xml
Accept-Encoding: gzip, deflate

userName=yourusername&amp;password=yourpassword
</code></pre>
   <h4>Response</h4>
<pre><code>HTTP/1.1 200 Ok
Cache-Control: private
Content-Type: application/json; charset=utf-8

{
  "count": 0,
  "startIndex": 0,
  "status": 0,
  "response": {
    "token": "sdjhfskjdhkqy739459234",
    "expires": "2013-01-13T16:35:42.7564317+04:00"
  }
}
</code></pre>
        </li>
        <li>In case authentication is successful, a token which will look like <strong>sdjhfskjdhkqy739459234</strong> will be received</li>
        <li>Use this token every time you call API methods inserting it to the HTTP Header: Authorization
               <h4>Sample Request</h4>
<pre><code>GET api/2.0/people/@self.json HTTP/1.1
Host: portal.teamlab.com
Accept: application/json,application/xml
Accept-Encoding: gzip, deflate
Authorization:<strong>sdjhfskjdhkqy739459234</strong>
</code></pre>
        </li>
    </ol>
</asp:Content>
