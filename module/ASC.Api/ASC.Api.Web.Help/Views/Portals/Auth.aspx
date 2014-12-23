<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Authentication
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <span class="hdr">How to pass authentication?</span>
    </h1>
    <p>One needs to perform several easy steps to pass authentication:</p>
    <p>1. Send POST request, containing two parameters: <b>userName</b> and <b>password</b>, to the <a href="<%=Url.DocUrl("authentication", null, "post", "api/2.0/authentication", "portals")%>">api/2.0/authentication</a> address</p>
    <div class="header-gray">Request</div>
    <pre>POST /api/2.0/authentication.json HTTP/1.1
Host: portal.onlyoffice.com
Accept: application/json,application/xml
Accept-Encoding: gzip, deflate
userName=yourusername&amp;password=yourpassword</pre>
            <div class="header-gray">Response</div>
            <pre>HTTP/1.1 200 Ok
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
}</pre>
    <p>2. In case authentication is successful, a token which will look like <b>sdjhfskjdhkqy739459234</b> will be received</p>
    <p>3. Use this token every time you call API methods inserting it to the HTTP Header: Authorization</p>
    <div class="header-gray">Sample Request</div>
    <pre>GET api/2.0/people/@self.json HTTP/1.1
Host: portal.onlyoffice.com
Accept: application/json,application/xml
Accept-Encoding: gzip, deflate
Authorization:sdjhfskjdhkqy739459234</pre>
</asp:Content>
