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
