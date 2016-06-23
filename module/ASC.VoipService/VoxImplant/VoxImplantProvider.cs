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
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ASC.VoipService.Twilio;
using RestSharp;
using Yahoo.Yui.Compressor;

namespace ASC.VoipService.VoxImplant
{
    public class VoxImplantProvider : IVoipProvider
    {
        private readonly RestClient restClient;
        private readonly string accountEmail;
        private readonly string apiKey;


        public VoxImplantProvider(string accountEmail, string apiKey)
        {
            this.accountEmail = accountEmail;
            this.apiKey = apiKey;

            restClient = new RestClient("https://api.voximplant.com/platform_api/");
        }

        #region IVoipService

        public VoipPhone DeleteNumber(VoipPhone phone)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "DeactivatePhoneNumber/",
                Parameters = { new Parameter { Name = "phone_id", Value = phone.Id, Type = ParameterType.QueryString } }
            };

            var result = Execute<VoxImplantBaseResponse>(request);
            ThrowIfError(result);
            return phone;
        }

        public VoipPhone BuyNumber(string phoneNumber)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "AttachPhoneNumber/",
                Parameters = { new Parameter { Name = "phone_number", Value = phoneNumber, Type = ParameterType.QueryString } }
            };

            var result = Execute<VoxImplantBaseResponse>(request);
            ThrowIfError(result);
            return new VoipPhone { Number = phoneNumber };
        }

        public IEnumerable<VoipPhone> GetExistingPhoneNumbers()
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "GetPhoneNumbers/"
            };

            var result = Execute<ListInfoTypeResponse<AttachedPhoneInfoType>>(request);
            ThrowIfError(result);
            return result.Result.Select(r => new VoipPhone { Id = r.PhoneID.ToString(CultureInfo.InvariantCulture), Number = r.PhoneNumber });
        }

        public IEnumerable<VoipPhone> GetAvailablePhoneNumbers(PhoneNumberType phoneNumberType, string isoCountryCode)
        {
            var request = new RestRequest
                {
                Method = Method.GET,
                Resource = "GetNewPhoneNumbers/"
            };

            request.AddParameter("country_code", isoCountryCode);

            var result = Execute<ListInfoTypeResponse<NewPhoneInfoType>>(request);
            ThrowIfError(result);
            return result.Result.Select(r => new VoipPhone {Number = r.PhoneNumber, Id = r.PhoneID});
        }

        public AttachedPhoneInfoType GetPhone(string id)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "GetPhoneNumbers/",
                Parameters = { new Parameter { Name = "phone_id", Value = id, Type = ParameterType.QueryString } }
            };

            var result = Execute<ListInfoTypeResponse<AttachedPhoneInfoType>>(request);
            ThrowIfError(result);

            return result.Result.First();
        }

        public VoipPhone GetPhone(object[] data)
        {
            return new VoxImplantPhone(this)
                {
                    Id = (string) data[0],
                    Number = (string) data[1],
                    Alias = (string) data[2],
                    Settings = new VoxImplantSettings((string) data[3])
                };
        }

        public string GetToken(Agent agent, int seconds = 86400)
        {
            throw new NotImplementedException();
        }
        

        public string GetRecord(string callId)
        {
            return GetCallHistory().SelectMany(r => r.Calls).First(r => r.CallID == callId).RecordURL;
        }

        public void UpdateSettings(VoipPhone phone)
        {
            ApplicationInfoType application;
            RuleInfoType rule;
            ScenarioInfoType scenario;
            var attachedPhone = GetPhone(phone.Id);

            if (attachedPhone.ApplicationID != 0)
            {
                application = GetApplications(attachedPhone.ApplicationName).First();
            }
            else
            {
                application = AddApplication(phone.Settings.Name);
                BindApplication(application, attachedPhone);
            }

            var users = GetUsers();

            foreach (var oper in phone.Settings.Operators)
            {
                var user = users.Find(r => r.UserName == phone.Settings.Name + oper.ClientID);

                if (user == null)
                {
                    user = AddUser(new UserInfoType { UserName = phone.Settings.Name + oper.ClientID, UserDisplayName = phone.Settings.Name + oper.ClientID }, "111111");
                    users.Add(user);
                }

                if (user.Applications == null || !user.Applications.Any())
                {
                    BindUser(user.UserID, application.ApplicationID);
                    user.Applications = new List<ApplicationInfoType> { application };
                }
            }

            var rules = GetRules(attachedPhone.ApplicationID);
            if (!rules.Any())
            {
                rule = AddRule(new RuleInfoType { ApplicationID = attachedPhone.ApplicationID, RuleName = phone.Settings.Name, RulePattern = ".*" });
                rules.Add(rule);
            }

            rule = rules.First();
            if (rule.Scenarios == null || !rule.Scenarios.Any())
            {
                rule.Scenarios = new List<ScenarioInfoType>();
                scenario = AddScenario(new ScenarioInfoType { ScenarioName = phone.Settings.Name, ScenarioScript = phone.Settings.Connect() });
                BindScenario(scenario.ScenarioID, rule.RuleID);

                rule.Scenarios.Add(scenario);

            }
            else
            {
                scenario = rule.Scenarios.First();
                scenario.ScenarioScript = phone.Settings.Connect();
                UpdateScenario(scenario);
            }
        }

        #endregion

        #region internal

        internal List<UserInfoType> GetUsers()
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "GetUsers/",
                Parameters =
                        {
                            new Parameter { Name = "with_applications", Value = true, Type = ParameterType.QueryString }
                        }
            };

            var users = Execute<ListInfoTypeResponse<UserInfoType>>(request);
            ThrowIfError(users);

            return users.Result;
        }

        internal UserInfoType AddUser(UserInfoType user, string password)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "AddUser/",
                Parameters =
                        {
                            new Parameter { Name = "user_display_name", Value = user.UserDisplayName, Type = ParameterType.QueryString },
                            new Parameter { Name = "user_name", Value = user.UserName, Type = ParameterType.QueryString },
                            new Parameter { Name = "user_password", Value = password, Type = ParameterType.QueryString },
                        }
            };

            user.UserID = Execute<UserInfoType>(request).UserID;

            return user;
        }

        internal void BindUser(string userId, string applicationId)
        {
            var request = new RestRequest
                {
                    Method = Method.POST,
                    Resource = "BindUser/",
                    Parameters =
                        {
                            new Parameter {Name = "user_id", Value = userId, Type = ParameterType.QueryString},
                            new Parameter {Name = "application_id", Value = applicationId, Type = ParameterType.QueryString},
                            new Parameter {Name = "bind", Value = true, Type = ParameterType.QueryString},
                        }
                };

            Execute<VoxImplantBaseResponse>(request);
        }


        internal List<ApplicationInfoType> GetApplications(string applicationName)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "GetApplications/",
                Parameters =
                        {
                            new Parameter { Name = "application_name", Value = applicationName, Type = ParameterType.QueryString },
                            new Parameter { Name = "count", Value = 1, Type = ParameterType.QueryString }
                        }
            };

            var applications = Execute<ListInfoTypeResponse<ApplicationInfoType>>(request);
            ThrowIfError(applications);

            return applications.Result;
        }

        internal ApplicationInfoType AddApplication(string applicationName)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "AddApplication/",
                Parameters =
                        {
                            new Parameter { Name = "application_name", Value = applicationName, Type = ParameterType.QueryString },
                        }
            };

            return Execute<ApplicationInfoType>(request);
        }

        internal void BindApplication(ApplicationInfoType application, AttachedPhoneInfoType phone)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "BindPhoneNumberToApplication/",
                Parameters =
                        {
                            new Parameter { Name = "application_id", Value = application.ApplicationID, Type = ParameterType.QueryString },
                            new Parameter { Name = "phone_id", Value = phone.PhoneID, Type = ParameterType.QueryString },
                            new Parameter { Name = "bind", Value = true, Type = ParameterType.QueryString },
                        }
            };

            Execute<VoxImplantBaseResponse>(request);

            phone.ApplicationID = Convert.ToInt32(application.ApplicationID);
            phone.ApplicationName = application.ApplicationName;
        }


        internal List<RuleInfoType> GetRules(int applicationID)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "GetRules/",
                Parameters =
                        {
                            new Parameter { Name = "application_id", Value = applicationID, Type = ParameterType.QueryString },
                            new Parameter { Name = "with_scenarios", Value = true, Type = ParameterType.QueryString },
                            new Parameter { Name = "count", Value = 1, Type = ParameterType.QueryString }
                        }
            };

            var rules = Execute<ListInfoTypeResponse<RuleInfoType>>(request);
            ThrowIfError(rules);

            return rules.Result;
        }

        internal RuleInfoType AddRule(RuleInfoType rule)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "AddRule/",
                Parameters =
                        {
                            new Parameter { Name = "application_id", Value = rule.ApplicationID, Type = ParameterType.QueryString },
                            new Parameter { Name = "rule_name", Value = rule.RuleName, Type = ParameterType.QueryString },
                            new Parameter { Name = "rule_pattern", Value = rule.RulePattern, Type = ParameterType.QueryString },
                        }
            };

            rule.RuleID = Execute<RuleInfoType>(request).RuleID;

            return rule;
        }


        internal ScenarioInfoType AddScenario(ScenarioInfoType scenario)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "AddScenario/",
                Parameters =
                        {
                            new Parameter { Name = "scenario_name", Value = scenario.ScenarioName, Type = ParameterType.QueryString },
                            new Parameter { Name = "scenario_script", Value = Compress(scenario.ScenarioScript), Type = ParameterType.QueryString }
                        }
            };

            scenario.ScenarioID = Execute<ScenarioInfoType>(request).ScenarioID;

            return scenario;
        }

        internal void BindScenario(string scenarioId, string ruleId)
        {
            var request = new RestRequest
                {
                    Method = Method.POST,
                    Resource = "BindScenario/",
                    Parameters =
                        {
                            new Parameter {Name = "bind", Value = true, Type = ParameterType.QueryString},
                            new Parameter {Name = "rule_id", Value = ruleId, Type = ParameterType.QueryString},
                            new Parameter {Name = "scenario_id", Value = scenarioId, Type = ParameterType.QueryString}
                        }
                };

            Execute<VoxImplantBaseResponse>(request);
        }

        internal void UpdateScenario(ScenarioInfoType scenario)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "SetScenarioInfo/",
                Parameters =
                        {
                            new Parameter { Name = "scenario_id", Value = scenario.ScenarioID, Type = ParameterType.QueryString },
                            new Parameter { Name = "scenario_name", Value = scenario.ScenarioName, Type = ParameterType.QueryString },
                            new Parameter { Name = "scenario_script", Value = Compress(scenario.ScenarioScript), Type = ParameterType.QueryString }
                        }
            };

            var result = Execute<VoxImplantBaseResponse>(request);
            ThrowIfError(result);
        }

        internal ScenarioResponse StartScenario(string ruleID)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "StartScenarios/",
                Parameters =
                    {
                        new Parameter
                            {
                                Name = "rule_id", 
                                Value = ruleID, 
                                Type = ParameterType.QueryString
                            }
                    }
            };


            var scenario = Execute<ScenarioResponse>(request);
            ThrowIfError(scenario);

            return scenario;
        }

        internal List<ScenarioInfoType> GetScenarios(string scenarioID)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "GetScenarios/",
                Parameters =
                        {
                            new Parameter { Name = "scenario_id", Value = scenarioID, Type = ParameterType.QueryString },
                            new Parameter { Name = "with_script", Value = true, Type = ParameterType.QueryString }
                        }
            };

            var scenario = Execute<ListInfoTypeResponse<ScenarioInfoType>>(request);
            ThrowIfError(scenario);

            return scenario.Result;
        }


        internal List<CallSessionInfoType> GetCallHistory()
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "GetCallHistory/",
                Parameters = { new Parameter { Name = "with_calls", Value = true, Type = ParameterType.QueryString } }
            };

            var scenario = Execute<ListInfoTypeResponse<CallSessionInfoType>>(request);
            ThrowIfError(scenario);

            return scenario.Result;
        }


        internal T Execute<T>(IRestRequest request) where T : new()
        {
            request.Parameters.Add(new Parameter { Name = "api_key", Value = apiKey, Type = ParameterType.QueryString });
            request.Parameters.Add(new Parameter { Name = "account_email", Value = accountEmail, Type = ParameterType.QueryString });

            return restClient.Execute<T>(request).Data;
        }

        internal static void ThrowIfError(VoxImplantBaseResponse response)
        {
            if (response == null || response.Error == null) return;
            throw new Exception(response.Error.Msg);
        }

        private string Compress(string input)
        {
            var compressor = new JavaScriptCompressor
            {
                CompressionType = CompressionType.Standard,
                Encoding = Encoding.UTF8,
                ObfuscateJavascript = true,
            };

            return compressor.Compress(input);
        }

        #endregion
    }
}
