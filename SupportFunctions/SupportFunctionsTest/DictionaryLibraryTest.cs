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

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class DictionaryLibraryTest
    {
        private const string AddChildPartialRequest = TestPageRoot + "?addChild&pageName=" + PageName + "&pageContent=";
        private const string DeletePageRequest = TestPage + "?deletePage&confirmed=yes" + FitNessePageTest.UriSeparator;
        private const string PageDataRequest = TestPage + "?pageData" + FitNessePageTest.UriSeparator;

        private const string PageName = "PageName";

        //private const string SaveDataPartialRequest = TestPage + "?saveData&editTime=1&pageContent=";
        private const string TestPage = TestPageRoot + "." + PageName;

        private const string TestPageRoot = "http://localhost:8080/TestPage";

        private static string DictionaryHeader(string tableName) => "!|Dictionary|Having|Name|" + tableName + "|\n|Key|Value|\n";

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibraryConstructorTest()
        {
            var simpleList = new Dictionary<string, string>
            {
                {"1", "a"},
                {"2", "b"},
                {"3", "c"}
            };

            var lib = new DictionaryLibrary(simpleList);
            Assert.AreEqual(3, lib.Count);
            Assert.AreEqual("a", lib.Get("1"));
        }

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibraryDeletePageTest()
        {
            var lib = new DictionaryLibrary
            {
                MyFitNessePage = new FitNessePageMock(""),
                PageRoot = TestPageRoot
            };
            Assert.AreEqual(TestPageRoot, lib.PageRoot);
            lib.DeletePage(PageName);
            Assert.AreEqual(DeletePageRequest, (lib.MyFitNessePage as FitNessePageMock)?.UsedUri);
        }

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibraryDeleteTableFromPageTest()
        {
            var lib = new DictionaryLibrary
            {
                MyFitNessePage = new FitNessePageMock(string.Empty),
                PageRoot = TestPageRoot
            };
            Assert.IsFalse(lib.DeleteTableFromPage("TableName", PageName));
        }

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibraryGetFromTest()
        {
            var simpleList = new Dictionary<string, string>
            {
                {"1", "a"},
                {"2", "b"},
                {"3", "c"}
            };
            Assert.AreEqual("c", DictionaryLibrary.GetFrom("3", simpleList));
            Assert.IsNull(DictionaryLibrary.GetFrom("4", simpleList));
            Assert.AreEqual(simpleList, CommonFunctions.Echo(simpleList));
        }

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibraryLoadEmptyPageTest()
        {
            var lib = new DictionaryLibrary
            {
                MyFitNessePage = new FitNessePageMock(""),
                PageRoot = TestPageRoot
            };

            Assert.IsFalse(lib.LoadTableFromPage("Main", "EmptyPage"));
            Assert.AreEqual(0, lib.Count);
        }

        [TestMethod, TestCategory("Integration")]
        public void DictionaryLibraryLoadSave()
        {
            const string defaultFileName = "DictionaryStore.json";
            Assert.IsFalse(File.Exists(defaultFileName), "File does not exist at start");
            var saver = new DictionaryLibrary();
            Assert.AreEqual(0, saver.Count);
            saver.AddValue("key2", "value 2");
            saver.AddValue("key1", "value 1");
            saver.AddValue("key3", "value 3");
            Assert.AreEqual(3, saver.Count);
            Assert.IsFalse(File.Exists(defaultFileName), "File does not exist before saving");
            Assert.IsFalse(saver.DeleteFile(defaultFileName));
            saver.FileName = string.Empty;
            saver.SaveFile(defaultFileName);
            Assert.AreEqual(defaultFileName, saver.FileName, "SaveFile sets file name");
            Assert.IsTrue(File.Exists(defaultFileName), "File exists after saving");

            var loader = new DictionaryLibrary {FileName = string.Empty};
            Assert.AreEqual(0, loader.Count, "Loader is empty before loading");
            Assert.IsTrue(loader.LoadFile(defaultFileName), "Loading after saving succeeds");
            Assert.AreEqual(defaultFileName, loader.FileName, "LoadFile sets file name");
            Assert.AreEqual(3, loader.Count);
            Assert.AreEqual("value 1", loader.Get("key1"));
            Assert.AreEqual("value 2", loader.Get("key2"));
            Assert.AreEqual("value 3", loader.Get("key3"));
            loader.AddValue("key4", "value 4");
            loader.Remove("key3");
            loader.SaveFile();
            loader.Clear();
            Assert.IsTrue(loader.LoadFile(), "Can load");
            Assert.AreEqual("value 4", loader.Get("key4"));
            Assert.IsNull(loader.Get("key3"), "Key 3 no longer there");

            Assert.IsTrue(loader.DeleteFile(), "File deleted");
            Assert.IsFalse(File.Exists(defaultFileName), "File does not exist after deleting");
            try
            {
                loader.LoadFile();
            }
            catch (Exception)
            {
                Assert.AreEqual(3, loader.Count, "original entries stay intact after failed load");
                return;
            }
            Assert.Fail("No exception thrown after loading deleted file");
        }

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibraryNewPageTest()
        {
            const string tableName = "TestTable";
            var lib = new DictionaryLibrary
            {
                MyFitNessePage = new FitNessePageMock(null),
                PageRoot = TestPageRoot
            };
            lib.AddValue("key", "value");
            lib.SaveTableToPage(tableName, PageName);
            var expected = PageDataRequest + AddChildPartialRequest +
                           DictionaryHeader(tableName) + "|key|value|\n" + FitNessePageTest.UriSeparator;
            Assert.AreEqual(expected, (lib.MyFitNessePage as FitNessePageMock)?.UsedUri);
        }

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibrarySaveToPageTest()
        {
            const string tableName = "replace";

            const string pageContent =
                "comments\n\n" +
                "!| Dictionary | not ok |\n| Key | Value |\n| key1 | value 1 |\n| key2 | value 2 |\n| key3 | value 3 |\n\nmore comments\n\n" +
                "!| Dictionary | replace |\n| Key | Value |\n| key4 | value 4 |\n| key5 | value 5 |\n| key6 | value 6 |\n\n yet more comments\n\n" +
                "!| Dictionary | not ok 2 |\n| Key | Value |\n| 7 | 8 |\n\nand even more comments\n";

            var saver = new DictionaryLibrary
            {
                MyFitNessePage = new FitNessePageMock(pageContent),
                PageRoot = TestPageRoot
            };
            saver.AddValue("key9", "value 9");
            saver.AddValue("key10", "value 10");
            saver.AddValue("key11", "value 11");
            saver.SaveTableToPage(tableName, PageName);
            var expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest +
                           "comments\n\n" +
                           "!| Dictionary | not ok |\n| Key | Value |\n| key1 | value 1 |\n| key2 | value 2 |\n| key3 | value 3 |\n\nmore comments\n\n" +
                           DictionaryHeader(tableName) + "|key9|value 9|\n|key10|value 10|\n|key11|value 11|\n\n yet more comments\n\n" +
                           "!| Dictionary | not ok 2 |\n| Key | Value |\n| 7 | 8 |\n\nand even more comments\n" + FitNessePageTest.UriSeparator;
            Assert.AreEqual(expected, (saver.MyFitNessePage as FitNessePageMock)?.UsedUri);
        }

        [TestMethod, TestCategory("Unit")]
        public void DictionaryLibrarySetToInTest()
        {
            var lib = new DictionaryLibrary
            {
                MyFitNessePage = new FitNessePageMock(string.Empty),
                PageRoot = TestPageRoot
            };
            lib.SetTo("key1", "value1");
            Assert.AreEqual("value1", lib.Get("key1"));
            lib.SetTo("key1", "value2");
            Assert.AreEqual("value2", lib.Get("key1"));
        }

        [TestMethod, TestCategory("Integration")]
        public void DictionaryLibraryWaitForFileTest()
        {
            const string fileName = "test.txt";
            var lib = new DictionaryLibrary();
            if (File.Exists(fileName)) File.Delete(fileName);
            Assert.IsFalse(lib.WaitForFile(fileName));
            File.Create(fileName).Close();
            Assert.IsTrue(lib.WaitForFile(fileName));
            File.Delete(fileName);
        }
    }
}