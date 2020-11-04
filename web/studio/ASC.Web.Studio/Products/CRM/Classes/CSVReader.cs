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


/* CSVReader - a simple open source C# class library to read CSV data
 * by Andrew Stellman - http://www.stellman-greene.com/CSVReader
 * 
 * CSVReader.cs - Class to read CSV data from a string, file or stream
 * 
 * download the latest version: http://svn.stellman-greene.com/CSVReader
 * 
 * (c) 2008, Stellman & Greene Consulting
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Stellman & Greene Consulting nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY STELLMAN & GREENE CONSULTING ''AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL STELLMAN & GREENE CONSULTING BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Text;

namespace ASC.Web.CRM.Classes
{
    /// <summary>
    /// Read CSV-formatted data from a file or TextReader
    /// </summary>
    public class CSVReader : IDisposable
    {
        public const string NEWLINE = "\r\n";
        public const string COMMASEP = ",";
        public const string SEMICOLONSEP = ";";

        // Field Separator (default is comma)
        private static string FieldSep = COMMASEP;

        /// <summary>
        /// This reader will read all of the CSV data
        /// </summary>
        private BinaryReader reader;

        /// <summary>
        /// The number of rows to scan for types when building a DataTable (0 to scan the whole file)
        /// </summary>
        public int ScanRows = 0;

        #region Constructors

        /// <summary>
        /// Read CSV-formatted data from a file
        /// </summary>
        /// <param name="filename">Name of the CSV file</param>
        public CSVReader(FileInfo csvFileInfo)
        {
            if (csvFileInfo == null)
                throw new ArgumentNullException("Null FileInfo passed to CSVReader");

            this.reader = new BinaryReader(File.OpenRead(csvFileInfo.FullName));
        }

        /// <summary>
        /// Read CSV-formatted data from a string
        /// </summary>
        /// <param name="csvData">String containing CSV data</param>
        public CSVReader(string csvData)
        {
            if (csvData == null)
                throw new ArgumentNullException("Null string passed to CSVReader");


            this.reader = new BinaryReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvData)));
        }

        /// <summary>
        /// Read CSV-formatted data from a TextReader
        /// </summary>
        /// <param name="reader">TextReader that's reading CSV-formatted data</param>
        public CSVReader(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("Null TextReader passed to CSVReader");

            this.reader = new BinaryReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd())));
        }

        #endregion



        string currentLine = "";
        /// <summary>
        /// Read the next row from the CSV data
        /// </summary>
        /// <returns>A list of objects read from the row, or null if there is no next row</returns>
        public List<object> ReadRow()
        {
            // ReadLine() will return null if there's no next line
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return null;

            StringBuilder builder = new StringBuilder();

            // Read the next line
            while ((reader.BaseStream.Position < reader.BaseStream.Length) && (!builder.ToString().EndsWith(NEWLINE)))
            {
                char c = reader.ReadChar();
                builder.Append(c);
            }

            currentLine = builder.ToString();
            if (currentLine.EndsWith(NEWLINE))
                currentLine = currentLine.Remove(currentLine.IndexOf(NEWLINE), NEWLINE.Length);

            // Build the list of objects in the line
            List<object> objects = new List<object>();
            while (currentLine != "")
                objects.Add(ReadNextObject());
            return objects;
        }

        /// <summary>
        /// Read the next object from the currentLine string
        /// </summary>
        /// <returns>The next object in the currentLine string</returns>
        private object ReadNextObject()
        {
            if (currentLine == null)
                return null;

            // Check to see if the next value is quoted
            bool quoted = false;
            if (currentLine.StartsWith("\""))
                quoted = true;

            // Find the end of the next value
            string nextObjectString = "";
            int i = 0;
            int len = currentLine.Length;
            bool foundEnd = false;
            while (!foundEnd && i <= len)
            {
                // Check if we've hit the end of the string
                if ((!quoted && i == len) // non-quoted strings end with a comma or end of line
                    || (!quoted && currentLine.Substring(i, 1) == FieldSep)
                    // quoted strings end with a quote followed by a comma or end of line
                    || (quoted && i == len - 1 && currentLine.EndsWith("\""))
                    || (quoted && currentLine.Substring(i, 2) == "\"" + FieldSep))
                    foundEnd = true;
                else
                    i++;
            }
            if (quoted)
            {
                if (i > len || !currentLine.Substring(i, 1).StartsWith("\""))
                    throw new FormatException("Invalid CSV format: " + currentLine.Substring(0, i));
                i++;
            }
            nextObjectString = currentLine.Substring(0, i).Replace("\"\"", "\"");

            if (i < len)
                currentLine = currentLine.Substring(i + 1);
            else
                currentLine = "";

            if (quoted)
            {
                if (nextObjectString.StartsWith("\""))
                    nextObjectString = nextObjectString.Substring(1);
                if (nextObjectString.EndsWith("\""))
                    nextObjectString = nextObjectString.Substring(0, nextObjectString.Length - 1);
                return nextObjectString;
            }
            else
            {
                object convertedValue;
                StringConverter.ConvertString(nextObjectString, out convertedValue);
                return convertedValue;
            }
        }

        /// <summary>
        /// Read the row data read using repeated ReadRow() calls and build a DataColumnCollection with types and column names
        /// </summary>
        /// <param name="headerRow">True if the first row contains headers</param>
        /// <returns>System.Data.DataTable object populated with the row data</returns>
        public DataTable CreateDataTable(bool headerRow)
        {
            // Read the CSV data into rows
            List<List<object>> rows = new List<List<object>>();
            List<object> readRow = null;
            while ((readRow = ReadRow()) != null)
                rows.Add(readRow);

            // The types and names (if headerRow is true) will be stored in these lists
            List<Type> columnTypes = new List<Type>();
            List<string> columnNames = new List<string>();

            // Read the column names from the header row (if there is one)
            if (headerRow)
                foreach (object name in rows[0])
                    columnNames.Add(name.ToString());

            // Read the column types from each row in the list of rows
            bool headerRead = false;
            foreach (List<object> row in rows)
                if (headerRead || !headerRow)
                    for (int i = 0; i < row.Count; i++)
                        // If we're adding a new column to the columnTypes list, use its type.
                        // Otherwise, find the common type between the one that's there and the new row.
                        if (columnTypes.Count < i + 1)
                            columnTypes.Add(row[i].GetType());
                        else
                            columnTypes[i] = StringConverter.FindCommonType(columnTypes[i], row[i].GetType());
                else
                    headerRead = true;

            // Create the table and add the columns
            DataTable table = new DataTable();
            for (int i = 0; i < columnTypes.Count; i++)
            {
                table.Columns.Add();
                table.Columns[i].DataType = columnTypes[i];
                if (i < columnNames.Count)
                    table.Columns[i].ColumnName = columnNames[i];
            }

            // Add the data from the rows
            headerRead = false;
            foreach (List<object> row in rows)
                if (headerRead || !headerRow)
                {
                    DataRow dataRow = table.NewRow();
                    for (int i = 0; i < row.Count; i++)
                        dataRow[i] = row[i];
                    table.Rows.Add(dataRow);
                }
                else
                    headerRead = true;

            return table;
        }

        /// <summary>
        /// Read a CSV file into a table
        /// </summary>
        /// <param name="filename">Filename of CSV file</param>
        /// <param name="headerRow">True if the first row contains column names</param>
        /// <param name="scanRows">The number of rows to scan for types when building a DataTable (0 to scan the whole file)</param>
        /// <param name="fieldSeparator">The field separator character</param>
        /// <returns>System.Data.DataTable object that contains the CSV data</returns>
        public static DataTable ReadCSVFile(string filename, bool headerRow, int scanRows, String fieldSeparator)
        {
            FieldSep = fieldSeparator;
            return ReadCSVFile(filename, headerRow, scanRows);
        }

        /// <summary>
        /// Read a CSV file into a table
        /// </summary>
        /// <param name="filename">Filename of CSV file</param>
        /// <param name="headerRow">True if the first row contains column names</param>
        /// <param name="fieldSeparator">The field separator character</param>
        /// <returns>System.Data.DataTable object that contains the CSV data</returns>
        public static DataTable ReadCSVFile(string filename, bool headerRow, String fieldSeparator)
        {
            FieldSep = fieldSeparator;
            return ReadCSVFile(filename, headerRow);
        }

        /// <summary>
        /// Read a CSV file into a table
        /// </summary>
        /// <param name="filename">Filename of CSV file</param>
        /// <param name="headerRow">True if the first row contains column names</param>
        /// <param name="scanRows">The number of rows to scan for types when building a DataTable (0 to scan the whole file)</param>
        /// <returns>System.Data.DataTable object that contains the CSV data</returns>
        public static DataTable ReadCSVFile(string filename, bool headerRow, int scanRows)
        {
            using (CSVReader reader = new CSVReader(new FileInfo(filename)))
            {
                reader.ScanRows = scanRows;
                return reader.CreateDataTable(headerRow);
            }
        }

        /// <summary>
        /// Read a CSV file into a table
        /// </summary>
        /// <param name="filename">Filename of CSV file</param>
        /// <param name="headerRow">True if the first row contains column names</param>
        /// <returns>System.Data.DataTable object that contains the CSV data</returns>
        public static DataTable ReadCSVFile(string filename, bool headerRow)
        {
            using (CSVReader reader = new CSVReader(new FileInfo(filename)))
                return reader.CreateDataTable(headerRow);
        }



        #region IDisposable Members

        public void Dispose()
        {
            if (reader != null)
            {
                try
                {
                    // Can't call BinaryReader.Dispose due to its protection level
                    reader.Close();
                }
                catch { }
            }
        }

        #endregion
    }
}
