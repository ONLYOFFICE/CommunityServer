/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Globalization;
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
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;
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
                { FileType.Document, "text" },
                { FileType.Spreadsheet, "spreadsheet" },
                { FileType.Presentation, "presentation" }
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
            Document = new DocumentConfig
                {
                    Info =
                        {
                            File = file,
                        },
                };
            EditorConfig = new EditorConfiguration(this);
        }

        public EditorType Type
        {
            set { Document.Info.Type = value; }
            get { return Document.Info.Type; }
        }

        #region Property

        [DataMember(Name = "document")]
        public DocumentConfig Document;

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

        [DataMember(Name = "editorConfig")]
        public EditorConfiguration EditorConfig;

        [DataMember(Name = "token", EmitDefaultValue = false)]
        public string Token;

        [DataMember(Name = "type")]
        public string TypeString
        {
            set { Type = (EditorType)Enum.Parse(typeof (EditorType), value, true); }
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

        [DataMember(Name = "error", EmitDefaultValue = false)]
        public string ErrorMessage;

        #endregion

        public static string Serialize(Configuration configuration)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof (Configuration));
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

            public DocumentConfig()
            {
                Info = new InfoConfig();
                Permissions = new PermissionsConfig();
            }

            private string _key = string.Empty;
            private string _fileUri;
            private string _title;


            [DataMember(Name = "fileType")]
            public string FileType
            {
                set { }
                get { return Info.File.ConvertedExtension.Trim('.'); }
            }

            [DataMember(Name = "info")]
            public InfoConfig Info;

            [DataMember(Name = "key")]
            public string Key
            {
                set { _key = value; }
                get { return DocumentServiceConnector.GenerateRevisionId(_key); }
            }

            [DataMember(Name = "permissions")]
            public PermissionsConfig Permissions;

            [DataMember(Name = "title")]
            public string Title
            {
                set { _title = value; }
                get { return _title ?? Info.File.Title; }
            }

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


                //todo: obsolete since DS v5.4
                [DataMember(Name = "author")]
                public string Author
                {
                    set { }
                    get { return File.CreateByString; }
                }

                //todo: obsolete since DS v5.4
                [DataMember(Name = "created")]
                public string Created
                {
                    set { }
                    get { return File.CreateOnString; }
                }

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

                [DataMember(Name = "owner")]
                public string Owner
                {
                    set { }
                    get { return File.CreateByString; }
                }

                [DataMember(Name = "uploaded")]
                public string Uploaded
                {
                    set { }
                    get { return File.CreateOnString; }
                }

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
                //todo: obsolete since DS v5.5
                [DataMember(Name = "changeHistory")]
                public bool ChangeHistory = false;

                [DataMember(Name = "comment")]
                public bool Comment = true;

                [DataMember(Name = "download")]
                public bool Download = true;

                [DataMember(Name = "edit")]
                public bool Edit = true;

                [DataMember(Name = "fillForms")]
                public bool FillForms = true;

                [DataMember(Name = "print")]
                public bool Print = true;

                [DataMember(Name = "modifyFilter")]
                public bool ModifyFilter = true;

                //todo: obsolete since DS v6.0
                [DataMember(Name = "rename")]
                public bool Rename = false;

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

                User = _userInfo.ID.Equals(ASC.Core.Configuration.Constants.Guest.ID)
                           ? new UserConfig
                               {
                                   Id = Guid.NewGuid().ToString(),
                                   Name = FilesCommonResource.Guest,
                               }
                           : new UserConfig
                               {
                                   Id = _userInfo.ID.ToString(),
                                   Name = _userInfo.DisplayUserName(false),
                               };
            }

            public bool ModeWrite = false;

            private readonly Configuration _configuration;
            private readonly UserInfo _userInfo;
            private EmbeddedConfig _embeddedConfig;

            [DataMember(Name = "actionLink", EmitDefaultValue = false)]
            public ActionLinkConfig ActionLink;

            public string ActionLinkString
            {
                get { return null; }
                set
                {
                    try
                    {
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                        {
                            var serializer = new DataContractJsonSerializer(typeof (ActionLinkConfig));
                            ActionLink = (ActionLinkConfig)serializer.ReadObject(ms);
                        }
                    }
                    catch (Exception)
                    {
                        ActionLink = null;
                    }
                }
            }

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

                    using (var fileDao = Global.DaoFactory.GetFileDao())
                    {
                        var files = EntryManager.GetTemplates(fileDao, filter, false, Guid.Empty, string.Empty, false);
                        var listTemplates = from file in files
                                            select
                                                new TemplatesConfig
                                                {
                                                    Image = CommonLinkUtility.GetFullAbsolutePath("skins/default/images/filetype/thumb/" + extension + ".png"),
                                                    Name = file.Title,
                                                    Title = file.Title,
                                                    Url = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(file.ID))
                                                };
                        return listTemplates.ToList();
                    }
                }
            }

            [DataMember(Name = "callbackUrl", EmitDefaultValue = false)]
            public string CallbackUrl;

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

            [DataMember(Name = "plugins", EmitDefaultValue = false)]
            public PluginsConfig Plugins;

            [DataMember(Name = "customization", EmitDefaultValue = false)]
            public CustomizationConfig Customization;

            [DataMember(Name = "embedded", EmitDefaultValue = false)]
            public EmbeddedConfig Embedded
            {
                set { _embeddedConfig = value; }
                get { return _configuration.Document.Info.Type == EditorType.Embedded ? _embeddedConfig : null; }
            }

            [DataMember(Name = "encryptionKeys", EmitDefaultValue = false)]
            public EncryptionKeysConfig EncryptionKeys;

            [DataMember(Name = "fileChoiceUrl", EmitDefaultValue = false)]
            public string FileChoiceUrl;

            [DataMember(Name = "lang")]
            public string Lang
            {
                set { }
                get { return _userInfo.GetCulture().Name; }
            }

            [DataMember(Name = "mode")]
            public string Mode
            {
                set { }
                get { return ModeWrite ? "edit" : "view"; }
            }

            [DataMember(Name = "saveAsUrl", EmitDefaultValue = false)]
            public string SaveAsUrl;

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
                        var files = EntryManager.GetRecent(fileDao, filter, false, Guid.Empty, string.Empty, false);

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

            [DataMember(Name = "sharingSettingsUrl", EmitDefaultValue = false)]
            public string SharingSettingsUrl;

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
                [DataMember(Name = "action", EmitDefaultValue = false)]
                public ActionConfig Action;


                [DataContract(Name = "action", Namespace = "")]
                public class ActionConfig
                {
                    [DataMember(Name = "type", EmitDefaultValue = false)]
                    public string Type;

                    [DataMember(Name = "data", EmitDefaultValue = false)]
                    public string Data;
                }


                public static string Serialize(ActionLinkConfig actionLinkConfig)
                {
                    using (var ms = new MemoryStream())
                    {
                        var serializer = new DataContractJsonSerializer(typeof (ActionLinkConfig));
                        serializer.WriteObject(ms, actionLinkConfig);
                        ms.Seek(0, SeekOrigin.Begin);
                        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                    }
                }
            }

            [DataContract(Name = "embedded", Namespace = "")]
            public class EmbeddedConfig
            {
                public string ShareLinkParam;

                [DataMember(Name = "embedUrl", EmitDefaultValue = false)]
                public string EmbedUrl
                {
                    set { }
                    get { return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FilesBaseAbsolutePath + FilesLinkUtility.EditorPage + "?" + FilesLinkUtility.Action + "=embedded" + ShareLinkParam); }
                }

                [DataMember(Name = "saveUrl", EmitDefaultValue = false)]
                public string SaveUrl
                {
                    set { }
                    get { return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath + "?" + FilesLinkUtility.Action + "=download" + ShareLinkParam); }
                }

                [DataMember(Name = "shareUrl", EmitDefaultValue = false)]
                public string ShareUrl
                {
                    set { }
                    get { return CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FilesBaseAbsolutePath + FilesLinkUtility.EditorPage + "?" + FilesLinkUtility.Action + "=view" + ShareLinkParam); }
                }

                [DataMember(Name = "toolbarDocked")]
                public string ToolbarDocked = "top";
            }

            [DataContract(Name = "encryptionKeys", Namespace = "")]
            public class EncryptionKeysConfig
            {
                [DataMember(Name = "cryptoEngineId", EmitDefaultValue = false)]
                public string CryptoEngineId = "{FFF0E1EB-13DB-4678-B67D-FF0A41DBBCEF}";

                [DataMember(Name = "privateKeyEnc", EmitDefaultValue = false)]
                public string PrivateKeyEnc;

                [DataMember(Name = "publicKey", EmitDefaultValue = false)]
                public string PublicKey;
            }

            [DataContract(Name = "plugins", Namespace = "")]
            public class PluginsConfig
            {
                [DataMember(Name = "pluginsData", EmitDefaultValue = false)]
                public string[] PluginsData
                {
                    set { }
                    get
                    {
                        var plugins = new List<string>();

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

                    Customer = new CustomerConfig(_configuration);
                    Logo = new LogoConfig(_configuration);
                }

                private readonly Configuration _configuration;
                public string GobackUrl;
                public bool IsRetina = false;


                [DataMember(Name = "about")]
                public bool About
                {
                    set { }
                    get { return !CoreContext.Configuration.Standalone || CoreContext.Configuration.CustomMode; }
                }

                [DataMember(Name = "customer")]
                public CustomerConfig Customer;

                [DataMember(Name = "feedback", EmitDefaultValue = false)]
                public FeedbackConfig Feedback
                {
                    set { }
                    get
                    {
                        if (CoreContext.Configuration.Standalone) return null;
                        if (!AdditionalWhiteLabelSettings.Instance.FeedbackAndSupportEnabled) return null;

                        return new FeedbackConfig
                            {
                                Url = CommonLinkUtility.GetRegionalUrl(
                                    AdditionalWhiteLabelSettings.Instance.FeedbackAndSupportUrl,
                                    CultureInfo.CurrentCulture.TwoLetterISOLanguageName),
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


                [DataContract(Name = "customer", Namespace = "")]
                public class CustomerConfig
                {
                    public CustomerConfig(Configuration configuration)
                    {
                        _configuration = configuration;
                    }

                    private readonly Configuration _configuration;


                    [DataMember(Name = "logo")]
                    public string Logo
                    {
                        set { }
                        get { return CommonLinkUtility.GetFullAbsolutePath(TenantLogoHelper.GetLogo(WhiteLabelLogoTypeEnum.Dark, !_configuration.EditorConfig.Customization.IsRetina)); }
                    }

                    [DataMember(Name = "name")]
                    public string Name
                    {
                        set { }
                        get
                        {
                            return (TenantWhiteLabelSettings.Load().LogoText ?? "")
                                .Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("/", "\\/");
                        }
                    }
                }

                [DataContract(Name = "feedback", Namespace = "")]
                public class FeedbackConfig
                {
                    [DataMember(Name = "url")]
                    public string Url;

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
                            return
                                _configuration.Type == EditorType.Embedded
                                    ? null
                                    : CommonLinkUtility.GetFullAbsolutePath(TenantLogoHelper.GetLogo(WhiteLabelLogoTypeEnum.DocsEditor, !_configuration.EditorConfig.Customization.IsRetina));
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
                                    : CommonLinkUtility.GetFullAbsolutePath(TenantLogoHelper.GetLogo(WhiteLabelLogoTypeEnum.Dark, !_configuration.EditorConfig.Customization.IsRetina));
                        }
                    }

                    [DataMember(Name = "url")]
                    public string Url
                    {
                        set { }
                        get { return CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetDefault()); }
                    }
                }
            }

            [DataContract(Name = "recentconfig", Namespace = "")]
            public class RecentConfig
            {
                [DataMember(Name = "folder", EmitDefaultValue = false)]
                public string Folder;

                [DataMember(Name = "title", EmitDefaultValue = false)]
                public string Title;

                [DataMember(Name = "url", EmitDefaultValue = false)]
                public string Url;
            }

            [DataContract(Name = "templatesconfig", Namespace = "")]
            public class TemplatesConfig
            {
                [DataMember(Name = "image", EmitDefaultValue = false)]
                public string Image;

                //todo: obsolete since DS v5.6
                [DataMember(Name = "name", EmitDefaultValue = false)]
                public string Name;

                [DataMember(Name = "title", EmitDefaultValue = false)]
                public string Title;

                [DataMember(Name = "url", EmitDefaultValue = false)]
                public string Url;
            }

            [DataContract(Name = "user", Namespace = "")]
            public class UserConfig
            {
                [DataMember(Name = "id", EmitDefaultValue = false)]
                public string Id;

                [DataMember(Name = "name", EmitDefaultValue = false)]
                public string Name;
            }

            #endregion
        }

        #endregion
    }
}