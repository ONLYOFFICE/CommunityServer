/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
 Copyright (c) 2003-2014, CKSource - Frederico Knabben. All rights reserved.
 For licensing, see LICENSE.md or http://ckeditor.com/license
*/
CKEDITOR.dialog.add("textfield",function(b){function e(a){var a=a.element,c=this.getValue();c?a.setAttribute(this.id,c):a.removeAttribute(this.id)}function f(a){this.setValue(a.hasAttribute(this.id)&&a.getAttribute(this.id)||"")}var g={email:1,password:1,search:1,tel:1,text:1,url:1};return{title:b.lang.forms.textfield.title,minWidth:350,minHeight:150,onShow:function(){delete this.textField;var a=this.getParentEditor().getSelection().getSelectedElement();if(a&&"input"==a.getName()&&(g[a.getAttribute("type")]||
!a.getAttribute("type")))this.textField=a,this.setupContent(a)},onOk:function(){var a=this.getParentEditor(),c=this.textField,b=!c;b&&(c=a.document.createElement("input"),c.setAttribute("type","text"));c={element:c};b&&a.insertElement(c.element);this.commitContent(c);b||a.getSelection().selectElement(c.element)},onLoad:function(){this.foreach(function(a){if(a.getValue&&(a.setup||(a.setup=f),!a.commit))a.commit=e})},contents:[{id:"info",label:b.lang.forms.textfield.title,title:b.lang.forms.textfield.title,
elements:[{type:"hbox",widths:["50%","50%"],children:[{id:"_cke_saved_name",type:"text",label:b.lang.forms.textfield.name,"default":"",accessKey:"N",setup:function(a){this.setValue(a.data("cke-saved-name")||a.getAttribute("name")||"")},commit:function(a){a=a.element;this.getValue()?a.data("cke-saved-name",this.getValue()):(a.data("cke-saved-name",!1),a.removeAttribute("name"))}},{id:"value",type:"text",label:b.lang.forms.textfield.value,"default":"",accessKey:"V",commit:function(a){if(CKEDITOR.env.ie&&
!this.getValue()){var c=a.element,d=new CKEDITOR.dom.element("input",b.document);c.copyAttributes(d,{value:1});d.replace(c);a.element=d}else e.call(this,a)}}]},{type:"hbox",widths:["50%","50%"],children:[{id:"size",type:"text",label:b.lang.forms.textfield.charWidth,"default":"",accessKey:"C",style:"width:50px",validate:CKEDITOR.dialog.validate.integer(b.lang.common.validateNumberFailed)},{id:"maxLength",type:"text",label:b.lang.forms.textfield.maxChars,"default":"",accessKey:"M",style:"width:50px",
validate:CKEDITOR.dialog.validate.integer(b.lang.common.validateNumberFailed)}],onLoad:function(){CKEDITOR.env.ie7Compat&&this.getElement().setStyle("zoom","100%")}},{id:"type",type:"select",label:b.lang.forms.textfield.type,"default":"text",accessKey:"M",items:[[b.lang.forms.textfield.typeEmail,"email"],[b.lang.forms.textfield.typePass,"password"],[b.lang.forms.textfield.typeSearch,"search"],[b.lang.forms.textfield.typeTel,"tel"],[b.lang.forms.textfield.typeText,"text"],[b.lang.forms.textfield.typeUrl,
"url"]],setup:function(a){this.setValue(a.getAttribute("type"))},commit:function(a){var c=a.element;if(CKEDITOR.env.ie){var d=c.getAttribute("type"),e=this.getValue();d!=e&&(d=CKEDITOR.dom.element.createFromHtml('<input type="'+e+'"></input>',b.document),c.copyAttributes(d,{type:1}),d.replace(c),a.element=d)}else c.setAttribute("type",this.getValue())}}]}]}});