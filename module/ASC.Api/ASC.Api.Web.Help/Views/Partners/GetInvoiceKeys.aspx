<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Invoice Keys
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "invoices"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/getinvoicekeys</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Invoice keys method is used to get the list of all the keys generated for the invoice with the ID specified in the request.</p>
				
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
					id
					<div class="infotext">sent in URL</div>
				</td>
				<td>Invoice ID which the keys will be listed for</td>
				<td>number</td>
				<td>128</td>
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
            public void GetInvoiceKeysTest()
            {
                var id = 128;
            
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/GetInvoiceKeys/{0}", id);
                    client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                }
            }
    </pre>
		
    <div class="header-gray">
        Returns
        <span id="clipLink">Get link to this headline</span>
        <a id="returns"></a>
    </div>
    <p>The response will return the list of all keys generated for the invoice with their available parameters.</p>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>