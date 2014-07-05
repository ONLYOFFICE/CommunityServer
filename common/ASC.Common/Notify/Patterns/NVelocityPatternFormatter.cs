/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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