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
(function(){function d(c,d){var b;try{b=c.getSelection().getRanges()[0]}catch(f){return null}b.shrink(CKEDITOR.SHRINK_TEXT);return c.elementPath(b.getCommonAncestor()).contains(d,1)}function e(c,e){var b=c.lang.liststyle;if("bulletedListStyle"==e)return{title:b.bulletedTitle,minWidth:300,minHeight:50,contents:[{id:"info",accessKey:"I",elements:[{type:"select",label:b.type,id:"type",align:"center",style:"width:150px",items:[[b.notset,""],[b.circle,"circle"],[b.disc,"disc"],[b.square,"square"]],setup:function(a){this.setValue(a.getStyle("list-style-type")||
h[a.getAttribute("type")]||a.getAttribute("type")||"")},commit:function(a){var b=this.getValue();b?a.setStyle("list-style-type",b):a.removeStyle("list-style-type")}}]}],onShow:function(){var a=this.getParentEditor();(a=d(a,"ul"))&&this.setupContent(a)},onOk:function(){var a=this.getParentEditor();(a=d(a,"ul"))&&this.commitContent(a)}};if("numberedListStyle"==e){var g=[[b.notset,""],[b.lowerRoman,"lower-roman"],[b.upperRoman,"upper-roman"],[b.lowerAlpha,"lower-alpha"],[b.upperAlpha,"upper-alpha"],
[b.decimal,"decimal"]];(!CKEDITOR.env.ie||7<CKEDITOR.env.version)&&g.concat([[b.armenian,"armenian"],[b.decimalLeadingZero,"decimal-leading-zero"],[b.georgian,"georgian"],[b.lowerGreek,"lower-greek"]]);return{title:b.numberedTitle,minWidth:300,minHeight:50,contents:[{id:"info",accessKey:"I",elements:[{type:"hbox",widths:["25%","75%"],children:[{label:b.start,type:"text",id:"start",validate:CKEDITOR.dialog.validate.integer(b.validateStartNumber),setup:function(a){this.setValue(a.getFirst(f).getAttribute("value")||
a.getAttribute("start")||1)},commit:function(a){var b=a.getFirst(f),c=b.getAttribute("value")||a.getAttribute("start")||1;a.getFirst(f).removeAttribute("value");var d=parseInt(this.getValue(),10);isNaN(d)?a.removeAttribute("start"):a.setAttribute("start",d);a=b;b=c;for(d=isNaN(d)?1:d;(a=a.getNext(f))&&b++;)a.getAttribute("value")==b&&a.setAttribute("value",d+b-c)}},{type:"select",label:b.type,id:"type",style:"width: 100%;",items:g,setup:function(a){this.setValue(a.getStyle("list-style-type")||h[a.getAttribute("type")]||
a.getAttribute("type")||"")},commit:function(a){var b=this.getValue();b?a.setStyle("list-style-type",b):a.removeStyle("list-style-type")}}]}]}],onShow:function(){var a=this.getParentEditor();(a=d(a,"ol"))&&this.setupContent(a)},onOk:function(){var a=this.getParentEditor();(a=d(a,"ol"))&&this.commitContent(a)}}}}var f=function(c){return c.type==CKEDITOR.NODE_ELEMENT&&c.is("li")},h={a:"lower-alpha",A:"upper-alpha",i:"lower-roman",I:"upper-roman",1:"decimal",disc:"disc",circle:"circle",square:"square"};
CKEDITOR.dialog.add("numberedListStyle",function(c){return e(c,"numberedListStyle")});CKEDITOR.dialog.add("bulletedListStyle",function(c){return e(c,"bulletedListStyle")})})();