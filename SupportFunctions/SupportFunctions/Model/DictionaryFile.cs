// Copyright 2015-2024 Rik Essenius
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
using System.Threading;
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class DictionaryFile
    {
        private readonly string _fileName;
        private bool _fileExists;
        private string _fileToWaitFor;

        public DictionaryFile(string fileName) => _fileName = fileName;

        public double TimeoutSeconds { get; set; } = 1.0;

        public bool Delete()
        {
            if (!File.Exists(_fileName)) return false;
            File.Delete(_fileName);
            return !File.Exists(_fileName);
        }

        private static bool IsFileLocked(string file)
        {
            try
            {
                using (File.OpenRead(file))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        public Dictionary<string, string> Load()
        {
            var dictionary = new Dictionary<string, string>();

            var input = File.ReadAllText(_fileName);

            try
            {
                dictionary = DictionarySerializer.Deserialize(input);
                return dictionary;
            }
            catch (Exception e) when (e.GetType() == DictionarySerializer.ParseExceptionType)
            {
                // unable to parse as json, retry with binary
                using var fs = File.OpenRead(_fileName);
                using var reader = new BinaryReader(fs);
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadString();
                    dictionary[key] = value;
                }

                return dictionary;
            }
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            _fileExists = e.Name == _fileToWaitFor;
        }

        public void Save(Dictionary<string, string> dictionary)
        {
            if (dictionary.Count == 0)
            {
                Delete();
            }
            else
            {
                File.WriteAllText(_fileName, DictionarySerializer.Serialize(dictionary));
            }
        }

        public bool WaitFor()
        {
            var fileExists = File.Exists(_fileName);
            if (!fileExists)
            {
                // Wait for the file to appear
                var path = Path.GetDirectoryName(Path.GetFullPath(_fileName));
                Requires.NotNull(path, nameof(path));
                _fileToWaitFor = Path.GetFileName(_fileName);
                using var fileSystemWatcher = new FileSystemWatcher(path!);
                fileSystemWatcher.Created += OnCreated;
                fileSystemWatcher.EnableRaisingEvents = true;
                fileExists = WaitUntil(() => _fileExists);
            }

            // if it is there, wait until it is not locked.
            return fileExists && WaitUntil(() => !IsFileLocked(_fileName));
        }

        private bool WaitUntil(Func<bool> expression)
        {
            const int checksPerSecond = 10;
            const int sleepTime = 1000 / checksPerSecond;
            var timeout = Convert.ToInt32(checksPerSecond * TimeoutSeconds);
            var waitedTimes = 0;
            while (!expression() && waitedTimes < timeout)
            {
                waitedTimes++;
                Thread.Sleep(sleepTime);
            }

            return expression();
        }
    }
}
