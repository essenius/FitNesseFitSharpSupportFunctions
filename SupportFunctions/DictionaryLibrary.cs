// Copyright 2015-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SupportFunctions.Model;

namespace SupportFunctions
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    public class DictionaryLibrary
    {
        private const string DefaultFileName = "DictionaryStore.json";

        internal FitNessePage MyFitNessePage = new FitNessePage();

        public DictionaryLibrary()
        {
            FileName = DefaultFileName;
            Dictionary = new Dictionary<string, string>();
        }

        public DictionaryLibrary(Dictionary<string, string> input) : this() => Dictionary = input;

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Functions for handling libraries"},
            {nameof(AddValue), "Add a key/value pair to the dictionary. Fails if key exists"},
            {nameof(AddValueTo), "Add a key/value pair to the specified dictionary. Fails if key exists"},
            {nameof(Clear), "Delete all key/value pairs in a dictionary"},
            {nameof(Clear) + "`1", "Delete all key/value pairs in the specified dictionary"},
            {nameof(Count), "the number of key/value pairs in the dictionary"},
            {nameof(Dictionary), "The contents of the dictionary"},
            {nameof(DeleteFile), "Delete the default file (clean up after temporary storage)"},
            {nameof(DeleteFile) + "`1", "Delete a file"},
            {nameof(DeletePage), "Delete a FitNesse page under the Page Root"},
            {nameof(DeleteTableFromPage), "Remove a data table from a page under the page root"},
            {
                nameof(FileName),
                "set/get the filename used to load/save dictionaries. Can also be set implicitly via the " +
                "Load, Save and Wait functions. Default name is DictionaryStore.json"
            },
            {nameof(Get), "Get the value from a key/value pair in the dictionary"},
            {nameof(GetFrom), "Get the value from a key/value pair in the specified dictionary"},
            {nameof(LoadFile), "Load the default dictionary file"},
            {nameof(LoadFile) + "`1", "Load a specified dictionary file (and set as default)"},
            {
                nameof(LoadTableFromPage),
                "Load a data table from a page under the page root. The dictionary is cleared before the load, so if the operation fails, the dictionary is empty"
            },
            {nameof(PageRoot), "Set/get the page root when using persistence to FitNesse pages"},
            {nameof(Remove), "Remove a key/value pair from the dictionary"},
            {nameof(RemoveFrom), "Remove a key/value pair from the specified dictionary"},
            {nameof(SaveFile) + "`1", "Save a dictionary to the specified file (and set as default)"},
            {nameof(SaveFile), "Save a dictionary to the default file"},
            {nameof(SaveTableToPage), "Save a data table onto a page under the page root"},
            {nameof(SetTo), "Set the value of a key/value pair in the dictionary"},
            {nameof(SetToIn), "Set the value of a key/value pair in the specified dictionary"},
            {
                nameof(WaitForFile) + "`1",
                "Set a file as default and Wait for it to appear and become unlocked. This function is useful when you use different processes that need to use the same file"
            },
            {nameof(WaitForFile), "Wait for the default file to appear and become unlocked. "}
        };

        public int Count => Dictionary.Count;

        public Dictionary<string, string> Dictionary { get; }

        public string FileName { get; set; }

        public string PageRoot
        {
            get => MyFitNessePage.PageRoot;
            set => MyFitNessePage.PageRoot = value;
        }

        public static void AddValueTo(string key, string value, Dictionary<string, string> dictionary)
        {
            Debug.Assert(dictionary != null, "dictionary != null");
            dictionary.Add(key, value);
        }

        public static void Clear(Dictionary<string, string> dictionary) => dictionary.Clear();

        private static void CopyDictionary(Dictionary<string, string> source, IDictionary<string, string> target)
        {
            target.Clear();
            foreach (var entry in source)
            {
                target.Add(entry.Key, entry.Value);
            }
        }

        public static string GetFrom(string key, Dictionary<string, string> dictionary)
        {
            Debug.Assert(dictionary != null, "dictionary != null");
            return !dictionary.ContainsKey(key) ? null : dictionary[key];
        }

        public static void RemoveFrom(string key, Dictionary<string, string> dictionary) => dictionary.Remove(key);

        public static void SetToIn(string key, string value, Dictionary<string, string> dictionary)
        {
            Debug.Assert(dictionary != null, "dictionary != null");
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                AddValueTo(key, value, dictionary);
            }
        }

        public void AddValue(string key, string value) => AddValueTo(key, value, Dictionary);

        public void Clear() => Clear(Dictionary);

        public bool DeleteFile(string fileName)
        {
            FileName = fileName;
            return DeleteFile();
        }

        public bool DeleteFile() => new DictionaryFile(FileName).Delete();

        public void DeletePage(string pageName) => MyFitNessePage.DeletePage(pageName);

        public bool DeleteTableFromPage(string tableName, string pageName) => MyFitNessePage.DeleteTableFromPage(tableName, pageName);

        public object Get(string key) => GetFrom(key, Dictionary);

        public bool LoadFile()
        {
            CopyDictionary(new DictionaryFile(FileName).Load(), Dictionary);
            return Dictionary.Count > 0;
        }

        public bool LoadFile(string fileName)
        {
            FileName = fileName;
            return LoadFile();
        }

        public bool LoadTableFromPage(string tableName, string page)
        {
            CopyDictionary(MyFitNessePage.LoadTableFromPage(tableName, page), Dictionary);
            return Dictionary.Count > 0;
        }

        public void Remove(string key) => RemoveFrom(key, Dictionary);

        public void SaveFile() => new DictionaryFile(FileName).Save(Dictionary);

        public void SaveFile(string fileName)
        {
            FileName = fileName;
            SaveFile();
        }

        public void SaveTableToPage(string tableName, string pageName) => MyFitNessePage.SaveTableToPage(tableName, pageName, Dictionary);

        public void SetTo(string key, string value) => SetToIn(key, value, Dictionary);

        public bool WaitForFile(string fileName)
        {
            FileName = fileName;
            return WaitForFile();
        }

        public bool WaitForFile() => new DictionaryFile(FileName).WaitFor();
    }
}