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


#region Import

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Resources;
using ASC.Web.Files.Api;
using ASC.Web.Studio.Utility;

using Autofac;

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

using Newtonsoft.Json.Linq;

using File = System.IO.File;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

#endregion

namespace ASC.Web.CRM.Classes
{

    class SendBatchEmailsOperation : IProgressItem, IDisposable
    {
        private readonly bool _storeInHistory;
        private readonly ILog _log;
        private readonly SMTPServerSetting _smtpSetting;
        private readonly Guid _currUser;
        private readonly int _tenantID;
        private readonly List<int> _contactID;
        private readonly String _subject;
        private readonly String _bodyTempate;
        private readonly List<int> _fileID;
        private int historyCategory;
        private double _exactPercentageValue = 0;


        public object Id { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }


        private SendBatchEmailsOperation()
            : this(new List<int>(), new List<int>(), String.Empty, String.Empty, false)
        {
        }

        public SendBatchEmailsOperation(
              List<int> fileID,
              List<int> contactID,
              String subject,
              String bodyTempate,
              bool storeInHistory)
        {
            Id = TenantProvider.CurrentTenantID;
            Percentage = 0;
            _fileID = fileID;
            _contactID = contactID;
            _subject = subject;
            _bodyTempate = bodyTempate;

            _log = LogManager.GetLogger("ASC.CRM.MailSender");
            _tenantID = TenantProvider.CurrentTenantID;
            _smtpSetting = Global.TenantSettings.SMTPServerSetting;
            _currUser = SecurityContext.CurrentAccount.ID;
            _storeInHistory = storeInHistory;

            Status = new
            {
                RecipientCount = _contactID.Count,
                EstimatedTime = 0,
                DeliveryCount = 0
            };
        }

        private void AddToHistory(int contactID, String content, DaoFactory _daoFactory)
        {
            if (contactID == 0 || String.IsNullOrEmpty(content)) return;

            var historyEvent = new RelationshipEvent()
            {
                ContactID = contactID,
                Content = content,
                CreateBy = _currUser,
                CreateOn = TenantUtil.DateTimeNow(),
            };
            if (historyCategory == 0)
            {
                var listItemDao = _daoFactory.ListItemDao;

                // HACK
                var listItem = listItemDao.GetItems(ListType.HistoryCategory).Find(item => item.AdditionalParams == "event_category_email.png");
                if (listItem == null)
                {
                    listItemDao.CreateItem(
                        ListType.HistoryCategory,
                        new ListItem { AdditionalParams = "event_category_email.png", Title = CRMCommonResource.HistoryCategory_Note });
                }
                historyCategory = listItem.ID;
            }

            historyEvent.CategoryID = historyCategory;
            var relationshipEventDao = _daoFactory.RelationshipEventDao;
            historyEvent = relationshipEventDao.CreateItem(historyEvent);

            if (historyEvent.ID > 0 && _fileID != null && _fileID.Count > 0)
            {
                relationshipEventDao.AttachFiles(historyEvent.ID, _fileID.ToArray());
            }
        }

        public void RunJob()
        {
            SmtpClient smtpClient = null;
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(_tenantID);
                SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(_currUser));

                smtpClient = GetSmtpClient();

                using (var scope = DIHelper.Resolve())
                {
                    var _daoFactory = scope.Resolve<DaoFactory>();
                    var userCulture = CoreContext.UserManager.GetUsers(_currUser).GetCulture();

                    Thread.CurrentThread.CurrentCulture = userCulture;
                    Thread.CurrentThread.CurrentUICulture = userCulture;

                    var contactCount = _contactID.Count;

                    if (contactCount == 0)
                    {
                        Complete();
                        return;
                    }

                    MailSenderDataCache.Insert((SendBatchEmailsOperation)Clone());

                    var from = new MailboxAddress(_smtpSetting.SenderDisplayName, _smtpSetting.SenderEmailAddress);
                    var filePaths = new List<string>();
                    using (var fileDao = FilesIntegration.GetFileDao())
                    {
                        foreach (var fileID in _fileID)
                        {
                            var fileObj = fileDao.GetFile(fileID);
                            if (fileObj == null) continue;
                            using (var fileStream = fileDao.GetFileStream(fileObj))
                            {
                                var directoryPath = Path.Combine(Path.GetTempPath(), "teamlab", _tenantID.ToString(),
                                    "crm/files/mailsender/");
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }
                                var filePath = Path.Combine(directoryPath, fileObj.Title);
                                using (var newFileStream = File.Create(filePath))
                                {
                                    fileStream.StreamCopyTo(newFileStream);
                                }
                                filePaths.Add(filePath);
                            }
                        }
                    }

