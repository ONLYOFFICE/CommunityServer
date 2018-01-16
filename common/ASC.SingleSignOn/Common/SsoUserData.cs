using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.SingleSignOn.Common
{
    [Serializable]
    public class SsoUserData
    {
        private const int MAX_NUMBER_OF_SYMBOLS = 64;

        [DataMember(Name = "nameID")]
        public string NameId { get; set; }

        [DataMember(Name = "sessionID")]
        public string SessionId { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "firstName")]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName")]
        public string LastName { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }

        private const string MOB_PHONE = "mobphone";
        private const string EXT_MOB_PHONE = "extmobphone";

        public UserInfo ToUserInfo(bool checkExistance = false)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return Constants.LostUser;
            }

            var userInfo = Constants.LostUser;

            if (checkExistance)
            {
                userInfo = CoreContext.UserManager.GetSsoUserByNameId(NameId);

                if (Equals(userInfo, Constants.LostUser))
                {
                    userInfo = CoreContext.UserManager.GetUserByEmail(Email);
                }
            }

            if (Equals(userInfo, Constants.LostUser))
            {
                userInfo = new UserInfo
                {
                    Email = Email,
                    FirstName = TrimToLimit(FirstName),
                    LastName = TrimToLimit(LastName),
                    SsoNameId = NameId,
                    SsoSessionId = SessionId,
                    Location = Location,
                    Title = Title,
                    ActivationStatus = EmployeeActivationStatus.Activated,
                    WorkFromDate = TenantUtil.DateTimeNow()
                };

                if (string.IsNullOrEmpty(Phone))
                    return userInfo;

                var contacts = new List<string> {EXT_MOB_PHONE, Phone};
                userInfo.Contacts = contacts;
            }
            else
            {
                userInfo.Email = Email;
                userInfo.FirstName = TrimToLimit(FirstName);
                userInfo.LastName = TrimToLimit(LastName);
                userInfo.SsoNameId = NameId;
                userInfo.SsoSessionId = SessionId;
                userInfo.Location = Location;
                userInfo.Title = Title;

                var portalUserContacts = userInfo.Contacts;

                var newContacts = new List<string>();
                var phones = new List<string>();
                var otherContacts = new List<string>();

                for (int i = 0, n = portalUserContacts.Count; i < n; i += 2)
                {
                    if (i + 1 >= portalUserContacts.Count)
                        continue;

                    var type = portalUserContacts[i];
                    var value = portalUserContacts[i + 1];

                    switch (type)
                    {
                        case EXT_MOB_PHONE:
                            break;
                        case MOB_PHONE:
                            phones.Add(value);
                            break;
                        default:
                            otherContacts.Add(type);
                            otherContacts.Add(value);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(Phone))
                {
                    if (phones.Exists(p => p.Equals(Phone)))
                    {
                        phones.Remove(Phone);
                    }

                    newContacts.Add(EXT_MOB_PHONE);
                    newContacts.Add(Phone);
                }

                phones.ForEach(p =>
                {
                    newContacts.Add(MOB_PHONE);
                    newContacts.Add(p);
                });

                newContacts.AddRange(otherContacts);

                userInfo.Contacts = newContacts;
            }

            return userInfo;
        }

        private static string TrimToLimit(string str, int limit = MAX_NUMBER_OF_SYMBOLS)
        {
            return string.IsNullOrEmpty(str)
                ? ""
                : str.Length > limit
                    ? str.Substring(0, MAX_NUMBER_OF_SYMBOLS)
                    : str;
        }
    }

    [Serializable]
    public class LogoutSsoUserData
    {
        [DataMember(Name = "nameID")]
        public string NameId { get; set; }

        [DataMember(Name = "sessionID")]
        public string SessionId { get; set; }
    }
}
