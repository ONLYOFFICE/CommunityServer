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

;window.DefaultMobile = (function (DefaultMobile) {  if (!DefaultMobile) {    console.log('Default.default: has no DefaultMobile');    return DefaultMobile;  }  var lastFilterValue = '';    DefaultMobile.filter_people_contacts = function (evt) {    var       $page = null,      hasCorrect = false,      item = null,      $items = null,      itemsInd = 0,      index = null,      $indexes = null,      indexesInd = 0,      value = '',      filtervalue = evt.target.value;    if (typeof filtervalue !== 'string' || (filtervalue = (filtervalue.replace(/^\s+|\s+$/g, '')).toLowerCase()) === lastFilterValue) {      return undefined;    }    $page = $('div.ui-page-active:first');    if (filtervalue) {      $page.find('form.search-form:first').addClass('active');    } else {      $page.find('form.search-form:first').removeClass('active');    }    $indexes = $page.find('li.item-index');    indexesInd = $indexes.length;    while (indexesInd--) {      index = $indexes[indexesInd];      hasCorrect = false;      $items = $(index).find('li.item-persone');      itemsInd = $items.length;      while (itemsInd--) {        item = $items[itemsInd];        value = item.getAttribute('data-itemname');        if (!value) {          continue;        }        if (value.toLowerCase().indexOf(filtervalue) === -1) {          $(item).addClass('uncorrect-item');        } else {          hasCorrect = true;          $(item).removeClass('uncorrect-item');        }      }      if (hasCorrect === false) {        $(index).addClass('uncorrect-item');      } else {        $(index).removeClass('uncorrect-item');      }    }    lastFilterValue = filtervalue;  }  return DefaultMobile;})(DefaultMobile);;(function($) {    TeamlabMobile.bind(TeamlabMobile.events.peoplePage, onPeoplePage);  TeamlabMobile.bind(TeamlabMobile.events.profilePage, onProfilePage);    function onProfilePage (data) {    data = {pagetitle : ASC.Resources.LblPeopleTitle, title : data.displayname, type : 'people-item', item : data};    var $page = DefaultMobile.renderPage('people-item-page', 'page-people-profile', 'people-item-' + data.item.id, data.item.displayname, data);  }  function onPeoplePage (data) {    data = {pagetitle : ASC.Resources.LblPeopleTitle, type : 'people', indexes : data, filters : []};    var $page = DefaultMobile.renderPage('people-page', 'page-people', 'people', ASC.Resources.LblPeopleTitle, data);  }})(jQuery);