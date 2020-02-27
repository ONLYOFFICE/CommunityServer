/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Linq;
using System.Reflection;
using ASC.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ASC.VoipService
{
    public class VoipSettings
    {
        public string VoiceUrl { get; set; }

        public string Name { get; set; }

        public List<Agent> Operators { get; set; }

        public Queue Queue { get; set; }

        public Agent Caller { get { return Operators.FirstOrDefault(r => r.Id == SecurityContext.CurrentAccount.ID); } }

        public WorkingHours WorkingHours { get; set; }

        public string VoiceMail { get; set; }

        public string GreetingAudio { get; set; }

        public string HoldAudio { get; set; }

        public bool AllowOutgoingCalls { get; set; }

        public bool Pause { get; set; }

        public bool Record { get; set; }

        internal string JsonSettings
        {
            get
            {
                return JsonConvert.SerializeObject(
                    new
                    {
                        Operators,
                        GreetingAudio,
                        Name,
                        Queue,
                        WorkingHours,
                        VoiceMail,
                        HoldAudio,
                        AllowOutgoingCalls,
                        Pause,
                        Record
                    },
                    new JsonSerializerSettings { ContractResolver = CustomSerializeContractResolver.Instance });
            }
            set
            {
                try
                {
                    var settings = JsonConvert.DeserializeObject<VoipSettings>(value, new JsonSerializerSettings {ContractResolver = CustomSerializeContractResolver.Instance });

                    Operators = settings.Operators ?? new List<Agent>();
                    Name = settings.Name;
                    Queue = settings.Queue;
                    WorkingHours = settings.WorkingHours;
                    GreetingAudio = settings.GreetingAudio;
                    VoiceMail = settings.VoiceMail;
                    HoldAudio = settings.HoldAudio;
                    AllowOutgoingCalls = settings.AllowOutgoingCalls;
                    Pause = settings.Pause;
                    Record = settings.Record;
                }
                catch (Exception)
                {

                }

            }
        }


        public VoipSettings()
        {
            Operators = new List<Agent>();
        }

        public VoipSettings(string settings)
        {
            JsonSettings = settings;
        }

        public virtual string Connect(bool user = true, string contactId = null)
        {
            throw new NotImplementedException();
        }

        public virtual string Redirect(string to)
        {
            throw new NotImplementedException();
        }

        public virtual string Dequeue(bool reject)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return JsonSettings;
        }

        public static VoipSettings GetSettings(string settings)
        {
            return new VoipSettings {JsonSettings = settings};
        }
    }

    class CustomSerializeContractResolver : CamelCasePropertyNamesContractResolver
    {
        public static readonly CustomSerializeContractResolver Instance = new CustomSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyName == "voiceMail")
            {
                property.Converter = new VoiceMailConverter();
            }

            return property;
        }
    }

    class VoiceMailConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType != null && reader.ValueType.Name == "String")
            {
                return reader.Value;
            }

            var jObject = JObject.Load(reader);
            var url = jObject.Value<string>("url");

            return !string.IsNullOrEmpty(url) ? url : "";
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}