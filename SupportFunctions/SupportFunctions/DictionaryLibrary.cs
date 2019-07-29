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
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"), 
     Documentation("Functions for handling libraries")]
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

        [Documentation("the number of key/value pairs in the dictionary")]
        public int Count => Dictionary.Count;

        [Documentation("The contents of the dictionary")]
        public Dictionary<string, string> Dictionary { get; }

        [Documentation("set/get the filename used to load/save dictionaries. Can also be set implicitly via the Load, Save and Wait functions. " +
                       "Default name is DictionaryStore.json")]
        public string FileName { get; set; }

        [Documentation("Set/get the page root when using persistence to FitNesse pages")]
        public string PageRoot
        {
            get => MyFitNessePage.PageRoot;
            set => MyFitNessePage.PageRoot = value;
        }

        [Documentation("Add a key/value pair to the dictionary. Fails if key exists")]
        public void AddValue(string key, string value) => AddValueTo(key, value, Dictionary);

        [Documentation("Add a key/value pair to the specified dictionary. Fails if key exists")]
        public static void AddValueTo(string key, string value, Dictionary<string, string> dictionary)
        {
            Debug.Assert(dictionary != null, "dictionary != null");
            dictionary.Add(key, value);
        }

        [Documentation("Delete all key/value pairs in the specified dictionary")]
        public static void Clear(Dictionary<string, string> dictionary) => dictionary.Clear();

        [Documentation("Delete all key/value pairs in a dictionary")]
        public void Clear() => Clear(Dictionary);

        private static void CopyDictionary(Dictionary<string, string> source, IDictionary<string, string> target)
        {
            target.Clear();
            foreach (var entry in source)
            {
                target.Add(entry.Key, entry.Value);
            }
        }

        [Documentation("Delete a file")]
        public bool DeleteFile(string fileName)
        {
            FileName = fileName;
            return DeleteFile();
        }

        [Documentation("Delete the default file (clean up after temporary storage)")]
        public bool DeleteFile() => new DictionaryFile(FileName).Delete();

        [Documentation("Delete a FitNesse page under the Page Root")]
        public void DeletePage(string pageName) => MyFitNessePage.DeletePage(pageName);

        [Documentation("Remove a data table from a page under the page root")]
        public bool DeleteTableFromPage(string tableName, string pageName) => MyFitNessePage.DeleteTableFromPage(tableName, pageName);

        [Documentation("Get the value from a key/value pair in the dictionary")]
        public object Get(string key) => GetFrom(key, Dictionary);

        [Documentation("Get the value from a key/value pair in the specified dictionary")]
        public static string GetFrom(string key, Dictionary<string, string> dictionary)
        {
            Debug.Assert(dictionary != null, "dictionary != null");
            return !dictionary.ContainsKey(key) ? null : dictionary[key];
        }

        [Documentation("Load the default dictionary file")]
        public bool LoadFile()
        {
            CopyDictionary(new DictionaryFile(FileName).Load(), Dictionary);
            return Dictionary.Count > 0;
        }

        [Documentation("Load a specified dictionary file (and set as default)")]
        public bool LoadFile(string fileName)
        {
            FileName = fileName;
            return LoadFile();
        }

        [Documentation("Load a data table from a page under the page root. The dictionary is cleared before the load, " +
                       "so if the operation fails, the dictionary is empty")]
        public bool LoadTableFromPage(string tableName, string page)
        {
            CopyDictionary(MyFitNessePage.LoadTableFromPage(tableName, page), Dictionary);
            return Dictionary.Count > 0;
        }

        [Documentation("Remove a key/value pair from the dictionary")]
        public void Remove(string key) => RemoveFrom(key, Dictionary);

        [Documentation("Remove a key/value pair from the specified dictionary")]
        public static void RemoveFrom(string key, Dictionary<string, string> dictionary) => dictionary.Remove(key);

        [Documentation("Save a dictionary to the default file")]
        public void SaveFile() => new DictionaryFile(FileName).Save(Dictionary);

        [Documentation("Save a dictionary to the specified file (and set as default)")]
        public void SaveFile(string fileName)
        {
            FileName = fileName;
            SaveFile();
        }

        [Documentation("Save a data table onto a page under the page root")]
        public void SaveTableToPage(string tableName, string pageName) => MyFitNessePage.SaveTableToPage(tableName, pageName, Dictionary);

        [Documentation("Set the value of a key/value pair in the dictionary")]
        public void SetTo(string key, string value) => SetToIn(key, value, Dictionary);

        [Documentation("Set the value of a key/value pair in the specified dictionary")]
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

        [Documentation("Set a file as default and wait for it to appear and become unlocked. " +
                       "This function is useful when you use different processes that need to use the same file")]
        public bool WaitForFile(string fileName)
        {
            FileName = fileName;
            return WaitForFile();
        }

        [Documentation("Wait for the default file to appear and become unlocked.")]
        public bool WaitForFile() => new DictionaryFile(FileName).WaitFor();
    }
}