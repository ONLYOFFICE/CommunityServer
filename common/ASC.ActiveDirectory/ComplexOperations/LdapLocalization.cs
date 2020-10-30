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


using System.Resources;

namespace ASC.ActiveDirectory.ComplexOperations
{
    public class LdapLocalization
    {
        private readonly ResourceManager _resourceManager;
        private readonly ResourceManager _notifyResourceManager;

        public LdapLocalization(ResourceManager resourceManager = null, ResourceManager notifyResourceManager = null)
        {
            _resourceManager = resourceManager;
            _notifyResourceManager = notifyResourceManager;
        }

        public string FirstName
        {
            get
            {
                const string def_key = "FirstName";
                const string def_val = "First Name";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LastName
        {
            get
            {
                const string def_key = "LastName";
                const string def_val = "Last Name";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsModifyLdapUsers
        {
            get
            {
                const string def_key = "LdapSettingsModifyLdapUsers";
                const string def_val = "Modifying LDAP users on ordinary portal users";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsTenantQuotaSettled
        {
            get
            {
                const string def_key = "LdapSettingsTenantQuotaSettled";
                const string def_val = "The current pricing plan user limit has been reached";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorCantCreateUsers
        {
            get
            {
                const string def_key = "LdapSettingsErrorCantCreateUsers";
                const string def_val = "Users could not be created, the received data are incorrect.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsInternalServerError
        {
            get
            {
                const string def_key = "LdapSettingsInternalServerError";
                const string def_val = "Server internal error.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusGettingUsersFromLdap
        {
            get
            {
                const string def_key = "LdapSettingsStatusGettingUsersFromLdap";
                const string def_val = "Retrieving the user list from the LDAP server";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusGivingRights
        {
            get
            {
                const string def_key = "LdapSettingsStatusGivingRights";
                const string def_val = "Setting user {0} as {1} admin";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorUsersNotFound
        {
            get
            {
                const string def_key = "LdapSettingsErrorUsersNotFound";
                const string def_val = "No users could be found.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusRemovingOldRights
        {
            get
            {
                const string def_key = "LdapSettingsStatusRemovingOldRights";
                const string def_val = "Removing outdated access rights that have been loaded via LDAP earlier";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusRemovingOldUsers
        {
            get
            {
                const string def_key = "LdapSettingsStatusRemovingOldUsers";
                const string def_val = "Removing outdated user profiles that have been loaded via LDAP earlier";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusSavingUserPhoto
        {
            get
            {
                const string def_key = "LdapSettingsStatusSavingUserPhoto";
                const string def_val = "Saving photo";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusSavingUsers
        {
            get
            {
                const string def_key = "LdapSettingsStatusSavingUsers";
                const string def_val = "Saving users";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusSyncingUsers
        {
            get
            {
                const string def_key = "LdapSettingsStatusSyncingUsers";
                const string def_val = "Syncing users";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusRemovingOldGroups
        {
            get
            {
                const string def_key = "LdapSettingsStatusRemovingOldGroups";
                const string def_val = "Removing outdated groups that have been loaded via LDAP earlier";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusGettingGroupsFromLdap
        {
            get
            {
                const string def_key = "LdapSettingsStatusGettingGroupsFromLdap";
                const string def_val = "Retrieving the group list from the LDAP server";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorGroupsNotFound
        {
            get
            {
                const string def_key = "LdapSettingsErrorGroupsNotFound";
                const string def_val = "No groups could be found.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusSavingGroups
        {
            get
            {
                const string def_key = "LdapSettingsStatusSavingGroups";
                const string def_val = "Saving groups";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorCantGetLdapSettings
        {
            get
            {
                const string def_key = "LdapSettingsErrorCantGetLdapSettings";
                const string def_val = "The server could not get settings.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusCheckingLdapSettings
        {
            get
            {
                const string def_key = "LdapSettingsStatusCheckingLdapSettings";
                const string def_val = "Checking LDAP support settings";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusLoadingBaseInfo
        {
            get
            {
                const string def_key = "LdapSettingsStatusLoadingBaseInfo";
                const string def_val = "Loading LDAP base info";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusCertificateVerification
        {
            get
            {
                const string def_key = "LdapSettingsStatusCertificateVerification";
                const string def_val = "Certificate verification";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongServerOrPort
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongServerOrPort";
                const string def_val =
                    "Unable to connect to LDAP server. Please check if the server address and port number are correct.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongUserDn
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongUserDN";
                const string def_val = "Incorrect User DN.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorIncorrectLdapFilter
        {
            get
            {
                const string def_key = "LdapSettingsErrorIncorrectLdapFilter";
                const string def_val = "Invalid User Filter value.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorLostRights
        {
            get
            {
                const string def_key = "LdapSettingsErrorLostRights";
                const string def_val = "You attempted to take away admin rights from yourself. Your admin rights was unaffected.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorRemovedYourself
        {
            get
            {
                const string def_key = "LdapSettingsErrorRemovedYourself";
                const string def_val = "Your account has been unlinked from LDAP. You may need to set a password for your account because you won't be able to login using LDAP password.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongLoginAttribute
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongLoginAttribute";
                const string def_val = "Could not get Login Attribute for one or several users.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongGroupDn
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongGroupDN";
                const string def_val = "Incorrect Group DN.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongGroupFilter
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongGroupFilter";
                const string def_val = "Could not get Group Attribute for one or several groups.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongGroupAttribute
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongGroupAttribute";
                const string def_val = "Could not get User Attribute for one or several users.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongUserAttribute
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongUserAttribute";
                const string def_val = "Could not get User Attribute for one or several users.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorWrongGroupNameAttribute
        {
            get
            {
                const string def_key = "LdapSettingsErrorWrongGroupNameAttribute";
                const string def_val = "Could not obtain Group Name Attribute for one or several groups.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorCredentialsNotValid
        {
            get
            {
                const string def_key = "LdapSettingsErrorCredentialsNotValid";
                const string def_val = "Incorrect login or password.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsConnectError
        {
            get
            {
                const string def_key = "LdapSettingsConnectError";
                const string def_val =
                    "A more secure authentication type is required. Please use encripted connection or enable plain authentication on the server.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStrongAuthRequired
        {
            get
            {
                const string def_key = "LdapSettingsStrongAuthRequired";
                const string def_val =
                    "A more secure authentication type is required. Please use encripted connection or enable plain authentication on the server.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsWrongSidAttribute
        {
            get
            {
                const string def_key = "LdapSettingsWrongSidAttribute";
                const string def_val =
                    "Unique ID for user/group objects could not be obtained. By default the system will try to match one of the following identifiers: entryUUID, nsuniqueid, GUID, objectSid. If none of the attributes corresponds to your LDAP server, please specify the necessary attribute in the ldap.unique.id setting of the web.appsettings.config file.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsTlsNotSupported
        {
            get
            {
                const string def_key = "LdapSettingsTlsNotSupported";
                const string def_val = "StartTLS not supported for current LDAP server.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorDomainNotFound
        {
            get
            {
                const string def_key = "LdapSettingsErrorDomainNotFound";
                const string def_val = "LDAP domain not found.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorUnknownError
        {
            get
            {
                const string def_key = "LdapSettingsErrorUnknownError";
                const string def_val = "Unknown error.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusSavingSettings
        {
            get
            {
                const string def_key = "LdapSettingsStatusSavingSettings";
                const string def_val = "Saving settings";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsErrorCantSaveLdapSettings
        {
            get
            {
                const string def_key = "LdapSettingsErrorCantSaveLdapSettings";
                const string def_val = "The server could not save settings.";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string ErrorEmailEmpty
        {
            get
            {
                const string def_key = "ErrorEmailEmpty";
                const string def_val = "Email field is empty";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string ErrorAccessDenied
        {
            get
            {
                const string def_key = "ErrorAccessDenied";
                const string def_val = "No permissions to perform this action";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusAddingGroupUser
        {
            get
            {
                const string def_key = "LdapSettingsStatusAddingGroupUser";
                const string def_val = "adding user";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusRemovingGroupUser
        {
            get
            {
                const string def_key = "LdapSettingsStatusRemovingGroupUser";
                const string def_val = "removing user";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusUpdatingAccessRights
        {
            get
            {
                const string def_key = "LdapSettingsStatusUpdatingAccessRights";
                const string def_val = "Updating users access rights";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusUpdatingUserPhotos
        {
            get
            {
                const string def_key = "LdapSettingsStatusUpdatingUserPhotos";
                const string def_val = "Updating user photos";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string LdapSettingsStatusDisconnecting
        {
            get
            {
                const string def_key = "LdapSettingsStatusDisconnecting";
                const string def_val = "LDAP disconnecting";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        public string NotifyButtonJoin
        {
            get
            {
                const string def_key = "ButtonAccessYourPortal";
                const string def_val = "Click here to join the portal";

                return GetValueOrDefault(def_key, def_val);
            }
        }

        private string GetValueOrDefault(string key, string defaultValue)
        {
            try
            {
                var val = _resourceManager != null ? _resourceManager.GetString(key) : null;
                if (val == null && _notifyResourceManager != null)
                {
                    val = _notifyResourceManager.GetString(key);
                }

                return val != null ? val : defaultValue;
            }
            catch
            {
                //
            }

            return defaultValue;
        }
    }
}
