/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ASC.Notify.Messages;

namespace ASC.Notify.Patterns
{
    public abstract class PatternFormatter : IPatternFormatter
    {
        private readonly bool doformat;
        private readonly string tagSearchPattern;


        protected Regex RegEx
        {
            get;
            private set;
        }


        public PatternFormatter()
        {
        }

        public PatternFormatter(string tagSearchRegExp)
            : this(tagSearchRegExp, false)
        {
        }

        internal PatternFormatter(string tagSearchRegExp, bool formatMessage)
        {
            if (String.IsNullOrEmpty(tagSearchRegExp)) throw new ArgumentException("tagSearchRegExp");

            tagSearchPattern = tagSearchRegExp;
            RegEx = new Regex(tagSearchPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            doformat = formatMessage;
        }

        public string[] GetTags(IPattern pattern)
        {
            if (pattern == null) throw new ArgumentNullException("pattern");

            var findedTags = new List<string>(SearchTags(pattern.Body));
            Array.ForEach(SearchTags(pattern.Subject), tag => { if (!findedTags.Contains(tag)) findedTags.Add(tag); });
            return findedTags.ToArray();
        }

        public void FormatMessage(INoticeMessage message, ITagValue[] tagsValues)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.Pattern == null) throw new ArgumentException("message");
            if (tagsValues == null) throw new ArgumentNullException("tagsValues");

            BeforeFormat(message, tagsValues);

            message.Subject = FormatText(doformat ? message.Subject : message.Pattern.Subject, tagsValues);
            message.Body = FormatText(doformat ? message.Body : message.Pattern.Body, tagsValues);

            AfterFormat(message);
        }

        protected abstract string FormatText(string text, ITagValue[] tagsValues);

        protected virtual void BeforeFormat(INoticeMessage message, ITagValue[] tagsValues)
        {
        }

        protected virtual void AfterFormat(INoticeMessage message)
        {
        }

        protected virtual string[] SearchTags(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(tagSearchPattern)) return new string[0];

            var maches = RegEx.Matches(text);
            var findedTags = new List<string>(maches.Count);
            foreach (Match mach in maches)
            {
                var tag = mach.Groups["tagName"].Value;
                if (!findedTags.Contains(tag)) findedTags.Add(tag);
            }

            return findedTags.ToArray();
        }
    }
}