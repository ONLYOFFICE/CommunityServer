<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    License Server API
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <span class="hdr">License Server API</span>
    </h1>

    <p>License Server is used to get license for Enterprise functionality of Document Server and allows you to get the license for your ONLYOFFICE Enterprise Edition, and receive the license information in the reply to the request.</p>
    <p>To be able to use License Server API you need to receive <b>customer_id</b> and <b>customer_secret</b> which are issued to every licensed customer after the application and sent in the activation email message.</p>


    <h2>Getting the License</h2>

    <p>To get a license the following POST request must be sent:</p>
    <p><b>https://license.onlyoffice.com/v1.1/&lt;customer_id&gt;/users?sign=&lt;sign&gt;</b></p>
    <p>With a json-object in the body:</p>
    <pre>
{
    "users": ["&lt;user1_hash&gt;", "&lt;user2_hash&gt;", ...],
    "start_date": "&lt;startDate&gt;",
    "end_date": "&lt;endDate&gt;"
}
</pre>
    <p>Where:</p>
    <ul>
        <li><b>&lt;customer_id&gt;</b> - is your company ID (issued upon application and sent to you in the application email message);</li>
        <li><b>&lt;sign&gt;</b> - is the request signature (see the <a class="underline" href="<%= Url.Action("license") %>#requestSignature">Request Signature</a> section);</li>
        <li><b>&lt;startDate&gt;</b> - is the license start date in YYYY-MM-DD format;</li>
        <li><b>&lt;endDate&gt;</b> - is the license end date in YYYY-MM-DD format;</li>
        <li><b>&lt;user1_hash&gt;</b> - is the user hash. See the <a class="underline" href="<%= Url.Action("license") %>#userHash">User Hash</a> section below for more information on the <i>user_hash</i> calculation.</li>
    </ul>
    <p>In response to this request the server will return the license file with the list of all users.</p>


    <h2 id="userHash">User Hash</h2>
    <p>The user hash parameter (<i>user_hash</i>) must be calculated using the following algorithm:</p>
    <pre>
user_hash = sha256_hex(user_id + user_first_name + user_last_name)
</pre>
    <p>Where:</p>
    <ul>
        <li><b>user_id</b> - is the user ID which value is used as a user identifier when opening documents in the <a class="underline" href="<%= Url.Action("editor") %>#user">editorConfig.user.id</a> object;</li>
        <li><b>user_first_name</b> - is the user first name which value is used as a user first name when opening documents in the <a class="underline" href="<%= Url.Action("editor") %>#user">editorConfig.user.firstname</a> object;</li>
        <li><b>user_last_name</b> - is the user last name which value is used as a user last name when opening documents in the <a class="underline" href="<%= Url.Action("editor") %>#user">editorConfig.user.lastname</a> object.</li>
    </ul>
    <div class="header-gray">Example:</div>
    <pre>
user_id="78e1e841-8314-48465-8fc0-e7d6451b6475"
user_first_name="John"
user_last_name="Smith"

user_hash = sha256("78e1e841-8314-48465-8fc0-e7d6451b6475JohnSmith")
//user_hash = cde4faae257f01ddbc5b096d26c6480b7bfa41daf16c489c4276927ddcaf5244
</pre>


    <h2 id="requestSignature">Request Signature</h2>
    <p>To make sure that the request has been sent by you and not by any malefactors on behalf of your application, all API request must be signed. The signature is calculated using a special algorithm, the result of signature calculation is sent in the sign parameter of the request. The service will check the signature and will return the reply only in case the signature is correct.</p>
    <p>The algorithm used to calculate the sign parameter:</p>
    <pre>
sign = sha256_hex(URI + request_body + customer_secret)
</pre>
    <p>Where:</p>
    <ul>
        <li><b>customer_secret</b> - is issued to you upon the application and sent to you in the application email message.</li>
    </ul>
    <div class="header-gray">Example:</div>
    <p>Let's imagine that</p>
    <pre>
customer_id="onlyoffice"
user_hash="cde4faae257f01ddbc5b096d26c6480b7bfa41daf16c489c4276927ddcaf5244"
customer_secret="9876543210"
</pre>
    <p>and we want to get license from 11/05/2015 to 12/05/2015.</p>
    <p>We want to send the following request:</p>
    <p><b>https://license.onlyoffice.com/v1.1/onlyofffice/users</b></p>
    <p>with body:</p>
    <pre>
{
    "users": ["cde4faae257f01ddbc5b096d26c6480b7bfa41daf16c489c4276927ddcaf5244"],
    "start_date": "2015-11-05",
    "end_date": "2015-12-05"
}
</pre>
    <p>Then the signature is calculated the following way:</p>
    <pre>
URI = /v1.1/onlyofffice/users

sign = sha256('/v1.1/onlyofffice/users{"users":["cde4faae257f01ddbc5b096d26c6480b7bfa41daf16c489c4276927ddcaf5244"],"start_date":"2015-11-05","end_date":"2015-12-05"}9876543210')
//sign = 46c50809646f0577248311cd318a4f5a927662df742c939324bbf61ca7af3543
</pre>
    <p>Resulting request will look like:</p>
    <p><b>https://license.onlyoffice.com/v1.1/onlyofffice/users?sign=46c50809646f0577248311cd318a4f5a927662df742c939324bbf61ca7af3543</b></p>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
