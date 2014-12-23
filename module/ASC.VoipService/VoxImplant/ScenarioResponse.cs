/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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