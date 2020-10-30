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
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using ASC.Common.Logging;

namespace ASC.ActiveDirectory.Base.Data
{
    [Serializable]
    [DataContract]
    public class LdapCertificateConfirmRequest
    {
        private volatile bool _approved;
        private volatile bool _requested;
        private volatile string _serialNumber;
        private volatile string _issuerName;
        private volatile string  _subjectName;
        private volatile string _hash;
        private volatile int[] _certificateErrors;

        [DataMember]
        public bool Approved { get { return _approved; } set { _approved = value; } }

        [DataMember]
        public bool Requested { get { return _requested; } set { _requested = value; } }

        [DataMember]
        public string SerialNumber { get { return _serialNumber; } set { _serialNumber = value; } }

        [DataMember]
        public string IssuerName { get { return _issuerName; } set { _issuerName = value; } }

        [DataMember]
        public string SubjectName { get { return _subjectName; } set { _subjectName = value; } }

        [DataMember]
        public DateTime ValidFrom { get; set; }

        [DataMember]
        public DateTime ValidUntil { get; set; }

        [DataMember]
        public string Hash { get { return _hash; } set { _hash = value; } }

        [DataMember]
        public int[] CertificateErrors { get { return _certificateErrors; } set { _certificateErrors = value; } }

        private enum LdapCertificateProblem
        {
            CertExpired = -2146762495,
            CertCnNoMatch = -2146762481,
            // ReSharper disable once UnusedMember.Local
            CertIssuerChaining = -2146762489,
            CertUntrustedCa = -2146762478,
            // ReSharper disable once UnusedMember.Local
            CertUntrustedRoot = -2146762487,
            CertMalformed = -2146762488,
            CertUnrecognizedError = -2146762477
        }

        public static int[] GetLdapCertProblems(X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors, ILog log = null)
        {
            var certificateErrors = new List<int>();
            try
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return certificateErrors.ToArray();
                }

                var expDate = DateTime.Parse(certificate.GetExpirationDateString()).ToUniversalTime();
                var utcNow = DateTime.UtcNow;
                if (expDate < utcNow && expDate.AddDays(1) >= utcNow)
                {
                    certificateErrors.Add((int)LdapCertificateProblem.CertExpired);
                }

                if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
                {
                    certificateErrors.Add((int)LdapCertificateProblem.CertMalformed);
                }

                if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
                {
                    if (log != null)
                    {
                        log.WarnFormat("GetLdapCertProblems: {0}",
                            Enum.GetName(typeof(SslPolicyErrors), LdapCertificateProblem.CertCnNoMatch));
                    }

                    certificateErrors.Add((int) LdapCertificateProblem.CertCnNoMatch);
                }

                if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
                {
                    if (log != null)
                    {
                        log.WarnFormat("GetLdapCertProblems: {0}",
                            Enum.GetName(typeof(SslPolicyErrors), LdapCertificateProblem.CertCnNoMatch));
                    }

                    certificateErrors.Add((int) LdapCertificateProblem.CertUntrustedCa);
                }
            }
            catch (Exception ex)
            {
                if (log != null) 
                    log.ErrorFormat("GetLdapCertProblems() failed. Error: {0}", ex);
                certificateErrors.Add((int) LdapCertificateProblem.CertUnrecognizedError);
            }

            return certificateErrors.ToArray();
        }

        public static LdapCertificateConfirmRequest FromCert(X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors, bool approved = false, bool requested = false, ILog log = null)
        {
            var certificateErrors = GetLdapCertProblems(certificate, chain, sslPolicyErrors, log);

            try
            {
                string serialNumber = "", issuerName = "", subjectName = "", hash = "";
                DateTime validFrom = DateTime.UtcNow, validUntil = DateTime.UtcNow;

                LdapUtils.SkipErrors(() => serialNumber = certificate.GetSerialNumberString(), log);
                LdapUtils.SkipErrors(() => issuerName = certificate.Issuer, log);
                LdapUtils.SkipErrors(() => subjectName = certificate.Subject, log);
                LdapUtils.SkipErrors(() => validFrom = DateTime.Parse(certificate.GetEffectiveDateString()), log);
                LdapUtils.SkipErrors(() => validUntil = DateTime.Parse(certificate.GetExpirationDateString()), log);
                LdapUtils.SkipErrors(() => hash = certificate.GetCertHashString(), log);

                var certificateConfirmRequest = new LdapCertificateConfirmRequest
                {
                    SerialNumber = serialNumber,
                    IssuerName = issuerName,
                    SubjectName = subjectName,
                    ValidFrom = validFrom,
                    ValidUntil = validUntil,
                    Hash = hash,
                    CertificateErrors = certificateErrors,
                    Approved = approved,
                    Requested = requested
                };

                return certificateConfirmRequest;
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.ErrorFormat("LdapCertificateConfirmRequest.FromCert() failed. Error: {0}", ex);
                return null;
            }
        }
    }
}
