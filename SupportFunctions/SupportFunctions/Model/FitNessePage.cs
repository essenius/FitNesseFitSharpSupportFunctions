﻿// Copyright 2016-2024 Rik Essenius
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class FitNessePage
    {
        private const string DefaultPageRoot = "http://localhost:8080/DataStore";
        private const string Linefeed = "\n";
        private const char Pipe = '|';

        private List<string> _line = new List<string>();

        public string PageRoot { get; set; } = DefaultPageRoot;

        private void AddPage(string pageName, IEnumerable<string> lines)
        {
            var command = PageRoot + "?addChild&pageName=" + pageName + "&pageContent=";
            var payload = lines.Aggregate(
                string.Empty,
                (current, entry) => current + entry + Linefeed
            );
            _ = RestCall(command + Uri.EscapeDataString(payload));
        }

        private static void CheckColumnNames(string line)
        {
            const string errorTemplate = "{0} column must be called '{1}' instead of '{2}'";
            var columnNames = ExtractKeyValuePair(line);
            if (!columnNames.Key.EqualsIgnoreCase("key"))
            {
                throw new FormatException(string.Format(errorTemplate, "First", "Key", columnNames.Key));
            }

            if (!columnNames.Value.EqualsIgnoreCase("value"))
            {
                throw new FormatException(string.Format(errorTemplate, "Second", "Value", columnNames.Value));
            }
        }

        private static IEnumerable<string> CreateTable(string tableName, Dictionary<string, string> dictionary)
        {
            var list = new List<string>
            {
                "!" + Pipe + "Dictionary" + Pipe + "Having" + Pipe + "Name" + Pipe + tableName + Pipe,
                Pipe + "Key" + Pipe + "Value" + Pipe
            };
            list.AddRange(dictionary.Select(entry => Pipe + entry.Key + Pipe + entry.Value + Pipe));
            return list;
        }

        public void DeletePage(string page)
        {
            var command = FullPageName(page) + "?deletePage&confirmed=yes";
            var result = RestCall(command);
            Log(result, "DeletePage");
        }

        public bool DeleteTableFromPage(string tableName, string pageName)
        {
            if (!GetPageContent(pageName)) return false;
            var index = FindTableNamed(tableName);
            if (IsAtEnd(index)) return false;
            RemoveTableAt(index);
            SavePage(pageName);
            return true;
        }

        private int DoUntilTableEndFrom(int lineNo, Action<string> action)
        {
            while (lineNo < _line.Count && IsTableLine(_line[lineNo]))
            {
                action(_line[lineNo]);
                lineNo++;
            }

            return lineNo;
        }

        private static KeyValuePair<string, string> ExtractKeyValuePair(string line)
        {
            var part = line.Split(Pipe);
            if (part.Length != 4)
            {
                throw new FormatException(
                    "Row should have 2 cells: Key and Value. Found " + (part.Length - 2) + ": " + line);
            }

            return new KeyValuePair<string, string>(part[1].Trim(), part[2].Trim());
        }

        private Dictionary<string, string> ExtractTableAt(int lineNo)
        {
            var dict = new Dictionary<string, string>();
            CheckColumnNames(_line[lineNo + 1]);
            DoUntilTableEndFrom(lineNo + 2, s =>
            {
                var kvp = ExtractKeyValuePair(s);
                dict.Add(kvp.Key, kvp.Value);
            });
            return dict;
        }

        private int FindTableNamed(string tableName)
        {
            var searchPosition = 0;
            while (searchPosition < _line.Count)
            {
                var tableStartLineNo = FindTableStartFrom(searchPosition);
                if (IsAtEnd(tableStartLineNo) || TableIsNamed(_line[tableStartLineNo], tableName))
                    return tableStartLineNo;
                searchPosition = DoUntilTableEndFrom(tableStartLineNo + 1, s => { });
            }

            return searchPosition;
        }

        private int FindTableStartFrom(int lineNo)
        {
            while (lineNo < _line.Count && !IsTableLine(_line[lineNo]))
            {
                lineNo++;
            }

            return lineNo;
        }

        private string FullPageName(string pageName) => PageRoot + "." + pageName;

        private bool GetPageContent(string pageName)
        {
            var uri = FullPageName(pageName) + "?pageData";
            _line = RestCall(uri);
            return _line.Count > 0;
        }

        private void InsertTableAt(string tableName, Dictionary<string, string> dictionary, int index)
        {
            Requires.Condition(index >= 0, $"{nameof(index)} >= 0");
            Requires.Condition(index <= _line.Count, $"{nameof(index)} <= line count");

            var table = CreateTable(tableName, dictionary).ToList();

            // make sure there is at least one non-table line between two tables
            if (NeedSpacerAtStart(index)) table.Insert(0, string.Empty);
            if (NeedSpacerAtEnd(index)) table.Add(string.Empty);

            if (index < _line.Count)
            {
                _line.InsertRange(index, table);
            }
            else
            {
                _line.AddRange(table);
            }
        }

        private bool IsAtEnd(int index) => index >= _line.Count;

        private static bool IsTableLine(string line)
        {
            var trimmedLine = line.Trim();
            char[] trimChars = { '!' };
            return !string.IsNullOrEmpty(trimmedLine) && trimmedLine.TrimStart(trimChars)[0] == Pipe;
        }

        public Dictionary<string, string> LoadTableFromPage(string tableName, string page)
        {
            var emptyDictionary = new Dictionary<string, string>();
            if (!GetPageContent(page)) return emptyDictionary;
            var index = FindTableNamed(tableName);
            return IsAtEnd(index) ? emptyDictionary : ExtractTableAt(index);
        }

        private static void Log(IEnumerable<string> data, string title)
        {
            using var file = new StreamWriter("FitNessePage.log", true);
            file.WriteLine("---" + title + "---");
            foreach (var line in data)
            {
                file.WriteLine(line);
            }

            file.Close();
        }

        private bool NeedSpacerAtEnd(int index) => index < _line.Count && IsTableLine(_line[index]);

        private bool NeedSpacerAtStart(int index) => index > 0 && IsTableLine(_line[index - 1]);

        private static IEnumerable<string> ReadLines(Stream stream)
        {
            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line)
            {
                yield return line;
            }
        }

        private void RemoveTableAt(int index)
        {
            while (IsTableLine(_line[index]))
            {
                _line.RemoveAt(index);
            }
        }

        protected virtual List<string> RestCall(string uri)
        {
            var encodedUri = new Uri(uri);


            using var httpClient = new HttpClient();
            try
            {
                var response = httpClient.GetAsync(encodedUri).Result;

                Requires.Condition(response.StatusCode == HttpStatusCode.OK, "Web response status is OK");
                var stream = response.Content.ReadAsStreamAsync().Result;
                Requires.NotNull(stream, nameof(stream));
                var result = ReadLines(stream).ToList();
                return result;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private void SavePage(string pageName)
        {
            //the savePage responder acts a bit weird; it sometimes shows a merge page and I don't know what triggers that
            //var command = FullPageName(pageName) + "?saveData&editTime=1&pageContent=";
            //var uri = _line.Aggregate(command, (current, entry) => current + entry + Linefeed);
            //RestCall(uri);

            // so instead, we just delete the page and create a new one.
            DeletePage(pageName);
            AddPage(pageName, _line);
        }

        public void SaveTableToPage(string tableName, string pageName, Dictionary<string, string> dictionary)
        {
            if (GetPageContent(pageName))
            {
                // we already have a page, so we need to check if we need to overwrite a table or add a new one
                var index = FindTableNamed(tableName);
                if (!IsAtEnd(index)) RemoveTableAt(index);
                InsertTableAt(tableName, dictionary, index);
                SavePage(pageName);
            }
            else
            {
                // page didn't exist, so create a new one with just the table
                AddPage(pageName, CreateTable(tableName, dictionary));
            }
        }

        private static bool TableIsNamed(string line, string tableName)
        {
            var parts = line.Split(Pipe);
            // first and last are always empty. But also other parts could be, so don't remove empty ones
            if (parts.Length != 4 && parts.Length != 6)
                return false;
            if (!parts[1].EqualsIgnoreCase("dictionary"))
                return false;
            if (parts.Length == 4) return parts[2].EqualsIgnoreCase(tableName);
            if (!parts[2].EqualsIgnoreCase("given") && !parts[2].EqualsIgnoreCase("having"))
                return false;
            return parts[3].EqualsIgnoreCase("name") && parts[4].EqualsIgnoreCase(tableName);
        }
    }
}
