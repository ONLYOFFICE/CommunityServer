using System;
using System.Text;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth20
{
    public class OAuth20Token : ICloudStorageAccessToken
    {
        public String AccessToken { get; set; }

        public String RefreshToken { get; set; }

        public double ExpiresIn { get; set; }

        public String ClientID { get; set; }

        public String ClientSecret { get; set; }

        public String RedirectUri { get; set; }

        public DateTime? Timestamp { get; set; }

        public bool IsExpired
        {
            get
            {
                if (Timestamp.HasValue && !ExpiresIn.Equals(default(double)))
                    return DateTime.UtcNow > Timestamp + TimeSpan.FromSeconds(ExpiresIn);
                return true;
            }
        }

        public static OAuth20Token FromJson(String json)
        {
            var parser = new JsonHelper();

            if (!parser.ParseJsonMessage(json))
                return null;

            var accessToken = parser.GetProperty("access_token");
            var refreshToken = parser.GetProperty("refresh_token");

            if (String.IsNullOrEmpty(accessToken))
                return null;

            var token = new OAuth20Token
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ClientID = parser.GetProperty("client_id"),
                    ClientSecret = parser.GetProperty("client_secret"),
                    RedirectUri = parser.GetProperty("redirect_uri"),
                };

            double expiresIn;
            if (double.TryParse(parser.GetProperty("expires_in"), out expiresIn))
                token.ExpiresIn = expiresIn;

            DateTime timestamp;
            if (DateTime.TryParse(parser.GetProperty("timestamp"), out timestamp))
                token.Timestamp = timestamp;

            return token;
        }

        public String ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat(" \"access_token\": \"{0}\"", AccessToken);
            sb.AppendFormat(", \"refresh_token\": \"{0}\"", RefreshToken);
            sb.AppendFormat(", \"expires_in\": \"{0}\"", ExpiresIn);
            sb.AppendFormat(", \"client_id\": \"{0}\"", ClientID);
            sb.AppendFormat(", \"client_secret\": \"{0}\"", ClientSecret);
            sb.AppendFormat(", \"redirect_uri\": \"{0}\"", RedirectUri);
            sb.AppendFormat(", \"timestamp\": \"{0}\"", Timestamp);
            sb.Append("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return RefreshToken;
        }
    }
}