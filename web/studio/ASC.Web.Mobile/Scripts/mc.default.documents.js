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


;window.DefaultMobile = (function (DefaultMobile) {
  if (!DefaultMobile) {
    console.log('Default.default: has no DefaultMobile');
    return DefaultMobile;
  }

  function getUploadData ($page) {
    var data = {
      folderid  : $page.find('input.document-folderid:first').removeClass('error-field').val(),
      files     : $page.find('div.file-wrapper:first').removeClass('error-field').find('input.document-file:first'),
      file      : null
    };

    data.files = data.files.length > 0 && data.files[0].files.length > 0 ? data.files[0].files : null;
    data.file = data.files.length > 0 ? data.files[0] : null;

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        switch (fld) {
          case 'file':
          case 'files':
            continue;
        }
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'folderid' : errors.push($page.find('input.document-folderid:first').addClass('error-field')); break;
          case 'files'    : errors.push($page.find('div.file-wrapper:first').addClass('error-field')); break;
          case 'file'     : errors.push($page.find('div.file-wrapper:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
      var filesize = 0;
      //filesize = data.files[0].size || data.files[0].fileSize || 0;
      filesize = data.file ? data.file.size || data.file.fileSize || 0 : 0;
      if (filesize !== 0 && filesize >= TeamlabMobile.constants.maxUploadSize) {
          ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrFileSize), 'error', ASC.Resources.ErrFileSize);
          return null;
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getFileData ($page) {
    var data = {
      folderid  : $page.find('input.document-folderid:first').removeClass('error-field').val(),
      title     : $page.find('input.document-title:first').removeClass('error-field').val() || '',
      content   : $page.find('textarea.document-content:first').removeClass('error-field').val() || ''
    };

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
          if (data[fld] !== null) {
            continue;
          }
          switch (fld) {
            case 'folderid' : errors.push($page.find('input.document-folderid:first').addClass('error-field')); break;
            case 'content'  : errors.push($page.find('textarea.document-content:first').addClass('error-field')); break;
            case 'title'    : errors.push($page.find('input.document-title:first').addClass('error-field')); break;
          }
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getFolderData ($page) {
    var data = {
      folderid  : $page.find('input.folder-folderid:first').removeClass('error-field').val(),
      title     : $page.find('input.folder-title:first').removeClass('error-field').val() || ''
    };

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'folderid' : errors.push($page.find('input.folder-folderid:first').addClass('error-field')); break;
          case 'title'    : errors.push($page.find('input.folder-title:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  
  DefaultMobile.create_documents_folder = function (evt, $page, $item) {
    var data = getFolderData($page);
    if (data && TeamlabMobile.addDocumentsItem('folder', data.folderid, data)) {
      jQuery(document).trigger('changepage');
      $item.addClass('disabled');
    }
  };

  DefaultMobile.create_documents_file = function (evt, $page, $item) {
    var data = getFileData($page);
    if (data && TeamlabMobile.addDocumentsItem('document', data.folderid, data)) {
      jQuery(document).trigger('changepage');
      $item.addClass('disabled');
    }
  };

  DefaultMobile.upload_documents_file = function (evt, $page, $item) {
    var data = getUploadData($page);
    if (data && TeamlabMobile.addDocumentsItem('file', data.folderid, data.file)) {
      jQuery(document).trigger('changepage');
      $item.addClass('disabled');
    }
  };

  return DefaultMobile;
})(DefaultMobile);

;(function($) {
  
  TeamlabMobile.bind(TeamlabMobile.events.addDocsItemDialog, onAddDocsItemDialog);

  TeamlabMobile.bind(TeamlabMobile.events.documentsPage, onDocumentsPage);
  TeamlabMobile.bind(TeamlabMobile.events.documentPage, onDocumentPage);
  TeamlabMobile.bind(TeamlabMobile.events.addItemPage, onAddItemPage);
  TeamlabMobile.bind(TeamlabMobile.events.addItemFilePage, onAddItemFilePage);
  TeamlabMobile.bind(TeamlabMobile.events.addItemFolderPage, onAddItemFolderPage);
  TeamlabMobile.bind(TeamlabMobile.events.addItemDocumentPage, onAddItemDocumentPage);
  TeamlabMobile.bind(TeamlabMobile.events.addItemFile, onAddItemFile);
  TeamlabMobile.bind(TeamlabMobile.events.addItemDocument, onAddItemFile);

  
  function onAddDocsItemDialog (folderid) {
    var data = {dialogtotle : ASC.Resources.LblDocumentsTitle, folderid : folderid, fileupload : $.support.fileupload, type : 'documents-additem'};

    var $dialog = DefaultMobile.renderDialog('documents-additem-dialog', 'dialog-documents-additem', 'documents-additem' + Math.floor(Math.random() * 1000000), ' ', data);
  }

  function onDocumentsPage (data, id, parent, foldertitle, rootfoldertype, rootfolder, permissions) {    
    data = {pagetitle : foldertitle || ASC.Resources.LblDocumentsTitle, type : 'documents', id : id, parent : parent, rootfoldertype : rootfoldertype, permissions : permissions, items : data};
    if (rootfoldertype === 'folder-user' && rootfolder) {
      data.folderid = rootfolder.id;
    }

    var $page = DefaultMobile.renderPage('documents-page', 'page-documents', 'documents', ASC.Resources.LblDocumentsTitle, data);
  }

  function onDocumentPage (data, params, folderid) {
    data = {pagetitle : data.filename, type : 'documents-item', item : data, filetype : data.filetype, folderid : folderid, back : null};

    if (params.hasOwnProperty('back') && params.back) {
      data.back = params.back;
    }

    var $page = DefaultMobile.renderPage('documents-item-page', 'page-documents-file', 'documents-file', ASC.Resources.LblDocumentsTitle, data);
  }

  function onAddItemPage (data) {
    data = {pagetitle : ASC.Resources.LblDocumentsTitle, title : ' ', type : 'documents-additem', back : data.back, folderid : data.folderid, fileupload : $.support.fileupload};

    var $page = DefaultMobile.renderPage('documents-additem-page', 'page-documents-additem', 'documents-additem', ' ', data);
  }

  function onAddItemFilePage (data) {
    data = {pagetitle : ASC.Resources.LblUploadFile, title : ' ', type : 'documents-addfile', back : data.back, folderid : data.folderid, fileupload : $.support.fileupload};

    var $page = DefaultMobile.renderPage('documents-addfile-page', 'page-documents-addfile', 'documents-addfile-' + Math.floor(Math.random() * 1000000), ' ', data);
  }

  function onAddItemFolderPage (data) {
    data = {pagetitle : ASC.Resources.LblCreateFolder, title: ' ', type: 'documents-addfolder', back : data.back, folderid: data.folderid, fileupload: $.support.fileupload };

    var $page = DefaultMobile.renderPage('documents-addfolder-page', 'page-documents-addfolder', 'documents-addfolder', ' ', data);
  }

  function onAddItemDocumentPage (data) {
    data = {pagetitle : ASC.Resources.LblCreateDocument, title : ' ', type : 'documents-adddocument', back : data.back, folderid : data.folderid, fileupload : $.support.fileupload};

    var $page = DefaultMobile.renderPage('documents-adddocument-page', 'page-documents-adddocument', 'documents-adddocument', ' ', data);
  }

  function onAddItemFile (data, params) {
    var $page = $('div.ui-page-active:first').removeClass('docitemfile-loading');
    if ($page.length > 0) {
      $page.find('button').removeClass('disabled');
      var 
        newinput = document.createElement('input'),
        $oldinput = $page.find('input.document-file:first');

      if ($oldinput.length > 0) {
        $oldinput.after(newinput);
        var oldinput = $oldinput[0];
        newinput.setAttribute('type', oldinput.getAttribute('type'));
        newinput.setAttribute('class', oldinput.getAttribute('class'));
        $oldinput.remove();
      }
    }

    var folderid = params.hasOwnProperty('folderid') ? params.folderid || -1 : -1;
    if (folderid !== -1) {
      ASC.Controls.AnchorController.lazymove(TeamlabMobile.anchors.folder + folderid);
    }
  }
})(jQuery);
