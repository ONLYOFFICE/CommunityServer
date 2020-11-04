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
using System.Security;
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Exceptions;
using ASC.Api.Utils;
using ASC.Core.Tenants;
using ASC.CRM.Core;
using ASC.Data.Storage;
using ASC.Specific;
using ASC.VoipService;
using ASC.VoipService.Dao;
using ASC.VoipService.Twilio;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.CRM.Resources;
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
        [Read(@"voip/numbers/unlinked")]
        public IEnumerable<VoipPhone> GetUnlinkedPhoneNumbers()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var listPhones =  VoipProvider.GetExistingPhoneNumbers();
            var buyedPhones = DaoFactory.VoipDao.GetNumbers();

            return listPhones.Where(r => buyedPhones.All(b => r.Id != b.Id)).ToList();
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

            return DaoFactory.VoipDao.GetNumbers();
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

            VoipProvider.CreateQueue(newPhone);
            SetDefaultAudio(newPhone);

            VoipProvider.UpdateSettings(newPhone);
            return DaoFactory.VoipDao.SaveOrUpdateNumber(newPhone);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        [Create(@"voip/numbers/link")]
        public VoipPhone LinkNumber(string id)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var newPhone = VoipProvider.GetPhone(id);

            VoipProvider.CreateQueue(newPhone);
            SetDefaultAudio(newPhone);

            VoipProvider.UpdateSettings(newPhone);

            return DaoFactory.VoipDao.SaveOrUpdateNumber(newPhone);
        }

        public void SetDefaultAudio(VoipPhone newPhone)
        {
            var storage = StorageFactory.GetStorage("", "crm");
            const string path = "default/";
            var files = storage.ListFilesRelative("voip", path, "*.*", true)
                               .Select(filePath => new
                                   {
                                       path = CommonLinkUtility.GetFullAbsolutePath(storage.GetUri("voip", Path.Combine(path, filePath)).ToString()),
                                       audioType = (AudioType)Enum.Parse(typeof (AudioType), Directory.GetParent(filePath).Name, true)
                                   }).ToList();

            var audio = files.Find(r => r.audioType == AudioType.Greeting);
            newPhone.Settings.GreetingAudio = audio != null ? audio.path : "";

            audio = files.Find(r => r.audioType == AudioType.HoldUp);
            newPhone.Settings.HoldAudio = audio != null ? audio.path : "";

            audio = files.Find(r => r.audioType == AudioType.VoiceMail);
            newPhone.Settings.VoiceMail = audio != null ? audio.path : "";

            audio = files.Find(r => r.audioType == AudioType.Queue);
            newPhone.Settings.Queue.WaitUrl = audio != null ? audio.path : "";
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

            var dao = DaoFactory.VoipDao;
            var phone = dao.GetNumber(numberId).NotFoundIfNull();

            VoipProvider.DisablePhone(phone);
            dao.DeleteNumber(numberId);
            new SignalRHelper(phone.Number).Reload();

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
            return DaoFactory.VoipDao.GetNumber(numberId).NotFoundIfNull();
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
            return DaoFactory.VoipDao.GetCurrentNumber().NotFoundIfNull();
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
        public VoipPhone UpdateSettings(string numberId, string greeting, string holdUp, string wait, string voiceMail, WorkingHours workingHours, bool? allowOutgoingCalls, bool? record, string alias)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var dao = DaoFactory.VoipDao;
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

            var dao = DaoFactory.VoipDao;
            var numbers = dao.GetNumbers();

            if (queue != null)
            {
                foreach (var number in numbers)
                {
                    if (number.Settings.Queue == null || string.IsNullOrEmpty(number.Settings.Queue.Id))
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

            var dao = DaoFactory.VoipDao;
            var number = dao.GetNumbers().FirstOrDefault(r => r.Settings.Queue != null);
            if (number != null)
            {
                return new {queue = number.Settings.Queue, pause = number.Settings.Pause};
            }

            var files = StorageFactory.GetStorage("", "crm").ListFiles("voip", "default/" + AudioType.Queue.ToString().ToLower(), "*.*", true);
            var file = files.FirstOrDefault();
            return new { queue = new Queue(null, "Default", 5, file != null ? CommonLinkUtility.GetFullAbsolutePath(file.ToString()) : "", 5), pause = false };
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

            foreach (var audioType in Enum.GetNames(typeof (AudioType)))
            {
                var type = (AudioType)Enum.Parse(typeof (AudioType), audioType);

                var path = audioType.ToLower();
                var store = Global.GetStore();
                var filePaths = store.ListFilesRelative("voip", path, "*", true);
                result.AddRange(
                    filePaths.Select(filePath =>
                                     GetVoipUpload(store.GetUri("voip", Path.Combine(path, filePath)), Path.GetFileName(filePath), type)));

                path = "default/" + audioType.ToLower();
                store = StorageFactory.GetStorage("", "crm");
                filePaths = store.ListFilesRelative("voip", path, "*.*", true);
                result.AddRange(
                    filePaths.Select(filePath =>
                                     GetVoipUpload(store.GetUri("voip", Path.Combine(path, filePath)), Path.GetFileName(filePath), type, true)));
            }

            return result;
        }

        private static VoipUpload GetVoipUpload(Uri link, string fileName, AudioType audioType, bool isDefault = false)
        {
            return new VoipUpload
            {
                Path = CommonLinkUtility.GetFullAbsolutePath(link.ToString()),
                Name = fileName,
                AudioType = audioType,
                IsDefault = isDefault
            };
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

            var dao = DaoFactory.VoipDao;
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
                        if (number.Settings.VoiceMail == result.Path)
                        {
                            number.Settings.VoiceMail = CommonLinkUtility.GetFullAbsolutePath(defAudio.ToString());
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
            return DaoFactory.VoipDao.GetNumber(numberId).Settings.Operators.Select(r => r.Id);
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

            if (DaoFactory.VoipDao.GetNumbers().SelectMany(r => r.Settings.Operators).Any(r => operators.Contains(r.Id)))
            {
                throw new ArgumentException("Duplicate", "operators");
            }

            var dao = DaoFactory.VoipDao;
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

            var dao = DaoFactory.VoipDao;
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

            var dao = DaoFactory.VoipDao;
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

            if (allowOutgoingCalls.HasValue)
            {
                new SignalRHelper(phone.Number).Reload(operatorId.ToString());
            }

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
            var number = DaoFactory.VoipDao.GetCurrentNumber().NotFoundIfNull();
            if (!number.Settings.Caller.AllowOutgoingCalls) throw new SecurityException(CRMErrorsResource.AccessDenied);

            var contactPhone = to.TrimStart('+');
            var contact = string.IsNullOrEmpty(contactId) ? 
                GetContactsByContactInfo(ContactInfoType.Phone, contactPhone, null, null).FirstOrDefault() : 
                GetContactByID(Convert.ToInt32(contactId));

            if (contact == null)
            {
                contact = ToContactWrapper(new VoipEngine(DaoFactory).CreateContact(contactPhone));
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
            var dao = DaoFactory.VoipDao;
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
            var dao = DaoFactory.VoipDao;
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
            var dao = DaoFactory.VoipDao;
            var call = dao.GetCall(callId).NotFoundIfNull();
            var number = dao.GetCurrentNumber().NotFoundIfNull();

            if (call.ContactId != 0)
            {
                var contact = DaoFactory.ContactDao.GetByID(call.ContactId);
                var managers = CRMSecurity.GetAccessSubjectGuidsTo(contact);

                if (!managers.Contains(Guid.Parse(to)))
                {
                    managers.Add(Guid.Parse(to));
                    CRMSecurity.SetAccessTo(contact, managers);
                }
            }

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
            var dao = DaoFactory.VoipDao;
            
            var call = dao.GetCall(callId) ?? new VoipCall();

            call.Id = callId;
            call.From = Update.IfNotEmptyAndNotEquals(call.From, from);
            call.To = Update.IfNotEmptyAndNotEquals(call.To, to);
            call.AnsweredBy = Update.IfNotEmptyAndNotEquals(call.AnsweredBy, answeredBy);

            try
            {
                if (call.ContactId == 0)
                {
                    var contactPhone = call.Status == VoipCallStatus.Incoming || call.Status == VoipCallStatus.Answered ? call.From : call.To;
                    if (!string.IsNullOrEmpty(contactId))
                    {
                        call.ContactId = Convert.ToInt32(contactId);
                    }
                    else 
                    {
                        new VoipEngine(DaoFactory).GetContact(call);
                    }

                    if (call.ContactId == 0)
                    {
                        contactPhone = contactPhone.TrimStart('+');
                        var person = CreatePerson(contactPhone, TenantUtil.DateTimeFromUtc(DateTime.UtcNow).ToString("yyyy-MM-dd hh:mm"), null, 0, null, ShareType.None, new List<Guid> { SecurityContext.CurrentAccount.ID }, null, null);
                        DaoFactory.ContactInfoDao.Save(new ContactInfo { ContactID = person.ID, IsPrimary = true, InfoType = ContactInfoType.Phone, Data = contactPhone });
                        call.ContactId = person.ID;
                    }
                }
            }
            catch (Exception)
            {

            }

            if (status.HasValue)
            {
                call.Status = status.Value;
            }

            if (call.Price == 0 && price.HasValue)
            {
                call.Price = price.Value;
            }

            call = dao.SaveOrUpdateCall(call);

            if (call.ContactId == 0) return new VoipCallWrapper(call);
            try
            {
                var contact = GetContactByID(call.ContactId);
                contact = GetContactWithFotos(contact);

                return new VoipCallWrapper(call, contact);
            }
            catch (Exception)
            {
                return new VoipCallWrapper(call);
            }

        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Create(@"voip/price/{callId:\w+}")]
        public void SavePrice(string callId)
        {
            new VoipEngine(DaoFactory).SaveAdditionalInfo(callId);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <short></short>
        /// <category>Voip</category>
        /// <returns></returns>
        [Read(@"voip/call")]
        public IEnumerable<VoipCallWrapper> GetCalls(string callType, ApiDateTime from, ApiDateTime to, Guid? agent, int? client, int? contactID)
        {
            var voipDao = DaoFactory.VoipDao;

            var filter = new VoipCallFilter
                {
                    Type = callType,
                    FromDate = from != null ? from.UtcTime : (DateTime?)null,
                    ToDate = to != null ? to.UtcTime.AddDays(1).AddMilliseconds(-1) : (DateTime?)null,
                    Agent = agent,
                    Client = client,
                    ContactID = contactID,
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
            var voipDao = DaoFactory.VoipDao;
            var defaultSmallPhoto = ContactPhotoManager.GetSmallSizePhoto(-1, false);   

            var calls = voipDao.GetMissedCalls(SecurityContext.CurrentAccount.ID, 10, DateTime.UtcNow.AddDays(-7)).Select(
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
            var call = DaoFactory.VoipDao.GetCall(callId);

            new VoipEngine(DaoFactory).GetContact(call);

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