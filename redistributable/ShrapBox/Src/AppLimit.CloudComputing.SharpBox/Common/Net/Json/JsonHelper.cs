using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Json
{
    internal class JsonHelper
    {
        private JToken _tokenInfo;

        public bool ParseJsonMessage(string jsonmsg)
        {
            _tokenInfo = JObject.Parse(jsonmsg);

            return true;
        }

        public string GetProperty(string key)
        {
            var val = Convert.ToString(_tokenInfo.SelectToken(key));
            return val.Replace("\"", "");
        }

        public List<String> GetListProperty(string key)
        {
            var lstRet = new List<String>();

            var childs = _tokenInfo.SelectToken(key) as JArray;
            if (childs == null || childs.Count <= 0)
                return lstRet;

            foreach (var child in (IEnumerable<JToken>)childs)
            {
                lstRet.Add(child.ToString());
            }

            return lstRet;
        }

        public Boolean GetBooleanProperty(string key)
        {
            var bv = _tokenInfo.SelectToken(key);
            if (bv == null)
                return false;

            var trueValue = new JValue(true);
            return trueValue.Equals(bv);
        }

        public string GetSubObjectString(string key)
        {
            var data = _tokenInfo.SelectToken(key);
            if (data != null)
                return data.ToString();
            return "";
        }

        public int GetPropertyInt(string key)
        {
            return Convert.ToInt32(GetProperty(key));
        }

        public DateTime GetDateTimeProperty(string key)
        {
            // read the string 
            var dateTime = GetProperty(key).Trim();

            // convert
            return JsonDateTimeConverter.GetDateTimeProperty(dateTime);
        }
    }
}