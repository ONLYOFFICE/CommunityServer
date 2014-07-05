<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserSelector.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserSelector" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<div id="usrdialog_<%=_selectorID%>" style="display: none;">
<sc:Container runat="server" ID="_container">
<Header>
    <%=HttpUtility.HtmlEncode(this.Title??"")%>
</Header>
<Body>


    <%if (MobileDetector.IsMobile)
      {%>
    <div  class="userSelectorMobVer"><%=HttpUtility.HtmlEncode(this.SelectedUserListTitle ?? "")%></div>
    <select id="selectmobile_<%=_selectorID%>" multiple="multiple" size="15"
        class="comboBox" onchange="<%=BehaviorID%>.ChangeMobileSelect();">
    </select>
    <%}
      else
      {%>

    <div class="clearFix studioUserSelector">
        <div class="leftBox">
            <div class="title header-base medium bold"><%=HttpUtility.HtmlEncode(this.UserListTitle ?? "")%><span id="selectAll_usrdialog_rightBox" class="link dotline gray" onclick="<%=BehaviorID%>.SelectAll (); <%=BehaviorID%>.ClearUsrdialogLeftBox();"><%=Resources.Resource.SelectAllButton%></span></div>
            <div id="usrdialog_leftBox_<%=_selectorID%>" class="content borderBase">            
            </div>
            <div class="borderBase filerUsers">
				<div id="employeeFilterInputCloseImage" class="employeeFilterInputCloseImageGrey float-right"					
					onclick="employeeFilterInputCloseImageClick();">&nbsp;</div>
				<input type="text" id="employeeFilterInput" class="employeeFilterInputGreyed"
					value="<%=Resources.Resource.QuickSearch%>"
					title="<%=Resources.Resource.QuickSearch%>"
					onkeyup="filterEmployees();"					
					onfocus="onEmployeeFilterInputFocus();"
					onblur="onEmployeeFilterInputFocusLost();" />
            </div>
        </div>
        <div class="centerBox userSelectorRightLeft">            
            <div>
                <a href="javascript:void(0);" onclick="<%=BehaviorID%>.Select(); filterEmployees(); return false;">                    
                    <img src="<%=WebImageSupplier.GetAbsoluteWebPath("to_right.png")%>" alt="" />
                </a>
            </div>
            <div class="userSelectorLeft">
                <a href="javascript:void(0);" onclick="<%=BehaviorID%>.Unselect(); filterEmployees(); return false;">
                    <img src="<%=WebImageSupplier.GetAbsoluteWebPath("to_left.png")%>" alt="" />
                </a>
            </div>
        </div>
        <div class="rightBox">
            <div class="title header-base medium bold"><%=HttpUtility.HtmlEncode(this.SelectedUserListTitle ?? "")%><span id="clear_usrdialog_rightBox" class="link dotline gray" onclick="<%=BehaviorID%>.ClearSelection (); <%=BehaviorID%>.ClearUsrdialogRightBox();"><%=Resources.Resource.ClearButton%></span></div>
            <div id="usrdialog_rightBox_<%=_selectorID%>" class="content borderBase">
            </div>
        </div>
    </div>

    <%} %>
    
    <div class="clearFix">
        <%=CustomBottomHtml%>
    </div>
    <div class="clearFix middle-button-container">
        <a class="button blue middle" href="javascript:void(0);" onclick="<%=BehaviorID%>.ApplyAndCloseDialog(); return false;"><%=Resources.Resource.SaveButton%></a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle" href="javascript:void(0);" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;"><%=Resources.Resource.CancelButton%></a>        
    </div>    
</Body>
</sc:Container>
</div>