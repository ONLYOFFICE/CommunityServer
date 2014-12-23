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
(function(a){CKEDITOR.config.jqueryOverrideVal="undefined"==typeof CKEDITOR.config.jqueryOverrideVal?!0:CKEDITOR.config.jqueryOverrideVal;"undefined"!=typeof a&&(a.extend(a.fn,{ckeditorGet:function(){var a=this.eq(0).data("ckeditorInstance");if(!a)throw"CKEditor is not initialized yet, use ckeditor() with a callback.";return a},ckeditor:function(g,d){if(!CKEDITOR.env.isCompatible)throw Error("The environment is incompatible.");if(!a.isFunction(g))var k=d,d=g,g=k;var i=[],d=d||{};this.each(function(){var b=
a(this),c=b.data("ckeditorInstance"),f=b.data("_ckeditorInstanceLock"),h=this,j=new a.Deferred;i.push(j.promise());if(c&&!f)g&&g.apply(c,[this]),j.resolve();else if(f)c.once("instanceReady",function(){setTimeout(function(){c.element?(c.element.$==h&&g&&g.apply(c,[h]),j.resolve()):setTimeout(arguments.callee,100)},0)},null,null,9999);else{if(d.autoUpdateElement||"undefined"==typeof d.autoUpdateElement&&CKEDITOR.config.autoUpdateElement)d.autoUpdateElementJquery=!0;d.autoUpdateElement=!1;b.data("_ckeditorInstanceLock",
!0);c=a(this).is("textarea")?CKEDITOR.replace(h,d):CKEDITOR.inline(h,d);b.data("ckeditorInstance",c);c.on("instanceReady",function(d){var e=d.editor;setTimeout(function(){if(e.element){d.removeListener();e.on("dataReady",function(){b.trigger("dataReady.ckeditor",[e])});e.on("setData",function(a){b.trigger("setData.ckeditor",[e,a.data])});e.on("getData",function(a){b.trigger("getData.ckeditor",[e,a.data])},999);e.on("destroy",function(){b.trigger("destroy.ckeditor",[e])});e.on("save",function(){a(h.form).submit();
return!1},null,null,20);if(e.config.autoUpdateElementJquery&&b.is("textarea")&&a(h.form).length){var c=function(){b.ckeditor(function(){e.updateElement()})};a(h.form).submit(c);a(h.form).bind("form-pre-serialize",c);b.bind("destroy.ckeditor",function(){a(h.form).unbind("submit",c);a(h.form).unbind("form-pre-serialize",c)})}e.on("destroy",function(){b.removeData("ckeditorInstance")});b.removeData("_ckeditorInstanceLock");b.trigger("instanceReady.ckeditor",[e]);g&&g.apply(e,[h]);j.resolve()}else setTimeout(arguments.callee,
100)},0)},null,null,9999)}});var f=new a.Deferred;this.promise=f.promise();a.when.apply(this,i).then(function(){f.resolve()});this.editor=this.eq(0).data("ckeditorInstance");return this}}),CKEDITOR.config.jqueryOverrideVal&&(a.fn.val=CKEDITOR.tools.override(a.fn.val,function(g){return function(d){if(arguments.length){var k=this,i=[],f=this.each(function(){var b=a(this),c=b.data("ckeditorInstance");if(b.is("textarea")&&c){var f=new a.Deferred;c.setData(d,function(){f.resolve()});i.push(f.promise());
return!0}return g.call(b,d)});if(i.length){var b=new a.Deferred;a.when.apply(this,i).done(function(){b.resolveWith(k)});return b.promise()}return f}var f=a(this).eq(0),c=f.data("ckeditorInstance");return f.is("textarea")&&c?c.getData():g.call(f)}})))})(window.jQuery);