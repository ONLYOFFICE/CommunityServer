/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Utility;
using Microsoft.Practices.ServiceLocation;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Services.DocumentService
{
    [DataContract(Name = "service_params", Namespace = "")]
    public class DocumentServiceParams
    {
        public static readonly Dictionary<FileType, string> DocType = new Dictionary<FileType, string>
            {
                {Web.Core.Files.FileType.Document, "text"},
                {Web.Core.Files.FileType.Spreadsheet, "spreadsheet"},
                {Web.Core.Files.FileType.Presentation, "presentation"}
            };

        #region Nested Classes

        public enum EditorType
        {
            Desktop,
            Mobile,
            Embedded
        }

        #endregion

        #region Constructor

        private static Dictionary<CultureInfo, Dictionary<FileType, Dictionary<string, string>>> _docTemplates;
        private string _breadCrumbs;
        private string _folderUri;
        private string _fileUri;

        private readonly UserInfo _user;

        private FileType _fileTypeCache = Web.Core.Files.FileType.Unknown;

        private string _key = string.Empty;

        public DocumentServiceParams()
        {
            _user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
        }

        public bool ModeWrite = false;
        public EditorType Type = EditorType.Desktop;

        #endregion

        #region Property

        [DataMember(Name = "file")]
        public File File;

        [DataMember(Name = "canEdit")]
        public bool CanEdit;

        [DataMember(Name = "canReview")]
        public bool CanReview;

        [DataMember(Name = "key")]
        public string Key
        {
            set { _key = value; }
            get { return DocumentServiceConnector.GenerateRevisionId(_key); }
        }

        [DataMember(Name = "vkey")]
        public string Vkey
        {
            set { }
            get { return DocumentServiceConnector.GenerateValidateKey(Key); }
        }

        [DataMember(Name = "mode")]
        public string Mode
        {
            set { }
            get { return ModeWrite ? "edit" : "view"; }
        }

        [DataMember(Name = "type")]
        public string TypeString
        {
            set { Type = (EditorType)Enum.Parse(typeof(EditorType), value, true); }
            get { return Type.ToString().ToLower(); }
        }

        [DataMember(Name = "sharingSettings")]
        public ItemList<AceShortWrapper> SharingSettings
        {
            get
            {
                if (Type == EditorType.Embedded
                    || !FileSharing.CanSetAccess(File)) return new ItemList<AceShortWrapper>();

                try
                {
                    var docService = ServiceLocator.Current.GetInstance<IFileStorageService>();
                    return docService.GetSharedInfoShort(File.UniqID);
                }
                catch
                {
                    return new ItemList<AceShortWrapper>();
                }
            }
            set { }
        }

        [DataMember(Name = "folderUrl")]
        public string FolderUrl
        {
            set { _folderUri = value; }
            get
            {
                if (Type == EditorType.Embedded) return string.Empty;
                if (!SecurityContext.IsAuthenticated) return string.Empty;
                if (_folderUri != null)
                    return _folderUri;

                using (var folderDao = Global.DaoFactory.GetFolderDao())
                {
                    try
                    {
                        var parent = folderDao.GetFolder(File.FolderID);
                        if (File.RootFolderType == FolderType.USER
                            && !Equals(File.RootFolderId, Global.FolderMy)
                            && !Global.GetFilesSecurity().CanRead(parent))
                            return PathProvider.GetFolderUrl(Global.FolderShare);

                        return PathProvider.GetFolderUrl(parent);
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }
            }
        }

        [DataMember(Name = "url")]
        public string FileUri
        {
            set { _fileUri = value; }
            get
            {
                if (!string.IsNullOrEmpty(_fileUri))
                    return _fileUri;

                _fileUri = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileStreamUrl(File));
                return _fileUri;
            }
        }

        [DataMember(Name = "filePath")]
        public string FilePath
        {
            set { }
            get
            {
                if (Type == EditorType.Embedded) return string.Empty;
                if (string.IsNullOrEmpty(_breadCrumbs))
                {
                    const string crumbsSeporator = " \\ ";

                    var breadCrumbsList = EntryManager.GetBreadCrumbs(File.FolderID);
                    _breadCrumbs = String.Join(crumbsSeporator, breadCrumbsList.Select(folder => folder.Title).ToArray());
                }

                return _breadCrumbs;
            }
        }

        [DataMember(Name = "documentType")]
        public string DocumentType
        {
            set { }
            get
            {
                string documentType;

                DocType.TryGetValue(GetFileType, out documentType);
                return documentType;
            }
        }

        [DataMember(Name = "fileTypeNum")]
        private FileType GetFileType
        {
            set { }
            get
            {
                if (_fileTypeCache == Web.Core.Files.FileType.Unknown)
                    _fileTypeCache = FileUtility.GetFileTypeByFileName(File.Title);
                return _fileTypeCache;
            }
        }

        [DataMember(Name = "fileType")]
        public string FileType
        {
            set { }
            get { return File.ConvertedExtension.Trim('.'); }
        }

        [DataMember(Name = "user")]
        public string[] User
        {
            get
            {
                return
                    _user.ID.Equals(ASC.Core.Configuration.Constants.Guest.ID)
                        ? new[] { Guid.NewGuid().ToString(), FilesCommonResource.Guest, string.Empty }
                        : new[] { _user.ID.ToString(), _user.FirstName, _user.LastName };
            }
            set { }
        }

        [DataMember(Name = "lang")]
        public string Lang
        {
            get { return _user.GetCulture().Name; }
            set { }
        }

        [DataMember(Name = "templates")]
        public ItemDictionary<string, string> DocTemplates
        {
            get
            {
                if (!SecurityContext.IsAuthenticated || _user.IsVisitor()) return new ItemDictionary<string, string>();
                if (Type == EditorType.Embedded) return new ItemDictionary<string, string>();
                var lang = _user.GetCulture();
                if (_docTemplates == null)
                    _docTemplates = new Dictionary<CultureInfo, Dictionary<FileType, Dictionary<string, string>>>();
                if (!_docTemplates.ContainsKey(lang))
                    _docTemplates.Add(lang, new Dictionary<FileType, Dictionary<string, string>>());
                if (!_docTemplates[lang].ContainsKey(GetFileType))
                    _docTemplates[lang].Add(GetFileType, GetDocumentTemplates(lang, GetFileType));

                return new ItemDictionary<string, string>(_docTemplates[lang][GetFileType]);
            }
            set { }
        }

        [DataMember(Name = "viewerUrl", EmitDefaultValue = false)]
        public string ViewerUrl;

        [DataMember(Name = "embeddedUrl", EmitDefaultValue = false)]
        public string EmbeddedUrl;

        [DataMember(Name = "downloadUrl", EmitDefaultValue = false)]
        public string DownloadUrl;

        [DataMember(Name = "sharingSettingsUrl", EmitDefaultValue = false)]
        public string SharingSettingsUrl;

        [DataMember(Name = "fileChoiceUrl", EmitDefaultValue = false)]
        public string FileChoiceUrl;

        [DataMember(Name = "mergeFolderUrl", EmitDefaultValue = false)]
        public string MergeFolderUrl;

        [DataMember(Name = "linkToEdit", EmitDefaultValue = false)]
        public string LinkToEdit;

        #endregion

        #region Methods

        private static Dictionary<string, string> GetDocumentTemplates(CultureInfo culture, FileType fileType)
        {
            var result = new Dictionary<string, string>();

            var storeTemplate = Global.GetStoreTemplate();

            var path = FileConstant.TemplateDocPath;
            if (!storeTemplate.IsDirectory(path))
                return result;
            path += culture + "/";
            if (!storeTemplate.IsDirectory(path))
                path = FileConstant.TemplateDocPath + "default/";

            var docExt = FileUtility.InternalExtension[fileType] ?? FileUtility.InternalExtension[Web.Core.Files.FileType.Document];
            const string icnExt = ".png";
            foreach (var file in storeTemplate.ListFilesRelative("", path, "*" + docExt, false))
            {
                if (String.IsNullOrEmpty(file)) continue;

                var fileName = Path.GetFileNameWithoutExtension(file);

                var icnUri = storeTemplate.GetUri(path + fileName + icnExt).ToString();

                result.Add(fileName, CommonLinkUtility.GetFullAbsolutePath(icnUri));
            }

            return result;
        }

        public static string Serialize(DocumentServiceParams documentServiceParams)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(DocumentServiceParams));
                serializer.WriteObject(ms, documentServiceParams);
                ms.Seek(0, SeekOrigin.Begin);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        #endregion
    }
}