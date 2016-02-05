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
        <li>
            <b>users</b> - identifier of the user who accessed or edited the document.
        </li>
        <li>
            <b>url</b> - the link to the edited document to be saved with the document storage service.
        </li>
    </ul>

    <p>Status <b>1</b> is received every user connection to or disconnection from document co-editing.</p>
    <p>Status <b>2</b> (<b>3</b>) is received 10 seconds after the document is closed for editing by the last user.</p>

    <div class="header-gray">Sample of JSON object sent to the 'callbackUrl' address by document editing service</div>
    <pre>
{
    'key': 'key',
    'status': 2,
    'users': ['6D5A81D0-3482-4E8C-871D-BA197ABAADE2', '17173203-CC82-442A-B12D-DAE9940D7334'],
    'url': 'http://documentserver/url-to-edited-document.docx',
}
</pre>

    <p>The <b>document storage service</b> must return the following response, otherwise the <b>document editor</b> will display an error message:</p>

    <div class="header-gray">Response from the document storage service</div>
    <pre>
{
    'error': 0
}
</pre>

    <p>The <b>document manager</b> and <b>document storage service</b> are either included to Community Server or must be implemented by the software integrators who use ONLYOFFICE™ Document Server on their own server.</p>

    <div class="header-gray">.Net (C#) document save example</div>
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
    <div class="note"><i>PATH_FOR_SAVE</i> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div class="header-gray">Java document save example</div>
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
    <div class="note"><i>pathForSave</i> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div class="header-gray">Node.js document save example</div>
    <pre>
app.post("/track", function (req, res) {

    var updateFile = function (response, body, path) {
        if (body.status == 2)
        {
            var file = syncRequest("GET", body.url);
            fileSystem.writeFileSync(path, file.getBody());
        }

        response.write("{\"error\":0}");
        response.end();
    }

    var readbody = function (request, response, path) {
        var content = "";
        request.on('data', function (data) {
            content += data;
        });
        request.on('end', function () {
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
    <div class="note"><i>pathForSave</i> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div class="header-gray">PHP document save example</div>
    <pre>
&lt;?php

if (($body_stream = file_get_contents('php://input'))===FALSE){
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
    <div class="note"><i>$path_for_save</i> is the absolute path to your computer folder where the file will be saved including the file name.</div>

    <div class="header-gray">Ruby document save example</div>
    <pre>
class ApplicationController < ActionController::Base
    def index
        body = request.body.read

        file_data = JSON.parse(body)
        status = file_data['status'].to_i

        if status == 2
            download_uri = file_data['url']
            uri = URI.parse(download_uri)
            http = Net::HTTP.new(uri.host, uri.port)

            if download_uri.start_with?('https')
                http.use_ssl = true
                http.verify_mode = OpenSSL::SSL::VERIFY_NONE
            end

            req = Net::HTTP::Get.new(uri.request_uri)
            res = http.request(req)
            data = res.body

            File.open(path_for_save, 'wb') do |file|
                file.write(data)
            end
        end
        render :text => '{"error":0}'
    end
end
</pre>
    <div class="note"><i>path_for_save</i> is the absolute path to your computer folder where the file will be saved including the file name.</div>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
