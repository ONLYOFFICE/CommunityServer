using ASC.SingleSignOn.Common;
using ASC.SingleSignOn.Saml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.SingleSignOn.Tests.SAML
{
    [TestClass]
    public class ResponseTests
    {
        [TestMethod]
        public void CheckUserOneLoginInvalidCrtResponse()
        {
            var ssoSettings = new SsoSettings
            {
                EnableSso = true,
                Issuer = @"https://app.onelogin.com/saml/metadata/372455",
                SsoEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-post/sso/37245",
                SloEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-redirect/slo/372455",
                TokenType = "SAML",
                ValidationType = "X.509",
                PublicKey = @"-----BEGIN CERTIFICATE-----
MIIEXzCCA0egAwIBAgIUUVnh6ZmH1MMsmSNtRTI1gYZy+6gwDQYJKoZIhvcNAQEF
BQAwcDELMAkGA1UEBhMCVVMxKTAnBgNVBAoMIExlaWJuaXogVW5pdmVyc2l0w6R0
IElUIFNlcnZpY2VzMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9u
ZUxvZ2luIEFjY291bnQgOTc2ODEwHhcNMTYxMjExMTAwMjExWhcNMjExMjEyMTAw
MjExWjBwMQswCQYDVQQGEwJVUzEpMCcGA1UECgwgTGVpYm5peiBVbml2ZXJzaXTD
pHQgSVQgU2VydmljZXMxFTATBgNVBAsMDE9uZUxvZ2luIElkUDEfMB0GA1UEAwwW
T25lTG9naW4gQWNjb3VudCA5NzY4MTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBAJI/b23mx7YUI4N2UkhOE2Moy+KFsuKRtqzZYXmEvYg9IFulXOL6CN/C
cR0LhCOdb1XGUxd5ZCErE3VslPpsASj/H/ZWyJEeS6nZtU3CdHJ3zCJb06HFYROm
+FxdxlgCT4R2gBEhuOv6pK9bgaFYO7YEcuRpKYI+/BU4fHFJSU2DyBvoKSTCoHFt
mKM11++oWg8onbYfY5Wt+F8gL6hmvhq6eAInRAWXJ/Mkt48spEesXNGHwvIZPSSi
52qTCTx+nYxw6IDzLtA4Jg8oN4aCC79ULXR+Fyrhz20ShItTixWi0M5QUyPqkxO2
9O/M3VvLTxd8pKsJoziaCLipnpuPtDcCAwEAAaOB8DCB7TAMBgNVHRMBAf8EAjAA
MB0GA1UdDgQWBBTkjOARt4qAEkZaIVGdwiTvAaKRizCBrQYDVR0jBIGlMIGigBTk
jOARt4qAEkZaIVGdwiTvAaKRi6F0pHIwcDELMAkGA1UEBhMCVVMxKTAnBgNVBAoM
IExlaWJuaXogVW5pdmVyc2l0w6R0IElUIFNlcnZpY2VzMRUwEwYDVQQLDAxPbmVM
b2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgOTc2ODGCFFFZ4emZ
h9TDLJkjbUUyNYGGcvuoMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOC
AQEAYPV9R48h8MuifkaMXX9hUJ3FktVH0lA/rFpxXxKQbrE778Ye8Hc1MhvfaDiV
hE9A4EpVO+LJBlIgNzGyt8s0dUsb7kxLlxmkksyAxz1yQzKwq+XF0YjOkGXVSzJ9
oi0dqz+ROiXfMr30yh9f8tCErIWMP5uMZlIQMzKmG+laoTssqye584IgV/5LbHJn
/Z920rgY6DXDV8EteF0Sl8smOCP6Zwop5cUYnsT4NN/MCVAtRuF0AlQuQMy58Hyg
Zh8ICXV37JJOkTAKLhVZrYWcJBF/bFIoW4lJvhkOstcFh3qNDgySHfyghgP5/mPf
j9HtX6VzPCmLo8FKUnQ0lwacJg==
-----END CERTIFICATE-----",
                ClientCertificateFileName = "",
                ClientPassword = ""
            };

            var resp = @"<samlp:Response xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion"" xmlns:samlp=""urn:oasis:names:tc:SAML:2.0:protocol"" ID=""pfxd77b98e8-a049-b381-eacc-4fb547ea8fc7"" Version=""2.0"" IssueInstant=""2016-12-12T10:15:06Z"" Destination=""{recipient}"" InResponseTo=""_596da3d5-e8c6-449a-bd4d-84ccee212a2f"">
	                    <saml:Issuer>https://app.onelogin.com/saml/metadata/610001</saml:Issuer>
	                    <ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
		                    <ds:SignedInfo>
			                    <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
			                    <ds:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
			                    <ds:Reference URI=""#pfxd77b98e8-a049-b381-eacc-4fb547ea8fc7"">
				                    <ds:Transforms>
					                    <ds:Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
					                    <ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
				                    </ds:Transforms>
				                    <ds:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
				                    <ds:DigestValue>+GThzgaybYo32syv2nLwP30n6iw=</ds:DigestValue>
			                    </ds:Reference>
		                    </ds:SignedInfo>
		                    <ds:SignatureValue>PoMWjdalW3xcSM0luajRCt5YfZcyxz4XWgJiYT3UANSWSel6VzjlU0SzzcqW8IGtszcEqM3rvHX13mGFvJR0KK8+GNzCl2Ornnt2BdwZx4EJ2cxB1A0QnA1690DWUfdI+dz77/RY7HnQn/a53MqlvN0WQwIhAoogWR9AnMrmkKW+lfy5Cdbv7Q0n6JKJ+cPcL2ZHXHSoWh/SXjZkRXtE2Z5rLE5p9mcJExWOgd8biwsDJH1hE4r3X5OqW8MBK2T2VMtz+wC4RRkh/zBhZ8jLXyRH4DYo1HtR/jM42g29XGGDIZVTcfT/DlXCbnPh2xpMC/ZpNapoU9CSKREkwTq72A==</ds:SignatureValue>
		                    <ds:KeyInfo>
			                    <ds:X509Data>
				                    <ds:X509Certificate>MIIEXzCCA0egAwIBAgIUUVnh6ZmH1MMsmSNtRTI1gYZy+6gwDQYJKoZIhvcNAQEFBQAwcDELMAkGA1UEBhMCVVMxKTAnBgNVBAoMIExlaWJuaXogVW5pdmVyc2l0w6R0IElUIFNlcnZpY2VzMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgOTc2ODEwHhcNMTYxMjExMTAwMjExWhcNMjExMjEyMTAwMjExWjBwMQswCQYDVQQGEwJVUzEpMCcGA1UECgwgTGVpYm5peiBVbml2ZXJzaXTDpHQgSVQgU2VydmljZXMxFTATBgNVBAsMDE9uZUxvZ2luIElkUDEfMB0GA1UEAwwWT25lTG9naW4gQWNjb3VudCA5NzY4MTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJI/b23mx7YUI4N2UkhOE2Moy+KFsuKRtqzZYXmEvYg9IFulXOL6CN/CcR0LhCOdb1XGUxd5ZCErE3VslPpsASj/H/ZWyJEeS6nZtU3CdHJ3zCJb06HFYROm+FxdxlgCT4R2gBEhuOv6pK9bgaFYO7YEcuRpKYI+/BU4fHFJSU2DyBvoKSTCoHFtmKM11++oWg8onbYfY5Wt+F8gL6hmvhq6eAInRAWXJ/Mkt48spEesXNGHwvIZPSSi52qTCTx+nYxw6IDzLtA4Jg8oN4aCC79ULXR+Fyrhz20ShItTixWi0M5QUyPqkxO29O/M3VvLTxd8pKsJoziaCLipnpuPtDcCAwEAAaOB8DCB7TAMBgNVHRMBAf8EAjAAMB0GA1UdDgQWBBTkjOARt4qAEkZaIVGdwiTvAaKRizCBrQYDVR0jBIGlMIGigBTkjOARt4qAEkZaIVGdwiTvAaKRi6F0pHIwcDELMAkGA1UEBhMCVVMxKTAnBgNVBAoMIExlaWJuaXogVW5pdmVyc2l0w6R0IElUIFNlcnZpY2VzMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgOTc2ODGCFFFZ4emZh9TDLJkjbUUyNYGGcvuoMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAYPV9R48h8MuifkaMXX9hUJ3FktVH0lA/rFpxXxKQbrE778Ye8Hc1MhvfaDiVhE9A4EpVO+LJBlIgNzGyt8s0dUsb7kxLlxmkksyAxz1yQzKwq+XF0YjOkGXVSzJ9oi0dqz+ROiXfMr30yh9f8tCErIWMP5uMZlIQMzKmG+laoTssqye584IgV/5LbHJn/Z920rgY6DXDV8EteF0Sl8smOCP6Zwop5cUYnsT4NN/MCVAtRuF0AlQuQMy58HygZh8ICXV37JJOkTAKLhVZrYWcJBF/bFIoW4lJvhkOstcFh3qNDgySHfyghgP5/mPfj9HtX6VzPCmLo8FKUnQ0lwacJg==</ds:X509Certificate>
			                    </ds:X509Data>
		                    </ds:KeyInfo>
	                    </ds:Signature>
	                    <samlp:Status>
		                    <samlp:StatusCode Value=""urn:oasis:names:tc:SAML:2.0:status:Success"" />
	                    </samlp:Status>
	                    <saml:EncryptedAssertion>
		                    <xenc:EncryptedData xmlns:xenc=""http://www.w3.org/2001/04/xmlenc#"" Type=""http://www.w3.org/2001/04/xmlenc#Element"">
			                    <xenc:EncryptionMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#aes256-cbc"" />
			                    <ds:KeyInfo xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
				                    <xenc:EncryptedKey>
					                    <xenc:EncryptionMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p"" />
					                    <xenc:CipherData>
						                    <xenc:CipherValue>RQ/o9zxwBpyC6FLg1zSSGFLRsUw2QRWW6C96JLkpOxc5CRvQ6MMkfUT2hLd+vaCu
                    GX2Xaa7EMYykCAp8FVq+veUw5BV2alebu87PF/PO0bNrCDJDbKexTeu0WPqHWoUn
                    SWPH+rgVtodkl8ctRn2u7BWDbWv3udE433KtAjvVIFWQI8HB0ENlQhxXMX8wSkhp
                    Wig53tgvXccvokm+W4tv4nJsNWTrY5ie3YBSwByeuj/JOhdBfJfaM0+aM8h3napl
                    WCuoeD2NQUzq+deDAa4li6d/azDS28040lpd5AS2Xn79ZB4hmwSDa+7u8Zfms1Op
                    W+dYFuIastT2EqLFzDuvuA==</xenc:CipherValue>
					                    </xenc:CipherData>
				                    </xenc:EncryptedKey>
			                    </ds:KeyInfo>
			                    <xenc:CipherData>
				                    <xenc:CipherValue>78OakTKYv9+Iqxffdskv2fQCKeUfYuw2EARqXEvLluljK9ysD3xLcx76Hpmp64di
                    K19q10mYXFWz/I6V+m53Yef5f++IueGfUPJbc8GLt2IzmSiOtlUzmx/A6/GYtwC1
                    RTVlYbIJve8WORsh27cEyglbdJU6hNBbqNKF504QOv9p5R8vFfdvpzzCbUQnE5C3
                    WTOBOb1JapEMM+tqGDl6N44uvIg5kzA+OQakFpmDOSTZSoYNPpdvKl/Qdm5pOc8o
                    Cwtok4lqgz+xAacCudAeTi/4r3stiUvIwTt05Y0ckD4HKRlKDjnLgyUWIPXzen/+
                    RZekUg+Cy9RyeWjmjL1CN7nlNjd948vRD/HnSG3JkDefJh3h06uYRkY2JhKGrRTF
                    /oK5fprcX1evh5qH4BMR/Iccdx1giBbNihs+8Ul2QVXFbBrpgxSS3KRyVk34i6wF
                    FoQpp8k6v4mmNFgB2hAxFaRPo/unx+8KBJkqUQRbKzTrnpnsg5OIdu1cyPSg3qyZ
                    uhfPK4XCdlleN7NXFPMRMIqBud2+QzXtoAt4tr8ylqDq1aQvkDHJ2Vqc4jrYqil7
                    iVBFMiviklIApDrWjGaDoLCrcGknDQzUWNGpne7COapfVuVwHoFXyJFS4zPCBDY9
                    HJTTl8SO0F4YTsBN7kCdpYJ8g+3Cwll5JKkNu3gNcWjcCb5VDu2OyQiiSmoI0vFs
                    RamlnFEV07AMzIcZyLD8ZmrZdhv1+Y/O9wlOMq9bswvr5alDI/DmSHP5bTW2KDeE
                    CK/3lX/ytiiTYr95hqr/NRMfxrjKS9sp4r7CcBd7Jlv3Y+3k+bBaZcT6zpfaqkrv
                    AzR8oNJoo5uKV1esLfWzQ7lqFdAHmLCjZ4zZY80CRcm2hTw/4NBpedu+l9Cu+ex0
                    xHJMOmNcvWGsT1RmYEHg/Dr50fN2Nl3hEvSVm2DgJiuMyVDbAIqnxDd0YKXFUEg9
                    VZR7k8aLtTGDT2J9Y/mJbfLEgMyldC6GkkJD7PRVboywZiiXqueLf6ey0Z5DLBJe
                    qDD/MqJqbKq3zHSv+B2OP9YMxoOm/AeUjMjLEMAJnlHFrCqFBvlb5cVfBp3xwAhh
                    6pHQ8+7Q0gElJI23zs3HbGj8gQ45yT3D3paxZxwXas1PgHaC3QTwpWBQBfHdPH+m
                    vYFP1PElByr9cZ3bRM2bYaQbo6FK7Lb/IWhYBYDzZP9vXk+zVS9KnkvMJEfdR1oC
                    2jWNVGePR3GW//Pg13I9Oeallv3e+HAv0lFCXkvfZ9Q8F5+COlbfQz4nLOurB8jV
                    muqUFp+/YSsjdOBFzpttulnGtkt0eQm9kVbeM+ME6NqkgGoSwUAjR4JALura0YY0
                    8v5BUhciwX57ca6eZoBGCBmRN4FJlfV20aXSiKXHKpnzpb1x3P/ZIqwiBDdhMVS+
                    jlMBgN9yINxON2IKytll2GbSWLxdkLLtnlgOxpPgwV8t5aDzbEBV6NeSQKXQWMDn
                    r47godM2rKl8/n6DsqDu/0IIb4k+LPVKxm1TWLBw507+lzK8g5lCILSGHU7GPDGu
                    VQAH1tTslVU8OLrAiReZZFbQpuk3DzEoAe0U9Rfm5GhBqWL4BC4VXlR9M9P6mk83
                    8xvqsJmJ9ElhwrhBvqsIZIELvZDftuMLqvhc9WHzHsL9Xni3Tlz3b3ntsZfIOOEs
                    6jItUtEQEtWZuJkZdHgNhSgAy0i5v1LDVgLP69MLaXv77INFrh4c1SLJhyEwuPiG
                    /N0K+O2V4V4uqN31lUG8CA==</xenc:CipherValue>
			                    </xenc:CipherData>
		                    </xenc:EncryptedData>
	                    </saml:EncryptedAssertion>
                    </samlp:Response>";

            var t = new SamlResponse(ssoSettings);

            t.LoadXml(resp);

            Assert.IsFalse(t.IsValid());
        }

        [TestMethod]
        public void CheckUserShibbolethInvalidCrtResponse()
        {
            var ssoSettings = new SsoSettings
            {
                EnableSso = true,
                Issuer = @"https://app.onelogin.com/saml/metadata/372455",
                SsoEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-post/sso/37245",
                SloEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-redirect/slo/372455",
                TokenType = "SAML",
                ValidationType = "X.509",
                PublicKey = @"-----BEGIN CERTIFICATE-----
MIIEXzCCA0egAwIBAgIUUVnh6ZmH1MMsmSNtRTI1gYZy+6gwDQYJKoZIhvcNAQEF
BQAwcDELMAkGA1UEBhMCVVMxKTAnBgNVBAoMIExlaWJuaXogVW5pdmVyc2l0w6R0
IElUIFNlcnZpY2VzMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9u
ZUxvZ2luIEFjY291bnQgOTc2ODEwHhcNMTYxMjExMTAwMjExWhcNMjExMjEyMTAw
MjExWjBwMQswCQYDVQQGEwJVUzEpMCcGA1UECgwgTGVpYm5peiBVbml2ZXJzaXTD
pHQgSVQgU2VydmljZXMxFTATBgNVBAsMDE9uZUxvZ2luIElkUDEfMB0GA1UEAwwW
T25lTG9naW4gQWNjb3VudCA5NzY4MTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBAJI/b23mx7YUI4N2UkhOE2Moy+KFsuKRtqzZYXmEvYg9IFulXOL6CN/C
cR0LhCOdb1XGUxd5ZCErE3VslPpsASj/H/ZWyJEeS6nZtU3CdHJ3zCJb06HFYROm
+FxdxlgCT4R2gBEhuOv6pK9bgaFYO7YEcuRpKYI+/BU4fHFJSU2DyBvoKSTCoHFt
mKM11++oWg8onbYfY5Wt+F8gL6hmvhq6eAInRAWXJ/Mkt48spEesXNGHwvIZPSSi
52qTCTx+nYxw6IDzLtA4Jg8oN4aCC79ULXR+Fyrhz20ShItTixWi0M5QUyPqkxO2
9O/M3VvLTxd8pKsJoziaCLipnpuPtDcCAwEAAaOB8DCB7TAMBgNVHRMBAf8EAjAA
MB0GA1UdDgQWBBTkjOARt4qAEkZaIVGdwiTvAaKRizCBrQYDVR0jBIGlMIGigBTk
jOARt4qAEkZaIVGdwiTvAaKRi6F0pHIwcDELMAkGA1UEBhMCVVMxKTAnBgNVBAoM
IExlaWJuaXogVW5pdmVyc2l0w6R0IElUIFNlcnZpY2VzMRUwEwYDVQQLDAxPbmVM
b2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgOTc2ODGCFFFZ4emZ
h9TDLJkjbUUyNYGGcvuoMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOC
AQEAYPV9R48h8MuifkaMXX9hUJ3FktVH0lA/rFpxXxKQbrE778Ye8Hc1MhvfaDiV
hE9A4EpVO+LJBlIgNzGyt8s0dUsb7kxLlxmkksyAxz1yQzKwq+XF0YjOkGXVSzJ9
oi0dqz+ROiXfMr30yh9f8tCErIWMP5uMZlIQMzKmG+laoTssqye584IgV/5LbHJn
/Z920rgY6DXDV8EteF0Sl8smOCP6Zwop5cUYnsT4NN/MCVAtRuF0AlQuQMy58Hyg
Zh8ICXV37JJOkTAKLhVZrYWcJBF/bFIoW4lJvhkOstcFh3qNDgySHfyghgP5/mPf
j9HtX6VzPCmLo8FKUnQ0lwacJg==
-----END CERTIFICATE-----",
                ClientCertificateFileName = "sp.pfx",
                ClientPassword = "Password"
            };

            var resp = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                        <saml2p:Response xmlns:saml2p=""urn:oasis:names:tc:SAML:2.0:protocol"" Destination=""https://office.cloud.uni-hannover.de/samllogin.ashx"" ID=""_119c4cd3c4b23255776edbe143a38357"" InResponseTo=""_485309fa-b7b1-4d72-bceb-18bc3433a729"" IssueInstant=""2016-12-12T08:38:38.995Z"" Version=""2.0"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	                        <saml2:Issuer xmlns:saml2=""urn:oasis:names:tc:SAML:2.0:assertion"" Format=""urn:oasis:names:tc:SAML:2.0:nameid-format:entity"">https://sso.idm.uni-hannover.de/idp/shibboleth</saml2:Issuer>
	                        <ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
		                        <ds:SignedInfo>
			                        <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
			                        <ds:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
			                        <ds:Reference URI=""#_119c4cd3c4b23255776edbe143a38357"">
				                        <ds:Transforms>
					                        <ds:Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
					                        <ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"">
						                        <ec:InclusiveNamespaces xmlns:ec=""http://www.w3.org/2001/10/xml-exc-c14n#"" PrefixList=""xs"" />
					                        </ds:Transform>
				                        </ds:Transforms>
				                        <ds:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
				                        <ds:DigestValue>TBMuHbLmhq6dtF0THsLxSCx6UKg=</ds:DigestValue>
			                        </ds:Reference>
		                        </ds:SignedInfo>
		                        <ds:SignatureValue>IwDNps0tbGzGSa7aRetDNujpU2TrdLDCkFTYp1JNHMJ1hy1MZp/tCSnTjrNjob5wISf825cEjptQkKlWUu8K/FYjRzs1T16n/ilS4mm4Jrr1HMFZhg2uMYyS0Zvpy3c3fbesfS0XomCj3v/ObjEg3nga0b8I+bxGufINx/CZ9LwKEZGfPKdqQfcYaUl0Ke68MGfXUSXzS/LHwJuAEjIfWe6+kN2TGJQEwsnO1J/a4izsj778udaEk8Z5wom2/l27yVuuj36CMTUkRqsSKk9EhEPhnGytdQI+odJArEZiRRA25uMXbO3MdDM7KBh+WZM/zJTb/c8Y9/Rh7zmdStQRqg==</ds:SignatureValue>
		                        <ds:KeyInfo>
			                        <ds:X509Data>
				                        <ds:X509Certificate>MIIFszCCBJugAwIBAgIHGXFE2RydJTANBgkqhkiG9w0BAQsFADBnMQswCQYDVQQGEwJERTEmMCQG
                        A1UEChMdTGVpYm5peiBVbml2ZXJzaXRhZXQgSGFubm92ZXIxDTALBgNVBAsTBFJSWk4xITAfBgNV
                        BAMTGENBIGRlciBMVUggKFVILUNBKSAtIEcwMzAeFw0xNTA1MTIwOTI2MzRaFw0xODA4MDgwOTI2
                        MzRaMIGRMQswCQYDVQQGEwJERTEWMBQGA1UECAwNTmllZGVyc2FjaHNlbjERMA8GA1UEBwwISGFu
                        bm92ZXIxJjAkBgNVBAoMHUxlaWJuaXogVW5pdmVyc2l0YWV0IEhhbm5vdmVyMQ0wCwYDVQQLDARM
                        VUlTMSAwHgYDVQQDDBdzc28uaWRtLnVuaS1oYW5ub3Zlci5kZTCCASIwDQYJKoZIhvcNAQEBBQAD
                        ggEPADCCAQoCggEBAL9C8SoIdmtXpVqPaZ00VN+5IAu1pwfBtvgsKQHBQ89PfGS41zDcaB1hiHTB
                        8JuGkRwsS7wwmO0FZZp6u1/lIADYabMp53LKd2zC04T1PPRevQPJ4OyTpyrosRGk7rv0Gj62ePFT
                        JKFx7dHDjKN1ms0elawwNId63IX+SKvvLqtPGilLU3+FEM9EJxdNz2NAEfywXpX7v+J6Y65iDdri
                        XODS7dWc2b2XOLBzRgG8AXgB6Otgf63NBKXMjB5mmrryWuz8sB1jwQbedGHw83aaOYYQ4ZtXuu85
                        VSNiO/SH57ZBOExHa7gNehc/svKI2+ArMZgUX/fJoZvy3oCAlSysXmsCAwEAAaOCAjcwggIzME8G
                        A1UdIARIMEYwEQYPKwYBBAGBrSGCLAEBBAMDMBEGDysGAQQBga0hgiwCAQQDATAPBg0rBgEEAYGt
                        IYIsAQEEMA0GCysGAQQBga0hgiweMAkGA1UdEwQCMAAwCwYDVR0PBAQDAgXgMB0GA1UdJQQWMBQG
                        CCsGAQUFBwMCBggrBgEFBQcDATAdBgNVHQ4EFgQUkqBGogzHkzzGhDv1l4PEGYMVb/wwHwYDVR0j
                        BBgwFoAU0/vgTeRNiIX0UalveXXpwCkhLhMwIgYDVR0RBBswGYIXc3NvLmlkbS51bmktaGFubm92
                        ZXIuZGUweQYDVR0fBHIwcDA2oDSgMoYwaHR0cDovL2NkcDEucGNhLmRmbi5kZS91aC1jYS9wdWIv
                        Y3JsL2dfY2FjcmwuY3JsMDagNKAyhjBodHRwOi8vY2RwMi5wY2EuZGZuLmRlL3VoLWNhL3B1Yi9j
                        cmwvZ19jYWNybC5jcmwwgckGCCsGAQUFBwEBBIG8MIG5MDMGCCsGAQUFBzABhidodHRwOi8vb2Nz
                        cC5wY2EuZGZuLmRlL09DU1AtU2VydmVyL09DU1AwQAYIKwYBBQUHMAKGNGh0dHA6Ly9jZHAxLnBj
                        YS5kZm4uZGUvdWgtY2EvcHViL2NhY2VydC9nX2NhY2VydC5jcnQwQAYIKwYBBQUHMAKGNGh0dHA6
                        Ly9jZHAyLnBjYS5kZm4uZGUvdWgtY2EvcHViL2NhY2VydC9nX2NhY2VydC5jcnQwDQYJKoZIhvcN
                        AQELBQADggEBAJ05NSoXAkCSq9SoKOGnthKCtO3bRLm6Psp/db4pV/Aids6Rz8Pt/c6SOPhlsnFl
                        pEl1V8aVnVCsKy8xNQrJDeRi+l1c0wqRfg9sWlhzJ7oy8PhnFgBAXmvqolEcd88Om+3SQt5W6KJT
                        GRPNO0vmN1V7BDlEmQw3GTXeiaLQz2y52nvzgKCJr4GJdJEbLihoRVKi48YWWMM7w+Uu6Pa5Iln3
                        7CUQa0lPSOCwbJVcNCb/6GK95KTHUIlmRn5xVkGx6QdQhYzK64WT6MCg38ngdPModLUjUazdfRHh
                        raWHSbOjCuvbllvvLleZIJU1fC4KKXY54mjD3AkQa56u58KyT9g=</ds:X509Certificate>
			                        </ds:X509Data>
		                        </ds:KeyInfo>
	                        </ds:Signature>
	                        <saml2p:Status>
		                        <saml2p:StatusCode Value=""urn:oasis:names:tc:SAML:2.0:status:Success"" />
	                        </saml2p:Status>
	                        <saml2:Assertion xmlns:saml2=""urn:oasis:names:tc:SAML:2.0:assertion"" ID=""_1e4fab4836c03c1ef9f8f76e6a0a8ab1"" IssueInstant=""2016-12-12T08:38:38.995Z"" Version=""2.0"">
		                        <saml2:Issuer Format=""urn:oasis:names:tc:SAML:2.0:nameid-format:entity"">https://sso.idm.uni-hannover.de/idp/shibboleth</saml2:Issuer>
		                        <saml2:Subject>
			                        <saml2:NameID Format=""urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"" NameQualifier=""https://sso.idm.uni-hannover.de/idp/shibboleth"">casselt@luis.uni-hannover.de</saml2:NameID>
			                        <saml2:SubjectConfirmation Method=""urn:oasis:names:tc:SAML:2.0:cm:bearer"">
				                        <saml2:SubjectConfirmationData Address=""130.75.5.118"" InResponseTo=""_485309fa-b7b1-4d72-bceb-18bc3433a729"" NotOnOrAfter=""2016-12-12T08:43:38.995Z"" Recipient=""https://office.cloud.uni-hannover.de/samllogin.ashx"" />
			                        </saml2:SubjectConfirmation>
		                        </saml2:Subject>
		                        <saml2:Conditions NotBefore=""2016-12-12T08:38:38.995Z"" NotOnOrAfter=""2016-12-12T08:43:38.995Z"">
			                        <saml2:AudienceRestriction>
				                        <saml2:Audience>https://office.cloud.uni-hannover.de/samllogin.ashx</saml2:Audience>
			                        </saml2:AudienceRestriction>
		                        </saml2:Conditions>
		                        <saml2:AuthnStatement AuthnInstant=""2016-12-12T08:38:20.083Z"" SessionIndex=""_9552061f6a8e69f464dd07621c18a527"">
			                        <saml2:SubjectLocality Address=""130.75.5.118"" />
			                        <saml2:AuthnContext>
				                        <saml2:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml2:AuthnContextClassRef>
			                        </saml2:AuthnContext>
		                        </saml2:AuthnStatement>
		                        <saml2:AttributeStatement>
			                        <saml2:Attribute FriendlyName=""eduPersonAffiliation"" Name=""urn:oid:1.3.6.1.4.1.5923.1.1.1.1"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">member</saml2:AttributeValue>
			                        </saml2:Attribute>
			                        <saml2:Attribute FriendlyName=""o"" Name=""urn:oid:2.5.4.10"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">Leibniz-Universit??t Hannover</saml2:AttributeValue>
			                        </saml2:Attribute>
			                        <saml2:Attribute FriendlyName=""eduPersonScopedAffiliation"" Name=""urn:oid:1.3.6.1.4.1.5923.1.1.1.9"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">member@uni-hannover.de</saml2:AttributeValue>
			                        </saml2:Attribute>
			                        <saml2:Attribute FriendlyName=""sn"" Name=""urn:oid:2.5.4.4"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">Casselt</saml2:AttributeValue>
			                        </saml2:Attribute>
			                        <saml2:Attribute FriendlyName=""givenName"" Name=""urn:oid:2.5.4.42"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">Torsten</saml2:AttributeValue>
			                        </saml2:Attribute>
			                        <saml2:Attribute FriendlyName=""schacHomeOrganizationType"" Name=""urn:oid:1.3.6.1.4.1.25178.1.2.10"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">urn:mace:terena.org:schac:homeOrganizationType:eu:higherEducationInstitution</saml2:AttributeValue>
			                        </saml2:Attribute>
			                        <saml2:Attribute FriendlyName=""schacHomeOrganization"" Name=""urn:oid:1.3.6.1.4.1.25178.1.2.9"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">uni-hannover.de</saml2:AttributeValue>
			                        </saml2:Attribute>
			                        <saml2:Attribute FriendlyName=""eduPersonEntitlement"" Name=""urn:oid:1.3.6.1.4.1.5923.1.1.1.7"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:uri"">
				                        <saml2:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">urn:mace:dir:entitlement:common-lib-terms</saml2:AttributeValue>
			                        </saml2:Attribute>
		                        </saml2:AttributeStatement>
	                        </saml2:Assertion>
                        </saml2p:Response>";

            var t = new SamlResponse(ssoSettings);

            t.LoadXml(resp);

            Assert.IsFalse(t.IsValid());
        }

        [TestMethod]
        public void CreateUserOldFormatResponse()
        {
            var ssoSettings = new SsoSettings
            {
                EnableSso = true,
                Issuer = @"https://app.onelogin.com/saml/metadata/372455",
                SsoEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-post/sso/37245",
                SloEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-redirect/slo/372455",
                TokenType = "SAML",
                ValidationType = "X.509",
                PublicKey = @"-----BEGIN CERTIFICATE-----
MIIEFDCCAvygAwIBAgIUbzED7v7Yw2hvihutV8GRCqM6Q20wDQYJKoZIhvcNAQEF
BQAwVzELMAkGA1UEBhMCVVMxEDAOBgNVBAoMB1RlYW1sYWIxFTATBgNVBAsMDE9u
ZUxvZ2luIElkUDEfMB0GA1UEAwwWT25lTG9naW4gQWNjb3VudCA0MTY2MTAeFw0x
NDA1MDUwOTQ2MzRaFw0xOTA1MDYwOTQ2MzRaMFcxCzAJBgNVBAYTAlVTMRAwDgYD
VQQKDAdUZWFtbGFiMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9u
ZUxvZ2luIEFjY291bnQgNDE2NjEwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQC/YH4pTv7a6ouTTYvqwWkjoEpYZG537Q7p4SlNStjH8V9xcP6id6zQzXQg
SB5veAd0kYcYbI8qkjDOPfQ1wQ7/DmHV3gKWoEFXqubRLND0eg+OV6EwpQjlsz9k
RrjCPUsAUucG8OlR6KpksxNaFzBLRmjIaFr4Tx3ZoRdFvtyJ03x3LSKz/gAy/C5O
Pfwx4qrwi2Qk8Z8J7hVsw2sIHGRuTTX36SCmDRORcGMToKXuBUmK0edRVD5TWkOA
iH5OXbF99evmTMsH8Pb+a5tSoByuLw5+2exF0HPjthPUSe1ezO4b8ieq7JG+uuR0
Q4dy+XRMKUmY2miRBqY11yQGswvHAgMBAAGjgdcwgdQwDAYDVR0TAQH/BAIwADAd
BgNVHQ4EFgQUvFref3WOeg/WxNs58la12Ycv+HgwgZQGA1UdIwSBjDCBiYAUvFre
f3WOeg/WxNs58la12Ycv+HihW6RZMFcxCzAJBgNVBAYTAlVTMRAwDgYDVQQKDAdU
ZWFtbGFiMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2lu
IEFjY291bnQgNDE2NjGCFG8xA+7+2MNob4obrVfBkQqjOkNtMA4GA1UdDwEB/wQE
AwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAH1xmYf/4BAmNF0SbNXSHUTUJjkcF1QRT
wpBL/Zrql7rtg9s/gf/cYHXsg6I3f9DGfGHXsyEDtWhfgA8KNjVKOKo71eCBiVaD
Mu7oWT8pEV+jOktM+twxBSfk15BZVmSBM/hexowpspQQLQIIqV3agDOIUL73KIeB
sLi5y4c+3u+lFZDDDSNLkVguGEa0ZA3n20uu5ZQsnym1PdD/2c137/Kw8tbrgBJP
9Ln5ua23qPQwuFmhFNXE9onzHCe+ygILDzVSRrgZH8SO4Z9wzEzC+L+Dj+niqHtu
K0tpi9M2+JxhPFtl91UksQ3Mmr0dmEOzWx37ONc23s+FJOe0bpqssQ==
-----END CERTIFICATE-----",
                ClientCertificateFileName = "sp.pfx",
                ClientPassword = "Password"
            };

            var resp = @"<samlp:Response xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion"" xmlns:samlp=""urn:oasis:names:tc:SAML:2.0:protocol"" ID=""pfx2b7403a6-ac72-a715-29c7-2caeeaa6b268"" Version=""2.0"" IssueInstant=""2016-12-15T07:44:17Z"" Destination=""http://localhost/samllogin.ashx"">
	                    <saml:Issuer>https://app.onelogin.com/saml/metadata/372455</saml:Issuer>
	                    <ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
		                    <ds:SignedInfo>
			                    <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
			                    <ds:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
			                    <ds:Reference URI=""#pfx2b7403a6-ac72-a715-29c7-2caeeaa6b268"">
				                    <ds:Transforms>
					                    <ds:Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
					                    <ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
				                    </ds:Transforms>
				                    <ds:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
				                    <ds:DigestValue>X5QQ8BTLsqUa0srVKiu1gvh9WWQ=</ds:DigestValue>
			                    </ds:Reference>
		                    </ds:SignedInfo>
		                    <ds:SignatureValue>uoT5y48lHhl95BbSzxO3ItvMUgs0IOto5JiqnxAxujy5DxKHnAa4AqcZ1ZrPT1ISiiNEqVyGUbdsZgNyhsi8mtV/NI12vgrha1VxFgIUJzb8zHUZkrDLbrZkSxMF3gN1hr2wZP+dQKeA5QNTITDJo3CUVezqF7nN73GQkfWZnG+Fri1jSXiBEqwvdHJq1W+Svkwj+nqIbl9uBz9Q6jsxa3jNdM5mCFDpkkz3U0+YoWtpiDLjOcH3xgDWfmHja1hKsruE9b3tVjrOfpyVTOHHMyrLF1wD8Ckd09+7JA9B5RLW5LraT8GRYJUSs1/yoiCke2k0QY2a4Q8AZroGfefhbQ==</ds:SignatureValue>
		                    <ds:KeyInfo>
			                    <ds:X509Data>
				                    <ds:X509Certificate>MIIEFDCCAvygAwIBAgIUbzED7v7Yw2hvihutV8GRCqM6Q20wDQYJKoZIhvcNAQEFBQAwVzELMAkGA1UEBhMCVVMxEDAOBgNVBAoMB1RlYW1sYWIxFTATBgNVBAsMDE9uZUxvZ2luIElkUDEfMB0GA1UEAwwWT25lTG9naW4gQWNjb3VudCA0MTY2MTAeFw0xNDA1MDUwOTQ2MzRaFw0xOTA1MDYwOTQ2MzRaMFcxCzAJBgNVBAYTAlVTMRAwDgYDVQQKDAdUZWFtbGFiMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgNDE2NjEwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC/YH4pTv7a6ouTTYvqwWkjoEpYZG537Q7p4SlNStjH8V9xcP6id6zQzXQgSB5veAd0kYcYbI8qkjDOPfQ1wQ7/DmHV3gKWoEFXqubRLND0eg+OV6EwpQjlsz9kRrjCPUsAUucG8OlR6KpksxNaFzBLRmjIaFr4Tx3ZoRdFvtyJ03x3LSKz/gAy/C5OPfwx4qrwi2Qk8Z8J7hVsw2sIHGRuTTX36SCmDRORcGMToKXuBUmK0edRVD5TWkOAiH5OXbF99evmTMsH8Pb+a5tSoByuLw5+2exF0HPjthPUSe1ezO4b8ieq7JG+uuR0Q4dy+XRMKUmY2miRBqY11yQGswvHAgMBAAGjgdcwgdQwDAYDVR0TAQH/BAIwADAdBgNVHQ4EFgQUvFref3WOeg/WxNs58la12Ycv+HgwgZQGA1UdIwSBjDCBiYAUvFref3WOeg/WxNs58la12Ycv+HihW6RZMFcxCzAJBgNVBAYTAlVTMRAwDgYDVQQKDAdUZWFtbGFiMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgNDE2NjGCFG8xA+7+2MNob4obrVfBkQqjOkNtMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAH1xmYf/4BAmNF0SbNXSHUTUJjkcF1QRTwpBL/Zrql7rtg9s/gf/cYHXsg6I3f9DGfGHXsyEDtWhfgA8KNjVKOKo71eCBiVaDMu7oWT8pEV+jOktM+twxBSfk15BZVmSBM/hexowpspQQLQIIqV3agDOIUL73KIeBsLi5y4c+3u+lFZDDDSNLkVguGEa0ZA3n20uu5ZQsnym1PdD/2c137/Kw8tbrgBJP9Ln5ua23qPQwuFmhFNXE9onzHCe+ygILDzVSRrgZH8SO4Z9wzEzC+L+Dj+niqHtuK0tpi9M2+JxhPFtl91UksQ3Mmr0dmEOzWx37ONc23s+FJOe0bpqssQ==</ds:X509Certificate>
			                    </ds:X509Data>
		                    </ds:KeyInfo>
	                    </ds:Signature>
	                    <samlp:Status>
		                    <samlp:StatusCode Value=""urn:oasis:names:tc:SAML:2.0:status:Success"" />
	                    </samlp:Status>
	                    <saml:Assertion xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Version=""2.0"" ID=""Ab0a04dff2d269719feeefdebfcfd763dcc089fd0"" IssueInstant=""2016-12-15T07:44:17Z"">
		                    <saml:Issuer>https://app.onelogin.com/saml/metadata/372455</saml:Issuer>
		                    <saml:Subject>
			                    <saml:NameID Format=""urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"">dev@teamlab.com</saml:NameID>
			                    <saml:SubjectConfirmation Method=""urn:oasis:names:tc:SAML:2.0:cm:bearer"">
				                    <saml:SubjectConfirmationData NotOnOrAfter=""2016-12-15T07:47:17Z"" Recipient=""http://localhost/samllogin.ashx"" />
			                    </saml:SubjectConfirmation>
		                    </saml:Subject>
		                    <saml:Conditions NotBefore=""2016-12-15T07:41:17Z"" NotOnOrAfter=""2016-12-15T07:47:17Z"">
			                    <saml:AudienceRestriction>
				                    <saml:Audience />
			                    </saml:AudienceRestriction>
		                    </saml:Conditions>
		                    <saml:AuthnStatement AuthnInstant=""2016-12-15T07:44:16Z"" SessionNotOnOrAfter=""2016-12-16T07:44:17Z"" SessionIndex=""_31ab1080-a4c8-0134-58ac-029f148bfcdd"">
			                    <saml:AuthnContext>
				                    <saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml:AuthnContextClassRef>
			                    </saml:AuthnContext>
		                    </saml:AuthnStatement>
		                    <saml:AttributeStatement>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""sex"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""title (должность)"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""birthdate"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""remote_photo_url"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""last_name"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">Bannov</saml:AttributeValue>
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""user_id"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""address"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""mobile_phone"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""email"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">dev@teamlab.com</saml:AttributeValue>
			                    </saml:Attribute>
			                    <saml:Attribute NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"" Name=""first_name"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">Alexey</saml:AttributeValue>
			                    </saml:Attribute>
		                    </saml:AttributeStatement>
	                    </saml:Assertion>
                    </samlp:Response>";

            var samlResponse = new SamlResponse(ssoSettings);

            samlResponse.LoadXml(resp);

            var email = samlResponse.GetNameID();
            var firstName = samlResponse.GetFirstName();
            var lastName = samlResponse.GetLastName();

            Assert.IsNotNull(email);
            Assert.IsNotNull(firstName);
            Assert.IsNotNull(lastName);
        }

        [TestMethod]
        public void CreateUserNewFormatResponse()
        {
            var ssoSettings = new SsoSettings
            {
                EnableSso = true,
                Issuer = @"https://app.onelogin.com/saml/metadata/372455",
                SsoEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-post/sso/37245",
                SloEndPoint = @"https://4testingteamlab.onelogin.com/trust/saml2/http-redirect/slo/372455",
                TokenType = "SAML",
                ValidationType = "X.509",
                PublicKey = @"-----BEGIN CERTIFICATE-----
MIIEFDCCAvygAwIBAgIUbzED7v7Yw2hvihutV8GRCqM6Q20wDQYJKoZIhvcNAQEF
BQAwVzELMAkGA1UEBhMCVVMxEDAOBgNVBAoMB1RlYW1sYWIxFTATBgNVBAsMDE9u
ZUxvZ2luIElkUDEfMB0GA1UEAwwWT25lTG9naW4gQWNjb3VudCA0MTY2MTAeFw0x
NDA1MDUwOTQ2MzRaFw0xOTA1MDYwOTQ2MzRaMFcxCzAJBgNVBAYTAlVTMRAwDgYD
VQQKDAdUZWFtbGFiMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9u
ZUxvZ2luIEFjY291bnQgNDE2NjEwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQC/YH4pTv7a6ouTTYvqwWkjoEpYZG537Q7p4SlNStjH8V9xcP6id6zQzXQg
SB5veAd0kYcYbI8qkjDOPfQ1wQ7/DmHV3gKWoEFXqubRLND0eg+OV6EwpQjlsz9k
RrjCPUsAUucG8OlR6KpksxNaFzBLRmjIaFr4Tx3ZoRdFvtyJ03x3LSKz/gAy/C5O
Pfwx4qrwi2Qk8Z8J7hVsw2sIHGRuTTX36SCmDRORcGMToKXuBUmK0edRVD5TWkOA
iH5OXbF99evmTMsH8Pb+a5tSoByuLw5+2exF0HPjthPUSe1ezO4b8ieq7JG+uuR0
Q4dy+XRMKUmY2miRBqY11yQGswvHAgMBAAGjgdcwgdQwDAYDVR0TAQH/BAIwADAd
BgNVHQ4EFgQUvFref3WOeg/WxNs58la12Ycv+HgwgZQGA1UdIwSBjDCBiYAUvFre
f3WOeg/WxNs58la12Ycv+HihW6RZMFcxCzAJBgNVBAYTAlVTMRAwDgYDVQQKDAdU
ZWFtbGFiMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2lu
IEFjY291bnQgNDE2NjGCFG8xA+7+2MNob4obrVfBkQqjOkNtMA4GA1UdDwEB/wQE
AwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAH1xmYf/4BAmNF0SbNXSHUTUJjkcF1QRT
wpBL/Zrql7rtg9s/gf/cYHXsg6I3f9DGfGHXsyEDtWhfgA8KNjVKOKo71eCBiVaD
Mu7oWT8pEV+jOktM+twxBSfk15BZVmSBM/hexowpspQQLQIIqV3agDOIUL73KIeB
sLi5y4c+3u+lFZDDDSNLkVguGEa0ZA3n20uu5ZQsnym1PdD/2c137/Kw8tbrgBJP
9Ln5ua23qPQwuFmhFNXE9onzHCe+ygILDzVSRrgZH8SO4Z9wzEzC+L+Dj+niqHtu
K0tpi9M2+JxhPFtl91UksQ3Mmr0dmEOzWx37ONc23s+FJOe0bpqssQ==
-----END CERTIFICATE-----",
                ClientCertificateFileName = "sp.pfx",
                ClientPassword = "Password"
            };

            var resp = @"<samlp:Response xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion"" xmlns:samlp=""urn:oasis:names:tc:SAML:2.0:protocol"" ID=""Rc9b304b2b5a664d7f0d5bf3e29468cffb4c84a99"" Version=""2.0"" IssueInstant=""2016-12-15T06:57:29Z"" Destination=""{recipient}"" InResponseTo=""_23d8fbf6-fbf6-45f0-9696-30c194678d1f"">
	                    <saml:Issuer>https://app.onelogin.com/saml/metadata/611062</saml:Issuer>
	                    <samlp:Status>
		                    <samlp:StatusCode Value=""urn:oasis:names:tc:SAML:2.0:status:Success"" />
	                    </samlp:Status>
	                    <saml:Assertion xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Version=""2.0"" ID=""pfx2d5f413b-4d6b-7cb7-0443-0555a75c349b"" IssueInstant=""2016-12-15T06:57:29Z"">
		                    <saml:Issuer>https://app.onelogin.com/saml/metadata/611062</saml:Issuer>
		                    <ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
			                    <ds:SignedInfo>
				                    <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
				                    <ds:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
				                    <ds:Reference URI=""#pfx2d5f413b-4d6b-7cb7-0443-0555a75c349b"">
					                    <ds:Transforms>
						                    <ds:Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
						                    <ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
					                    </ds:Transforms>
					                    <ds:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
					                    <ds:DigestValue>f57dlmviTMfLH60oljRM18BaOKY=</ds:DigestValue>
				                    </ds:Reference>
			                    </ds:SignedInfo>
			                    <ds:SignatureValue>c38QLRNXrfdw5gg1FV48SbJExSGsAC880XvS51Z2nazdDRUijwTRSE8EVtNWPXvErXzwGOnIDMydW5VvDZJkxQX2BBdTpu7LlE17HjCc2uZQMFJEjXJnMH61ZT5u8at8uJe+ZGgDVNH+gIQY+97mTuH5SoiPJVe9sCpJ7kkbtyTY6BK0NUyex0bPO5AGOa+dDKec0ftJENnRQZVDWdVM2gSktw0NqnjchPnPwXNWlHpUCYeTaYJGdkKptz7ahC8GS70spTMo5UZ3rl9Up7xwDHvPw/tB/l27Nx4KAu9XAJN0lDoire1kXvZHwCMedQU/rJHZ0gh1p37QqZ4q3Uun7g==</ds:SignatureValue>
			                    <ds:KeyInfo>
				                    <ds:X509Data>
					                    <ds:X509Certificate>MIIELDCCAxSgAwIBAgIUXGWXRlq9vJJeJ8ETdIQQu0mEfTQwDQYJKoZIhvcNAQEFBQAwXzELMAkGA1UEBhMCVVMxGDAWBgNVBAoMD21vbm8ubWFpbC40dGVzdDEVMBMGA1UECwwMT25lTG9naW4gSWRQMR8wHQYDVQQDDBZPbmVMb2dpbiBBY2NvdW50IDk3ODA1MB4XDTE2MTIxMzA4NTAxMloXDTIxMTIxNDA4NTAxMlowXzELMAkGA1UEBhMCVVMxGDAWBgNVBAoMD21vbm8ubWFpbC40dGVzdDEVMBMGA1UECwwMT25lTG9naW4gSWRQMR8wHQYDVQQDDBZPbmVMb2dpbiBBY2NvdW50IDk3ODA1MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuRT7E3fQShswERdxbiUYAwwgINXGTies7EHJaHfpOawBL/CCSds21wzvWyRXnXTVU31vPvPnCa8bagkgjm4Kks02o97ydMyIgnuh87hXynCttvppUrB7NnL0xzstJgmQ2Ylzw4p3GBD+1opAWwcr0Yktq8yjZX/HioI9guPHlprR0X6TIDSkhaN4YF9ghgwlaCXVZ7YCcunHjf6zuEPCPPI+ZxSZJh5qwSaAyxnRue+P+zEsNjVpn/d77ZvZusDRLZ2kjqmX8qlAdDUoOzd+Uaoy7UE7JoWlX5u9GQnVRBwbdxYaxBy3vUAV9o34vbgl/lU6fWKEFjJeY4K34VywfQIDAQABo4HfMIHcMAwGA1UdEwEB/wQCMAAwHQYDVR0OBBYEFNml7ySss6RBpr03kPkHVxWIh6sYMIGcBgNVHSMEgZQwgZGAFNml7ySss6RBpr03kPkHVxWIh6sYoWOkYTBfMQswCQYDVQQGEwJVUzEYMBYGA1UECgwPbW9uby5tYWlsLjR0ZXN0MRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgOTc4MDWCFFxll0ZavbySXifBE3SEELtJhH00MA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAFTQ1yfGKruvn8aWg/3NbUyMSuVDI/5WToFPZ5M2VSxEiWwaxzX6FgWfGlrrgTjZKuutu9rWOwOtBwN4oC/vSAxncE18O1P8I095jNqVfGryYm00CscnTiSDjzY3UdrV24G+0rWuiPDoS19kJYOVOkPqqLe02Kx+4b6OFpu6Ag+wEfKXwBFEADc3YdjLt5XkLn6+/kBSPIKXnQzI0yC4YrkrTFZTeISmhMERhaS8xDVOH9O0EW5BeOcVgt+owhug0zYaXftlJTAHcCffhotGqz8hFE+bz3OQKaw5DWTYVegAfigQnYB0OIYtq/7U5QyRXgTG/cunzggMz3e4FZ4RZew==</ds:X509Certificate>
				                    </ds:X509Data>
			                    </ds:KeyInfo>
		                    </ds:Signature>
		                    <saml:Subject>
			                    <saml:NameID Format=""urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"">director@teamlab.io</saml:NameID>
			                    <saml:SubjectConfirmation Method=""urn:oasis:names:tc:SAML:2.0:cm:bearer"">
				                    <saml:SubjectConfirmationData NotOnOrAfter=""2016-12-15T07:00:29Z"" Recipient=""{recipient}"" InResponseTo=""_23d8fbf6-fbf6-45f0-9696-30c194678d1f"" />
			                    </saml:SubjectConfirmation>
		                    </saml:Subject>
		                    <saml:Conditions NotBefore=""2016-12-15T06:54:29Z"" NotOnOrAfter=""2016-12-15T07:00:29Z"">
			                    <saml:AudienceRestriction>
				                    <saml:Audience>{audience}</saml:Audience>
			                    </saml:AudienceRestriction>
		                    </saml:Conditions>
		                    <saml:AuthnStatement AuthnInstant=""2016-12-15T06:57:28Z"" SessionNotOnOrAfter=""2016-12-16T06:57:29Z"" SessionIndex=""_6019bfe0-a4c1-0134-a7d4-069f6a536843"">
			                    <saml:AuthnContext>
				                    <saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml:AuthnContextClassRef>
			                    </saml:AuthnContext>
		                    </saml:AuthnStatement>
		                    <saml:AttributeStatement>
			                    <saml:Attribute Name=""PersonImmutableID"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute Name=""User.LastName"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">Safronov</saml:AttributeValue>
			                    </saml:Attribute>
			                    <saml:Attribute Name=""User.email"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">director@teamlab.io</saml:AttributeValue>
			                    </saml:Attribute>
			                    <saml:Attribute Name=""memberOf"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"" />
			                    </saml:Attribute>
			                    <saml:Attribute Name=""User.FirstName"" NameFormat=""urn:oasis:names:tc:SAML:2.0:attrname-format:basic"">
				                    <saml:AttributeValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""xs:string"">Alexey</saml:AttributeValue>
			                    </saml:Attribute>
		                    </saml:AttributeStatement>
	                    </saml:Assertion>
                    </samlp:Response>";

            var samlResponse = new SamlResponse(ssoSettings);

            samlResponse.LoadXml(resp);

            var email = samlResponse.GetNameID();
            var firstName = samlResponse.GetFirstName();
            var lastName = samlResponse.GetLastName();

            Assert.IsNotNull(email);
            Assert.IsNotNull(firstName);
            Assert.IsNotNull(lastName);
        }
    }
}
