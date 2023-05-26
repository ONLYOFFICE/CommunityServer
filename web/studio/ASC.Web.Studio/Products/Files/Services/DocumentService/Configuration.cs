/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.Core.Users;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Utility;

using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Services.DocumentService
{
    [DataContract(Name = "editorConfig", Namespace = "")]
    public class Configuration
    {
        public static readonly Dictionary<FileType, string> DocType = new Dictionary<FileType, string>
            {
                { FileType.Document, "word" },
                { FileType.Spreadsheet, "cell" },
                { FileType.Presentation, "slide" }
            };

        public enum EditorType
        {
            Desktop,
            Mobile,
            Embedded,
            External,
        }

        private FileType _fileTypeCache = FileType.Unknown;

        public Configuration(File file)
        {
            Document = new DocumentConfig(file);
            EditorConfig = new EditorConfiguration(this);
        }

        public EditorType Type
        {
            set { Document.Info.Type = value; }
            get { return Document.Info.Type; }
        }

        #region Property
        ///<type name="document">ASC.Web.Files.Services.DocumentService.Configuration.DocumentConfig, ASC.Web.Files</type>
        [DataMember(Name = "document")]
        public DocumentConfig Document;

        ///<example name="documentType">documentType</example>
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

        ///<type name="editorConfig">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration, ASC.Web.Files</type>
        [DataMember(Name = "editorConfig")]
        public EditorConfiguration EditorConfig;

        ///<example name="token">token</example>
        [DataMember(Name = "token", EmitDefaultValue = false)]
        public string Token;

        ///<example name="type">type</example>
        [DataMember(Name = "type")]
        public string TypeString
        {
            set { Type = (EditorType)Enum.Parse(typeof(EditorType), value, true); }
            get { return Type.ToString().ToLower(); }
        }

        private FileType GetFileType
        {
            set { }
            get
            {
                if (_fileTypeCache == FileType.Unknown)
                    _fileTypeCache = FileUtility.GetFileTypeByFileName(Document.Info.File.Title);
                return _fileTypeCache;
            }
        }
        ///<example name="error">ErrorMessage</example>
        [DataMember(Name = "error", EmitDefaultValue = false)]
        public string ErrorMessage;

        #endregion

        public static string Serialize(Configuration configuration)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(Configuration));
                serializer.WriteObject(ms, configuration);
                ms.Seek(0, SeekOrigin.Begin);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }


        #region Nested Classes

        [DataContract(Name = "document", Namespace = "")]
        public class DocumentConfig
        {
            public string SharedLinkKey;

            public DocumentConfig(File file)
            {
                Info = new InfoConfig
                {
                    File = file
                };
                Permissions = new PermissionsConfig();

                ReferenceData = new FileReference.FileReferenceData
                {
                    FileKey = file.ID.ToString(),
                    InstanceId = CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString(),
                };
            }

            private string _key = string.Empty;
            private string _fileUri;
            private string _title;

            ///<example name="fileType">fileType</example>
            [DataMember(Name = "fileType")]
            public string FileType
            {
                set { }
                get { return Info.File.ConvertedExtension.Trim('.'); }
            }

            ///<type name="info">ASC.Web.Files.Services.DocumentService.Configuration.InfoConfig, ASC.Web.Files</type>
            [DataMember(Name = "info")]
            public InfoConfig Info;

            ///<example name="key">key</example>
            [DataMember(Name = "key")]
            public string Key
            {
                set { _key = value; }
                get { return DocumentServiceConnector.GenerateRevisionId(_key); }
            }

            ///<type name="permissions">ASC.Web.Files.Services.DocumentService.Configuration.DocumentConfig.PermissionsConfig, ASC.Web.Files</type>
            [DataMember(Name = "permissions")]
            public PermissionsConfig Permissions;

            ///<type name="referenceData">ASC.Web.Files.Services.DocumentService.Configuration.DocumentConfig.ReferenceDataConfig, ASC.Web.Files</type>
            [DataMember(Name = "referenceData")]
            public FileReference.FileReferenceData ReferenceData;

            ///<example type="title">title</example>
            [DataMember(Name = "title")]
            public string Title
            {
                set { _title = value; }
                get { return _title ?? Info.File.Title; }
            }

            ///<example name="url">url</example>
            [DataMember(Name = "url")]
            public string Url
            {
                set { _fileUri = DocumentServiceConnector.ReplaceCommunityAdress(value); }
                get
                {
                    if (!string.IsNullOrEmpty(_fileUri))
                        return _fileUri;
                    var last = Permissions.Edit || Permissions.Review || Permissions.Comment;
                    _fileUri = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileStreamUrl(Info.File, SharedLinkKey, last));
                    return _fileUri;
                }
            }


            #region Nested Classes

            [DataContract(Name = "info", Namespace = "")]
            public class InfoConfig
            {
                public File File;

                public EditorType Type = EditorType.Desktop;
                private string _breadCrumbs;
                private bool? _favorite;
                private bool _favoriteIsSet;


                ///<example name="favorite">favorite</example>
                [DataMember(Name = "favorite", EmitDefaultValue = false)]
                public bool? Favorite
                {
                    set
                    {
                        _favoriteIsSet = true;
                        _favorite = value;
                    }
                    get
                    {
                        if (_favoriteIsSet) return _favorite;
                        if (!SecurityContext.IsAuthenticated || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return null;
                        if (File.FolderID == null
                            || File.Encrypted) return null;
                        return File.IsFavorite;
                    }
                }

                ///<example name="folder">folder</example>
                [DataMember(Name = "folder", EmitDefaultValue = false)]
                public string Folder
                {
                    set { }
                    get
                    {
                        if (Type == EditorType.Embedded || Type == EditorType.External) return null;
                        if (string.IsNullOrEmpty(_breadCrumbs))
                        {
                            const string crumbsSeporator = " \\ ";

                            var breadCrumbsList = EntryManager.GetBreadCrumbs(File.FolderID);
                            _breadCrumbs = String.Join(crumbsSeporator, breadCrumbsList.Select(folder => folder.Title).ToArray());
                        }

                        return _breadCrumbs;
                    }
                }

                ///<example name="owner">owner</example>
                [DataMember(Name = "owner")]
                public string Owner
                {
                    set { }
                    get { return File.CreateByString; }
                }

                ///<example name="uploaded">uploaded</example>
                [DataMember(Name = "uploaded")]
                public string Uploaded
                {
                    set { }
                    get { return File.CreateOnString; }
                }

                ///<type name="sharingSettings">ASC.Web.Files.Services.DocumentService.Configuration.AceShortWrapper, ASC.Web.Files</type>
                ///<collection>list</collection>
                [DataMember(Name = "sharingSettings", EmitDefaultValue = false)]
                public ItemList<AceShortWrapper> SharingSettings
                {
                    set { }
                    get
                    {
                        if (Type == EditorType.Embedded
                            || Type == EditorType.External
                            || !FileSharing.CanSetAccess(File)) return null;

                        try
                        {
                            return Global.FileStorageService.GetSharedInfoShort(File.UniqID);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }

            [DataContract(Name = "permissions", Namespace = "")]
            public class PermissionsConfig
            {
                ///<example name="changeHistory">true</example>
                [DataMember(Name = "changeHistory")]
                public bool ChangeHistory = false;

                ///<example name="comment">true</example>
                [DataMember(Name = "comment")]
                public bool Comment = true;

                ///<example name="download">true</example>
                [DataMember(Name = "download")]
                public bool Download = true;

                ///<example name="edit">true</example>
                [DataMember(Name = "edit")]
                public bool Edit = true;

                ///<example name="fillForms">true</example>
                [DataMember(Name = "fillForms")]
                public bool FillForms = true;

                ///<example name="print">true</example>
                [DataMember(Name = "print")]
                public bool Print = true;

                ///<example name="modifyFilter">true</example>
                [DataMember(Name = "modifyFilter")]
                public bool ModifyFilter = true;

                ///<example name="rename">true</example>
                [DataMember(Name = "rename")]
                public bool Rename = false;

                ///<example name="review">true</example>
                [DataMember(Name = "review")]
                public bool Review = true;
            }

            #endregion
        }

        [DataContract(Name = "editorConfig", Namespace = "")]
        public class EditorConfiguration
        {
            public EditorConfiguration(Configuration configuration)
            {
                _configuration = configuration;

                Customization = new CustomizationConfig(_configuration);

                Plugins = new PluginsConfig();

                Embedded = new EmbeddedConfig();
                _userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                if (!_userInfo.ID.Equals(ASC.Core.Configuration.Constants.Guest.ID))
                {
                    User = new UserConfig
                    {
                        Id = _userInfo.ID.ToString(),
                        Name = _userInfo.DisplayUserName(false),
                    };
                }
                else if (!SecurityContext.IsAuthenticated && FileShareLink.TryGetSessionId(out var sessionId))
                {
                    User = new UserConfig { Id = sessionId.ToString() };
                }
            }

            public bool ModeWrite = false;

            private readonly Configuration _configuration;
            private readonly UserInfo _userInfo;
            private EmbeddedConfig _embeddedConfig;

            ///<type name="actionLink">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.ActionLinkConfig, ASC.Web.Files</type>
            [DataMember(Name = "actionLink", EmitDefaultValue = false)]
            public ActionLinkConfig ActionLink;

            [DataMember(Name = "coEditing", EmitDefaultValue = false)]
            public CoEditingConfig CoEditing
            {
                set { }
                get
                {
                    return !ModeWrite && User == null
                      ? new CoEditingConfig
                      {
                          Fast = false,
                          Change = false
                      }
                      : null;
                }
            }

            public string ActionLinkString
            {
                get { return null; }
                set
                {
                    try
                    {
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                        {
                            var serializer = new DataContractJsonSerializer(typeof(ActionLinkConfig));
                            ActionLink = (ActionLinkConfig)serializer.ReadObject(ms);
                        }
                    }
                    catch (Exception)
                    {
                        ActionLink = null;
                    }
                }
            }

            ///<type name="templates">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.TemplatesConfig, ASC.Web.Files</type>
            ///<collection>list</collection>
            [DataMember(Name = "templates", EmitDefaultValue = false)]
            public List<TemplatesConfig> Templates
            {
                set { }
                get
                {
                    if (!SecurityContext.IsAuthenticated || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return null;
                    if (!FilesSettings.TemplatesSection) return null;

                    var extension = FileUtility.GetInternalExtension(_configuration.Document.Title).TrimStart('.');
                    var filter = FilterType.FilesOnly;
                    switch (_configuration.GetFileType)
                    {
                        case FileType.Document:
                            filter = FilterType.DocumentsOnly;
                            break;
                        case FileType.Spreadsheet:
                            filter = FilterType.SpreadsheetsOnly;
                            break;
                        case FileType.Presentation:
                            filter = FilterType.PresentationsOnly;
                            break;
                    }

                    using (var folderDao = Global.DaoFactory.GetFolderDao())
                    using (var fileDao = Global.DaoFactory.GetFileDao())
                    {
                        var files = EntryManager.GetTemplates(folderDao, fileDao, filter, false, Guid.Empty, string.Empty, false);
                        var listTemplates = from file in files
                                            select
                                                new TemplatesConfig
                                                {
                                                    Image = CommonLinkUtility.GetFullAbsolutePath("skins/default/images/filetype/thumb/" + extension + ".png"),
                                                    Title = file.Title,
                                                    Url = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(file.ID))
                                                };
                        return listTemplates.ToList();
                    }
                }
            }
            ///<example name="callbackUrl">callbackUrl</example>
            [DataMember(Name = "callbackUrl", EmitDefaultValue = false)]
            public string CallbackUrl
            {
                set { }
                get { return ModeWrite ? DocumentServiceTracker.GetCallbackUrl(_configuration.Document.Info.File.ID.ToString(), _configuration.Document.SharedLinkKey) : null; }
            }

            ///<example name="createUrl">createUrl</example>
            [DataMember(Name = "createUrl", EmitDefaultValue = false)]
            public string CreateUrl
            {
                set { }
                get
                {
                    if (_configuration.Document.Info.Type != EditorType.Desktop) return null;
                    if (!SecurityContext.IsAuthenticated || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return null;

                    return GetCreateUrl(_configuration.GetFileType);
                }
            }
            ///<type name="plugins">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.PluginsConfig, ASC.Web.Files</type>
            [DataMember(Name = "plugins", EmitDefaultValue = false)]
            public PluginsConfig Plugins;

            ///<type name="customization">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.CustomizationConfig, ASC.Web.Files</type>
            [DataMember(Name = "customization", EmitDefaultValue = false)]
            public CustomizationConfig Customization;

            ///<type name="embedded">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.EmbeddedConfig, ASC.Web.Files</type>
            [DataMember(Name = "embedded", EmitDefaultValue = false)]
            public EmbeddedConfig Embedded
            {
                set { _embeddedConfig = value; }
                get { return _configuration.Document.Info.Type == EditorType.Embedded ? _embeddedConfig : null; }
            }

            ///<type name="encryptionKeys">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.EncryptionKeysConfig, ASC.Web.Files</type>
            [DataMember(Name = "encryptionKeys", EmitDefaultValue = false)]
            public EncryptionKeysConfig EncryptionKeys;

            ///<example name="fileChoiceUrl">fileChoiceUrl</example>
            [DataMember(Name = "fileChoiceUrl", EmitDefaultValue = false)]
            public string FileChoiceUrl;

            ///<example name="lang">lang</example>
            [DataMember(Name = "lang")]
            public string Lang
            {
                set { }
                get { return _userInfo.GetCulture().Name; }
            }

            ///<example name="mode">mode</example>
            [DataMember(Name = "mode")]
            public string Mode
            {
                set { }
                get { return ModeWrite ? "edit" : "view"; }
            }

            ///<example name="saveAsUrl">saveAsUrl</example>
            [DataMember(Name = "saveAsUrl", EmitDefaultValue = false)]
            public string SaveAsUrl;

            ///<type name="recent">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.RecentConfig, ASC.Web.Files</type>
            ///<collection>list</collection>
            [DataMember(Name = "recent", EmitDefaultValue = false)]
            public List<RecentConfig> Recent
            {
                set { }
                get
                {
                    if (!SecurityContext.IsAuthenticated || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return null;
                    if (!FilesSettings.RecentSection) return null;

                    var filter = FilterType.FilesOnly;
                    switch (_configuration.GetFileType)
                    {
                        case FileType.Document:
                            filter = FilterType.DocumentsOnly;
                            break;
                        case FileType.Spreadsheet:
                            filter = FilterType.SpreadsheetsOnly;
                            break;
                        case FileType.Presentation:
                            filter = FilterType.PresentationsOnly;
                            break;
                    }

                    using (var folderDao = Global.DaoFactory.GetFolderDao())
                    using (var fileDao = Global.DaoFactory.GetFileDao())
                    {
                        var files = EntryManager.GetRecent(folderDao, fileDao, filter, false, Guid.Empty, string.Empty, false);

                        var listRecent = from file in files
                                         where !Equals(_configuration.Document.Info.File.ID, file.ID)
                                         select
                                             new RecentConfig
                                             {
                                                 Folder = folderDao.GetFolder(file.FolderID).Title,
                                                 Title = file.Title,
                                                 Url = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(file.ID))
                                             };
                        return listRecent.ToList();
                    }
                }
            }

            ///<example name="sharingSettingsUrl">sharingSettingsUrl</example>
            [DataMember(Name = "sharingSettingsUrl", EmitDefaultValue = false)]
            public string SharingSettingsUrl;

            ///<type name="user">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.UserConfig, ASC.Web.Files</type>
            [DataMember(Name = "user")]
            public UserConfig User;

            private static string GetCreateUrl(FileType fileType)
            {
                String title;
                switch (fileType)
                {
                    case FileType.Document:
                        title = FilesJSResource.TitleNewFileText;
                        break;
                    case FileType.Spreadsheet:
                        title = FilesJSResource.TitleNewFileSpreadsheet;
                        break;
                    case FileType.Presentation:
                        title = FilesJSResource.TitleNewFilePresentation;
                        break;
                    default:
                        return null;
                }

                string documentType;
                DocType.TryGetValue(fileType, out documentType);

                return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath)
                       + "?" + FilesLinkUtility.Action + "=create"
                       + "&doctype=" + documentType
                       + "&" + FilesLinkUtility.FileTitle + "=" + HttpUtility.UrlEncode(title);
            }


            #region Nested Classes

            [DataContract(Name = "actionLink", Namespace = "")]
            public class ActionLinkConfig
            {
                ///<type name="action">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.ActionLinkConfig.ActionConfig, ASC.Web.Files</type>
                [DataMember(Name = "action", EmitDefaultValue = false)]
                public ActionConfig Action;


                #region Nested Classes

                [DataContract(Name = "action", Namespace = "")]
                public class ActionConfig
                {
                    ///<example type="type">type</example>
                    [DataMember(Name = "type", EmitDefaultValue = false)]
                    public string Type;

                    ///<example type="data">data</example>
                    [DataMember(Name = "data", EmitDefaultValue = false)]
                    public string Data;
                }

                #endregion

                public static string Serialize(ActionLinkConfig actionLinkConfig)
                {
                    using (var ms = new MemoryStream())
                    {
                        var serializer = new DataContractJsonSerializer(typeof(ActionLinkConfig));
                        serializer.WriteObject(ms, actionLinkConfig);
                        ms.Seek(0, SeekOrigin.Begin);
                        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                    }
                }
            }

            [DataContract(Name = "coEditing", Namespace = "")]
            public class CoEditingConfig
            {
                public bool Fast;

                [DataMember(Name = "mode", EmitDefaultValue = false)]
                public string Mode
                {
                    set { }
                    get { return Fast ? "fast" : "strict"; }
                }

                [DataMember(Name = "change")]
                public bool Change;
            }

            [DataContract(Name = "embedded", Namespace = "")]
            public class EmbeddedConfig
            {
                public string ShareLinkParam;
                ///<example name="embedUrl">embedUrl</example>
                [DataMember(Name = "embedUrl", EmitDefaultValue = false)]
                public string EmbedUrl
                {
                    set { }
                    get { return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FilesBaseAbsolutePath + FilesLinkUtility.EditorPage + "?" + FilesLinkUtility.Action + "=embedded" + ShareLinkParam); }
                }

                ///<example name="saveUrl">saveUrl</example>
                [DataMember(Name = "saveUrl", EmitDefaultValue = false)]
                public string SaveUrl
                {
                    set { }
                    get { return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath + "?" + FilesLinkUtility.Action + "=download" + ShareLinkParam); }
                }

                ///<example name="shareUrl">shareUrl</example>
                [DataMember(Name = "shareUrl", EmitDefaultValue = false)]
                public string ShareUrl
                {
                    set { }
                    get { return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FilesBaseAbsolutePath + FilesLinkUtility.EditorPage + "?" + FilesLinkUtility.Action + "=view" + ShareLinkParam); }
                }

                ///<example name="toolbarDocked">top</example>
                [DataMember(Name = "toolbarDocked")]
                public string ToolbarDocked = "top";
            }

            [DataContract(Name = "encryptionKeys", Namespace = "")]
            public class EncryptionKeysConfig
            {
                ///<example name="cryptoEngineId">{FFF221EB-135B-4118-B17D-FF0A44BBCEF}</example>
                [DataMember(Name = "cryptoEngineId", EmitDefaultValue = false)]
                public string CryptoEngineId = "{FFF0E1EB-13DB-4678-B67D-FF0A41DBBCEF}";

                ///<example name="privateKeyEnc">privateKeyEnc</example>
                [DataMember(Name = "privateKeyEnc", EmitDefaultValue = false)]
                public string PrivateKeyEnc;

                ///<example name="publicKey">publicKey</example>
                [DataMember(Name = "publicKey", EmitDefaultValue = false)]
                public string PublicKey;
            }

            [DataContract(Name = "plugins", Namespace = "")]
            public class PluginsConfig
            {
                ///<example name="PluginsData">pluginsData</example>
                ///<collection>list</collection>
                [DataMember(Name = "pluginsData", EmitDefaultValue = false)]
                public string[] PluginsData
                {
                    set { }
                    get
                    {
                        var plugins = new List<string>();

                        if (CoreContext.Configuration.Standalone
                            || !TenantExtra.GetTenantQuota().Free)
                        {
                            var easyBibHelper = ConsumerFactory.Get<EasyBibHelper>();
                            if (!string.IsNullOrEmpty(easyBibHelper.AppKey))
                            {
                                plugins.Add(CommonLinkUtility.GetFullAbsolutePath("ThirdParty/plugin/easybib/config.json"));
                            }

                            var wordpressLoginProvider = ConsumerFactory.Get<WordpressLoginProvider>();
                            if (!string.IsNullOrEmpty(wordpressLoginProvider.ClientID) &&
                                !string.IsNullOrEmpty(wordpressLoginProvider.ClientSecret) &&
                                !string.IsNullOrEmpty(wordpressLoginProvider.RedirectUri))
                            {
                                plugins.Add(CommonLinkUtility.GetFullAbsolutePath("ThirdParty/plugin/wordpress/config.json"));
                            }
                        }

                        return plugins.ToArray();
                    }
                }
            }

            [DataContract(Name = "customization", Namespace = "")]
            public class CustomizationConfig
            {
                public CustomizationConfig(Configuration configuration)
                {
                    _configuration = configuration;

                    if (CoreContext.Configuration.Standalone)
                        Customer = new CustomerConfig(_configuration);

                    Logo = new LogoConfig(_configuration);
                }

                private readonly Configuration _configuration;
                public string GobackUrl;
                public bool IsRetina = false;

                ///<example name="about">true</example>
                [DataMember(Name = "about")]
                public bool About
                {
                    set { }
                    get { return !CoreContext.Configuration.Standalone || CoreContext.Configuration.CustomMode; }
                }

                ///<type name="customer">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.CustomizationConfig.CustomerConfig, ASC.Web.Files</type>
                [DataMember(Name = "customer", EmitDefaultValue = false)]
                public CustomerConfig Customer;

                ///<type name="feedback">ASC.Web.Files.Services.DocumentService.Configuration.EditorConfiguration.CustomizationConfig.FeedbackConfig, ASC.Web.Files</type>
                [DataMember(Name = "feedback", EmitDefaultValue = false)]
                public FeedbackConfig Feedback
                {
                    set { }
                    get
                    {
                        if (CoreContext.Configuration.Standalone) return null;

                        var link = CommonLinkUtility.GetFeedbackAndSupportLink();

                        if (string.IsNullOrEmpty(link)) return null;

                        return new FeedbackConfig
                        {
                            Url = link
                        };
                    }
                }

                [DataMember(Name = "forcesave", EmitDefaultValue = false)]
                public bool Forcesave
                {
                    set { }
                    get
                    {
                        return FileUtility.CanForcesave
                               && !_configuration.Document.Info.File.ProviderEntry
                               && ThirdPartySelector.GetAppByFileId(_configuration.Document.Info.File.ID.ToString()) == null
                               && FilesSettings.Forcesave;
                    }
                }

                [DataMember(Name = "goback", EmitDefaultValue = false)]
                public GobackConfig Goback
                {
                    set { }
                    get
                    {
                        if (_configuration.Type == EditorType.Embedded || _configuration.Type == EditorType.External) return null;
                        if (!SecurityContext.IsAuthenticated) return null;
                        if (GobackUrl != null)
                        {
                            return new GobackConfig
                            {
                                Url = GobackUrl,
                            };
                        }

                        using (var folderDao = Global.DaoFactory.GetFolderDao())
                        {
                            try
                            {
                                var parent = folderDao.GetFolder(_configuration.Document.Info.File.FolderID);
                                var fileSecurity = Global.GetFilesSecurity();
                                if (_configuration.Document.Info.File.RootFolderType == FolderType.USER
                                    && !Equals(_configuration.Document.Info.File.RootFolderId, Global.FolderMy)
                                    && !fileSecurity.CanRead(parent))
                                {
                                    if (fileSecurity.CanRead(_configuration.Document.Info.File))
                                    {
                                        return new GobackConfig
                                        {
                                            Url = PathProvider.GetFolderUrl(Global.FolderShare),
                                        };
                                    }
                                    return null;
                                }

                                if(_configuration.Document.Info.File.Encrypted
                                    && _configuration.Document.Info.File.RootFolderType == FolderType.Privacy
                                    && !fileSecurity.CanRead(parent))
                                {
                                    parent = folderDao.GetFolder(Global.FolderPrivacy);
                                }

                                return new GobackConfig
                                {
                                    Url = PathProvider.GetFolderUrl(parent),
                                };
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        }
                    }
                }

                [DataMember(Name = "logo")]
                public LogoConfig Logo;

                [DataMember(Name = "mentionShare")]
                public bool MentionShare
                {
                    set { }
                    get
                    {
                        return SecurityContext.IsAuthenticated
                               && !_configuration.Document.Info.File.Encrypted
                               && FileSharing.CanSetAccess(_configuration.Document.Info.File);
                    }
                }

                [DataMember(Name = "reviewDisplay", EmitDefaultValue = false)]
                public string ReviewDisplay
                {
                    set { }
                    get { return _configuration.EditorConfig.ModeWrite ? null : "markup"; }
                }

                [DataMember(Name = "submitForm", EmitDefaultValue = false)]
                public bool SubmitForm
                {
                    set { }
                    get
                    {
                        if (_configuration.EditorConfig.ModeWrite
                          && _configuration.Document.Info.File.Access == ASC.Files.Core.Security.FileShare.FillForms)
                        {
                            using (var linkDao = Global.GetLinkDao())
                            using (var fileDao = Global.DaoFactory.GetFileDao())
                            {
                                var sourceId = linkDao.GetSource(_configuration.Document.Info.File.ID);
                                if (sourceId != null)
                                {
                                    var properties = fileDao.GetProperties(sourceId);
                                    return properties != null
                                        && properties.FormFilling != null
                                        && properties.FormFilling.CollectFillForm;
                                }
                            }
                        }
                        return false;
                    }
                }
                
                [DataMember(Name = "uiTheme", EmitDefaultValue = false)]
                public string UiTheme
                {
                    set { }
                    get
                    {
                        return ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark
                            ? "default-dark"
                            : "default-light";
                    }
                }


                #region Nested Classes

                [DataContract(Name = "customer", Namespace = "")]
                public class CustomerConfig
                {
                    public CustomerConfig(Configuration configuration)
                    {
                        _configuration = configuration;
                    }

                    private readonly Configuration _configuration;

                    ///<example name="logo">logo</example>
                    [DataMember(Name = "address")]
                    public string Address
                    {
                        set { }
                        get { return CompanyWhiteLabelSettings.Instance.Address; }
                    }

                    [DataMember(Name = "logo")]
                    public string Logo
                    {
                        set { }
                        get
                        {
                            return CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoAboutDark(!_configuration.EditorConfig.Customization.IsRetina));
                        }
                    }

                    [DataMember(Name = "logoDark ")]
                    public string LogoDark
                    {
                        set { }
                        get
                        {
                            return CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoAboutLight(!_configuration.EditorConfig.Customization.IsRetina));
                        }
                    }

                    ///<example name="mail">mail</example>
                    [DataMember(Name = "mail")]
                    public string Mail
                    {
                        set { }
                        get { return CompanyWhiteLabelSettings.Instance.Email; }
                    }

                    ///<example name="name">name</example>
                    [DataMember(Name = "name")]
                    public string Name
                    {
                        set { }
                        get { return CompanyWhiteLabelSettings.Instance.CompanyName; }
                    }

                    [DataMember(Name = "phone")]
                    public string Phone
                    {
                        set { }
                        get { return CompanyWhiteLabelSettings.Instance.Phone; }
                    }

                    [DataMember(Name = "www")]
                    public string Www
                    {
                        set { }
                        get { return CompanyWhiteLabelSettings.Instance.Site; }
                    }
                }

                [DataContract(Name = "feedback", Namespace = "")]
                public class FeedbackConfig
                {
                    ///<example name="url">url</example>
                    [DataMember(Name = "url")]
                    public string Url;

                    ///<example name="visible">true</example>
                    [DataMember(Name = "visible")]
                    public bool Visible = true;
                }

                [DataContract(Name = "goback", Namespace = "")]
                public class GobackConfig
                {
                    [DataMember(Name = "url", EmitDefaultValue = false)]
                    public string Url;
                }

                [DataContract(Name = "logo", Namespace = "")]
                public class LogoConfig
                {
                    public LogoConfig(Configuration configuration)
                    {
                        _configuration = configuration;
                    }

                    private readonly Configuration _configuration;


                    [DataMember(Name = "image")]
                    public string Image
                    {
                        set { }
                        get
                        {
                            var fillingForm = FileUtility.CanWebRestrictedEditing(_configuration.Document.Title);

                            return
                                _configuration.Type == EditorType.Embedded
                                || fillingForm
                                    ? CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoDocsEditorEmbed(!_configuration.EditorConfig.Customization.IsRetina))
                                    : CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoDocsEditor(!_configuration.EditorConfig.Customization.IsRetina));
                        }
                    }

                    [DataMember(Name = "imageDark")]
                    public string ImageDark
                    {
                        set { }
                        get
                        {
                            return CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoDocsEditor(!_configuration.EditorConfig.Customization.IsRetina));
                        }
                    }

                    [DataMember(Name = "imageEmbedded", EmitDefaultValue = false)]
                    public string ImageEmbedded
                    {
                        set { }
                        get
                        {
                            return
                                _configuration.Type != EditorType.Embedded
                                    ? null
                                    : CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoDocsEditorEmbed(!_configuration.EditorConfig.Customization.IsRetina));
                        }
                    }

                    [DataMember(Name = "url", EmitDefaultValue = false)]
                    public string Url
                    {
                        set { }
                        get { return SecurityContext.IsAuthenticated ? CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetDefault()) : null; }
                    }
                }

                #endregion
            }

            [DataContract(Name = "recentconfig", Namespace = "")]
            public class RecentConfig
            {
                ///<example name="folder">folder</example>
                [DataMember(Name = "folder", EmitDefaultValue = false)]
                public string Folder;

                ///<example name="title">title</example>
                [DataMember(Name = "title", EmitDefaultValue = false)]
                public string Title;

                ///<example name="url">url</example>
                [DataMember(Name = "url", EmitDefaultValue = false)]
                public string Url;
            }

            [DataContract(Name = "templatesconfig", Namespace = "")]
            public class TemplatesConfig
            {
                ///<example name="image">image</example>
                [DataMember(Name = "image", EmitDefaultValue = false)]
                public string Image;

                ///<example name="title">title</example>
                [DataMember(Name = "title", EmitDefaultValue = false)]
                public string Title;

                ///<example name="url">url</example>
                [DataMember(Name = "url", EmitDefaultValue = false)]
                public string Url;
            }

            [DataContract(Name = "user", Namespace = "")]
            public class UserConfig
            {
                ///<example name="id">id</example>
                [DataMember(Name = "id", EmitDefaultValue = false)]
                public string Id;

                ///<example name="name">name</example>
                [DataMember(Name = "name", EmitDefaultValue = false)]
                public string Name;
            }

            #endregion
        }

        #endregion
    }
}