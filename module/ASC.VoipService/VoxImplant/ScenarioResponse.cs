/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.VoipService.VoxImplant
{
    [DataContract]
    public class ListInfoTypeResponse<T> : VoxImplantBaseResponse
    {
        public List<T> Result { get; set; }
    }

    [DataContract]
    public class ScenarioResponse : VoxImplantBaseResponse
    {
        [DataMember(Name = "media_session_access_url")]
        public string MediaSessionAccessURL { get; set; }

        [DataMember(Name = "result")]
        public string Result { get; set; }

    }

    [DataContract]
    public class ScenarioInfoType
    {
        [DataMember(Name = "scenario_id")]
        public string ScenarioID { get; set; }

        [DataMember(Name = "scenario_name")]
        public string ScenarioName { get; set; }

        [DataMember(Name = "scenario_script")]
        public string ScenarioScript { get; set; }

        [DataMember(Name = "modified")]
        public string Modified { get; set; }
    }

    [DataContract]
    public class RuleInfoType 
    {
        [DataMember(Name = "rule_id")]
        public string RuleID { get; set; }

        [DataMember(Name = "rule_name")]
        public string RuleName { get; set; }

        [DataMember(Name = "rule_pattern")]
        public string RulePattern { get; set; }

        [DataMember(Name = "application_id")]
        public int ApplicationID { get; set; }

        [DataMember(Name = "modified")]
        public string Modified { get; set; }

        [DataMember(Name = "scenarios")]
        public List<ScenarioInfoType> Scenarios { get; set; }
    }

    [DataContract]
    public class ApplicationInfoType 
    {
        [DataMember(Name = "application_id")]
        public string ApplicationID { get; set; }

        [DataMember(Name = "application_name")]
        public string ApplicationName { get; set; }

        [DataMember(Name = "modified")]
        public string Modified { get; set; }
    }

    [DataContract]
    public class UserInfoType 
    {
        [DataMember(Name = "balance")]
        public string Balance { get; set; }

        [DataMember(Name = "mobile_phone")]
        public string MobilePhone { get; set; }

        [DataMember(Name = "modified")]
        public string Modified { get; set; }

        [DataMember(Name = "parent_accounting")]
        public string ParentAccounting { get; set; }

        [DataMember(Name = "two_factor_auth_required")]
        public string TwoFactorAuthRequired { get; set; }

        [DataMember(Name = "user_active")]
        public string UserActive { get; set; }

        [DataMember(Name = "user_custom_data")]
        public string UserCustomData { get; set; }

        [DataMember(Name = "user_display_name")]
        public string UserDisplayName { get; set; }

        [DataMember(Name = "user_id")]
        public string UserID { get; set; }

        [DataMember(Name = "user_name")]
        public string UserName { get; set; }

        [DataMember(Name = "applications")]
        public List<ApplicationInfoType> Applications { get; set; }
    }

    [DataContract]
    public class CallSessionInfoType 
    {
        [DataMember(Name = "account_id")]
        public string AccountID { get; set; }

        [DataMember(Name = "application_id")]
        public string ApplicationID { get; set; }

        [DataMember(Name = "call_session_history_id")]
        public string CallSessionHistoryID { get; set; }

        [DataMember(Name = "duration")]
        public string Duration { get; set; }

        [DataMember(Name = "initiator_address")]
        public string InitiatorAddress { get; set; }

        [DataMember(Name = "log_file_url")]
        public string LogFileURL { get; set; }

        [DataMember(Name = "media_server_address")]
        public string MediaServerAddress { get; set; }

        [DataMember(Name = "start_date")]
        public string StartDate { get; set; }

        [DataMember(Name = "user_id")]
        public string UserID { get; set; }

        [DataMember(Name = "calls")]
        public List<CallInfoType> Calls { get; set; }
    }

    [DataContract]
    public class CallInfoType 
    {
        [DataMember(Name = "call_id")]
        public string CallID { get; set; }

        [DataMember(Name = "cost")]
        public string Cost { get; set; }

        [DataMember(Name = "custom_data")]
        public string CustomData { get; set; }

        [DataMember(Name = "duration")]
        public int Duration { get; set; }

        [DataMember(Name = "incoming")]
        public bool Incoming { get; set; }

        [DataMember(Name = "local_number")]
        public string LocalNumber { get; set; }

        [DataMember(Name = "media_server_address")]
        public string MediaServerAddress { get; set; }

        [DataMember(Name = "record_url")]
        public string RecordURL { get; set; }

        [DataMember(Name = "remote_number")]
        public string RemoteNumber { get; set; }

        [DataMember(Name = "remote_number_type")]
        public string RemoteNumberType { get; set; }

        [DataMember(Name = "start_time")]
        public string StartTime { get; set; }

        [DataMember(Name = "successful")]
        public bool Successful { get; set; }

        [DataMember(Name = "transaction_id")]
        public int TransactionID { get; set; }
    }
}