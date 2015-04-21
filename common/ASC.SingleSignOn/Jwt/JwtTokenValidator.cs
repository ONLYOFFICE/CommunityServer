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


using ASC.SingleSignOn.Common;
using ASC.Web.Studio.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ASC.SingleSignOn.Jwt
{
    public class JwtTokenValidator
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(JwtTokenValidator));
        private readonly int MAX_CLOCK_SKEW = 1;
        private const string JWT = "jwt";

        public string TokenString { get; set; }
        public SecurityToken JwtSecurityToken { get; set; }
        public ClaimsPrincipal ClaimsPrincipalReceived { get; set; }

        public bool IsValidEmail(ClaimsPrincipal claimsPrincipal)
        {
            Claim emailClaim = claimsPrincipal.FindFirst(x => x.Type == ClaimTypes.Email);
            if (emailClaim == null)
            {
                _log.ErrorFormat("No mandatory parameter: {0}", ClaimTypes.Email);
                return false;
            }
            try
            {
                new MailAddress(emailClaim.Value);
                return true;
            }
            catch
            {
                _log.ErrorFormat("Wrong email format: {0}", emailClaim.Value);
                return false;
            }
        }

        public void ValidateJsonWebToken(string tokenString, SsoSettings settings, IList<string> audiences)
        {
            try
            {

                TokenString = tokenString;
                SecurityToken securityToken;
                _log.DebugFormat("JWT Validation securityAlgorithm={0}, audience[0]={1}, audience[1]={2}", settings.ValidationType, audiences[0], audiences[1]);

                switch (settings.ValidationType)
                {
                    case ValidationTypes.RSA_SHA256:
                        RSACryptoServiceProvider publicOnly = new RSACryptoServiceProvider();
                        //"<RSAKeyValue><Modulus>zeyPa4SwRb0IO+KMq20760ZmaUvy/qzecdOkRUNdNpdUe1E72Xt1WkAcWNu24/UeS3pETu08rVTqHJUMfhHcSKgL7LAk/MMj2inGFxop1LipGZSnqZhnjsfj1ERJL5eXs1O9hqyAcXvY4A2wo67qqv/lbHLKTW59W+YQkbIOVR4nQlbh1lK1TIY+oqK0J/5Ileb4QfERn0Rv/J/K0fy6VzLmVt+kg9MRNxYwnVsC3m5/kIu1fw3OpZxcaCC68SRqLLb/UXmaJM8NXYKkAkHKxT4DQqSk6KbFSQG6qi49Q34akohekzxjxmmGeoO5tsFCuMJofKAsBKKtOkLPaJD2rQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>"
                        publicOnly.FromXmlString(settings.PublicKey);
                        securityToken = new RsaSecurityToken(publicOnly);
                        break;
                    case ValidationTypes.HMAC_SHA256:
                        //var key = "zeyPa4SwRb0IO+KMq20760ZmaUvy/qzecdOkRUNdNpdUe1E72Xu24/UeS3pETu";
                        securityToken = new System.ServiceModel.Security.Tokens.BinarySecretSecurityToken(GetBytes(settings.PublicKey));
                        break;
                    case ValidationTypes.X509:
                        var certificate = new Certificate();
                        certificate.LoadCertificate(settings.PublicKey);
                        securityToken = new X509SecurityToken(certificate.cert);
                        break;
                    default:
                        _log.ErrorFormat("ValidationType has wrong value: {0}", settings.ValidationType);
                        throw new ArgumentException("ValidationType has wrong value");
                }
                TokenValidationParameters validationParams = new TokenValidationParameters();
                validationParams.ValidIssuer = settings.Issuer;
                validationParams.ValidAudiences = audiences;
                validationParams.ValidateIssuer = true;
                validationParams.ValidateIssuerSigningKey = true;
                validationParams.ValidateAudience = true;
                validationParams.ValidateActor = true;
                validationParams.IssuerSigningToken = securityToken;

                JwtSecurityTokenHandler recipientTokenHandler = new JwtSecurityTokenHandler();
                recipientTokenHandler.TokenLifetimeInMinutes = MAX_CLOCK_SKEW;
                SecurityToken validatedToken = null;
                ClaimsPrincipalReceived = recipientTokenHandler.ValidateToken(TokenString, validationParams, out validatedToken);
                JwtSecurityToken = validatedToken;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("JWT Validation error. {0}", e);
            }
        }

        public bool CheckJti()
        {
            Claim jtiClaim = ClaimsPrincipalReceived.FindFirst(x => x.Type == SupportedClaimTypes.Jti);

            if (jtiClaim == null)
            {
                _log.ErrorFormat("Jti Claim is null.");
                return false;
            }
            bool result;
            _log.DebugFormat("Jti Validation Jti={0}", jtiClaim.Value);

            var wrapper = new CommonDbWrapper();
            if (!wrapper.JtiIsExists(jtiClaim.Value))
            {
                wrapper.SaveJti(JWT, TenantProvider.CurrentTenantID, jtiClaim.Value, JwtSecurityToken.ValidTo.AddMinutes(MAX_CLOCK_SKEW));
                result = true;
            }
            else
            {
                _log.ErrorFormat("The same JTI as in one of previouses JWT");
                result = false;
            }
            wrapper.RemoveOldJtis();
            return result;
        }

        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}