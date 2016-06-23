/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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