using System;
using System.Collections.Generic;
using System.IO;

using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl
{
    internal class OAuthStreamParser
    {        
        static public OAuthToken ParseTokenInformation(Stream data)
        {
            Dictionary<String, String> parameters = ParseParameterResult(data);            
            return new OAuthToken(parameters["oauth_token"], parameters["oauth_token_secret"]);
        }

        static private Dictionary<String, String> ParseParameterResult(Stream data)
        {        
            String result = GetResultString(data);

            if (result.Length > 0)
            {
                var parsedParams = new Dictionary<string, string>();

                // 1. split at "&"
                String[] parameters = result.Split('&');

                foreach (String paramSet in parameters)
                {
                    String[] param2 = paramSet.Split('=');
                    parsedParams.Add(param2[0], param2[1]);
                }

                return parsedParams;
            }

            return null;
        }


        static private String GetResultString(Stream data)
        {            
            var reader = new StreamReader(data);
            return reader.ReadToEnd();         
        }

    }
}
