using SelectelSharp.Headers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace SelectelSharp.Headers
{
    internal static class HeaderParsers
    {
        internal static int ToInt(string value)
        {
            int result;
            return int.TryParse(value, out result) ? result : 0;
        }

        internal static long ToLong(string value)
        {
            long result;
            return long.TryParse(value, out result) ? result : 0;
        }

        internal static T ParseHeaders<T>(NameValueCollection headers)
            where T : new()
        {
            var obj = new T();
            ParseHeaders(obj, headers);
            return obj;
        }

        internal static void ParseHeaders(object obj, NameValueCollection headers)
        {
            var props = obj
                .GetType()
                .GetProperties();

            foreach (var prop in props)
            {
                var headerAttr = GetAttribute<HeaderAttribute>(prop);
                if (headerAttr != null)
                {
                    if (headerAttr.CustomHeaders)
                    {
                        var customHeadersKeys = headers
                            .AllKeys
                            .Where(x => x.ToLower().StartsWith(HeaderKeys.XContainerMetaPrefix.ToLower()))
                            .Where(x => x.ToLower() != HeaderKeys.XContainerMetaType.ToLower())
                            .Where(x => x.ToLower() != HeaderKeys.XContainerMetaGallerySecret.ToLower());                        

                        if (customHeadersKeys.Any())
                        {
                            var customHeaders = new Dictionary<string, string>();
                            foreach (var key in customHeadersKeys)
                            {
                                customHeaders.Add(key, headers[key]);
                            }
                            prop.SetValue(obj, customHeaders);
                        }
                    }
                    else if (headerAttr.CORSHeaders)
                    {
                        var cors = new CORSHeaders(headers);
                        prop.SetValue(obj, cors);
                    }
                    else
                    {
                        var value = headers[headerAttr.HeaderKey];
                        if (value == null)
                            continue;

                        if (value.GetType().Equals(prop.PropertyType))
                        {
                            prop.SetValue(obj, value);
                        }
                        else
                        {
                            try
                            {
                                var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                                prop.SetValue(obj, convertedValue);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private static T GetAttribute<T>(PropertyInfo pi)
            where T: Attribute
        {
            var attr = pi.GetCustomAttributes<T>();
            if (attr.Any())
            {
                return attr.First();
            }
            else
            {
                return null;
            }
        }
    }
}
