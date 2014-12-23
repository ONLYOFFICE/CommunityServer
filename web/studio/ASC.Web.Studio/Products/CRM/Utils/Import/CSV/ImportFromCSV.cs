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

        private static readonly ProgressQueue _importQueue = new ProgressQueue(1, TimeSpan.FromSeconds(15), true);

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
            return _importQueue.GetStatus(String.Format("{0}_{1}", TenantProvider.CurrentTenantID, (int)entityType));
        }

        public static IProgressItem Start(EntityType entityType, String CSVFileURI, String importSettingsJSON)
        {
            lock (_syncObj)
            {
                var operation = GetStatus(entityType);

                if (operation == null)
                {
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