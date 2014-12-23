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
CKEDITOR.dialog.add("textarea",function(b){return{title:b.lang.forms.textarea.title,minWidth:350,minHeight:220,onShow:function(){delete this.textarea;var a=this.getParentEditor().getSelection().getSelectedElement();a&&"textarea"==a.getName()&&(this.textarea=a,this.setupContent(a))},onOk:function(){var a,b=this.textarea,c=!b;c&&(a=this.getParentEditor(),b=a.document.createElement("textarea"));this.commitContent(b);c&&a.insertElement(b)},contents:[{id:"info",label:b.lang.forms.textarea.title,title:b.lang.forms.textarea.title,
elements:[{id:"_cke_saved_name",type:"text",label:b.lang.common.name,"default":"",accessKey:"N",setup:function(a){this.setValue(a.data("cke-saved-name")||a.getAttribute("name")||"")},commit:function(a){this.getValue()?a.data("cke-saved-name",this.getValue()):(a.data("cke-saved-name",!1),a.removeAttribute("name"))}},{type:"hbox",widths:["50%","50%"],children:[{id:"cols",type:"text",label:b.lang.forms.textarea.cols,"default":"",accessKey:"C",style:"width:50px",validate:CKEDITOR.dialog.validate.integer(b.lang.common.validateNumberFailed),
setup:function(a){this.setValue(a.hasAttribute("cols")&&a.getAttribute("cols")||"")},commit:function(a){this.getValue()?a.setAttribute("cols",this.getValue()):a.removeAttribute("cols")}},{id:"rows",type:"text",label:b.lang.forms.textarea.rows,"default":"",accessKey:"R",style:"width:50px",validate:CKEDITOR.dialog.validate.integer(b.lang.common.validateNumberFailed),setup:function(a){this.setValue(a.hasAttribute("rows")&&a.getAttribute("rows")||"")},commit:function(a){this.getValue()?a.setAttribute("rows",
this.getValue()):a.removeAttribute("rows")}}]},{id:"value",type:"textarea",label:b.lang.forms.textfield.value,"default":"",setup:function(a){this.setValue(a.$.defaultValue)},commit:function(a){a.$.value=a.$.defaultValue=this.getValue()}}]}]}});