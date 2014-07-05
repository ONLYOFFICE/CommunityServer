<%@OutputCache Duration="120"  VaryByParam="culture"%>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommonPopupContainer.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.CommonPopupContainer" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div id="commonPopupContainer" style="display: none">
    <sc:Container ID="_popup" runat="server">
        <Header>
            <span class="commonPopupHeaderTitle"></span>
        </Header>
        <Body>
            <div class="commonPopupContent"></div>
        </Body>
    </sc:Container>
</div>