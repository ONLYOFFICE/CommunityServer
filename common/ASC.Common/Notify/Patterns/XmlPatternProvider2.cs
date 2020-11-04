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
using System.Reflection;
using System.Xml;
using ASC.Notify.Engine;
using ASC.Notify.Model;

namespace ASC.Notify.Patterns
{
    public class XmlPatternProvider2 : IPatternProvider
    {
        private readonly IDictionary<string, IPattern> patterns = new Dictionary<string, IPattern>();
        private readonly IPatternFormatter formatter = null;


        public Func<INotifyAction, string, NotifyRequest, IPattern> GetPatternMethod
        {
            get;
            set;
        }


        public XmlPatternProvider2(string xml)
            : this(xml, null)
        {

        }

        public XmlPatternProvider2(string xml, Func<INotifyAction, string, NotifyRequest, IPattern> getpattern)
        {
            GetPatternMethod = getpattern;

            var xdoc = new XmlDocument();
            xdoc.LoadXml(xml);

            var xformatter = xdoc.SelectSingleNode("/patterns/formatter") as XmlElement;
            if (xformatter != null)
            {
                var type = xformatter.GetAttribute("type");
                if (!string.IsNullOrEmpty(type))
                {
                    formatter = (IPatternFormatter)Activator.CreateInstance(Type.GetType(type, true));
                }
            }

            var references = new Dictionary<string, string>();

            foreach (XmlElement xpattern in xdoc.SelectNodes("/patterns/pattern"))
            {
                var id = xpattern.GetAttribute("id");
                var sender = xpattern.GetAttribute("sender");
                var reference = xpattern.GetAttribute("reference");

                if (string.IsNullOrEmpty(reference))
                {
                    var subject = GetResource(GetElementByTagName(xpattern, "subject"));

                    var xbody = GetElementByTagName(xpattern, "body");
                    var body = GetResource(xbody);
                    if (string.IsNullOrEmpty(body) && xbody != null && xbody.FirstChild is XmlText)
                    {
                        body = xbody.FirstChild.Value ?? string.Empty;
                    }

                    var styler = xbody != null ? xbody.GetAttribute("styler") : string.Empty;

                    patterns[id + sender] = new Pattern(id, subject, body, Pattern.HTMLContentType) { Styler = styler };
                }
                else
                {
                    references[id + sender] = reference + sender;
                }
            }

            foreach (var pair in references)
            {
                patterns[pair.Key] = patterns[pair.Value];
            }
        }

        public IPattern GetPattern(INotifyAction action, string senderName)
        {
            IPattern p;
            if (patterns.TryGetValue(action.ID + senderName, out p))
            {
                return p;
            }
            if (patterns.TryGetValue(action.ID, out p))
            {
                return p;
            }
            return null;
        }

        public IPatternFormatter GetFormatter(IPattern pattern)
        {
            return formatter;
        }


        private XmlElement GetElementByTagName(XmlElement e, string name)
        {
            var list = e.GetElementsByTagName(name);
            return list.Count == 0 ? null : list[0] as XmlElement;
        }

        private string GetResource(XmlElement e)
        {
            var result = string.Empty;

            if (e == null)
            {
                return result;
            }

            result = e.GetAttribute("resource");
            if (string.IsNullOrEmpty(result))
            {
                return result;
            }

            var array = result.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length < 2)
            {
                return result;
            }

            var resourceManagerType = Type.GetType(array[1], true, true);
            var property = resourceManagerType.GetProperty(array[0], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (property == null)
            {
                throw new NotifyException(string.Format("Resource {0} not found in resourceManager {1}", array[0], array[1]));
            }
            return property.GetValue(resourceManagerType, null) as string;
        }
    }
}