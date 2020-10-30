<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" MasterPageFile="Masters/BasicTemplate.Master" Inherits="ASC.Web.Sample.DefaultPage" %>

<%@ MasterType TypeName="ASC.Web.Sample.Masters.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <div>
        <h1>How to create your own module</h1>
        
        <ol>
            <li>
                <p>
                    Create an ASP.NET Web Application (<span class="bg">ASC.Web.Sample</span>)
                    project and put it to the <span class="bg">...web\studio\ASC.Web.Studio\Products\Sample</span> folder
                </p>
                <div class="note">The output dll file name must be "ASC.Web.*.dll"</div>
            </li>
            <li>
                <p>Connect the required references from <span class="bg">...\web\studio\ASC.Web.Studio\bin\</span></p>
<pre><code>ASC.Common.dll
ASC.Core.Common.dll
ASC.Data.Storage.dll
ASC.Web.Core.dll
ASC.Web.Studio.dll</code></pre>
            </li>
            <li>
                <p>Implement the <span class="bg">IProduct</span> interface in the <span class="bg">ProductEntryPoint.cs</span> file</p>
                <div class="note">The ProductID must be unique Guid (in VS2012 is generated as TOOLS->GUID->New GUID)</div>
            </li>
            <li>
                <p>Add the following in the <span class="bg">AssemblyInfo.cs</span> file:</p>
<pre><code>[assembly: Product(typeof(ASC.Web.Sample.Configuration.ProductEntryPoint))]</code></pre>
            </li>
            <li>
                <p class="none">Inherit the Master from <span class="bg">web\studio\ASC.Web.Studio\Masters\BaseTemplate.master</span></p>
            </li>
            <li>
                <p>Set the output path in the project properties as</p>
<pre><code>&lt;OutputPath&gt;..\..\bin\&lt;/OutputPath&gt;</code></pre>
                <p class="top">so that the builds were created at the <span class="bg">web\studio\ASC.Web.Studio\bin</span> folder</p>
            </li>
            <li>
                <p>The project can be built manually or using the builder.</p>
                <p>For the latter add the following lines to the <span class="bg">build\msbuild\build.proj</span> file:</p>
<pre><code>&lt;ProjectToBuild Include="$(ASCDir)web\studio\ASC.Web.Studio\Products\Sample\ASC.Web.Sample.csproj"/&gt;</code></pre>
                <p class="top">and run the <span class="bg">build\Build.bat</span> file</p>
            </li>
            <li>
                <p class="none">
                    After the build run the website at the <span class="bg">localhost:port</span> address,
                    go to the "Modules & Tools" Settings page (<span class="bg">http://localhost:port/Management.aspx?type=2</span>)
                    and enable the new Sample module.
                    It will be available in the portal header drop-down menu afterwards
                    or using the direct link: http://localhost:port/Products/Sample/Default.aspx
                </p>
            </li>
        </ol>
    </div>
</asp:Content>
