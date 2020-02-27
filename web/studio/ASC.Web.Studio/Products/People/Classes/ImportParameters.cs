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
using System.Text;

using ASC.Web.People.Resources;
using ASC.Web.People.Core.Import;

namespace ASC.Web.People.Classes
{
    internal class ImportParameters
    {
        internal Enum Id { get; private set; }
        internal Func<string> Title { get; private set; }

        protected ImportParameters(Enum id, Func<string> title)
        {
            Id = id;
            Title = title;
        }
    }

    internal abstract class ImportParameters<T> : ImportParameters
    {
        internal T Value { get; private set; }

        protected ImportParameters(Enum id, Func<string> title, T value) : base(id, title)
        {
            Value = value;
        }
    }

    internal class SeparatorParameters : ImportParameters<string>
    {
        internal static List<SeparatorParameters> Separator = new List<SeparatorParameters>
        {
            new SeparatorParameters(SeparatorEnum.Comma, () => PeopleResource.ImportSeparatorComma, ","),
            new SeparatorParameters(SeparatorEnum.Semicolon, () => PeopleResource.ImportSeparatorSemicolon, ";"),
            new SeparatorParameters(SeparatorEnum.Colon, () => PeopleResource.ImportSeparatorColon, ":"),
            new SeparatorParameters(SeparatorEnum.Tab, () => PeopleResource.ImportSeparatorTab, "\t"),
            new SeparatorParameters(SeparatorEnum.Space, () => PeopleResource.ImportSeparatorSpace, " ")
        };

        private SeparatorParameters(Enum id, Func<string> title, string value)
            : base(id, title, value)
        {
        }

        internal static string GetById(int id)
        {
            var item = Separator.FirstOrDefault(r => r != null && Convert.ToInt32(r.Id) == id);

            return (item ?? Separator[0]).Value;
        }
    }

    internal class EncodingParameters : ImportParameters<Encoding>
    {
        internal static List<EncodingParameters> Encodng = new List<EncodingParameters>
        {
            new EncodingParameters(EncodingEnum.UTF8, () => PeopleResource.ImportEncodingUTF8, Encoding.UTF8),
            new EncodingParameters(EncodingEnum.ASCII, () => PeopleResource.ImportEncodingASCII, Encoding.ASCII),
            new EncodingParameters(EncodingEnum.Windows1251, () => PeopleResource.ImportEncodingWindows1251, Encoding.GetEncoding(1251)),
            new EncodingParameters(EncodingEnum.CP866, () => PeopleResource.ImportEncodingCP866, Encoding.GetEncoding(866)),
            new EncodingParameters(EncodingEnum.KOI8R, () => PeopleResource.ImportEncodingKOI8R, Encoding.GetEncoding(20866))
        };

        private EncodingParameters(Enum id, Func<string> title, Encoding value)
            : base(id, title, value)
        {
        }

        internal static Encoding GetById(int id)
        {
            var item = Encodng.FirstOrDefault(r => r != null && Convert.ToInt32(r.Id) == id);

            return (item ?? Encodng[0]).Value;
        }
    }

    internal class DelimiterParameters : ImportParameters<string>
    {
        internal static List<DelimiterParameters> Delimiter = new List<DelimiterParameters>
        {
            new DelimiterParameters(DelimiterEnum.Doublequote, () => PeopleResource.ImportDelimiterDQ, "\""),
            new DelimiterParameters(DelimiterEnum.Singlequote, () => PeopleResource.ImportDelimiterSQ, "'")
        };

        private DelimiterParameters(Enum id, Func<string> title, string value) 
            : base(id, title, value)
        {
        }

        internal static string GetById(int id)
        {
            var item = Delimiter.FirstOrDefault(r => r != null && Convert.ToInt32(r.Id) == id);

            return (item ?? Delimiter[0]).Value;
        }
    }
}