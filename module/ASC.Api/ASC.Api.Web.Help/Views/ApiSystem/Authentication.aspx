<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Authentication
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <span class="hdr">Authentication</span>
    </h1>

    <p class="dscr">An http header is required to pass the authentication when performing the API request. The authentication requires the token to be used in the <em>Authorization</em> header of the http request.</p>
    <p><b>Authentication Token</b> is a string sequence in the following format: "ASC <em>pkey</em>:<em>datetime</em>:<em>hash</em>", where</p>
    <ul>
        <li><b>pkey</b> - random string (may be empty string),</li>
        <li><b>datetime</b> - current date and time in the "<em>yyyyMMddHHmmss</em>" format,</li>
        <li><b>hash</b> - hash value for the string in "<em>datetime</em>\n<em>pkey</em>" format.</li>
    </ul>
    <p>The hash value is calculated using the HMAC-SHA1 function with the key from the <em>core.machinekey</em> value of the Hosted Solution site <em>appSettings</em> configuration.</p>

    <p>Example Authentication Token will look like this: "<em>ASC abc:20160708120000:D94XPcnZ_y6uSx2jgUcgNdk4dro1</em>"</p>

    <div id="csharp" class="header-gray">.Net(C#) generating token example</div>
    <pre>
public string CreateAuthToken(string pkey, string machinekey)
{
    using (var hasher = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(machinekey)))
    {
        var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var hash = System.Web.HttpServerUtility.UrlTokenEncode(hasher.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", now, pkey))));
        return string.Format("ASC {0}:{1}:{2}", pkey, now, hash);
    }
}
</pre>

    <div id="php" class="header-gray">PHP generating token example</div>
    <pre>
function apisystem_authToken($pkey, $machinekey) {
    $now=gmdate('YmdHis');

    $authkey=hash_hmac('sha1', $now."\n".$pkey, $machinekey, true);
    $authkey=base64_encode($authkey);
    $authkey=str_replace(array("+", "/"), array("-", "_"), substr($authkey, 0, -1)).'1';

    return 'ASC '.$pkey.':'.$now.':'.$authkey;
}
</pre>

    <div id="bash" class="header-gray">Bash generating token example</div>
    <pre>
CreateAuthToken() {
    pkey="$1";
    machinekey=$(echo -n "$2");
    now=$(date +"%Y%m%d%H%M%S");
    authkey=$(echo -n -e "${now}\n${pkey}" | openssl dgst -sha1 -binary -mac HMAC -macopt key:$machinekey | sed -e 's/^.* //');
    authkey=$(echo -n "${authkey}" | base64);

    echo "ASC ${pkey}:${now}:${authkey}";
}
</pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
