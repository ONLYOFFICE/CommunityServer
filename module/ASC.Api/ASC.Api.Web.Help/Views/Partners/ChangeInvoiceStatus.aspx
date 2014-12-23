<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Invoice Status Change
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "invoices"}, new {@class = "up"}) %>
        <span class="hdr">POST /api/partnerapi/changeinvoicestatus</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Invoice status change method is used to change the status of the invoice with the selected ID for the status specified in the request.</p>
			
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
				<td>Invoice ID which the status must be changed for</td>
				<td>number</td>
				<td>128</td>
			</tr>
			<tr class="tablerow">
				<td>
					status
					<div class="infotext">sent in URL</div>
				</td>
				<td>New status of the invoice (can take the following values: <em>SENT</em> - <b>1</b>, <em>PAID</em> - <b>2</b>)</td>
				<td>status type</td>
				<td>2</td>
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
            public void ChangeInvoiceStatusTest()
            {
                var id = "";
                var status = "2";
            
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/ChangeInvoiceStatus?id={0}&amp;status={1}", id, status);
                    client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                }
            }
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>