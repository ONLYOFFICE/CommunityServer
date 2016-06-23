<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Alfresco ONLYOFFICE™ integration plugin
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Alfresco ONLYOFFICE™ integration plugin</span>
    </h1>

    <p class="dscr">This <a href="https://github.com/ONLYOFFICE/onlyoffice-alfresco" class="underline" target="_blank">plugin</a> enables users to edit office documents from Alfresco Share using ONLYOFFICE™ Document Server. Currently the following document formats can be opened and edited with this plugin: DOCX, XLSX, PPTX.</p>
    <p>The plugin will create a new <b>Edit in ONLYOFFICE</b> menu option within the document library for Office documents. This allows multiple users to collaborate in real time and to save back those changes to Alfresco.</p>
    <p>Tested with Enterprise 5.0.* and Community 5.1.*</p>


    <h2>Compiling</h2>
    <p>You will need:</p>
    <ul>
        <li>Gradle,</li>
        <li>Java 7 SDK or above,</li>
        <li>Parashift's Alfresco amp plugin which can be found <a href="https://bitbucket.org/parashift/alfresco-amp-plugin" class="underline" target="_blank">here</a>,</li>
        <li>Run <em>gradle amp</em> from the <em>repo</em> and <em>share</em> directories.</li>
    </ul>


    <h2>Installing ONLYOFFICE™ Document Server</h2>
    <p>You will need an instance of ONLYOFFICE™ Document Server that is resolvable and connectable both from Alfresco and any end clients (version 3.0 and later are supported for use with the plugin). If that is not the case, use the official ONLYOFFICE™ Document Server documentation page: <a href="http://helpcenter.onlyoffice.com/server/linux/document/linux-installation.aspx" class="underline">Document Server for Linux</a>. ONLYOFFICE™ Document Server must also be able to POST to Alfresco directly.</p>
    <p>The easiest way to start an instance of ONLYOFFICE™ Document Server is to use <a href="https://github.com/ONLYOFFICE/Docker-DocumentServer" class="underline">Docker</a>.</p>


    <h2>Installing Alfresco ONLYOFFICE™ integration plugin</h2>
    <p>To start using ONLYOFFICE™ Document Server with Alfresco, the following steps must be performed for Ubuntu 14.04:</p>
    <ol>
        <li>Remove gradle in case it has already been installed (it is needed to install the latest available version later at the next step):
            <span class="commandline">sudo apt-get remove gradle</span>
        </li>
        <li>Add the repository and install the latest version:
            <span class="commandline">sudo add-apt-repository ppa:cwchien/gradle
sudo apt-get update
sudo apt-get install gradle</span>
        </li>
        <li>The latest stable Oracle Java version is necessary for the successful build. If you do not have it installed, use the following commands to install Oracle Java 8:
            <span class="commandline">sudo add-apt-repository ppa:webupd8team/java
sudo apt-get update
sudo apt-get install oracle-java8-installer</span>
        </li>
        <li>Switch Java alternatives to Oracle Java:
            <span class="commandline">sudo update-alternatives --config java
sudo update-alternatives --config javac
sudo update-alternatives --config javaws</span>
        </li>
        <li>Build the necessary dependencies:
            <span class="commandline">git clone https://github.com/yeyan/alfresco-amp-plugin.git
cd alfresco-amp-plugin
gradle publish</span>
        </li>
        <li>Download the Alfresco ONLYOFFICE™ integration plugin source code:
            <span class="commandline">cd ..
git clone https://github.com/ONLYOFFICE/onlyoffice-alfresco.git</span>
        </li>
        <li>Compile packages in the <em>repo</em> and <em>share</em> directories:
            <span class="commandline">cd onlyoffice-alfresco/repo/
gradle amp
cd ../share/
gradle amp</span>
        </li>
        <li>Upload the compiled packages from <em>./build/amp</em> to the <em>amps/</em> and <em>amps_share/</em> directories accordingly for your Alfresco installation.</li>
        <li>Run the <em>bin/apply_amps.sh</em> script in Alfresco installation. You will see the two new modules being installed during the installation process. Press Enter to continue the installation.</li>
        <li>Add the <b>onlyoffice.url</b> property to <em>alfresco-global.properties</em>:
            <span class="commandline">onlyoffice.url=http://documentserver/</span>
        </li>
        <li>Restart Alfresco:
            <span class="commandline">./alfresco.sh stop
./alfresco.sh start</span>
        </li>
    </ol>
    <p>The module can be checked in administrator tools in Alfresco 5.1 or at <em>/share/page/modules/deploy</em> in Alfresco 5.0.</p>


    <h2>How it works</h2>
    <p>User navigates to a document within Alfresco Share and selects the <b>Edit in ONLYOFFICE</b> menu option.</p>
    <p>Alfresco Share makes a request to the repo end (URL of the form: <em>/parashift/onlyoffice/prepare?nodeRef={nodeRef}</em>).</p>
    <p>Alfresco Repo end prepares a JSON object for the Share with the following properties:</p>
    <ul>
        <li><b>docUrl</b> - the URL that ONLYOFFICE™ Document Server uses to download the document (includes the <em>alf_ticket</em> of the current user);</li>
        <li><b>callbackUrl</b> - the URL that ONLYOFFICE™ Document Server informs about status of the document editing;</li>
        <li><b>onlyofficeUrl</b> - the URL that the client needs to reply to ONLYOFFICE™ Document Server (provided by the onlyoffice.url property);</li>
        <li><b>key</b> - the UUID+Modified Timestamp to instruct ONLYOFFICE™ Document Server whether to download the document again or not;</li>
        <li><b>docTitle</b> - the document Title (name).</li>
    </ul>
    <p>Alfresco Share takes this object and constructs a page from a freemarker template, filling in all of those values so that the client browser can load up the editor.</p>
    <p>The client browser makes a request for the javascript library from ONLYOFFICE™ Document Server and sends ONLYOFFICE™ Document Server the docEditor configuration with the above properties.</p>
    <p>Then ONLYOFFICE™ Document Server downloads the document from Alfresco and the user begins editing.</p>
    <p>ONLYOFFICE™ Document Server sends a POST request to the <em>callback</em> URL to inform Alfresco that a user is editing the document.</p>
    <p>Alfresco locks the document, but still allows other users with write access the ability to collaborate in real time with ONLYOFFICE™ Document Server by leaving the Action present.</p>
    <p>When all users and client browsers are done with editing, they close the editing window.</p>
    <p>After 10 seconds of inactivity, ONLYOFFICE™ Document Server sends a POST to the <em>callback</em> URL letting Alfresco know that the clients have finished editing the document and closed it.</p>
    <p>Alfresco downloads the new version of the document, replacing the old one.</p>

    <br />
    <p>Download the Alfresco ONLYOFFICE™ integration plugin <a href="https://github.com/ONLYOFFICE/onlyoffice-alfresco" class="underline">here</a>.</p>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
