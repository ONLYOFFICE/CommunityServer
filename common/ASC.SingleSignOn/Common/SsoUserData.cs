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

        public UserInfo ToUserInfo(bool checkExistance = false)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return Constants.LostUser;
            }

            var userInfo = Constants.LostUser;

            if (checkExistance)
            {
                userInfo = CoreContext.UserManager.GetUserByEmail(Email);
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

                var contacts = new List<string> {MOB_PHONE, Phone};
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

                if (string.IsNullOrEmpty(Phone) || userInfo.Contacts.Contains(Phone))
                    return userInfo;

                userInfo.Contacts.Add(MOB_PHONE);
                userInfo.Contacts.Add(Phone);
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
