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