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


using System;
using ASC.Common.Utils;
using ASC.Notify.Messages;
using NVelocity;
using NVelocity.App.Events;

namespace ASC.Notify.Patterns
{
    public sealed class NVelocityPatternFormatter : PatternFormatter
    {
        public const string DefaultPattern = @"(^|[^\\])\$[\{]{0,1}(?<tagName>[a-zA-Z0-9_]+)";
        public const string NoStylePreffix = "==";
        public const string NoStyleSuffix = "==";

        private VelocityContext _nvelocityContext;

        public NVelocityPatternFormatter()
            : base(DefaultPattern)
        {
        }

        protected override void BeforeFormat(INoticeMessage message, ITagValue[] tagsValues)
        {
            _nvelocityContext = new VelocityContext();
            _nvelocityContext.AttachEventCartridge(new EventCartridge());
            _nvelocityContext.EventCartridge.ReferenceInsertion += EventCartridgeReferenceInsertion;
            foreach (var tagValue in tagsValues)
            {
                _nvelocityContext.Put(tagValue.Tag, tagValue.Value);
            }
            base.BeforeFormat(message, tagsValues);
        }

        protected override string FormatText(string text, ITagValue[] tagsValues)
        {
            if (String.IsNullOrEmpty(text)) return text;
            return VelocityFormatter.FormatText(text, _nvelocityContext);
        }

        protected override void AfterFormat(INoticeMessage message)
        {
            _nvelocityContext = null;
            base.AfterFormat(message);
        }

        private static void EventCartridgeReferenceInsertion(object sender, ReferenceInsertionEventArgs e)
        {
            var originalString = e.OriginalValue as string;
            if (originalString == null) return;
            var lines = originalString.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;
            e.NewValue = string.Empty;
            for (var i = 0; i < lines.Length - 1; i++)
            {
                e.NewValue += string.Format("{0}{1}{2}\n", NoStylePreffix, lines[i], NoStyleSuffix);
            }
            e.NewValue += string.Format("{0}{1}{2}", NoStylePreffix, lines[lines.Length - 1], NoStyleSuffix);
        }
    }
}