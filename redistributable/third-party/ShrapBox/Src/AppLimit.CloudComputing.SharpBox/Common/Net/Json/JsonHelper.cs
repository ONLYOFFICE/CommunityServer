using System;
using System.Collections.Generic;

#if MONOTOUCH || MONODROID
using System.IO;
using System.Json;
#else 
using Newtonsoft.Json.Linq;
#endif

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Json
{
	internal class JsonHelper
	{
		public JsonHelper ()
		{
		}
			
#if MONOTOUCH || MONODROID
        private JsonObject _jsonMsgObj;
		
		public bool ParseJsonMessage (string jsonmsg)
		{	
			try
			{				
				_jsonMsgObj = JsonObject.Parse(jsonmsg) as JsonObject;	
			}
			catch (Exception)
			{
				return false;	
			}
			
			return true;
		}

		private JsonValue GetPropertyJson(string key)
		{
			if ( _jsonMsgObj == null )
				return null;
			
			JsonValue v1 = null;
			if (!_jsonMsgObj.TryGetValue(key, out v1))
				return null;
			
			return v1;
		}
		
		public string GetProperty (string key)
		{
			JsonValue v1 = GetPropertyJson(key);
			if (v1 == null)
				return "";
			
			String v2 = v1.ToString();
			return v2.Replace("\"", "");
		}	
		
		public List<String> GetListProperty(string key)
		{
			List<String> ret =  new List<String>();	
			
			JsonArray jarr = GetPropertyJson(key) as JsonArray;
			if (jarr == null )
				return ret;
			
			foreach(JsonObject obj in jarr)
			{
				ret.Add(obj.ToString());	
			}
				        
			return ret;
		}
		
		public string GetSubObjectString(string key)
		{
			return GetProperty(key);
		}
		
		public Boolean GetBooleanProperty(string key)
		{
			String v = GetProperty(key);
			
			return Convert.ToBoolean(v);
		}
#else		
		private JToken _tokenInfo; 
		
		public bool ParseJsonMessage (string jsonmsg)
		{
			
			_tokenInfo = JObject.Parse(jsonmsg);
			
            return true;
		}

		public string GetProperty (string key)
		{            
			String val = Convert.ToString(_tokenInfo.SelectToken(key));
			return val.Replace("\"", "");
		}
		
		public List<String> GetListProperty(string key)
		{
			List<String> lstRet = new List<String>();
			
			var childs = _tokenInfo.SelectToken(key) as JArray;
            if (childs == null || childs.Count <= 0) 
				return lstRet;
								
			foreach(var child in (IEnumerable<JToken>)childs)
			{
				lstRet.Add(child.ToString());
			}
				           
			return lstRet;
		}
		
		public Boolean GetBooleanProperty(string key)
		{
			var bv = _tokenInfo.SelectToken(key);
			if ( bv== null )
				return false;
			
			var trueValue = new JValue(true);
			return trueValue.Equals(bv);
		}
		
		public string GetSubObjectString(string key)
		{
			var data = _tokenInfo.SelectToken(key);
			if ( data != null )
				return data.ToString();
			else
				return "";            
		}
#endif

        public int GetPropertyInt(string key)
		{
			return Convert.ToInt32(GetProperty(key));	
		}
              
        public DateTime GetDateTimeProperty(string key)
        {        
            // read the string 
            String dateTime = GetProperty(key).Trim();

            // convert
            return JsonDateTimeConverter.GetDateTimeProperty(dateTime);
        }
	}
}

