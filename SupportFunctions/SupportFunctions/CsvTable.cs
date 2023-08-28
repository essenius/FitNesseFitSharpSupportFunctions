// Copyright 2016-2023 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using static System.FormattableString;

namespace SupportFunctions
{
    /// <summary>CSV file handling. Used by Time Series</summary>
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class CsvTable
    {
        private string[] _headers;

        /// <summary>Initialize new Csv Table</summary>
        public CsvTable() => Data = new Collection<string[]>();

        /// <summary>Create a new CSV table with headers</summary>
        /// <param name="headers">list of headers</param>
        public CsvTable(string[] headers) : this() => _headers = headers;

        /// <summary>The number of columns in the CSV file</summary>
        public int ColumnCount => _headers.Length;

        internal Collection<string[]> Data { get; }

        /// <summary>The number of rows in the CSV file</summary>
        public int RowCount => Data.Count;

        private static string AddQuotesIfNeeded(string entry)
        {
            var needsQuotes = entry.Contains(",") || entry.Contains("\"") || entry.Contains("\r") || entry.Contains("\n");
            entry = entry.Replace("\"", "\"\"");
            if (needsQuotes) return "\"" + entry + "\"";
            return entry;
        }

        internal string DataCell(int row, int column) => row >= RowCount || column >= ColumnCount ? null : Data[row][column];

        /// <summary>Show the content of the CSV file in Table format</summary>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "FitNesse interface spec")]
        [SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "FitNesse interface spec")]
        public Collection<Collection<string>> DoTable(Collection<Collection<string>> inputTable)
        {
            var returnObject = new Collection<Collection<string>>();
            var row = new Collection<string>();
            foreach (var headerCell in _headers)
            {
                row.Add("report:" + headerCell);
            }

            returnObject.Add(row);
            foreach (var line in Data)
            {
                row = new Collection<string>();
                foreach (var cell in line)
                {
                    row.Add("report:" + cell);
                }

                returnObject.Add(row);
            }

            return returnObject;
        }

        internal string Header(int index) => index >= ColumnCount ? null : _headers[index];

        internal int HeaderIndex(string headerName)
        {
            for (var i = 0; i < _headers.Length; i++)
            {
                if (_headers[i].Equals(headerName, StringComparison.OrdinalIgnoreCase)) return i;
            }

            throw new ArgumentException(Invariant($"Header name {headerName} not recognized"));
        }

        /// <summary>Load a CSV Table from the specified file</summary>
        public void LoadFrom(string path) => LoadFrom(path, ",", "#", true);

        private void LoadFrom(string path, string delimiter, string comment, bool fieldsInQuotes)
        {
            using var csvParser = new TextFieldParser(path)
            {
                CommentTokens = new[] { comment }
            };
            csvParser.SetDelimiters(delimiter);
            csvParser.HasFieldsEnclosedInQuotes = fieldsInQuotes;

            _headers = csvParser.ReadFields();

            Data.Clear();
            while (!csvParser.EndOfData)
            {
                var fields = csvParser.ReadFields();
                Data.Add(fields);
            }
        }

        /// <summary>Load a CSV Table from the specified file</summary>
        public static CsvTable Parse(string input)
        {
            var csvTable = new CsvTable();
            csvTable.LoadFrom(input);
            return csvTable;
        }

        /// <returns>the CSV Table as a Query result</returns>
        public Collection<object> Query()
        {
            var returnObject = new Collection<object>();
            foreach (var line in Data)
            {
                var cellCounter = 0;
                var row = new Collection<object>();
                foreach (var cell in line)
                {
                    row.Add(new Collection<object> { _headers[cellCounter], cell });
                    cellCounter++;
                }

                returnObject.Add(row);
            }

            return returnObject;
        }

        internal string[] Row(int row) => row >= RowCount ? Array.Empty<string>() : Data[row];

        internal void SaveTo(string path)
        {
            var csvLines = new List<string>();
            var csvCell = _headers.Select(AddQuotesIfNeeded);
            csvLines.Add(string.Join(",", csvCell));
            csvLines.AddRange(Data.Select(row =>
                row.Select(AddQuotesIfNeeded)).Select(dataRow => string.Join(",", dataRow)));
            var csvText = string.Join(Environment.NewLine, csvLines);
            File.WriteAllText(path, csvText);
        }

        /// <returns>CsvTable</returns>
        public override string ToString() => "CsvTable";
    }
}
