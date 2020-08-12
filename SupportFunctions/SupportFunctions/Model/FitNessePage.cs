// Copyright 2016-2020 Rik Essenius
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class FitNessePage
    {
        private const string DefaultPageRoot = "http://localhost:8080/DataStore";
        private const string Linefeed = "\n";
        private const char Pipe = '|';

        private List<string> _line;

        public FitNessePage()
        {
            _line = new List<string>();
            PageRoot = DefaultPageRoot;
        }

        public string PageRoot { get; set; }

        private void AddPage(string pageName, IEnumerable<string> lines)
        {
            var command = PageRoot + "?addChild&pageName=" + pageName + "&pageContent=";
            var payload = lines.Aggregate(string.Empty, (current, entry) => current + entry + Linefeed);
            var _ = RestCall(command + Uri.EscapeDataString(payload));
        }

        private static void CheckColumnNames(string line)
        {
            var columnNames = ExtractKeyValuePair(line);
            if (!columnNames.Key.EqualsIgnoreCase("key"))
            {
                throw new FormatException("First column must be called 'Key' instead of '" + columnNames.Key + "'");
            }
            if (!columnNames.Value.EqualsIgnoreCase("value"))
            {
                throw new FormatException("Second column must be called 'Value' instead of '" + columnNames.Value + "'");
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
                throw new FormatException("Row should have 2 cells: Key and Value. Found " + (part.Length - 2) + ": " + line);
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
                if (IsAtEnd(tableStartLineNo) || TableIsNamed(_line[tableStartLineNo], tableName)) return tableStartLineNo;
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
            Debug.Assert(index >= 0);
            Debug.Assert(index <= _line.Count);

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
            char[] trimChars = {'!'};
            return !string.IsNullOrEmpty(trimmedLine) && trimmedLine.TrimStart(trimChars)[0] == Pipe;
        }

        public Dictionary<string, string> LoadTableFromPage(string tableName, string page)
        {
            var emptyDictionary = new Dictionary<string, string>();
            if (!GetPageContent(page)) return emptyDictionary;
            var index = FindTableNamed(tableName);
            return IsAtEnd(index) ? emptyDictionary : ExtractTableAt(index);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static void Log(IEnumerable<string> data, string title)
        {
            using (var file = new StreamWriter("FitNessePage.log", true))
            {
                file.WriteLine("---" + title + "---");
                foreach (var line in data)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }
        }

        private bool NeedSpacerAtEnd(int index) => index < _line.Count && IsTableLine(_line[index]);

        private bool NeedSpacerAtStart(int index) => index > 0 && IsTableLine(_line[index - 1]);

        private static IEnumerable<string> ReadLines(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
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
            var webRequest = (HttpWebRequest) WebRequest.Create(encodedUri);
            webRequest.Method = "GET";
            try
            {
                var webResponse = (HttpWebResponse) webRequest.GetResponse();
                Debug.Assert(webResponse.StatusCode == HttpStatusCode.OK);
                var stream = webResponse.GetResponseStream();
                Debug.Assert(stream != null, "stream != null");
                var result = ReadLines(stream).ToList();
                webResponse.Close();
                return result;
            }
            catch (WebException)
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
            if (parts.Length != 4 && parts.Length != 6) return false;
            if (!parts[1].EqualsIgnoreCase("dictionary")) return false;
            if (parts.Length == 4) return parts[2].EqualsIgnoreCase(tableName);
            if (!parts[2].EqualsIgnoreCase("given") && !parts[2].EqualsIgnoreCase("having")) return false;
            return parts[3].EqualsIgnoreCase("name") && parts[4].EqualsIgnoreCase(tableName);
        }
    }
}