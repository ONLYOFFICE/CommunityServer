<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Key Generation
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "keys"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/generatekeys</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Key generation method is used to generate new keys for the partner with the ID and the number of keys specified in the request.</p>

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
                    partnerId
                    <div class="infotext">sent in URL</div>
                </td>
                <td>Partner ID</td>
                <td>GUID</td>
                <td>00000000-c2fb-4de6-a8ae-e59a834a3cc7</td>
            </tr>
			<tr class="tablerow">
				<td>
                    count
                    <div class="infotext">sent in URL</div>
				</td>
				<td>Number of keys to be generated</td>
				<td>number</td>
				<td>2</td>
			</tr>
			<tr class="tablerow">
				<td>
                    tariff
                    <div class="infotext">sent in URL</div>
				</td>
				<td>Pricing plan details represented in the following way: maximal number of users for the pricing plan plus duration (month or year)
						<br />e.g. <em>10month</em> is the pricing plan for maximum of 10 users per month, <em>20year</em> is the pricing plan for maximum of 20 users per year, etc.</td>
				<td>string</td>
				<td>20year</td>
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
            public void GenerateKeysTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
                var count = 2;
                var tariff = "20year";
                var result = string.Empty;
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/GenerateKeys?partnerId={0}&amp;count={1}&amp;tariff={2}", partnerId, count, tariff);
                    client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                }
            }
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>