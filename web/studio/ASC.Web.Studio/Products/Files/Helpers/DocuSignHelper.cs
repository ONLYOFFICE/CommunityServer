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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.HttpHandlers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
using File = ASC.Files.Core.File;
using Folder = ASC.Files.Core.Folder;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Helpers
{
    public class DocuSignToken
    {
        public static ILog Log = Global.Logger;

        public const string AppAttr = "docusign";

        public static OAuth20Token GetToken()
        {
            return Token.GetToken(AppAttr);
        }

        public static void DeleteToken(Guid? userId = null)
        {
            Token.DeleteToken(AppAttr, userId);
        }

        public static void SaveToken(OAuth20Token token)
        {
            if (token == null) throw new ArgumentNullException("token");

            Token.SaveToken(new Token(token, AppAttr));
        }

        internal static string GetRefreshedToken(OAuth20Token token)
        {
            if (token.IsExpired)
            {
                try
                {
                    Log.Info("DocuSign refresh token for user " + SecurityContext.CurrentAccount.ID);

                    var refreshed = DocuSignLoginProvider.Instance.RefreshToken(token.RefreshToken);

                    if (refreshed != null)
                    {
                        token.AccessToken = refreshed.AccessToken;
                        token.RefreshToken = refreshed.RefreshToken;
                        token.ExpiresIn = refreshed.ExpiresIn;
                        token.Timestamp = DateTime.UtcNow;

                        SaveToken(token);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("DocuSign refresh token for user " + SecurityContext.CurrentAccount.ID, ex);
                }
            }
            return token.AccessToken;
        }
    }

    public class DocuSignHelper
    {
        public static ILog Log = Global.Logger;

        public const string UserField = "userId";

        public static readonly List<string> SupportedFormats = new List<string>
            {
                ".as", ".asl", ".doc", ".docm", ".docx", ".dot", ".dotm", ".dotx", ".htm", ".html", ".msg", ".pdf", ".pdx", ".rtf", ".txt", ".wpd", ".wps", ".wpt", ".xps",
                ".emz", ".svg", ".svgz", ".vdx", ".vss", ".vst",
                ".bmp", ".cdr", ".dcx", ".gif", ".ico", ".jpg", ".jpeg", ".pct", ".pic", ".png", ".rgb", ".sam", ".tga", ".tif", ".tiff", ".wpg",
                ".dps", ".dpt", ".pot", ".potx", ".pps", ".ppt", ".pptm", ".pptx",
                ".csv", ".et", ".ett", ".xls", ".xlsm", ".xlsx", ".xlt"
            };

        public static long MaxFileSize = 25L*1024L*1024L;

        public static int MaxEmailLength = 10000;

        public static bool ValidateToken(OAuth20Token token)
        {
            GetDocuSignAccount(token);
            return true;
        }

        public static string SendDocuSign(object fileId, DocuSignData docuSignData, Dictionary<string, string> requestHeaders)
        {
            if (docuSignData == null) throw new ArgumentNullException("docuSignData");
            var token = DocuSignToken.GetToken();
            var account = GetDocuSignAccount(token);

            var configuration = GetConfiguration(account, token);
            File sourceFile;
            var document = CreateDocument(fileId, docuSignData.Name, docuSignData.FolderId, out sourceFile);

            var url = CreateEnvelope(account.AccountId, document, docuSignData, configuration);

            FilesMessageService.Send(sourceFile, requestHeaders, MessageAction.DocumentSendToSign, "DocuSign", sourceFile.Title);

            return url;
        }

        private static DocuSignAccount GetDocuSignAccount(OAuth20Token token)
        {
            if (token == null) throw new ArgumentNullException("token");

            var userInfoString = RequestHelper.PerformRequest(DocuSignLoginProvider.Instance.DocuSignHost + "/oauth/userinfo",
                                                              headers: new Dictionary<string, string> {{"Authorization", "Bearer " + DocuSignToken.GetRefreshedToken(token)}});

            Log.Debug("DocuSing userInfo: " + userInfoString);

            var userInfo = (DocuSignUserInfo) JsonConvert.DeserializeObject(userInfoString, typeof (DocuSignUserInfo));

            if (userInfo.Accounts == null || userInfo.Accounts.Count == 0) throw new Exception("Account is null");

            var account = userInfo.Accounts[0];
            return account;
        }

        private static DocuSign.eSign.Client.Configuration GetConfiguration(DocuSignAccount account, OAuth20Token token)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (token == null) throw new ArgumentNullException("token");

            var apiClient = new ApiClient(account.BaseUri + "/restapi");

            var configuration = new DocuSign.eSign.Client.Configuration {ApiClient = apiClient};
            configuration.AddDefaultHeader("Authorization", "Bearer " + DocuSignToken.GetRefreshedToken(token));

            return configuration;
        }

        private static Document CreateDocument(object fileId, string documentName, string folderId, out File file)
        {
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                file = fileDao.GetFile(fileId);
                if (file == null) throw new Exception(FilesCommonResource.ErrorMassage_FileNotFound);
                if (!Global.GetFilesSecurity().CanRead(file)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                if (!SupportedFormats.Contains(FileUtility.GetFileExtension(file.Title))) throw new ArgumentException(FilesCommonResource.ErrorMassage_NotSupportedFormat);
                if (file.ContentLength > MaxFileSize) throw new Exception(FileSizeComment.GetFileSizeExceptionString(MaxFileSize));

                byte[] fileBytes;
                using (var stream = fileDao.GetFileStream(file))
                {
                    var buffer = new byte[16*1024];
                    using (var ms = new MemoryStream())
                    {
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        fileBytes = ms.ToArray();
                    }
                }

                if (string.IsNullOrEmpty(documentName))
                {
                    documentName = file.Title;
                }

                var document = new Document
                    {
                        DocumentBase64 = Convert.ToBase64String(fileBytes),
                        DocumentFields = new List<NameValue>
                            {
                                new NameValue {Name = FilesLinkUtility.FolderId, Value = folderId},
                                new NameValue {Name = FilesLinkUtility.FileTitle, Value = file.Title},
                            },
                        DocumentId = "1", //file.ID.ToString(),
                        FileExtension = FileUtility.GetFileExtension(file.Title),
                        Name = documentName,
                    };

                return document;
            }
        }

        private static string CreateEnvelope(string accountId, Document document, DocuSignData docuSignData, DocuSign.eSign.Client.Configuration configuration)
        {
            var eventNotification = new EventNotification
                {
                    EnvelopeEvents = new List<EnvelopeEvent>
                        {
                            //new EnvelopeEvent {EnvelopeEventStatusCode = DocuSignStatus.Sent.ToString()},
                            //new EnvelopeEvent {EnvelopeEventStatusCode = DocuSignStatus.Delivered.ToString()},
                            new EnvelopeEvent {EnvelopeEventStatusCode = DocuSignStatus.Completed.ToString()},
                            new EnvelopeEvent {EnvelopeEventStatusCode = DocuSignStatus.Declined.ToString()},
                            new EnvelopeEvent {EnvelopeEventStatusCode = DocuSignStatus.Voided.ToString()},
                        },
                    IncludeDocumentFields = "true",
                    //RecipientEvents = new List<RecipientEvent>
                    //    {
                    //        new RecipientEvent {RecipientEventStatusCode = "Sent"},
                    //        new RecipientEvent {RecipientEventStatusCode = "Delivered"},
                    //        new RecipientEvent {RecipientEventStatusCode = "Completed"},
                    //        new RecipientEvent {RecipientEventStatusCode = "Declined"},
                    //        new RecipientEvent {RecipientEventStatusCode = "AuthenticationFailed"},
                    //        new RecipientEvent {RecipientEventStatusCode = "AutoResponded"},
                    //    },
                    Url = CommonLinkUtility.GetFullAbsolutePath(DocuSignHandler.Path + "?" + FilesLinkUtility.Action + "=webhook"),
                };
            Global.Logger.Debug("DocuSign hook url: " + eventNotification.Url);

            var signers = new List<Signer>();
            docuSignData.Users.ForEach(uid =>
                {
                    try
                    {
                        var user = CoreContext.UserManager.GetUsers(uid);
                        signers.Add(new Signer
                            {
                                Email = user.Email,
                                Name = user.DisplayUserName(false),
                                RecipientId = user.ID.ToString(),
                            });
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Signer is undefined", ex);
                    }
                });

            var envelopeDefinition = new EnvelopeDefinition
                {
                    CustomFields = new CustomFields
                        {
                            TextCustomFields = new List<TextCustomField>
                                {
                                    new TextCustomField {Name = UserField, Value = SecurityContext.CurrentAccount.ID.ToString()},
                                }
                        },
                    Documents = new List<Document> {document},
                    EmailBlurb = docuSignData.Message,
                    EmailSubject = docuSignData.Name,
                    EventNotification = eventNotification,
                    Recipients = new Recipients
                        {
                            Signers = signers,
                        },
                    Status = "created",
                };

            var envelopesApi = new EnvelopesApi(configuration);
            var envelopeSummary = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            Global.Logger.Debug("DocuSign createdEnvelope: " + envelopeSummary.EnvelopeId);

            var envelopeId = envelopeSummary.EnvelopeId;
            var url = envelopesApi.CreateSenderView(accountId, envelopeId, new ReturnUrlRequest
                {
                    ReturnUrl = CommonLinkUtility.GetFullAbsolutePath(DocuSignHandler.Path + "?" + FilesLinkUtility.Action + "=redirect")
                });
            Global.Logger.Debug("DocuSign senderView: " + url.Url);

            return url.Url;
        }

        public static File SaveDocument(string envelopeId, string documentId, string documentName, object folderId)
        {
            if (string.IsNullOrEmpty(envelopeId)) throw new ArgumentNullException("envelopeId");
            if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");

            var token = DocuSignToken.GetToken();
            var account = GetDocuSignAccount(token);
            var configuration = GetConfiguration(account, token);

            using (var fileDao = Global.DaoFactory.GetFileDao())
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                if (string.IsNullOrEmpty(documentName))
                {
                    documentName = "new.pdf";
                }

                Folder folder;
                if (folderId == null
                    || (folder = folderDao.GetFolder(folderId)) == null
                    || folder.RootFolderType == FolderType.TRASH
                    || !Global.GetFilesSecurity().CanCreate(folder))
                {
                    if (Global.FolderMy != null)
                    {
                        folderId = Global.FolderMy;
                    }
                    else
                    {
                        throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);
                    }
                }

                var file = new File
                    {
                        FolderID = folderId,
                        Comment = FilesCommonResource.CommentCreateByDocuSign,
                        Title = FileUtility.ReplaceFileExtension(documentName, ".pdf"),
                    };

                var envelopesApi = new EnvelopesApi(configuration);
                Log.Info("DocuSign webhook get stream: " + documentId);
                using (var stream = envelopesApi.GetDocument(account.AccountId, envelopeId, documentId))
                {
                    file.ContentLength = stream.Length;
                    file = fileDao.SaveFile(file, stream);
                }

                FilesMessageService.Send(file, MessageInitiator.ThirdPartyProvider, MessageAction.DocumentSignComplete, "DocuSign", file.Title);

                FileMarker.MarkAsNew(file);

                return file;
            }
        }


        [DataContract]
        [DebuggerDisplay("{AccountId} {BaseUri}")]
        private class DocuSignAccount
        {
            [DataMember(Name = "account_id", EmitDefaultValue = false)]
            public string AccountId { get; set; }

            [DataMember(Name = "base_uri", EmitDefaultValue = false)]
            public string BaseUri { get; set; }
        }

        [DataContract]
        private class DocuSignUserInfo
        {
            [DataMember(Name = "accounts", EmitDefaultValue = false)]
            public List<DocuSignAccount> Accounts { get; set; }
        }
    }

    [DataContract(Name = "docusign_data", Namespace = "")]
    [DebuggerDisplay("{Name}")]
    public class DocuSignData
    {
        [DataMember(Name = "folderId")]
        public string FolderId { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "name", IsRequired = true, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "users")]
        public ItemList<Guid> Users { get; set; }
    }

    public enum DocuSignStatus
    {
        Draft,
        Sent,
        Delivered,
        Completed,
        Declined,
        Voided,
    }
}