                    var templateManager = new MailTemplateManager(_daoFactory);
                    var deliveryCount = 0;

                    try
                    {
                        Error = string.Empty;
                        foreach (var contactID in _contactID)
                        {
                            _exactPercentageValue += 100.0 / contactCount;
                            Percentage = Math.Round(_exactPercentageValue);

                            if (IsCompleted) break; // User selected cancel

                            var contactInfoDao = _daoFactory.ContactInfoDao;
                            var startDate = DateTime.Now;

                            var contactEmails = contactInfoDao.GetList(contactID, ContactInfoType.Email, null, true);
                            if (contactEmails.Count == 0)
                            {
                                continue;
                            }

                            var recipientEmail = contactEmails[0].Data;

                            if (!recipientEmail.TestEmailRegex())
                            {
                                Error += string.Format(CRMCommonResource.MailSender_InvalidEmail, recipientEmail) +
                                         "<br/>";
                                continue;
                            }

                            var to = new MailboxAddress(recipientEmail);

                            var mimeMessage = new MimeMessage
                            {
                                Subject = _subject
                            };

                            mimeMessage.From.Add(from);
                            mimeMessage.To.Add(to);

                            var bodyBuilder = new BodyBuilder
                            {
                                HtmlBody = templateManager.Apply(_bodyTempate, contactID)
                            };

                            foreach (var filePath in filePaths)
                            {
                                bodyBuilder.Attachments.Add(filePath);
                            }

                            mimeMessage.Body = bodyBuilder.ToMessageBody();

                            mimeMessage.Headers.Add("Auto-Submitted", "auto-generated");

                            _log.Debug(GetLoggerRow(mimeMessage));

                            var success = false;

                            try
                            {
                                smtpClient.Send(mimeMessage);

                                success = true;
                            }
                            catch (SmtpCommandException ex)
                            {
                                _log.Error(Error, ex);

                                Error += string.Format(CRMCommonResource.MailSender_FailedDeliverException, recipientEmail) + "<br/>";
                            }

                            if (success)
                            {
                                if (_storeInHistory)
                                {
                                    AddToHistory(contactID, string.Format(CRMCommonResource.MailHistoryEventTemplate, mimeMessage.Subject), _daoFactory);
                                }

                                var endDate = DateTime.Now;
                                var waitInterval = endDate.Subtract(startDate);

                                deliveryCount++;

                                var estimatedTime =
                                    TimeSpan.FromTicks(waitInterval.Ticks * (_contactID.Count - deliveryCount));

                                Status = new
                                {
                                    RecipientCount = _contactID.Count,
                                    EstimatedTime = estimatedTime.ToString(),
                                    DeliveryCount = deliveryCount
                                };
                            }

                            if (MailSenderDataCache.CheckCancelFlag())
                            {
                                MailSenderDataCache.ResetAll();

                                throw new OperationCanceledException();
                            }

                            MailSenderDataCache.Insert((SendBatchEmailsOperation)Clone());

                            if (Percentage > 100)
                            {
                                Percentage = 100;

                                if (MailSenderDataCache.CheckCancelFlag())
                                {
                                    MailSenderDataCache.ResetAll();

                                    throw new OperationCanceledException();
                                }

                                MailSenderDataCache.Insert((SendBatchEmailsOperation)Clone());

                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _log.Debug("cancel mail sender");
                    }
                    finally
                    {
                        foreach (var filePath in filePaths)
                        {
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }
                    }

                    Status = new
                    {
                        RecipientCount = _contactID.Count,
                        EstimatedTime = TimeSpan.Zero.ToString(),
                        DeliveryCount = deliveryCount
                    };
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
                _log.Error(Error);
            }
            finally
            {
                if (smtpClient != null)
                {
                    smtpClient.Dispose();
                }
                Complete();
            }
        }

