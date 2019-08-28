<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Help.aspx.cs" MasterPageFile="Masters/BasicTemplate.Master" Inherits="ASC.Web.Sample.Help" %>

<%@ MasterType TypeName="ASC.Web.Sample.Masters.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <div>
        <h1>Help page</h1>

        <p>If you have any questions, visit the links below:</p>

        <ul class="help">
            <li>
                <a href="http://helpcenter.onlyoffice.com/" target="_blank" class="link underline">Help Center</a>
            </li>
            <li>
                <a href="http://cloud.onlyoffice.org/" target="_blank" class="link underline">SaaS Forum</a>
            </li>
            <li>
                <a href="http://dev.onlyoffice.org/" target="_blank" class="link underline">Server Forum</a>
            </li>
            <li>
                <a href="http://support.onlyoffice.com/" target="_blank" class="link underline">Support Contact Form</a>
            </li>
        </ul>
    </div>
</asp:Content>
