<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Callback handler
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Callback handler</span>
    </h1>

    <p>The <b>document editing service</b> informs the <b>document storage service</b> about status of the document editing using the <em>callbackUrl</em> from <a class="underline" href="<%= Url.Action("basic") %>">JavaScript API</a>.</p>
    <p>The <b>document editing service</b> will send POST request with the following information in body:</p>
    <ul>
        <li>
            <b>actions</b> - is an object received if the new user connected the document co-editing or disconnected from it. In the first case the <em>type</em> field value is <b>1</b> , in the other case - <b>0</b>. The <em>userid</em> field value is the identifier of the user who connected or disconnected from the document co-editing.
        </li>
        <li>
            <b>key</b> - identifier of the edited document
        </li>
        <li>
            <b>status</b> - status of the document. Can have the following values:
            <b>0</b> - no document with the key identifier could be found,
            <b>1</b> - document is being edited,
            <b>2</b> - document is ready for saving,
            <b>3</b> - document saving error has occurred,
            <b>4</b> - document is closed with no changes.
        </li>
        <li id="url">
            <b>url</b> - the link to the edited document to be saved with the document storage service. The link is present when the <em>status</em> value is equal to <b>2</b> or <b>3</b> only.
        </li>
        <li id="changesurl">
            <b>changesurl</b> - the link to the file with the document editing data used to track and display the document changes history. The link is present when the <em>status</em> value is equal to <b>2</b> or <b>3</b> only. The file must be saved and its address must be sent as <i>changesUrl</i> parameter using the <a class="underline" href="<%= Url.Action("methods") %>#setHistoryData">setHistoryData</a> method to show the changes corresponding to the specific document version.
        </li>
        <li id="changeshistory">
            <b>changeshistory</b> - the array of objects with the document changes history. The object is present when the status value is equal to <b>2</b> or <b>3</b> only. Must be sent as a property <em>changes</em> of the object sent as the argument to the <a class="underline" href="<%= Url.Action("methods") %>#refreshHistory">refreshHistory</a> method.
        </li>
        <li>
            <b>users</b> - identifier of the user who accessed or edited the document.
        </li>
    </ul>

    <p><em>Status</em> <b>1</b> is received every user connection to or disconnection from document co-editing.</p>
    <p><em>Status</em> <b>2</b> (<b>3</b>) is received 10 seconds after the document is closed for editing with the identifier of the user who was the last to send the changes to the document editing service.</p>
    <p><em>Status</em> <b>4</b> is received after the document is closed for editing with no changes by the last user.</p>

    <div id="status-1" class="header-gray">Sample of JSON object sent to the "callbackUrl" address by document editing service when two users are co-editing the document.</div>
    <pre>
{
    "actions": [{"type": 1, "userid": "78E1E841"}],
    "key": "key",
    "status": 1,
    "users": ["6D5A81D0", "78E1E841"],
}
</pre>

    <div id="status-2" class="header-gray">Sample of JSON object sent to the "callbackUrl" address by document editing service when the user changed the document and closed it for editing.</div>
    <pre>
{
    "key": "key",
    "status": 2,
    "url": "http://documentserver/url-to-edited-document.docx",
    "changesurl": "http://documentserver/url-to-changes.zip",
    "changeshistory": changeshistory,
    "users": ["6D5A81D0"],
}
</pre>

    <div id="status-4" class="header-gray">Sample of JSON object sent to the "callbackUrl" address by document editing service when the last user closed the document for editing without changes.</div>
    <pre>
{
    "key": "key",
    "status": 4,
}
</pre>

    <p>The <b>document storage service</b> must return the following response, otherwise the <b>document editor</b> will display an error message:</p>

    <div id="error-0" class="header-gray">Response from the document storage service</div>
    <pre>
{
    "error": 0
}
</pre>

    <p>The <b>document manager</b> and <b>document storage service</b> are either included to Community Server or must be implemented by the software integrators who use ONLYOFFICE™ Document Server on their own server.</p>

    <div id="csharp" class="header-gray">.Net (C#) document save example</div>
    <pre>
public class WebEditor : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string body;
        using (var reader = new StreamReader(context.Request.InputStream))
            body = reader.ReadToEnd();

        var fileData = new JavaScriptSerializer().Deserialize&lt;Dictionary&lt;string, object&gt;&gt;(body);
        if ((int) fileData["status"] == 2)
        {
            var req = WebRequest.Create((string) fileData["url"]);

            using (var stream = req.GetResponse().GetResponseStream())
            using (var fs = File.Open(PATH_FOR_SAVE, FileMode.Create))
            {
                var buffer = new byte[4096];
                int readed;
                while ((readed = stream.Read(buffer, 0, 4096)) != 0)
                    fs.Write(buffer, 0, readed);
            }
        }
        context.Response.Write("{\"error\":0}");
    }
}
</pre>
    <div class="note"><em>PATH_FOR_SAVE</em> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div id="java" class="header-gray">Java document save example</div>
    <pre>