        public static string GetLoggerRow(MimeMessage mailMessage)
        {
            if (mailMessage == null)
                return String.Empty;

            var result = new StringBuilder();

            result.AppendLine("From:" + mailMessage.From);
            result.AppendLine("To:" + mailMessage.To[0]);
            result.AppendLine("Subject:" + mailMessage.Subject);
            result.AppendLine("Body:" + mailMessage.Body);
            result.AppendLine("TenantID:" + TenantProvider.CurrentTenantID);

            foreach (var attachment in mailMessage.Attachments)
            {
                result.AppendLine("Attachment: " + attachment.ContentDisposition.FileName);
            }

            return result.ToString();
        }

        public object Clone()
        {
            var cloneObj = new SendBatchEmailsOperation();

            cloneObj.Error = Error;
            cloneObj.Id = Id;
            cloneObj.IsCompleted = IsCompleted;
            cloneObj.Percentage = Percentage;
            cloneObj.Status = Status;

            return cloneObj;
        }

        private void DeleteFiles()
        {
            if (_fileID == null || _fileID.Count == 0) return;

            using (var fileDao = FilesIntegration.GetFileDao())
            {
                foreach (var fileID in _fileID)
                {
                    var fileObj = fileDao.GetFile(fileID);
                    if (fileObj == null) continue;

                    fileDao.DeleteFile(fileObj.ID);
                }
            }
        }

        private SmtpClient GetSmtpClient()
        {
            var client = new SmtpClient
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                    WorkContext.IsMono || MailKit.MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                Timeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds
            };

            client.Connect(_smtpSetting.Host, _smtpSetting.Port,
                    _smtpSetting.EnableSSL ? SecureSocketOptions.Auto : SecureSocketOptions.None);

            if (_smtpSetting.RequiredHostAuthentication)
            {
                client.Authenticate(_smtpSetting.HostLogin, _smtpSetting.HostPassword);
            }

            return client;
        }

