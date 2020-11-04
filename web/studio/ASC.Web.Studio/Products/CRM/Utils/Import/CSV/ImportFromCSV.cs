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


using ASC.Common.Threading.Progress;
using ASC.CRM.Core;
using ASC.Web.Studio.Utility;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ASC.Web.CRM.Classes
{

    public class ImportFromCSV
    {
        #region Members

        private static readonly Object _syncObj = new Object();

        private static readonly ProgressQueue _importQueue = new ProgressQueue(3, TimeSpan.FromSeconds(15), true);

        public static readonly int MaxRoxCount = 10000;

        #endregion

        public static int GetQuotas()
        {
            return MaxRoxCount;
        }

        public static CsvReader CreateCsvReaderInstance(Stream CSVFileStream, ImportCSVSettings importCsvSettings)
        {
            var result = new CsvReader(
                new StreamReader(CSVFileStream, importCsvSettings.Encoding, true),
                importCsvSettings.HasHeader, importCsvSettings.DelimiterCharacter, importCsvSettings.QuoteType, '"', '#', ValueTrimmingOptions.UnquotedOnly) { SkipEmptyLines = true, SupportsMultiline = true, DefaultParseErrorAction = ParseErrorAction.AdvanceToNextLine, MissingFieldAction = MissingFieldAction.ReplaceByEmpty };

            return result;
        }

        public static String GetRow(Stream CSVFileStream, int index, String jsonSettings)
        {
            var importCSVSettings = new ImportCSVSettings(jsonSettings);

            using (CsvReader csv = CreateCsvReaderInstance(CSVFileStream, importCSVSettings))
            {
                int countRows = 0;

                index++;

                while (countRows++ != index && csv.ReadNextRecord()) ;

                return new JObject(new JProperty("data", new JArray(csv.GetCurrentRowFields(false).ToArray())),
                                   new JProperty("isMaxIndex", csv.EndOfStream)).ToString();
            }
        }

        public static JObject GetInfo(Stream CSVFileStream, String jsonSettings)
        {
            var importCSVSettings = new ImportCSVSettings(jsonSettings);

            using (CsvReader csv = CreateCsvReaderInstance(CSVFileStream, importCSVSettings))
            {
                csv.ReadNextRecord();

                var firstRowFields = csv.GetCurrentRowFields(false);

                String[] headerRowFields = csv.GetFieldHeaders().ToArray();

                if (!importCSVSettings.HasHeader)
                    headerRowFields = firstRowFields;

                return new JObject(
                    new JProperty("headerColumns", new JArray(headerRowFields)),
                    new JProperty("firstContactFields", new JArray(firstRowFields)),
                    new JProperty("isMaxIndex", csv.EndOfStream)
                    );
            }
        }

        public static IProgressItem GetStatus(EntityType entityType)
        {
            var result = _importQueue.GetStatus(String.Format("{0}_{1}", TenantProvider.CurrentTenantID, (int)entityType));

            if (result == null)
            {
                return ImportDataCache.Get(entityType);
            }

            return result;
        }

        public static IProgressItem Start(EntityType entityType, String CSVFileURI, String importSettingsJSON)
        {
            lock (_syncObj)
            {
                var operation = GetStatus(entityType);

                if (operation == null)
                {
                    var fromCache = ImportDataCache.Get(entityType);

                    if (fromCache != null)
                        return fromCache;

                    operation = new ImportDataOperation(entityType, CSVFileURI, importSettingsJSON);

                    _importQueue.Add(operation);
                }

                if (!_importQueue.IsStarted)
                    _importQueue.Start(x => x.RunJob());

                return operation;
            }
        }
    }
}