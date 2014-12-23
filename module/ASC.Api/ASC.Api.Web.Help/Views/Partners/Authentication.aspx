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
	    <span class="hdr">Authentication</span>
	</h1>
	<p class="dscr">An http header is required to pass the authenticatoin when performing the API request. The authentication requires the partner token to be used in the <em>Authorization</em> header of the http request. This token is unique for each partner and can be found at the following page: <a href="http://partners.onlyoffice.com/office#/partnersapi" class="underline" target="_blank">http://partners.onlyoffice.com/office#/partnersapi</a> in the <b>Authentication Token</b> field (it will look like this: <em>ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1</em>).</p>
	<p>Below is an example of the authentication:</p>
<pre>
var request = HttpWebRequest.CreateHttp(apiHost + url);
request.Method = "POST";
request.ContentType = "application/json";
request.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
</pre>
	<p>For the keys generation as hmac-sha1 key the partner private key is used. The private and the public keys can be found in the <b>Lead Generation</b> section of the <a href="https://partners.onlyoffice.com/office#/leads" class="underline" target="_blank">ONLYOFFICE™ Partners Program</a> web site.</p>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>