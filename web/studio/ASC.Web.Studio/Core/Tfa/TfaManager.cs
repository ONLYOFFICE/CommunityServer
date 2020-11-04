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
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ASC.Common.Caching;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Common;
using Google.Authenticator;
using Resources;


namespace ASC.Web.Studio.Core.TFA
{
    [Serializable]
    [DataContract]
    public class BackupCode
    {
        [DataMember(Name = "Code")]
        private string code;

        public string Code
        {
            get
            {
                try
                {
                    return InstanceCrypto.Decrypt(code);
                }
                catch
                {
                    //support old scheme stored in the DB
                    return Signature.Read<string>(code);
                }
            }
            set { code = InstanceCrypto.Encrypt(value); }
        }

        [DataMember(Name = "IsUsed")]
        public bool IsUsed { get; set; }

        public BackupCode(string code)
        {
            Code = code;
            IsUsed = false;
        }
    }

    public static class TfaManager
    {
        private static readonly TwoFactorAuthenticator Tfa = new TwoFactorAuthenticator();
        private static readonly ICache Cache = AscCache.Memory;

        public static SetupCode GenerateSetupCode(this UserInfo user, int size)
        {
            return Tfa.GenerateSetupCode(SetupInfo.TfaAppSender, user.Email, GenerateAccessToken(user), size, size, true);
        }

        public static bool ValidateAuthCode(this UserInfo user, string code, bool checkBackup = true)
        {
            if (!TfaAppAuthSettings.IsVisibleSettings
                || !TfaAppAuthSettings.Enable)
            {
                return false;
            }

            if (user == null || Equals(user, Constants.LostUser)) throw new Exception(Resource.ErrorUserNotFound);

            code = (code ?? "").Trim();

            if (string.IsNullOrEmpty(code)) throw new Exception(Resource.ActivateTfaAppEmptyCode);

            int counter;
            int.TryParse(Cache.Get<string>("tfa/" + user.ID), out counter);
            if (++counter > SetupInfo.LoginThreshold)
            {
                throw new Authorize.BruteForceCredentialException(Resource.TfaTooMuchError);
            }
            Cache.Insert("tfa/" + user.ID, counter.ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));

            if (!Tfa.ValidateTwoFactorPIN(GenerateAccessToken(user), code))
            {
                if (checkBackup && TfaAppUserSettings.BackupCodesForUser(user.ID).Any(x => x.Code == code && !x.IsUsed))
                {
                    TfaAppUserSettings.DisableCodeForUser(user.ID, code);
                }
                else
                {
                    throw new ArgumentException(Resource.TfaAppAuthMessageError);
                }
            }

            Cache.Insert("tfa/" + user.ID, (--counter).ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));

            if (!SecurityContext.IsAuthenticated)
            {
                var cookiesKey = SecurityContext.AuthenticateMe(user.ID);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
            }

            if (!TfaAppUserSettings.EnableForUser(user.ID))
            {
                user.GenerateBackupCodes();
                return true;
            }

            return false;
        }

        public static IEnumerable<BackupCode> GenerateBackupCodes(this UserInfo user)
        {
            var count = SetupInfo.TfaAppBackupCodeCount;
            var length = SetupInfo.TfaAppBackupCodeLength;

            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";

            var data = new byte[length];

            var list = new List<BackupCode>();

            using (var rngCrypto = new RNGCryptoServiceProvider())
            {
                for (var i = 0; i < count; i++)
                {
                    rngCrypto.GetBytes(data);

                    var result = new StringBuilder(length);
                    foreach (var b in data)
                    {
                        result.Append(alphabet[b % (alphabet.Length)]);
                    }

                    list.Add(new BackupCode(result.ToString()));
                }
            }

            TfaAppUserSettings.BackupCodes = list;
            return list;
        }

        private static string GenerateAccessToken(UserInfo user)
        {
            var userSalt = TfaAppUserSettings.GetSalt(user.ID);

            //from Signature.Create
            var machineSalt = Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant());
            var token = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(userSalt + machineSalt)));
            var encodedToken = HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(token));

            return encodedToken.Substring(0, 10);
        }
    }
}