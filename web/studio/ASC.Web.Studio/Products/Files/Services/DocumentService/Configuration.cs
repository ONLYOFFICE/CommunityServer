/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Web;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Thrdparty.Configuration;
using ASC.Web.Core.Files;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;
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
            public DocumentConfig()
            {
                Info = new InfoConfig();
                Permissions = new PermissionsConfig();
            }

            private string _key = string.Empty;
            private string _fileUri;


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
                set { }
                get { return Info.File.Title; }
            }

            [DataMember(Name = "url")]
            public string Url
            {
                set { _fileUri = value; }
                get
                {
                    if (!string.IsNullOrEmpty(_fileUri))
                        return _fileUri;

                    _fileUri = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileStreamUrl(Info.File));
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


                [DataMember(Name = "author")]
                public string Aouthor
                {
                    set { }
                    get { return File.CreateByString; }
                }

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
                [DataMember(Name = "changeHistory")]
                public bool ChangeHistory = false;

                [DataMember(Name = "download")]
                public bool Download = true;

                [DataMember(Name = "edit")]
                public bool Edit = true;

                [DataMember(Name = "print")]
                public bool Print = true;

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

            [DataMember(Name = "fileChoiceUrl", EmitDefaultValue = false)]
            public string FileChoiceUrl;

            [DataMember(Name = "lang")]
            public string Lang
            {
                set { }
                get { return _userInfo.GetCulture().Name; }
            }

            [DataMember(Name = "mergeFolderUrl", EmitDefaultValue = false)]
            public string MergeFolderUrl;

            [DataMember(Name = "mode")]
            public string Mode
            {
                set { }
                get { return ModeWrite ? "edit" : "view"; }
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
                        if (!string.IsNullOrEmpty(KeyStorage.Get("easyBibappkey")))
                        {
                            plugins.Add(CommonLinkUtility.GetFullAbsolutePath("ThirdParty/plugin/easybib/config.json"));
                        }

                        if (!string.IsNullOrEmpty(KeyStorage.Get("wpClientId")) &&
                            !string.IsNullOrEmpty(KeyStorage.Get("wpClientSecret")) &&
                            !string.IsNullOrEmpty(KeyStorage.Get("wpRedirectUrl")))
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
                    get { return !CoreContext.Configuration.Standalone; }
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

                [DataMember(Name = "goback", EmitDefaultValue = false)]
                public GobackConfig Goback
                {
                    set { }
                    get
                    {
                        if (_configuration.Type == EditorType.Embedded || _configuration.Type == EditorType.External) return null;
                        if (!SecurityContext.IsAuthenticated) return null;
                        if (GobackUrl != null)
                            return new GobackConfig
                                {
                                    Url = GobackUrl,
                                };

                        using (var folderDao = Global.DaoFactory.GetFolderDao())
                        {
                            try
                            {
                                var parent = folderDao.GetFolder(_configuration.Document.Info.File.FolderID);
                                if (_configuration.Document.Info.File.RootFolderType == FolderType.USER
                                    && !Equals(_configuration.Document.Info.File.RootFolderId, Global.FolderMy)
                                    && !Global.GetFilesSecurity().CanRead(parent))
                                    return new GobackConfig
                                        {
                                            Url = PathProvider.GetFolderUrl(Global.FolderShare),
                                        };

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
                        get { return CompanyWhiteLabelSettings.Instance.Site; }
                    }
                }
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