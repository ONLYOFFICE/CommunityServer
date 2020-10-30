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