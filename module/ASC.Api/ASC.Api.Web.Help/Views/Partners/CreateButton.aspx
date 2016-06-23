<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Button Creation
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "clients"}, new {@class = "up"}) %>
        <span class="hdr">POST /api/partnerapi/button</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Create the button to be able to buy a portal pricing plan via PayPal payment system.</p>

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
					tariffI
					<div class="infotext">sent in URL</div>
				</td>
				<td>Pricing plan identification number in ONLYOFFICE™ database</td>
				<td>number</td>
				<td>-53</td>
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
            public void CreateButtonTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
                var tariffId = -53;
                var result = string.Empty;
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/tariffs?partnerid={0}&amp;tariffId={1}", partnerId, tariffId);
                    client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                }
            }
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>