        private void Complete()
        {
            IsCompleted = true;
            Percentage = 100;
            _log.Debug("Completed");

            MailSenderDataCache.Insert((SendBatchEmailsOperation)Clone());

            Thread.Sleep(10000);
            MailSenderDataCache.ResetAll();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is SendBatchEmailsOperation)) return false;

            var curOperation = (SendBatchEmailsOperation)obj;
            return (curOperation.Id == Id) && (curOperation._tenantID == _tenantID);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ _tenantID.GetHashCode();
        }

        public void Dispose()
        {
            DeleteFiles();
        }
    }

    class MailSenderDataCache
    {
        public static readonly ICache Cache = AscCache.Default;

        public static String GetStateCacheKey()
        {
            return String.Format("{0}:crm:queue:sendbatchemails", TenantProvider.CurrentTenantID.ToString());
        }

        public static String GetCancelCacheKey()
        {
            return String.Format("{0}:crm:queue:sendbatchemails:cancel", TenantProvider.CurrentTenantID.ToString());
        }

        public static SendBatchEmailsOperation Get()
        {
            return Cache.Get<SendBatchEmailsOperation>(GetStateCacheKey());
        }

        public static void Insert(SendBatchEmailsOperation data)
        {
            Cache.Insert(GetStateCacheKey(), data, TimeSpan.FromMinutes(1));
        }

        public static bool CheckCancelFlag()
        {
            var fromCache = Cache.Get<String>(GetCancelCacheKey());

            if (!String.IsNullOrEmpty(fromCache))
                return true;

            return false;

        }

        public static void SetCancelFlag()
        {
            Cache.Insert(GetCancelCacheKey(), "true", TimeSpan.FromMinutes(1));
        }

        public static void ResetAll()
        {
            Cache.Remove(GetStateCacheKey());
            Cache.Remove(GetCancelCacheKey());
        }
    }

    public class MailSender
    {
        private static readonly Object _syncObj = new Object();
        private static readonly ProgressQueue _mailQueue = new ProgressQueue(2, TimeSpan.FromSeconds(60), true);
        private static readonly int quotas = 50;


        static MailSender()
        {
            int parsed;
            if (int.TryParse(ConfigurationManagerExtension.AppSettings["crm.mailsender.quotas"], out parsed))
            {
                quotas = parsed;
            }
        }

        public static int GetQuotas()
        {
            return quotas;
        }

        public static IProgressItem Start(List<int> fileID, List<int> contactID, String subject, String bodyTemplate, bool storeInHistory)
        {
            lock (_syncObj)
            {
                var operation = _mailQueue.GetStatus(TenantProvider.CurrentTenantID);

                if (operation == null)
                {
                    var mailSender = MailSenderDataCache.Get();

                    if (mailSender != null)
                        return mailSender;
                }

                if (operation == null)
                {
                    if (fileID == null)
                    {
                        fileID = new List<int>();
                    }
                    if (contactID == null || contactID.Count == 0 ||
                        String.IsNullOrEmpty(subject) || String.IsNullOrEmpty(bodyTemplate))
                    {
                        return null;
                    }

                    if (contactID.Count > GetQuotas())
                    {
                        contactID = contactID.Take(GetQuotas()).ToList();
                    }

                    operation = new SendBatchEmailsOperation(fileID, contactID, subject, bodyTemplate, storeInHistory);
                    _mailQueue.Add(operation);
                }

                if (!_mailQueue.IsStarted)
                {
                    _mailQueue.Start(x => x.RunJob());
                }
                return operation;
            }
        }

        private static SmtpClient GetSmtpClient(SMTPServerSetting smtpSetting)
        {
            var client = new SmtpClient
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) => MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                Timeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds
            };

            client.Connect(smtpSetting.Host, smtpSetting.Port,
                    smtpSetting.EnableSSL ? SecureSocketOptions.Auto : SecureSocketOptions.None);

            if (smtpSetting.RequiredHostAuthentication)
            {
                client.Authenticate(smtpSetting.HostLogin, smtpSetting.HostPassword);
            }

            return client;
        }

        public static void StartSendTestMail(string recipientEmail, string mailSubj, string mailBody)
        {
            var log = LogManager.GetLogger("ASC.CRM.MailSender");

            if (!recipientEmail.TestEmailRegex())
            {
                throw new Exception(string.Format(CRMCommonResource.MailSender_InvalidEmail, recipientEmail));
            }

            CoreContext.TenantManager.SetCurrentTenant(TenantProvider.CurrentTenantID);

            var smtpSetting = Global.TenantSettings.SMTPServerSetting;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var toAddress = new MailboxAddress(recipientEmail);
                    var fromAddress = new MailboxAddress(smtpSetting.SenderDisplayName, smtpSetting.SenderEmailAddress);

                    var mimeMessage = new MimeMessage
                    {
                        Subject = mailSubj
                    };

                    mimeMessage.From.Add(fromAddress);

                    mimeMessage.To.Add(toAddress);

                    var bodyBuilder = new BodyBuilder
                    {
                        TextBody = mailBody
                    };

                    mimeMessage.Body = bodyBuilder.ToMessageBody();

                    mimeMessage.Headers.Add("Auto-Submitted", "auto-generated");

                    using (var smtpClient = GetSmtpClient(smtpSetting))
                    {
                        smtpClient.Send(FormatOptions.Default, mimeMessage, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            });
        }

        public static IProgressItem GetStatus()
        {
            var result = _mailQueue.GetStatus(TenantProvider.CurrentTenantID);

            if (result == null)
                return MailSenderDataCache.Get();

            return result;
        }

        public static void Cancel()
        {
            lock (_syncObj)
            {
                var findedItem = _mailQueue.GetItems().Where(elem => (int)elem.Id == TenantProvider.CurrentTenantID);

                if (findedItem.Any())
                {
                    _mailQueue.Remove(findedItem.ElementAt(0));

                    MailSenderDataCache.ResetAll();
                }
                else
                {
                    MailSenderDataCache.SetCancelFlag();
                }
            }
        }
    }

    [Serializable]
    [DataContract]
    public class MailTemplateTag
    {
        [DataMember(Name = "sysname")]
        public String SysName { get; set; }

        [DataMember(Name = "display_name")]
        public String DisplayName { get; set; }

        [DataMember(Name = "category")]
        public String Category { get; set; }

        [DataMember(Name = "is_company")]
        public bool isCompany { get; set; }

        [DataMember(Name = "name")]
        public String Name { get; set; }


    }

    public class MailTemplateManager
    {

        #region Members

        private readonly Dictionary<String, IEnumerable<MailTemplateTag>> _templateTagsCache = new Dictionary<String, IEnumerable<MailTemplateTag>>();

        private readonly DaoFactory _daoFactory;

        #endregion

        #region Constructor

        public MailTemplateManager(DaoFactory daoFactory)
        {
            _daoFactory = daoFactory;
        }

        #endregion

        private IEnumerable<MailTemplateTag> GetTagsFrom(String template)
        {
            if (_templateTagsCache.ContainsKey(template)) return _templateTagsCache[template];

            var tags = GetAllTags();

            var result = new List<MailTemplateTag>();

            var _regex = new Regex("\\$\\((Person|Company)\\.[^<>\\)]*\\)");


            if (!_regex.IsMatch(template))
                return new List<MailTemplateTag>();

            foreach (Match match in _regex.Matches(template))
            {
                var findedTag = tags.Find(item => String.Compare(item.Name, match.Value) == 0);

                if (findedTag == null) continue;

                if (!result.Contains(findedTag))
                    result.Add(findedTag);
            }

            _templateTagsCache.Add(template, result);

            return result;
        }

        private String Apply(String template, IEnumerable<MailTemplateTag> templateTags, int contactID)
        {
            var result = template;


            var contactDao = _daoFactory.ContactDao;
            var contactInfoDao = _daoFactory.ContactInfoDao;
            var customFieldDao = _daoFactory.CustomFieldDao;

            var contact = contactDao.GetByID(contactID);

            if (contact == null)
                throw new ArgumentException(CRMErrorsResource.ContactNotFound);

            foreach (var tag in templateTags)
            {
                var tagParts = tag.SysName.Split(new[] { '_' });

                var source = tagParts[0];

                var tagValue = String.Empty;

                switch (source)
                {
                    case "common":

                        if (contact is Person)
                        {

                            var person = (Person)contact;

                            switch (tagParts[1])
                            {

                                case "firstName":
                                    tagValue = person.FirstName;

                                    break;
                                case "lastName":
                                    tagValue = person.LastName;

                                    break;
                                case "jobTitle":
                                    tagValue = person.JobTitle;
                                    break;
                                case "companyName":
                                    var relativeCompany = contactDao.GetByID(((Person)contact).CompanyID);

                                    if (relativeCompany != null)
                                        tagValue = relativeCompany.GetTitle();


                                    break;
                                default:
                                    tagValue = String.Empty;
                                    break;

                            }

                        }
                        else
                        {

                            var company = (Company)contact;

                            switch (tagParts[1])
                            {
                                case "companyName":
                                    tagValue = company.CompanyName;
                                    break;
                                default:
                                    tagValue = String.Empty;
                                    break;
                            }
                        }

                        break;
                    case "customField":
                        var tagID = Convert.ToInt32(tagParts[tagParts.Length - 1]);

                        var entityType = contact is Company ? EntityType.Company : EntityType.Person;

                        tagValue = customFieldDao.GetValue(entityType, contactID, tagID);

                        break;
                    case "contactInfo":
                        var contactInfoType = (ContactInfoType)Enum.Parse(typeof(ContactInfoType), tagParts[1]);
                        var category = Convert.ToInt32(tagParts[2]);
                        var contactInfos = contactInfoDao.GetList(contactID, contactInfoType, null, null);

                        if (contactInfos == null || contactInfos.Count == 0) break;

                        var contactInfo = contactInfos.FirstOrDefault(info => info.Category == category && info.IsPrimary) ??
                                          contactInfos.First();

                        if (contactInfoType == ContactInfoType.Address)
                        {
                            var addressPart = (AddressPart)Enum.Parse(typeof(AddressPart), tagParts[3]);

                            tagValue = JObject.Parse(contactInfo.Data)[addressPart.ToString().ToLower()].Value<String>();

                        }
                        else
                            tagValue = contactInfo.Data;

                        break;
                    default:
                        throw new ArgumentException(tag.SysName);
                }

                result = result.Replace(tag.Name, tagValue);
            }

            return result;
        }

        public String Apply(String template, int contactID)
        {
            return Apply(template, GetTagsFrom(template), contactID);
        }

        private String ToTagName(String value, bool isCompany)
        {
            return String.Format("$({0}.{1})", isCompany ? "Company" : "Person", value);
        }

        private List<MailTemplateTag> GetAllTags()
        {
            return GetTags(true).Union(GetTags(false)).ToList();
        }

        public List<MailTemplateTag> GetTags(bool isCompany)
        {

            var result = new List<MailTemplateTag>();

            if (isCompany)
            {

                result.Add(new MailTemplateTag
                {
                    DisplayName = CRMContactResource.CompanyName,
                    SysName = "common_companyName",
                    Category = CRMContactResource.GeneralInformation,
                    isCompany = isCompany,
                    Name = ToTagName("Company Name", isCompany)
                });

            }
            else
            {
                result.Add(new MailTemplateTag
                {
                    DisplayName = CRMContactResource.FirstName,
                    SysName = "common_firstName",
                    Category = CRMContactResource.GeneralInformation,
                    isCompany = false,
                    Name = ToTagName("First Name", isCompany)
                });

                result.Add(new MailTemplateTag
                {
                    DisplayName = CRMContactResource.LastName,
                    SysName = "common_lastName",
                    Category = CRMContactResource.GeneralInformation,
                    isCompany = false,
                    Name = ToTagName("Last Name", isCompany)
                });

                result.Add(new MailTemplateTag
                {
                    DisplayName = CRMContactResource.JobTitle,
                    SysName = "common_jobTitle",
                    Category = CRMContactResource.GeneralInformation,
                    isCompany = false,
                    Name = ToTagName("Job Title", isCompany)
                });


                result.Add(new MailTemplateTag
                {
                    DisplayName = CRMContactResource.CompanyName,
                    SysName = "common_companyName",
                    Category = CRMContactResource.GeneralInformation,
                    isCompany = false,
                    Name = ToTagName("Company Name", isCompany)
                });

            }

            #region Contact Infos

            foreach (ContactInfoType infoTypeEnum in Enum.GetValues(typeof(ContactInfoType)))
            {

                var localName = String.Format("contactInfo_{0}_{1}", infoTypeEnum, ContactInfo.GetDefaultCategory(infoTypeEnum));
                var localTitle = infoTypeEnum.ToLocalizedString();

                if (infoTypeEnum == ContactInfoType.Address)
                    foreach (AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart)))
                        result.Add(new MailTemplateTag
                        {
                            SysName = String.Format(localName + "_{0}_{1}", addressPartEnum, (int)AddressCategory.Work),
                            DisplayName = String.Format(localTitle + " {0}", addressPartEnum.ToLocalizedString()),
                            Category = CRMContactResource.GeneralInformation,
                            isCompany = isCompany,
                            Name = ToTagName(String.Format("{0} {1}", infoTypeEnum.ToString(), addressPartEnum.ToString()), isCompany)
                        });
                else
                    result.Add(new MailTemplateTag
                    {
                        SysName = localName,
                        DisplayName = localTitle,
                        Category = CRMContactResource.GeneralInformation,
                        isCompany = isCompany,
                        Name = ToTagName(infoTypeEnum.ToString(), isCompany)
                    });
            }

            #endregion

            #region Custom Fields

            var entityType = isCompany ? EntityType.Company : EntityType.Person;

            var customFieldsDao = _daoFactory.CustomFieldDao;

            var customFields = customFieldsDao.GetFieldsDescription(entityType);

            var category = CRMContactResource.GeneralInformation;

            foreach (var customField in customFields)
            {
                if (customField.FieldType == CustomFieldType.SelectBox) continue;
                if (customField.FieldType == CustomFieldType.CheckBox) continue;

                if (customField.FieldType == CustomFieldType.Heading)
                {
                    if (!String.IsNullOrEmpty(customField.Label))
                        category = customField.Label;

                    continue;
                }

                result.Add(new MailTemplateTag
                {
                    SysName = "customField_" + customField.ID,
                    DisplayName = customField.Label.HtmlEncode(),
                    Category = category,
                    isCompany = isCompany,
                    Name = ToTagName(customField.Label, isCompany)
                });
            }

            #endregion

            return result;
        }

    }
}