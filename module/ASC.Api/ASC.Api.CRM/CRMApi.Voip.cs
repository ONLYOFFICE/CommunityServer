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
using System.Security;
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Exceptions;
using ASC.Api.Utils;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Specific;
using ASC.VoipService.Dao;
using ASC.VoipService.Twilio;
using ASC.VoipService;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core.Voip;
using ASC.Web.Studio.Utility;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        #region Numbers 

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [Read(@"voip/numbers/available")]
        public IEnumerable<VoipPhone> GetAvailablePhoneNumbers(PhoneNumberType numberType, string isoCountryCode)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            if (string.IsNullOrEmpty(isoCountryCode)) throw new ArgumentException();
            return VoipProvider.GetAvailablePhoneNumbers(numberType, isoCountryCode);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Read(@"voip/numbers/existing")]
        public IEnumerable<VoipPhone> GetExistingPhoneNumbers()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            return DaoFactory.GetVoipDao().GetNumbers();
        }
        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Create(@"voip/numbers")]
        public VoipPhone BuyNumber(string number)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var newPhone = VoipProvider.BuyNumber(number);

            newPhone.Settings = new VoipSettings
                {
                    Queue = new Queue {Name = number, Size = 5, WaitTime = 30}
                };

            var files = StorageFactory.GetStorage("", "crm").ListFiles("voip", "default/", "*.*", true)
                                      .Select(r => new
                                          {
                                              path = CommonLinkUtility.GetFullAbsolutePath(r.ToString()),
                                              audioType = (AudioType)Enum.Parse(typeof(AudioType), Directory.GetParent(r.ToString()).Name, true)
                                          }).ToList();

            var audio = files.Find(r => r.audioType == AudioType.Greeting);
            if (audio != null)
            {
                newPhone.Settings.GreetingAudio = audio.path;
            }

            audio = files.Find(r => r.audioType == AudioType.HoldUp);
            if (audio != null)
            {
                newPhone.Settings.HoldAudio = audio.path;
            }

            audio = files.Find(r => r.audioType == AudioType.VoiceMail);
            if (audio != null)
            {
                newPhone.Settings.VoiceMail = new VoiceMail(audio.path, true);
            }

            audio = files.Find(r => r.audioType == AudioType.Queue);
            if (audio != null)
            {
                newPhone.Settings.Queue = new Queue(newPhone.Number, audio.path);
            }

            VoipProvider.UpdateSettings(newPhone);
            return DaoFactory.GetVoipDao().SaveOrUpdateNumber(newPhone);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Delete(@"voip/numbers/{numberId:\w+}")]
        public VoipPhone DeleteNumber(string numberId)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var dao = DaoFactory.GetVoipDao();
            var phone = dao.GetNumber(numberId).NotFoundIfNull();

            VoipProvider.DeleteNumber(phone);
            dao.DeleteNumber(phone);

            return phone;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Read(@"voip/numbers/{numberId:\w+}")]
        public VoipPhone GetNumber(string numberId)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            return DaoFactory.GetVoipDao().GetNumber(numberId).NotFoundIfNull();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Read(@"voip/numbers/current")]
        public VoipPhone GetCurrentNumber()
        {
            return DaoFactory.GetVoipDao().GetCurrentNumber().NotFoundIfNull();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Read(@"voip/token")]
        public string GetToken()
        {
            return VoipProvider.GetToken(GetCurrentNumber().Caller);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"voip/numbers/{numberId:\w+}/settings")]
        public VoipPhone UpdateSettings(string numberId, string greeting, string holdUp, string wait, VoiceMail voiceMail, WorkingHours workingHours, bool? allowOutgoingCalls, bool? record, string alias)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var dao = DaoFactory.GetVoipDao();
            var number = dao.GetNumber(numberId).NotFoundIfNull();

            number.Alias = Update.IfNotEmptyAndNotEquals(number.Alias, alias);
            number.Settings.GreetingAudio = Update.IfNotEmptyAndNotEquals(number.Settings.GreetingAudio, greeting);
            number.Settings.HoldAudio = Update.IfNotEmptyAndNotEquals(number.Settings.HoldAudio, holdUp);
            number.Settings.VoiceMail = Update.IfNotEmptyAndNotEquals(number.Settings.VoiceMail, voiceMail);
            number.Settings.WorkingHours = Update.IfNotEmptyAndNotEquals(number.Settings.WorkingHours, workingHours);

            if (!string.IsNullOrEmpty(wait))
            {
                number.Settings.Queue.WaitUrl = wait;
            }

            if (allowOutgoingCalls.HasValue)
            {
                number.Settings.AllowOutgoingCalls = allowOutgoingCalls.Value;
                if (!number.Settings.AllowOutgoingCalls)
                {
                    number.Settings.Operators.ForEach(r => r.AllowOutgoingCalls = false);
                }
            }

            if (record.HasValue)
            {
                number.Settings.Record = record.Value;
                if (!number.Settings.Record)
                {
                    number.Settings.Operators.ForEach(r => r.Record = false);
                }
            }

            dao.SaveOrUpdateNumber(number);

            return number;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"voip/numbers/settings")]
        public object UpdateSettings(Queue queue, bool pause)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var dao = DaoFactory.GetVoipDao();
            var numbers = dao.GetNumbers();

            if (queue != null)
            {
                foreach (var number in numbers)
                {
                    if (number.Settings.Queue == null)
                    {
                        var phone = number as TwilioPhone;
                        if (phone != null)
                        {
                            queue = phone.CreateQueue(phone.Number, queue.Size, queue.WaitUrl, queue.WaitTime * 60);
                        }

                        queue.Name = number.Number;
                        number.Settings.Queue = queue;
                    }
                    else
                    {
                        var oldQueue = number.Settings.Queue;
                        oldQueue.Size = Update.IfNotEmptyAndNotEquals(oldQueue.Size, queue.Size);
                        oldQueue.WaitTime = Update.IfNotEmptyAndNotEquals(oldQueue.WaitTime, queue.WaitTime * 60);
                        oldQueue.WaitUrl = Update.IfNotEmptyAndNotEquals(oldQueue.WaitUrl, queue.WaitUrl);
                    }

                    number.Settings.Pause = pause;

                    dao.SaveOrUpdateNumber(number);
                }
            }

            return new {queue, pause};
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>

        [Read(@"voip/numbers/settings")]
        public object GetVoipSettings()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var dao = DaoFactory.GetVoipDao();
            var number = dao.GetNumbers().FirstOrDefault(r => r.Settings.Queue != null);
            if (number != null)
            {
                return new {queue = number.Settings.Queue, pause = number.Settings.Pause};
            }

            var files = StorageFactory.GetStorage("", "crm").ListFiles("voip", "default/" + AudioType.Queue.ToString().ToLower(), "*.*", true);

            return new {queue = new Queue("Default", CommonLinkUtility.GetFullAbsolutePath(files.First().ToString())), pause = false};
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Read(@"voip/uploads")]
        public IEnumerable<VoipUpload> GetUploadedFilesUri()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var result = new List<VoipUpload>();

            foreach (var o in Enum.GetNames(typeof(AudioType)))
            {
                var files = Global.GetStore().ListFiles("voip", o.ToLower(), "*", true).ToList();
                files.AddRange(StorageFactory.GetStorage("", "crm").ListFiles("voip", "default/" + o.ToLower(), "*.*", true));

                result.AddRange(files.Select(r => new VoipUpload
                    {
                        Path = CommonLinkUtility.GetFullAbsolutePath(r.ToString()),
                        Name = Path.GetFileName(r.ToString()),
                        AudioType = (AudioType)Enum.Parse(typeof(AudioType), o)
                    }));
            }


            return result;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Delete(@"voip/uploads")]
        public VoipUpload DeleteUploadedFile(AudioType audioType, string fileName)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var store = Global.GetStore();
            var path = Path.Combine(audioType.ToString().ToLower(), fileName);
            var result = new VoipUpload
                {
                    AudioType = audioType,
                    Name = fileName,
                    Path = CommonLinkUtility.GetFullAbsolutePath(store.GetUri(path).ToString())
                };

            if (!store.IsFile("voip", path)) throw new ItemNotFoundException();
            store.Delete("voip", path);

            var dao = DaoFactory.GetVoipDao();
            var numbers = dao.GetNumbers();

            var defAudio = StorageFactory.GetStorage("", "crm").ListFiles("voip", "default/" + audioType.ToString().ToLower(), "*.*", true).FirstOrDefault();
            if (defAudio == null) return result;

            foreach (var number in numbers)
            {
                switch (audioType)
                {
                    case AudioType.Greeting:
                        if (number.Settings.GreetingAudio == result.Path)
                        {
                            number.Settings.GreetingAudio = CommonLinkUtility.GetFullAbsolutePath(defAudio.ToString());
                        }
                        break;
                    case AudioType.HoldUp:
                        if (number.Settings.HoldAudio == result.Path)
                        {
                            number.Settings.HoldAudio = CommonLinkUtility.GetFullAbsolutePath(defAudio.ToString());
                        }
                        break;
                    case AudioType.Queue:
                        var queue = number.Settings.Queue;
                        if (queue != null && queue.WaitUrl == result.Path)
                        {
                            queue.WaitUrl = CommonLinkUtility.GetFullAbsolutePath(defAudio.ToString());
                        }
                        break;
                    case AudioType.VoiceMail:
                        if (number.Settings.VoiceMail != null && number.Settings.VoiceMail.Url == result.Path)
                        {
                            number.Settings.VoiceMail.Url = CommonLinkUtility.GetFullAbsolutePath(defAudio.ToString());
                        }
                        break;
                }

                dao.SaveOrUpdateNumber(number);
            }

            return result;
        }

        #endregion

        #region Operators

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Read(@"voip/numbers/{numberId:\w+}/oper")]
        public IEnumerable<Guid> GetOperators(string numberId)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            return DaoFactory.GetVoipDao().GetNumber(numberId).Settings.Operators.Select(r => r.Id);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [Update(@"voip/numbers/{numberId:\w+}/oper")]
        public IEnumerable<Agent> AddOperators(string numberId, IEnumerable<Guid> operators)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            if (DaoFactory.GetVoipDao().GetNumbers().SelectMany(r => r.Settings.Operators).Any(r => operators.Contains(r.Id)))
            {
                throw new ArgumentException("Duplicate", "operators");
            }

            var dao = DaoFactory.GetVoipDao();
            var phone = dao.GetNumber(numberId);
            var lastOper = phone.Settings.Operators.LastOrDefault();
            var startOperId = lastOper != null ? Convert.ToInt32(lastOper.PostFix) + 1 : 100;

            var addedOperators = operators.Select(o => new Agent(o, AnswerType.Client, phone, (startOperId++).ToString(CultureInfo.InvariantCulture))).ToList();
            phone.Settings.Operators.AddRange(addedOperators);

            dao.SaveOrUpdateNumber(phone);
            return addedOperators;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Delete(@"voip/numbers/{numberId:\w+}/oper")]
        public Guid DeleteOperator(string numberId, Guid oper)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var dao = DaoFactory.GetVoipDao();
            var phone = dao.GetNumber(numberId);
            var startOperId = 100;

            phone.Settings.Operators.RemoveAll(r => r.Id == oper);
            phone.Settings.Operators.ToList()
                 .ForEach(r =>
                     {
                         r.PhoneNumber = phone.Number;
                         r.PostFix = startOperId.ToString(CultureInfo.InvariantCulture);
                         startOperId++;
                     });

            dao.SaveOrUpdateNumber(phone);
            return oper;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"voip/opers/{operatorId}")]
        public Agent UpdateOperator(Guid operatorId, AgentStatus? status, bool? allowOutgoingCalls, bool? record, AnswerType? answerType, string redirectToNumber)
        {
            if (!CRMSecurity.IsAdmin && !operatorId.Equals(SecurityContext.CurrentAccount.ID)) throw CRMSecurity.CreateSecurityException();

            var dao = DaoFactory.GetVoipDao();
            var phone = dao.GetNumbers().FirstOrDefault(r => r.Settings.Operators.Exists(a => a.Id == operatorId)).NotFoundIfNull();

            var oper = phone.Settings.Operators.Find(r => r.Id == operatorId);

            if (status.HasValue)
            {
                oper.Status = status.Value;
            }

            if (allowOutgoingCalls.HasValue)
            {
                oper.AllowOutgoingCalls = phone.Settings.AllowOutgoingCalls && allowOutgoingCalls.Value;
            }

            if (record.HasValue)
            {
                oper.Record = phone.Settings.Record && record.Value;
            }

            if (answerType.HasValue)
            {
                oper.Answer = answerType.Value;
            }

            if (!string.IsNullOrEmpty(redirectToNumber))
            {
                oper.RedirectToNumber = redirectToNumber;
            }

            dao.SaveOrUpdateNumber(phone);

            return oper;
        }

        #endregion

        #region Calls

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Create(@"voip/call")]
        public VoipCallWrapper MakeCall(string to, string contactId)
        {
            var number = DaoFactory.GetVoipDao().GetCurrentNumber().NotFoundIfNull();
            if (!number.Settings.Caller.AllowOutgoingCalls) throw new SecurityException(CRMErrorsResource.AccessDenied);

            var contactPhone = to.TrimStart('+');
            var contact = string.IsNullOrEmpty(contactId) ? 
                GetContactsByContactInfo(ContactInfoType.Phone, contactPhone, null, null).FirstOrDefault() : 
                GetContactByID(Convert.ToInt32(contactId));

            if (contact == null)
            {
                contact = CreatePerson(contactPhone, TenantUtil.DateTimeFromUtc(DateTime.UtcNow).ToString("yyyy-MM-dd hh:mm"), null, 0, null, ShareType.None, new List<Guid> { SecurityContext.CurrentAccount.ID }, null, null);
                DaoFactory.GetContactInfoDao().Save(new ContactInfo { ContactID = contact.ID, IsPrimary = true, InfoType = ContactInfoType.Phone, Data = contactPhone });
            }

            contact = GetContactWithFotos(contact);
            var call = number.Call(to, contact.ID.ToString(CultureInfo.InvariantCulture));
            return new VoipCallWrapper(call, contact);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Create(@"voip/call/{callId:\w+}/answer")]
        public VoipCallWrapper AnswerCall(string callId)
        {
            var dao = DaoFactory.GetVoipDao();
            var call = dao.GetCall(callId).NotFoundIfNull();
            var number = dao.GetCurrentNumber().NotFoundIfNull();
            number.AnswerQueueCall(call.Id);
            return new VoipCallWrapper(call);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Create(@"voip/call/{callId:\w+}/reject")]
        public VoipCallWrapper RejectCall(string callId)
        {
            var dao = DaoFactory.GetVoipDao();
            var call = dao.GetCall(callId).NotFoundIfNull();
            var number = dao.GetCurrentNumber().NotFoundIfNull();
            number.RejectQueueCall(call.Id);
            return new VoipCallWrapper(call);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Create(@"voip/call/{callId:\w+}/redirect")]
        public VoipCallWrapper ReditectCall(string callId, string to)
        {
            var dao = DaoFactory.GetVoipDao();
            var call = dao.GetCall(callId).NotFoundIfNull();
            var number = dao.GetCurrentNumber().NotFoundIfNull();
            number.RedirectCall(call.Id, to);
            return new VoipCallWrapper(call);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Create(@"voip/call/{callId:\w+}")]
        public VoipCallWrapper SaveCall(string callId, string from, string to, Guid answeredBy, VoipCallStatus? status, string contactId, decimal? price)
        {
            var dao = DaoFactory.GetVoipDao();
            
            var call = dao.GetCall(callId) ?? new VoipCall();

            call.Id = callId;
            call.From = Update.IfNotEmptyAndNotEquals(call.From, from);
            call.To = Update.IfNotEmptyAndNotEquals(call.To, to);
            call.AnsweredBy = Update.IfNotEmptyAndNotEquals(call.AnsweredBy, answeredBy);

            if (call.ContactId == 0)
            {
                var contactPhone = call.Status == VoipCallStatus.Incoming ? from : to;
                if (!string.IsNullOrEmpty(contactId))
                {
                    call.ContactId = Convert.ToInt32(contactId);
                }
                else if (status.HasValue && (status.Value == VoipCallStatus.Incoming || status.Value == VoipCallStatus.Outcoming))
                {
                    call.ContactId = DaoFactory.GetContactDao().GetContactIDsByContactInfo(ContactInfoType.Phone, contactPhone.TrimStart('+'), null, null).FirstOrDefault();
                }

                if (call.ContactId == 0)
                {
                    var person = CreatePerson(contactPhone, TenantUtil.DateTimeFromUtc(DateTime.UtcNow).ToString("yyyy-MM-dd hh:mm"), null, 0, null, ShareType.None, new List<Guid> { SecurityContext.CurrentAccount.ID }, null, null);
                    DaoFactory.GetContactInfoDao().Save(new ContactInfo { ContactID = person.ID, IsPrimary = true, InfoType = ContactInfoType.Phone, Data = contactPhone });
                    call.ContactId = person.ID;
                }
            }

            if (status.HasValue)
            {
                call.Status = status.Value;
            }

            if (call.Price == 0 && price.HasValue)
            {
                call.Price = price.Value;
                VoipPaymentSettings.Increment((int)(price.Value * 1000));
            }

            call = dao.SaveOrUpdateCall(call);

            if (call.ContactId == 0) return new VoipCallWrapper(call);

            var contact = GetContactByID(call.ContactId);
            contact = GetContactWithFotos(contact);

            return new VoipCallWrapper(call, contact);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Create(@"voip/callhistory/{callId:\w+}")]
        public VoipCallHistory SaveCallHistory(string callId, string parentCallId, Guid? answeredBy, string recordUrl, int recordDuration, decimal? price, 
            ApiDateTime queueDate = null, ApiDateTime answerDate = null, ApiDateTime endDialDate = null)
        {
            var dao = DaoFactory.GetVoipDao();
            var parentCall = dao.GetCall(parentCallId).NotFoundIfNull();

            var listItemDao = DaoFactory.GetListItemDao();
            var call = dao.GetCallHistoryById(parentCallId, callId);

            if (call == null)
            {
                if (parentCallId != callId)
                    call = dao.GetCallHistoryById(parentCallId, parentCallId);

                if (call == null)
                {
                    call = new VoipCallHistory { ID = callId };

                }
                else
                {
                    call.ID = callId;
                    dao.UpdateCallHistoryId(call);
                }
            }

            call.ParentID = parentCallId;

            if (answeredBy.HasValue)
            {
                call.AnsweredBy = answeredBy.Value;
            }

            call.QueueDate = Update.IfNotEmptyAndNotEquals(call.QueueDate, queueDate);
            call.AnswerDate = Update.IfNotEmptyAndNotEquals(call.AnswerDate, answerDate);

            if (endDialDate != null)
            {
                call.EndDialDate = endDialDate;
                var note = parentCall.Status == VoipCallStatus.Incoming ? CRMContactResource.HistoryVoipIncomingNote : CRMContactResource.HistoryVoipOutcomingNote;

                var category = listItemDao.GetByTitle(ListType.HistoryCategory, CRMCommonResource.HistoryCategory_Call);
                if (category == null)
                {
                    category = new ListItem(CRMCommonResource.HistoryCategory_Call, "event_category_call.png");
                    category.ID = listItemDao.CreateItem(ListType.HistoryCategory, category);
                }
                AddHistoryTo(null, 0, parentCall.ContactId, string.Format(note, call.EndDialDate.Subtract(call.AnswerDate).Seconds.ToString(CultureInfo.InvariantCulture)), category.ID, (ApiDateTime)(DateTime.UtcNow), null, null);
            }

            call.RecordUrl = Update.IfNotEmptyAndNotEquals(call.RecordUrl, recordUrl);


            if (recordDuration != 0)
            {
                call.RecordDuration = recordDuration;
            }

            if (call.Price == 0 && price.HasValue)
            {
                call.Price = price.Value;
                VoipPaymentSettings.Increment((int)(price.Value * 1000));
            }

            return dao.SaveOrUpdateCallHistory(call);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Read(@"voip/call")]
        public IEnumerable<VoipCallWrapper> GetCalls(string callType, ApiDateTime from, ApiDateTime to, Guid? agent, int? client)
        {
            var voipDao = DaoFactory.GetVoipDao();

            var filter = new VoipCallFilter
                {
                    Type = callType,
                    FromDate = from != null ? from.UtcTime : (DateTime?)null,
                    ToDate = to != null ? to.UtcTime.AddDays(1).AddMilliseconds(-1) : (DateTime?)null,
                    Agent = agent,
                    Client = client,
                    SortBy = _context.SortBy,
                    SortOrder = !_context.SortDescending,
                    SearchText = _context.FilterValue,
                    Offset = _context.StartIndex,
                    Max = _context.Count,
                };

            _context.SetDataPaginated();
            _context.SetDataFiltered();
            _context.SetDataSorted();
            _context.TotalCount = voipDao.GetCallsCount(filter);

            var defaultSmallPhoto = ContactPhotoManager.GetSmallSizePhoto(-1, false);            
            var calls = voipDao.GetCalls(filter).Select(
                r =>
                    {
                        ContactWrapper contact;
                        if (r.ContactId != 0)
                        {
                            contact = r.ContactIsCompany
                                          ? (ContactWrapper)new CompanyWrapper(r.ContactId) {DisplayName = r.ContactTitle}
                                          : new PersonWrapper(r.ContactId) {DisplayName = r.ContactTitle};
                            contact.SmallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(contact.ID, contact.IsCompany);
                        }
                        else
                        {
                            contact = new PersonWrapper(-1) { SmallFotoUrl = defaultSmallPhoto };
                        }            
                        return new VoipCallWrapper(r, contact);
                    }).ToList();
            return calls;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Read(@"voip/call/missed")]
        public IEnumerable<VoipCallWrapper> GetMissedCalls()
        {
            var voipDao = DaoFactory.GetVoipDao();
            var defaultSmallPhoto = ContactPhotoManager.GetSmallSizePhoto(-1, false);   

            var calls = voipDao.GetMissedCalls(SecurityContext.CurrentAccount.ID).Select(
                r =>
                {
                    ContactWrapper contact;
                    if (r.ContactId != 0)
                    {
                        contact = r.ContactIsCompany
                                      ? (ContactWrapper)new CompanyWrapper(r.ContactId) { DisplayName = r.ContactTitle }
                                      : new PersonWrapper(r.ContactId) { DisplayName = r.ContactTitle };
                        contact.SmallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(contact.ID, contact.IsCompany);
                    }
                    else
                    {
                        contact = new PersonWrapper(-1) { SmallFotoUrl = defaultSmallPhoto };
                    }
                    return new VoipCallWrapper(r, contact);
                }).ToList();

            _context.SetDataPaginated();
            _context.SetDataFiltered();
            _context.SetDataSorted();
            _context.TotalCount = calls.Count;

            return calls;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Read(@"voip/call/{callId:\w+}")]
        public VoipCallWrapper GetCall(string callId)
        {
            var call = DaoFactory.GetVoipDao().GetCall(callId);
            if (call.ContactId == 0) return new VoipCallWrapper(call);

            var contact = GetContactByID(call.ContactId);
            contact = GetContactWithFotos(contact);
            
            return new VoipCallWrapper(call, contact);
        }

        #endregion

        private static ContactWrapper GetContactWithFotos(ContactWrapper contact)
        {
            contact.SmallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(contact.ID, contact.IsCompany);
            contact.MediumFotoUrl = ContactPhotoManager.GetMediumSizePhoto(contact.ID, contact.IsCompany);

            return contact;
        }
    }
}