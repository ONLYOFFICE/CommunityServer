using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl
{
    internal class OAuthStreamParser
    {
        public static OAuthToken ParseTokenInformation(Stream data)
        {
            var parameters = ParseParameterResult(data);
            return new OAuthToken(parameters["oauth_token"], parameters["oauth_token_secret"]);
        }

        private static Dictionary<String, String> ParseParameterResult(Stream data)
        {
            var result = GetResultString(data);

            if (result.Length > 0)
            {
                var parameters = result.Split('&');

                return parameters.Select(paramSet => paramSet.Split('=')).ToDictionary(param2 => param2[0], param2 => param2[1]);
            }

            return null;
        }


        private static String GetResultString(Stream data)
        {
            using (var reader = new StreamReader(data))
            {
                return reader.ReadToEnd();
            }
        }
    }
}