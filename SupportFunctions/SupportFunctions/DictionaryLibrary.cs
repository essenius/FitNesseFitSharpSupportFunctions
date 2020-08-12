// Copyright 2015-2020 Rik Essenius
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
    /// <summary>Functions for handling libraries</summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    public class DictionaryLibrary
    {
        private const string DefaultFileName = "DictionaryStore.json";

        internal FitNessePage MyFitNessePage = new FitNessePage();

        /// <summary>Initialize a new DictionaryLibrary with default file name</summary>
        public DictionaryLibrary()
        {
            FileName = DefaultFileName;
            Dictionary = new Dictionary<string, string>();
        }

        /// <summary>Initialize a new DictionaryLibrary from an existing dictionary</summary>
        public DictionaryLibrary(Dictionary<string, string> input) : this() => Dictionary = input;

        /// <summary>the number of key/value pairs in the dictionary</summary>
        public int Count => Dictionary.Count;

        /// <summary>The contents of the dictionary</summary>
        public Dictionary<string, string> Dictionary { get; }

        /// <summary>
        ///     set/get the filename used to load/save dictionaries. Can also be set implicitly via the Load, Save and Wait functions.
        ///     Default name is DictionaryStore.json
        /// </summary>
        public string FileName { get; set; }

        /// <summary>Set/get the page root when using persistence to FitNesse pages</summary>
        public string PageRoot
        {
            get => MyFitNessePage.PageRoot;
            set => MyFitNessePage.PageRoot = value;
        }

        /// <summary>Add a key/value pair to the dictionary. Fails if key exists</summary>
        public void AddValue(string key, string value) => AddValueTo(key, value, Dictionary);

        /// <summary>Add a key/value pair to the specified dictionary. Fails if key exists</summary>
        public static void AddValueTo(string key, string value, Dictionary<string, string> dictionary)
        {
            Debug.Assert(dictionary != null, "dictionary != null");
            dictionary.Add(key, value);
        }

        /// <summary>Delete all key/value pairs in the specified dictionary</summary>
        public static void Clear(Dictionary<string, string> dictionary) => dictionary.Clear();

        /// <summary>Delete all key/value pairs in a dictionary</summary>
        public void Clear() => Clear(Dictionary);

        private static void CopyDictionary(Dictionary<string, string> source, IDictionary<string, string> target)
        {
            target.Clear();
            foreach (var entry in source)
            {
                target.Add(entry.Key, entry.Value);
            }
        }

        /// <summary>Delete a file</summary>
        public bool DeleteFile(string fileName)
        {
            FileName = fileName;
            return DeleteFile();
        }

        /// <summary>Delete the default file (clean up after temporary storage)</summary>
        public bool DeleteFile() => new DictionaryFile(FileName).Delete();

        /// <summary>Delete a FitNesse page under the Page Root</summary>
        public void DeletePage(string pageName) => MyFitNessePage.DeletePage(pageName);

        /// <summary>Remove a data table from a page under the page root</summary>
        public bool DeleteTableFromPage(string tableName, string pageName) => MyFitNessePage.DeleteTableFromPage(tableName, pageName);

        /// <summary>Get the value from a key/value pair in the dictionary</summary>
        public object Get(string key) => GetFrom(key, Dictionary);

        /// <summary>Get the value from a key/value pair in the specified dictionary</summary>
        public static string GetFrom(string key, Dictionary<string, string> dictionary)
        {
            Debug.Assert(dictionary != null, "dictionary != null");
            return !dictionary.ContainsKey(key) ? null : dictionary[key];
        }

        /// <summary>Load the default dictionary file</summary>
        public bool LoadFile()
        {
            CopyDictionary(new DictionaryFile(FileName).Load(), Dictionary);
            return Dictionary.Count > 0;
        }

        /// <summary>Load a specified dictionary file (and set as default)</summary>
        public bool LoadFile(string fileName)
        {
            FileName = fileName;
            return LoadFile();
        }

        /// <summary>
        ///     Load a data table from a page under the page root. The dictionary is cleared before the load, so if the operation fails,
        ///     the dictionary is empty
        /// </summary>
        /// <param name="tableName">the name of the table</param>
        /// <param name="page">the page reference</param>
        /// <returns>whether at least one item of data was loaded</returns>
        public bool LoadTableFromPage(string tableName, string page)
        {
            CopyDictionary(MyFitNessePage.LoadTableFromPage(tableName, page), Dictionary);
            return Dictionary.Count > 0;
        }

        /// <summary>Remove a key/value pair from the dictionary</summary>
        public void Remove(string key) => RemoveFrom(key, Dictionary);

        /// <summary>Remove a key/value pair from the specified dictionary</summary>
        public static void RemoveFrom(string key, Dictionary<string, string> dictionary) => dictionary.Remove(key);

        /// <summary>Save a dictionary to the default file</summary>
        public void SaveFile() => new DictionaryFile(FileName).Save(Dictionary);

        /// <summary>Save a dictionary to the specified file (and set as default)</summary>
        public void SaveFile(string fileName)
        {
            FileName = fileName;
            SaveFile();
        }

        /// <summary>Save a data table onto a page under the page root</summary>
        public void SaveTableToPage(string tableName, string pageName) => MyFitNessePage.SaveTableToPage(tableName, pageName, Dictionary);

        /// <summary>Set the value of a key/value pair in the dictionary</summary>
        public void SetTo(string key, string value) => SetToIn(key, value, Dictionary);

        /// <summary>Set the value of a key/value pair in the specified dictionary</summary>
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

        /// <summary>Set a file as default and wait for it to appear and become unlocked.</summary>
        /// <remarks>This function is useful when you use different processes that need to use the same file.</remarks>
        /// <param name="fileName">path to the file to use</param>
        public bool WaitForFile(string fileName)
        {
            FileName = fileName;
            return WaitForFile();
        }

        /// <summary>Wait for the default file to appear and become unlocked.</summary>
        public bool WaitForFile() => new DictionaryFile(FileName).WaitFor();
    }
}
