/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

/*
 Copyright (c) 2003-2014, CKSource - Frederico Knabben. All rights reserved.
 For licensing, see LICENSE.md or http://ckeditor.com/license
*/
(function(){function c(b){var c=this instanceof CKEDITOR.ui.dialog.checkbox;b.hasAttribute(this.id)&&(b=b.getAttribute(this.id),c?this.setValue(e[this.id]["true"]==b.toLowerCase()):this.setValue(b))}function d(b){var c=""===this.getValue(),a=this instanceof CKEDITOR.ui.dialog.checkbox,d=this.getValue();c?b.removeAttribute(this.att||this.id):a?b.setAttribute(this.id,e[this.id][d]):b.setAttribute(this.att||this.id,d)}var e={scrolling:{"true":"yes","false":"no"},frameborder:{"true":"1","false":"0"}};
CKEDITOR.dialog.add("iframe",function(b){var f=b.lang.iframe,a=b.lang.common,e=b.plugins.dialogadvtab;return{title:f.title,minWidth:350,minHeight:260,onShow:function(){this.fakeImage=this.iframeNode=null;var a=this.getSelectedElement();a&&(a.data("cke-real-element-type")&&"iframe"==a.data("cke-real-element-type"))&&(this.fakeImage=a,this.iframeNode=a=b.restoreRealElement(a),this.setupContent(a))},onOk:function(){var a;a=this.fakeImage?this.iframeNode:new CKEDITOR.dom.element("iframe");var c={},d=
{};this.commitContent(a,c,d);a=b.createFakeElement(a,"cke_iframe","iframe",!0);a.setAttributes(d);a.setStyles(c);this.fakeImage?(a.replace(this.fakeImage),b.getSelection().selectElement(a)):b.insertElement(a)},contents:[{id:"info",label:a.generalTab,accessKey:"I",elements:[{type:"vbox",padding:0,children:[{id:"src",type:"text",label:a.url,required:!0,validate:CKEDITOR.dialog.validate.notEmpty(f.noUrl),setup:c,commit:d}]},{type:"hbox",children:[{id:"width",type:"text",requiredContent:"iframe[width]",
style:"width:100%",labelLayout:"vertical",label:a.width,validate:CKEDITOR.dialog.validate.htmlLength(a.invalidHtmlLength.replace("%1",a.width)),setup:c,commit:d},{id:"height",type:"text",requiredContent:"iframe[height]",style:"width:100%",labelLayout:"vertical",label:a.height,validate:CKEDITOR.dialog.validate.htmlLength(a.invalidHtmlLength.replace("%1",a.height)),setup:c,commit:d},{id:"align",type:"select",requiredContent:"iframe[align]","default":"",items:[[a.notSet,""],[a.alignLeft,"left"],[a.alignRight,
"right"],[a.alignTop,"top"],[a.alignMiddle,"middle"],[a.alignBottom,"bottom"]],style:"width:100%",labelLayout:"vertical",label:a.align,setup:function(a,b){c.apply(this,arguments);if(b){var d=b.getAttribute("align");this.setValue(d&&d.toLowerCase()||"")}},commit:function(a,b,c){d.apply(this,arguments);this.getValue()&&(c.align=this.getValue())}}]},{type:"hbox",widths:["50%","50%"],children:[{id:"scrolling",type:"checkbox",requiredContent:"iframe[scrolling]",label:f.scrolling,setup:c,commit:d},{id:"frameborder",
type:"checkbox",requiredContent:"iframe[frameborder]",label:f.border,setup:c,commit:d}]},{type:"hbox",widths:["50%","50%"],children:[{id:"name",type:"text",requiredContent:"iframe[name]",label:a.name,setup:c,commit:d},{id:"title",type:"text",requiredContent:"iframe[title]",label:a.advisoryTitle,setup:c,commit:d}]},{id:"longdesc",type:"text",requiredContent:"iframe[longdesc]",label:a.longDescr,setup:c,commit:d}]},e&&e.createAdvancedTab(b,{id:1,classes:1,styles:1},"iframe")]}})})();