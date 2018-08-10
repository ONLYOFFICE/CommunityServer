/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using NVelocity;
using NVelocity.App;
using NVelocity.Runtime.Resource.Loader;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASC.Common.Utils
{
    public class TextLoader : ResourceLoader
    {
        public override void Init(Commons.Collections.ExtendedProperties configuration)
        {
            //nothing to configure
        }

        public override Stream GetResourceStream(string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }

        public override long GetLastModified(NVelocity.Runtime.Resource.Resource resource)
        {
            return 1;
        }

        public override bool IsSourceModified(NVelocity.Runtime.Resource.Resource resource)
        {
            return false;
        }
    }

    public class VelocityFormatter
    {
        private static bool initialized;
        private static readonly ConcurrentDictionary<string, Template> patterns = new ConcurrentDictionary<string, Template>();

        public static string FormatText(string templateText, IDictionary<string, object> values)
        {
            var nvelocityContext = new VelocityContext();
            foreach (var tagValue in values)
                nvelocityContext.Put(tagValue.Key, tagValue.Value);
            return FormatText(templateText, nvelocityContext);
        }

        public static string FormatText(string templateText, VelocityContext context)
        {
            if (!initialized)
            {
                var properties = new Commons.Collections.ExtendedProperties();
                properties.AddProperty("resource.loader", "custom");
                properties.AddProperty("custom.resource.loader.class", "ASC.Common.Utils.TextLoader; ASC.Common");
                properties.AddProperty("input.encoding", Encoding.UTF8.WebName);
                properties.AddProperty("output.encoding", Encoding.UTF8.WebName);
                Velocity.Init(properties);
                initialized = true;
            }

            using (var writer = new StringWriter())
            {
                Template template;
                var key = templateText.GetHashCode().ToString();
                if (!patterns.TryGetValue(key, out template))
                {
                    template = Velocity.GetTemplate(templateText);
                    patterns.TryAdd(key, template);
                }
                template.Merge(context, writer);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
