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

namespace LumenWorks.Framework.IO.Csv
{
    public static class CsvReaderExtension
    {
        public static String[] GetCurrentRowFields(this CsvReader csvReader, bool htmlEncodeColumn)
        {
            var fieldCount = csvReader.FieldCount;
            var result = new String[fieldCount];

            for (int index = 0; index < fieldCount; index++)
            {
                if (htmlEncodeColumn)
                    result[index] = csvReader[index].HtmlEncode().ReplaceSingleQuote();
                else
                    result[index] = csvReader[index];
            }

            return result;
        }
    }
}