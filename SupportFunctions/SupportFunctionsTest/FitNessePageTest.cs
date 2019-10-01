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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    [TestClass]
    public class FitNessePageTest
    {
        private const string AddChildPartialRequest = Server + "DataStore?addChild&pageName=PageName&pageContent=";
        private const string DeletePageRequest = Server + "DataStore.PageName?deletePage&confirmed=yes" + UriSeparator;
        private const string PageDataRequest = Server + "DataStore.PageName?pageData" + UriSeparator;

        //private const string SaveDataPartialRequest = Server + "DataStore.PageName?saveData&editTime=1&pageContent=";
        private const string Server = "http://localhost:8080/";

        public const string UriSeparator = "\f";

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageAddTest()
        {
            // The first entry ensures that characters that need to be escaped are handled correctly
            // the hash is a notorious one - if you leave that unescaped, FitNesse will ignore the rest of the page.
            var lines = new[] {"#escape characters<grin>&", "!|Dictionary|TableName|", "|key1|value1|"};
            var fitnessePage = new FitNessePageMock(null);
            var target = new PrivateObject(fitnessePage, new PrivateType(typeof(FitNessePage)));
            target.Invoke("AddPage", "PageName", lines);
            const string expected = AddChildPartialRequest + "#escape characters<grin>&\n!|Dictionary|TableName|\n|key1|value1|\n\f";
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(FormatException), "First column must be called 'Key' instead of 'Sleutel'")
        ]
        public void FitNessePageCheckColumnNamesWithWrongFirstColumnNameTrowsFormatException()
        {
            var target = new PrivateType(typeof(FitNessePage));
            target.InvokeStatic("CheckColumnNames", "|Sleutel|Waarde|");
        }

        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(FormatException), "Second column must be called 'Value' instead of 'Waarde'")]
        public void FitNessePageCheckColumnNamesWithWrongSecondColumnNameTrowsFormatException()
        {
            var target = new PrivateType(typeof(FitNessePage));
            target.InvokeStatic("CheckColumnNames", "|key|Waarde|");
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageCreateTableTest()
        {
            var dict = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}};
            var target = new PrivateType(typeof(FitNessePage));
            var result = ((List<string>)target.InvokeStatic("CreateTable", "TestTable", dict)).ToArray();
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("!|Dictionary|Having|Name|TestTable|", result[0]);
            Assert.AreEqual("|Key|Value|", result[1]);
            Assert.AreEqual("|key1|value1|", result[2]);
            Assert.AreEqual("|key2|value2|", result[3]);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageDeleteTableFromEmptyPageTest()
        {
            var fitnessePage = new FitNessePageMock(null);
            Assert.IsFalse(fitnessePage.DeleteTableFromPage("TableName", "PageName"));
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageDeleteTableFromPageTest()
        {
            const string pageContent = "\nq\n|Dictionary|TableName|\n|key|value|\n|key1|value1|\nq\n";
            var fitnessePage = new FitNessePageMock(pageContent);
            Assert.IsTrue(fitnessePage.DeleteTableFromPage("TableName", "PageName"));
            const string expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest + "\nq\nq\n" + UriSeparator;
            Assert.AreEqual(expected, fitnessePage.UsedUri);
            Assert.IsFalse(fitnessePage.DeleteTableFromPage("WrongTableName", "PageName"),
                "Non-existing table returns false");
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageDeleteTest()
        {
            var fitnessePage = new FitNessePageMock(null);
            var target = new PrivateObject(fitnessePage, new PrivateType(typeof(FitNessePage)));
            target.Invoke("DeletePage", "PageName");
            Assert.AreEqual(DeletePageRequest, fitnessePage.UsedUri);
        }

        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(FormatException), "Row should have 2 cells: Key and Value. Found 1: ||")]
        public void FitNessePageExtractKeyValuePairFailsTest()
        {
            var target = new PrivateType(typeof(FitNessePage));
            var _ = (KeyValuePair<string, string>)target.InvokeStatic("ExtractKeyValuePair", "||");
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageExtractKeyValuePairTest()
        {
            var target = new PrivateType(typeof(FitNessePage));
            var kvp = (KeyValuePair<string, string>)target.InvokeStatic("ExtractKeyValuePair", "|Sleutel|Waarde|");
            Assert.AreEqual("Sleutel", kvp.Key);
            Assert.AreEqual("Waarde", kvp.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageInsertTableAtTests()
        {
            var testcases = new[]
            {
                new object[]
                {
                    "AtFirstTableOnPage",
                    "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n",
                    1,
                    "\n!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\nq\n" +
                    "|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n" + UriSeparator
                },
                new object[]
                {
                    "AtEndOfPageEndingWithTable",
                    "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\n",
                    5,
                    "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\n" +
                    "\n!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" +
                    UriSeparator
                },
                new object[]
                {
                    "AtEndOfPageEndingWithWhitespace",
                    "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n",
                    6,
                    "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n" +
                    "!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" + UriSeparator
                },
                new object[]
                {
                    "AtEmptyPage",
                    null,
                    0,
                    "!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" + UriSeparator
                },
                new object[]
                {
                    "AtStartOfPageWithOneTable",
                    "!|Dictionary|TableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n",
                    0,
                    "!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n\n" +
                    "!|Dictionary|TableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" + UriSeparator
                }
            };
            foreach (var testcase in testcases)
            {
                var dict = new Dictionary<string, string> {{"key5", "value5"}, {"key6", "value6"}};
                var fitnessePage = new FitNessePageMock(testcase[1]?.ToString());
                fitnessePage.LoadTableFromPage("TableName", "PageName");
                var target = new PrivateObject(fitnessePage, new PrivateType(typeof(FitNessePage)));
                target.Invoke("InsertTableAt", "NewTableName", dict, testcase[2]);
                target.Invoke("SavePage", "PageName");
                var expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest + testcase[3];
                Assert.AreEqual(expected, fitnessePage.UsedUri, "Testcase " + testcase[0]);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageIsTableLineTest()
        {
            var target = new PrivateType(typeof(FitNessePage));
            Assert.IsTrue((bool)target.InvokeStatic("IsTableLine", "|Sleutel|Waarde|"), "table line without whitespace");
            Assert.IsTrue((bool)target.InvokeStatic("IsTableLine", " |Sleutel|Waarde| "), "table line with whitespace");
            Assert.IsFalse((bool)target.InvokeStatic("IsTableLine", "  "), "just whitespace");
            Assert.IsFalse((bool)target.InvokeStatic("IsTableLine", "comments"), "comments");
            Assert.IsFalse((bool)target.InvokeStatic("IsTableLine", "  comments"), "comments with whitespace");
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageLoadTableFromPageNegativeTestCases()
        {
            var testcases = new[]
            {
                new[] {"NullPage", null},
                new[] {"EmptyPage", string.Empty},
                new[] {"PageWithOneLine", "\n"},
                new[] {"PageWithTableWithDifferentName", "|Dictionary|Main|\n|key|value|\n"}
            };
            foreach (var testcase in testcases)
            {
                var fitnessePage = new FitNessePageMock(testcase[1]);
                var dict = fitnessePage.LoadTableFromPage("tableName", testcase[0]);
                Assert.AreEqual(dict.Count, 0, testcase[0]);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageLoadTableFromPageTestsWithOneResult()
        {
            var testcases = new[]
            {
                new[] {"PageWithSimpleTable", "|Dictionary|Main|\n|key|value|\n|key|value|\n"},
                new[] {"PageWithTableAndComments", "c\nc\n|Dictionary|Main|\n|key|value|\n|key|value|\nc\n"},
                new[]
                {
                    "PageWithMoreTables", "comment1\ncomment 2\n" +
                                          "|Dictionary|WrongName|\n|key|value|\n|key1|value1|\n\n" +
                                          "|Dictionary|Main|\n|key|value|\n|key|value|\ncomment 5\n" +
                                          "|Dictionary|WrongName2|\n|key|value|\n|key2|value2|\n"
                }
            };

            foreach (var testcase in testcases)
            {
                var fitnessePage = new FitNessePageMock(testcase[1]);
                var dict = fitnessePage.LoadTableFromPage("Main", testcase[0]);
                Assert.AreEqual(1, dict.Count, "Entry Count for testcase " + testcase[0]);
                Assert.AreEqual("value", dict["key"], "Entry for testcase " + testcase[0]);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageRemoveTableAtTest()
        {
            const string pageContent = "\nq\n|Dictionary|TableName|\n|key|value|\n|key1|value1|\nq\n";
            var fitnessePage = new FitNessePageMock(pageContent);
            fitnessePage.LoadTableFromPage("TableName", "PageName");
            var target = new PrivateObject(fitnessePage, new PrivateType(typeof(FitNessePage)));
            target.Invoke("RemoveTableAt", 2);
            target.Invoke("SavePage", "PageName");
            const string expected =
                PageDataRequest + DeletePageRequest + AddChildPartialRequest + "\nq\nq\n" + UriSeparator;
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageRemoveTableWithWhitespaceAtTest()
        {
            const string pageContent = "\nq\n|Dictionary|TableName|\n|key|value|\n|key1|value1|\nq\n";
            var fitnessePage = new FitNessePageMock(pageContent);
            fitnessePage.LoadTableFromPage("TableName", "PageName");
            var target = new PrivateObject(fitnessePage, new PrivateType(typeof(FitNessePage)));
            target.Invoke("RemoveTableAt", 2);
            target.Invoke("SavePage", "PageName");
            const string expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest + "\nq\nq\n" + UriSeparator;
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [TestMethod, TestCategory("Integration")]
        public void FitNessePageRestCallInvalidTest()
        {
            //requires FitNesse active locally on port 8080
            var fitnessePage = new FitNessePage();
            var target = new PrivateObject(fitnessePage);
            var stream = target.Invoke("RestCall", Server + "NonExistingPage?pageData");
            Assert.IsNull(stream);
        }

        [TestMethod, TestCategory("Integration")]
        public void FitNessePageRestCallValidTest()
        {
            //requires FitNesse active locally on port 8080
            var fitnessePage = new FitNessePage();
            var target = new PrivateObject(fitnessePage);
            var stream = target.Invoke("RestCall", Server + "?pageData");
            Assert.IsNotNull(stream);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageSavePageTest()
        {
            const string pageContent = "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n";
            var fitnessePage = new FitNessePageMock(pageContent);
            fitnessePage.LoadTableFromPage("TableName", "PageName");
            var target = new PrivateObject(fitnessePage, new PrivateType(typeof(FitNessePage)));
            target.Invoke("SavePage", "PageName");
            const string expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest +
                                    "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n" + UriSeparator;
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageSaveTableToPageTests()
        {
            var testcases = new[]
            {
                new[]
                {
                    "AtEnd",
                    "\nq\n|Dictionary|ExistingTableName|\n|key|value|\n|key1|value1|\n\nq\n",
                    "\nq\n|Dictionary|ExistingTableName|\n|key|value|\n|key1|value1|\n\nq\n" +
                    "!|Dictionary|Having|Name|TableName|\n|Key|Value|\n|key2|value2|\n|key3|value3|\n"
                },
                new[]
                {
                    "Replace",
                    "\nq\n|Dictionary|TableName|\n|key|value|key1|value1|\n\nq\n",
                    "\nq\n!|Dictionary|Having|Name|TableName|\n|Key|Value|\n|key2|value2|\n|key3|value3|\n\nq\n"
                },
                new[]
                {
                    "Empty",
                    null,
                    "!|Dictionary|Having|Name|TableName|\n|Key|Value|\n|key2|value2|\n|key3|value3|\n"
                }
            };
            foreach (var testcase in testcases)
            {
                var dict = new Dictionary<string, string> {{"key2", "value2"}, {"key3", "value3"}};
                var fitnessePage = new FitNessePageMock(testcase[1]);
                fitnessePage.SaveTableToPage("TableName", "PageName", dict);
                var expected = AddChildPartialRequest + testcase[2];

                // we're only interested in the last result
                var actual = fitnessePage.UsedUri.Split(new[] {UriSeparator}, StringSplitOptions.RemoveEmptyEntries).Last();
                Assert.AreEqual(expected, actual, "Test case " + testcase[0]);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageTableIsNamedNegativeTestcases()
        {
            var testcases = new[]
            {
                "| xx |",
                "|xx|yy|",
                "| Dictionary |xx|",
                "|xx|yy|zz|",
                "|Dictionary|xx|yy|zz|",
                "| Dictionary |having|xx|yy|",
                "|Dictionary| given |xx|yy|",
                "|Dictionary|having|name|xx|"
            };
            var target = new PrivateType(typeof(FitNessePage));
            foreach (var testcase in testcases)
            {
                Assert.IsFalse((bool)target.InvokeStatic("TableIsNamed", testcase, "TestTable"), "Testcase: " + testcase);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void FitNessePageTableIsNamedPostiveTestcases()
        {
            var testcases = new[]
            {
                "| Dictionary | TestTable |",
                "|Dictionary|having| name |TestTable|",
                "|Dictionary| given |Name| TestTable |"
            };
            var target = new PrivateType(typeof(FitNessePage));
            foreach (var testcase in testcases)
            {
                Assert.IsTrue((bool)target.InvokeStatic("TableIsNamed", testcase, "TestTable"), "Testcase: " + testcase);
            }
        }
    }
}