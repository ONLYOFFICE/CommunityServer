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