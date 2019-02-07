// Copyright 2016-2019 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    [TestClass]
    public class DictionaryFileTest
    {
        private static void AssertDictionariesEqual(IReadOnlyDictionary<string, string> expected,
            IReadOnlyDictionary<string, string> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Counts match");
            foreach (var item in expected.Keys)
            {
                Assert.IsTrue(actual.ContainsKey(item), "Key matches");
                Assert.AreEqual(expected[item], actual[item], "Value matches");
            }
        }

        [SuppressMessage("Microsoft.Usage",
             "CA2202:Do not dispose objects multiple times", Justification = "Bad rule"), TestMethod, TestCategory("Integration")]
        public void DictionaryFileLoadBinaryFile()
        {
            // test whether loading the (obsolete) binary format still works
            // arrange
            const string fileName = "DictionaryStore.bin";
            Assert.IsFalse(new DictionaryFile(fileName).Delete(), "File exists before");
            var dictionary = new Dictionary<string, string>
            {
                {"1", "a"},
                {"2", "b"},
                {"3", "c"}
            };
            using (var fs = File.OpenWrite(fileName))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(dictionary.Count);
                foreach (var pair in dictionary)
                {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }
            }
            // act
            var loadedDictionary = new DictionaryFile(fileName).Load();
            // assert
            AssertDictionariesEqual(dictionary, loadedDictionary);
            Assert.IsTrue(new DictionaryFile(fileName).Delete(), "Delete file after test");
            Assert.IsFalse(File.Exists(fileName), "File does not exist at end");
        }

        [TestMethod, TestCategory("Integration")]
        public void DictionaryFileSaveLoadTest()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"1", "a"},
                {"2", "b"},
                {"3", "c"}
            };
            const string defaultFileName = "DictionaryStore.json";
            new DictionaryFile(defaultFileName).Delete();
            Assert.IsFalse(File.Exists(defaultFileName), "File exists before saving");
            new DictionaryFile(defaultFileName).Save(dictionary);
            Assert.IsTrue(File.Exists(defaultFileName), "File exists after saving");
            var loadedDictionary = new DictionaryFile(defaultFileName).Load();
            AssertDictionariesEqual(dictionary, loadedDictionary);

            new DictionaryFile(defaultFileName).Save(new Dictionary<string, string>());
            Assert.IsFalse(File.Exists(defaultFileName), "File does not exist after saving empty dictionary");
        }

        [TestMethod, TestCategory("Integration")]
        public void DictionaryFileWaitForFileTest()
        {
            const string testFile = "test.txt";
            new DictionaryFile(testFile).Delete();
            var waiter = new DictionaryFile(testFile) {TimeoutSeconds = 0.2};
            var task = new Task(() =>
            {
                Thread.Sleep(100);
                File.WriteAllText(testFile, new string('a', 500000));
            });
            task.Start();
            Assert.IsTrue(waiter.WaitFor(), "Wait for delayed write works");
            task.Wait();
            Assert.IsTrue(waiter.WaitFor(), "Wait for existing file works");
            File.Delete(testFile);
        }

        [TestMethod, TestCategory("Integration")]
        public void DictionaryFileWaitForLockedFileTest()
        {
            const string fileName = "test.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            var file = File.Create(fileName);
            var waiter = new DictionaryFile(fileName) {TimeoutSeconds = 0.2};
            Assert.IsFalse(waiter.WaitFor());
            file.Close();
            File.Delete(fileName);
        }
    }
}