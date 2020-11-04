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
using System.Linq;
using System.Xml;

namespace ASC.Web.Studio.Utility.BBCodeParser
{
    public class ParserConfiguration
    {
        public bool IsHTMLEncode { get; set; }

        public List<TagConfiguration> TagConfigurations { get; private set; }
        public List<ExpressionReplacement> ExpressionReplacements { get; private set; }
        public List<RegularExpressionTemplate> RegExpTemplates { get; private set; }

        public TagConfiguration GetTagConfiguration(string tag)
        {
            return TagConfigurations.FirstOrDefault(tagConfiguration => tagConfiguration.Tag.ToLower() == tag.ToLower());
        }

        #region Construtors

        public ParserConfiguration() : this(null, true)
        {
        }

        public ParserConfiguration(bool isHTMLEncode) : this(null, isHTMLEncode)
        {
        }

        public ParserConfiguration(string configurationFile) : this(configurationFile, true)
        {
        }

        public ParserConfiguration(string configurationFile, bool isHTMLEncode)
        {
            IsHTMLEncode = isHTMLEncode;
            TagConfigurations = new List<TagConfiguration>();
            ExpressionReplacements = new List<ExpressionReplacement>();
            RegExpTemplates = new List<RegularExpressionTemplate>();

            if (!String.IsNullOrEmpty(configurationFile))
            {
                LoadConfigurationFromXml(configurationFile);
            }
        }

        #endregion

        public void LoadConfigurationFromXml(string configurationFile)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(configurationFile);

            var nodes = xmlDocument.SelectNodes("/configuration/parser/expressionReplacements/expressionReplacement");
            foreach (XmlNode node in nodes)
            {
                // Get the expression,
                var expression = node.SelectSingleNode("@expression").Value;
                expression = expression.Replace("\\n", "\n");

                // Get the replacement.
                var replacement = node.SelectSingleNode("@replacement").Value;
                replacement = replacement.Replace("\\n", "\n");

                // Build the expression replacement.
                var expressionReplacement = new ExpressionReplacement(expression, replacement);
                ExpressionReplacements.Add(expressionReplacement);
            }

            nodes = xmlDocument.SelectNodes("/configuration/tags/tag");
            foreach (XmlNode node in nodes)
            {
                var tag = node.SelectSingleNode("@name").InnerText;
                var replacement = node.SelectSingleNode("@replacement").InnerText;
                var alternativeReplacement =
                    node.SelectSingleNode("@alternativeReplacement") != null
                        ? node.SelectSingleNode("@alternativeReplacement").InnerText
                        : null;
                try
                {
                    var parseContent = Boolean.Parse(node.SelectSingleNode("@parseContent").InnerText);
                    TagConfigurations.Add(new TagConfiguration(tag, replacement, alternativeReplacement, parseContent));
                }
                catch (NullReferenceException)
                {
                    TagConfigurations.Add(new TagConfiguration(tag, replacement, alternativeReplacement));
                }
            }
        }
    }
}