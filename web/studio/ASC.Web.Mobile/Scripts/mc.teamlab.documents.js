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


;window.TeamlabMobile = (function (TeamlabMobile) {
  if (!TeamlabMobile) {
    console.log('Teamlab.documents: has no TeamlabMobile');
    return undefined;
  }

  var
    
    someVariable = null,
    
    templateIds = {
      
      pgdocuments : 'template-page-documents',
      pgdocsfile : 'template-page-documents-item',
      pgdocsadditem : 'template-page-documents-additem',
      pgdocsaddfile : 'template-page-documents-addfile',
      pgdocsaddfolder : 'template-page-documents-addfolder',
      pgdocsadddocument : 'template-page-documents-adddocument',
      pgdocseditdocument : 'template-page-documents-editdocument',
      dgdocsadditem : 'template-dialog-documents-additem'
    },
    staticAnchors = {
      
      folder : 'docs/'
    },
    anchorRegExp = {
      
      documents : /^docs[\/]*([\w\d=-]*)$/,
      doc_document : /^docs\/file\/([\w\d=-]+)[\/]*$/,
      doc_additem : /^docs[\/]*([\w\d=-]*)\/items\/add[\/]*$/,
      doc_additemfile : /^docs[\/]*([\w\d=-]*)\/files\/add[\/]*$/,
      doc_additemfolder : /^docs[\/]*([\w\d=-]*)\/folders\/add[\/]*$/,
      doc_additemdocument : /^docs[\/]*([\w\d=-]*)\/documents\/add[\/]*$/,
      doc_editdocument : /^docs[\/]*([\w\d=-]*)\/documents\/edit[\/]*$/
    },
    customEvents = {
      changePage : 'onchangepage',
      addComment : 'onaddcomment',
      loadComments : 'onloadcomments',
      
      documentsPage : 'ondocumentspage',
      documentPage : 'ondocumentpage',
      addItemPage : 'onadditempage',
      addItemFilePage : 'onadditemfilepage',
      addItemFolderPage : 'onadditemfolderpage',
      addItemDocumentPage : 'onadditemdocumentpage',
      editItemDocumentPage : 'onedititemdocumentpage',
      addItemFile : 'onadditemfile',
      addItemFolder : 'onadditemfolder',
      addItemDocument : 'onadditemdocument',
      editItemDocument : 'onedititemdocument',
      addDocsItemDialog: 'onadddocsitemdialog'
    },
    eventManager = TeamlabMobile.extendEventManager(customEvents),
    dialogMarkCollection = [
      
      {regexp : /^docs[\/]*([\w\d=-]*)\/items\/add[\/]*$/, evt : customEvents.addDocsItemDialog}
    ];

  
  TeamlabMobile.extendModule(templateIds, anchorRegExp, staticAnchors, dialogMarkCollection);

  
  ASC.Controls.AnchorController.bind(anchorRegExp.documents, onDocumentsAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.doc_document, onDocumentAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.doc_additem, onAddItemAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.doc_additemfile, onAddItemFileAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.doc_additemfolder, onAddItemFolderAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.doc_additemdocument, onAddItemDocumentAnch);

  
  function extensionsTargetRules (item) {
    var target = item.target;
    //switch (item.exttype) {
    //  case 'xls' :
    //  case 'ods' :
    //    if ($.platform.android && $.platform.version >= 2) {
    //      target = 'none';
    //    }
    //    break;
    //}
    item.target = target;
  };

  function getFolderItem (item) {
    items = TeamlabMobile.getFolderItem(item);
    if (item.target === 'self') {       
      item.href = item.type === 'file' ? '#docs/file/' + Base64.encode(item.id + '') : '#docs/' + Base64.encode(item.id + '');      
    }
    return item;
  }

  
  function onDocumentsAnch (params, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    id = id ? Base64.decode(id) : id;
    var fn = null;
    switch ((id + '' || 'root').toLowerCase()) {
      case 'root': id = '@my'; break;
      case 'shared': id = '@share'; break;
      case 'available': id = '@common'; break;
    }

    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getDocFolder(null, id, onGetFolder);
  }

  function onDocumentAnch (params, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    id = id ? Base64.decode(id) : id;
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getDocFile(null, id, onGetFile);
  }

  function onAddItemAnch (params, folderid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    folderid = folderid ? Base64.decode(folderid) : folderid || -1;
    params.folderid = folderid;
    params.back = 'docs/' + Base64.encode(folderid + '');

    eventManager.call(customEvents.changePage, window, []);
    eventManager.call(customEvents.addItemPage, window, [params]);
  }

  function onAddItemFileAnch (params, folderid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    folderid = folderid ? Base64.decode(folderid) : folderid || -1;
    params.folderid = folderid;
    params.back = 'docs/' + Base64.encode(folderid + '');

    eventManager.call(customEvents.changePage, window, []);
    eventManager.call(customEvents.addItemFilePage, window, [params]);
  }

  function onAddItemFolderAnch (params, folderid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    folderid = folderid ? Base64.decode(folderid) : folderid || -1;
    params.folderid = folderid;
    params.back = 'docs/' + Base64.encode(folderid + '');

    eventManager.call(customEvents.changePage, window, []);
    eventManager.call(customEvents.addItemFolderPage, window, [params]);
  }

  function onAddItemDocumentAnch (params, folderid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    folderid = folderid ? Base64.decode(folderid) : folderid || -1;
    params.folderid = folderid;
    params.back = 'docs/' + Base64.encode(folderid + '');

    eventManager.call(customEvents.changePage, window, []);
    eventManager.call(customEvents.addItemDocumentPage, window, [params]);
  }

  /* *** */

  function onAddFile (params, entry) {
    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.doc_additemfile) || ASC.Controls.AnchorController.testAnchor(anchorRegExp.doc_additemdocument)) {
      var folderid = entry.type === 'file' ? entry.folderId : entry.id;
      if (folderid !== -1) {
        ASC.Controls.AnchorController.lazymove(TeamlabMobile.anchors.folder + Base64.encode(folderid + ''));
      }
    }
  }

  function onGetFile (params, entry) {
    entry.href = TeamlabMobile.getViewUrl(entry);
    switch ((entry.filetype || '').toLowerCase()) {
      case 'txt':
        entry.canEdit = true;
        break;
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.doc_document)) {
      params.back = 'docs/' + Base64.encode(entry.folderId + '');

      eventManager.call(customEvents.documentPage, window, [entry, params, Base64.encode(entry.folderId + '')]);
    }
  }

  function onAddFolder (params, entry) {
    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.doc_additemfolder)) {
      var folderid = entry.id || -1;
      if (folderid !== -1) {
        ASC.Controls.AnchorController.lazymove(TeamlabMobile.anchors.folder + Base64.encode(folderid + ''));
      }
    }
  }

  function onGetFolder (params, entry) {
    var
      item = null,
      itemsInd = 0,
      permissions = {},
      items = null;

    items = [].concat(entry.folders).concat(entry.files);
    itemsInd = items ? items.length : 0;
    while (itemsInd--) {
      item = items[itemsInd];

      item = getFolderItem(item);
    }

    if (entry.parentFolder) {
      entry.parentFolder.classname = entry.parentFolder.type;
      entry.parentFolder.href = 'docs/' + Base64.encode(entry.parentFolder.id + '');
    }

    permissions.canadd = entry.canAddItems;

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.documents)) {
      eventManager.call(customEvents.documentsPage, window, [items, Base64.encode(entry.id + ''), entry.parentFolder, entry.title, entry.rootType, entry.rootfolder, permissions]);
    }
  }

  
  TeamlabMobile.addDocumentsItem = function(type, folderid, data) {
    var fn = null, callback = null;
    switch (type) {
      case 'folder' :
        return Teamlab.addDocFolder({type : type}, folderid, data, onAddFolder);
      case 'document' :
        return Teamlab.addDocFile({type : type}, folderid, 'text', data, onAddFile);
      case 'file' :
        return Teamlab.addDocFile({type : type}, folderid, 'upload', data, onAddFile);
    }
    return false;
  };

  return TeamlabMobile;
})(TeamlabMobile);
