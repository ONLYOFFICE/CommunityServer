<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Key Activation
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <h1>
        <%= Html.ActionLink(" ", "index", new {id = "keys"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/activatekey</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Key activation method is used to activate the key provided for the portal payment with the key code specified in the request.</p>
		
	<div class="header-gray">Parameters</div>   
    <table class="table">
        <colgroup>
            <col style="width: 20%"/>
            <col/>
            <col style="width: 110px"/>
            <col style="width: 20%"/>
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
				<td>
                    code
                    <div class="infotext">sent in URL</div>
				</td>
				<td>Key code that will be activated for the selected portal</td>
				<td>string</td>
				<td>1HMEV7S1I01N12LETBID</td>
			</tr>
			<tr class="tablerow">
				<td>
                    portal
                    <div class="infotext">sent in URL</div>
				</td>
				<td>Name of the portal which the activation key is provided for</td>
				<td>string</td>
				<td>test-portal-name</td>
			</tr>
	    </tbody>
    </table>
    
    <div class="header-gray">Example</div>
    <pre>
     [TestClass]
        public class ApiTests
        {
            private readonly string apiHost = "https://partners.onlyoffice.com";

            [TestMethod]
            public void ActivateKeyTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
                var code = "1HMEV7S1I01N12LETBID";
                var portalName = "test-portal-name";
                var result = string.Empty;
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/ActivateKey?code={0}&amp;portal={1}", HttpUtility.UrlEncode(code), HttpUtility.UrlEncode(portalName));
                    client.Headers.Add("Authorization","ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                 }
            }
    </pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>