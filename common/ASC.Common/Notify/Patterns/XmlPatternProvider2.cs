/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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