public class IndexServlet extends HttpServlet {
    @Override
    protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
        PrintWriter writer = response.getWriter();

        Scanner scanner = new Scanner(request.getInputStream()).useDelimiter("\\A");
        String body = scanner.hasNext() ? scanner.next() : "";

        JSONObject jsonObj = (JSONObject) new JSONParser().parse(body);

        if((long) jsonObj.get("status") == 2)
        {
            String downloadUri = (String) jsonObj.get("url");

            URL url = new URL(downloadUri);
            java.net.HttpURLConnection connection = (java.net.HttpURLConnection) url.openConnection();
            InputStream stream = connection.getInputStream();

            File savedFile = new File(pathForSave);
            try (FileOutputStream out = new FileOutputStream(savedFile)) {
                int read;
                final byte[] bytes = new byte[1024];
                while ((read = stream.read(bytes)) != -1) {
                    out.write(bytes, 0, read);
                }

                out.flush();
            }

            connection.disconnect();
        }
        writer.write("{\"error\":0}");
    }
}
</pre>
    <div class="note"><em>pathForSave</em> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div id="nodejs" class="header-gray">Node.js document save example</div>
    <pre>
var fs = require("fs");

app.post("/track", function (req, res) {

    var updateFile = function (response, body, path) {
        if (body.status == 2)
        {
            var file = syncRequest("GET", body.url);
            fs.writeFileSync(path, file.getBody());
        }

        response.write("{\"error\":0}");
        response.end();
    }

    var readbody = function (request, response, path) {
        var content = "";
        request.on("data", function (data) {
            content += data;
        });
        request.on("end", function () {
            var body = JSON.parse(content);
            updateFile(response, body, path);
        });
    }

    if (req.body.hasOwnProperty("status")) {
        updateFile(res, req.body, pathForSave);
    } else {
        readbody(req, res, pathForSave)
    }
});
</pre>
    <div class="note"><em>pathForSave</em> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div id="php" class="header-gray">PHP document save example</div>
    <pre>
&lt;?php

if (($body_stream = file_get_contents("php://input"))===FALSE){
    echo "Bad Request";
}

$data = json_decode($body_stream, TRUE);

if ($data["status"] == 2){
    $downloadUri = $data["url"];
        
    if (($new_data = file_get_contents($downloadUri))===FALSE){
        echo "Bad Response";
    } else {
        file_put_contents($path_for_save, $new_data, LOCK_EX);
    }
}
echo "{\"error\":0}";

?&gt;
</pre>
    <div class="note"><em>$path_for_save</em> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div id="ruby" class="header-gray">Ruby document save example</div>
    <pre>
class ApplicationController < ActionController::Base
    def index
        body = request.body.read

        file_data = JSON.parse(body)
        status = file_data["status"].to_i

        if status == 2
            download_uri = file_data["url"]
            uri = URI.parse(download_uri)
            http = Net::HTTP.new(uri.host, uri.port)

            if download_uri.start_with?("https")
                http.use_ssl = true
                http.verify_mode = OpenSSL::SSL::VERIFY_NONE
            end

            req = Net::HTTP::Get.new(uri.request_uri)
            res = http.request(req)
            data = res.body

            File.open(path_for_save, "wb") do |file|
                file.write(data)
            end
        end
        render :text => "{\"error\":0}"
    end
end
</pre>
    <div class="note"><em>path_for_save</em> is the absolute path to your computer folder where the file will be saved including the file name.</div>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>

