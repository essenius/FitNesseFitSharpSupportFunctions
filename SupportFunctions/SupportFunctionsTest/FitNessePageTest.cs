// Copyright 2015-2021 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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

        private const string Server = "http://localhost:8080/";

        public const string UriSeparator = "\f";
        private static MethodInfo _addPageMethod;
        private static MethodInfo _checkColumnNamesMethod;
        private static MethodInfo _createTableMethod;
        private static MethodInfo _extractKeyValuePairMethod;
        private static MethodInfo _insertTableAtMethod;
        private static MethodInfo _isTableLineMethod;
        private static MethodInfo _removeTableAtMethod;
        private static MethodInfo _restCallMethod;
        private static MethodInfo _savePageMethod;
        private static MethodInfo _tableIsNamedmethod;

        [ClassInitialize]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for ClassInitialize")]
        public static void ClassInitialize(TestContext testContext)
        {
            // all the private methods we're testing. Since they are generally used multiple times, we define them here to reduce reflection code.
            _addPageMethod = typeof(FitNessePage).GetMethod("AddPage", BindingFlags.Instance | BindingFlags.NonPublic);
            _checkColumnNamesMethod = typeof(FitNessePage).GetMethod("CheckColumnNames", BindingFlags.Static | BindingFlags.NonPublic);
            _createTableMethod = typeof(FitNessePage).GetMethod("CreateTable", BindingFlags.Static | BindingFlags.NonPublic);
            _extractKeyValuePairMethod = typeof(FitNessePage).GetMethod("ExtractKeyValuePair", BindingFlags.Static | BindingFlags.NonPublic);
            _insertTableAtMethod = typeof(FitNessePage).GetMethod("InsertTableAt", BindingFlags.Instance | BindingFlags.NonPublic);
            _isTableLineMethod = typeof(FitNessePage).GetMethod("IsTableLine", BindingFlags.Static | BindingFlags.NonPublic);
            _removeTableAtMethod = typeof(FitNessePage).GetMethod("RemoveTableAt", BindingFlags.Instance | BindingFlags.NonPublic);
            _restCallMethod = typeof(FitNessePage).GetMethod("RestCall", BindingFlags.Instance | BindingFlags.NonPublic);
            _savePageMethod = typeof(FitNessePage).GetMethod("SavePage", BindingFlags.Instance | BindingFlags.NonPublic);
            _tableIsNamedmethod = typeof(FitNessePage).GetMethod("TableIsNamed", BindingFlags.Static | BindingFlags.NonPublic);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageAddTest()
        {
            // The first entry ensures that characters that need to be escaped are handled correctly
            // the hash is a notorious one - if you leave that unescaped, FitNesse will ignore the rest of the page.
            var lines = new[] { "#escape characters<grin>&", "!|Dictionary|TableName|", "|key1|value1|" };
            var fitnessePage = new FitNessePageMock(null);
            _addPageMethod.Invoke(fitnessePage, new object[] { "PageName", lines });
            const string expected = AddChildPartialRequest + "#escape characters<grin>&\n!|Dictionary|TableName|\n|key1|value1|\n\f";
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(FormatException), "First column must be called 'Key' instead of 'Sleutel'")]
        public void FitNessePageCheckColumnNamesWithWrongFirstColumnNameTrowsFormatException() =>
            _checkColumnNamesMethod.Invoke(null, new object[] { "|Sleutel|Waarde|" });

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(FormatException), "Second column must be called 'Value' instead of 'Waarde'")]
        public void FitNessePageCheckColumnNamesWithWrongSecondColumnNameTrowsFormatException() =>
            _checkColumnNamesMethod.Invoke(null, new object[] { "|key|Waarde|" });

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageCreateTableTest()
        {
            var dict = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var result = ((List<string>)_createTableMethod.Invoke(null, new object[] { "TestTable", dict })).ToArray();
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("!|Dictionary|Having|Name|TestTable|", result[0]);
            Assert.AreEqual("|Key|Value|", result[1]);
            Assert.AreEqual("|key1|value1|", result[2]);
            Assert.AreEqual("|key2|value2|", result[3]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageDeleteTableFromEmptyPageTest()
        {
            var fitnessePage = new FitNessePageMock(null);
            Assert.IsFalse(fitnessePage.DeleteTableFromPage("TableName", "PageName"));
        }

        [TestMethod]
        [TestCategory("Unit")]
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

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageDeleteTest()
        {
            var fitnessePage = new FitNessePageMock(null);
            fitnessePage.DeletePage("PageName");
            Assert.AreEqual(DeletePageRequest, fitnessePage.UsedUri);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(FormatException), "Row should have 2 cells: Key and Value. Found 1: ||")]
        public void FitNessePageExtractKeyValuePairFailsTest()
        {
            var _ = (KeyValuePair<string, string>)_extractKeyValuePairMethod.Invoke(null, new object[] { "||" });
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageExtractKeyValuePairTest()
        {
            var keyValuePair = (KeyValuePair<string, string>)_extractKeyValuePairMethod.Invoke(null, new object[] { "|Sleutel|Waarde|" });
            Assert.AreEqual("Sleutel", keyValuePair.Key);
            Assert.AreEqual("Waarde", keyValuePair.Value);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(
            "AtFirstTableOnPage",
            "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n",
            1,
            "\n!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\nq\n" +
            "|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n" + UriSeparator)]
        [DataRow(
            "AtEndOfPageEndingWithTable",
            "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\n",
            5,
            "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\n" +
            "\n!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" +
            UriSeparator)]
        [DataRow(
            "AtEndOfPageEndingWithWhitespace",
            "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n",
            6,
            "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n" +
            "!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" + UriSeparator)]
        [DataRow(
            "AtEmptyPage",
            null,
            0,
            "!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" + UriSeparator)]
        [DataRow(
            "AtStartOfPageWithOneTable",
            "!|Dictionary|TableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n",
            0,
            "!|Dictionary|Having|Name|NewTableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n\n" +
            "!|Dictionary|TableName|\n|Key|Value|\n|key5|value5|\n|key6|value6|\n" + UriSeparator)]
        public void FitNessePageInsertTableAtTests(string testCase, string inputPage, int lineIndex, string expectedResult)
        {
                var dict = new Dictionary<string, string> { { "key5", "value5" }, { "key6", "value6" } };
                var fitnessePage = new FitNessePageMock(inputPage);
                fitnessePage.LoadTableFromPage("TableName", "PageName");
                _insertTableAtMethod.Invoke(fitnessePage, new object[] { "NewTableName", dict, lineIndex });
                _savePageMethod.Invoke(fitnessePage, new object[] { "PageName" });
                var expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest + expectedResult;
                Assert.AreEqual(expected, fitnessePage.UsedUri, "Testcase " + testCase);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("|Sleutel|Waarde|", true, "table line without whitespace")]
        [DataRow(" |Sleutel|Waarde| ", true, "table line with whitespace")]
        [DataRow("  ", false, "just whitespace")]
        [DataRow("comments", false, "comments")]
        [DataRow("  comments", false, "comments with whitespace")]
        public void FitNessePageIsTableLineTest(string tableLine, bool isMethod, string message) =>
            Assert.AreEqual(isMethod, _isTableLineMethod.Invoke(null, new object[] { tableLine }), message);

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("NullPage", null)]
        [DataRow("EmptyPage", "")]
        [DataRow("PageWithOneLine", "\n")]
        [DataRow("PageWithTableWithDifferentName", "|Dictionary|Main|\n|key|value|\n")]
        public void FitNessePageLoadTableFromPageNegativeTestCases(string tableName, string page)
        {
            var fitnessePage = new FitNessePageMock(page);
            var dict = fitnessePage.LoadTableFromPage("tableName", tableName);
            Assert.AreEqual(dict.Count, 0, tableName);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("PageWithSimpleTable", "|Dictionary|Main|\n|key|value|\n|key|value|\n")]
        [DataRow("PageWithTableAndComments", "c\nc\n|Dictionary|Main|\n|key|value|\n|key|value|\nc\n")]
        [DataRow(
            "PageWithMoreTables",
            "comment1\ncomment 2\n" +
            "|Dictionary|WrongName|\n|key|value|\n|key1|value1|\n\n" +
            "|Dictionary|Main|\n|key|value|\n|key|value|\ncomment 5\n" +
            "|Dictionary|WrongName2|\n|key|value|\n|key2|value2|\n")]
        public void FitNessePageLoadTableFromPageTestsWithOneResult(string testCase, string inputPage)
        {
            var fitnessePage = new FitNessePageMock(inputPage);
            var dict = fitnessePage.LoadTableFromPage("Main", testCase);
            Assert.AreEqual(1, dict.Count, "Entry Count for testcase " + testCase);
            Assert.AreEqual("value", dict["key"], "Entry for testcase " + testCase);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageRemoveTableAtTest()
        {
            const string pageContent = "\nq\n|Dictionary|TableName|\n|key|value|\n|key1|value1|\nq\n";
            var fitnessePage = new FitNessePageMock(pageContent);
            fitnessePage.LoadTableFromPage("TableName", "PageName");
            _removeTableAtMethod.Invoke(fitnessePage, new object[] { 2 });
            _savePageMethod.Invoke(fitnessePage, new object[] { "PageName" });
            const string expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest + "\nq\nq\n" + UriSeparator;
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageRemoveTableWithWhitespaceAtTest()
        {
            const string pageContent = "\nq\n|  Dictionary  |  TableName   |\n| key  |  value |\n| key1 | value1  |\nq\n";
            var fitnessePage = new FitNessePageMock(pageContent);
            fitnessePage.LoadTableFromPage("TableName", "PageName");
            _removeTableAtMethod.Invoke(fitnessePage, new object[] { 2 });
            _savePageMethod.Invoke(fitnessePage, new object[] { "PageName" });
            const string expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest + "\nq\nq\n" + UriSeparator;
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void FitNessePageRestCallInvalidTest()
        {
            //requires FitNesse active locally on port 8080
            var fitnessePage = new FitNessePage();
            var stream = _restCallMethod.Invoke(fitnessePage, new object[] { Server + "NonExistingPage?pageData" });
            Assert.IsNull(stream);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void FitNessePageRestCallValidTest()
        {
            //requires FitNesse active locally on port 8080
            var fitnessePage = new FitNessePage();
            var stream = _restCallMethod.Invoke(fitnessePage, new object[] { Server + "?pageData" });
            Assert.IsNotNull(stream);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FitNessePageSavePageTest()
        {
            const string pageContent = "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n";
            var fitnessePage = new FitNessePageMock(pageContent);
            fitnessePage.LoadTableFromPage("TableName", "PageName");
            _savePageMethod.Invoke(fitnessePage, new object[] { "PageName" });
            const string expected = PageDataRequest + DeletePageRequest + AddChildPartialRequest +
                                    "\nq\n|Dictionary|TableName|\n|Key|Value|\n|key1|value1|\nq\n" + UriSeparator;
            Assert.AreEqual(expected, fitnessePage.UsedUri);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(
            "AtEnd",
            "\nq\n|Dictionary|ExistingTableName|\n|key|value|\n|key1|value1|\n\nq\n",
            "\nq\n|Dictionary|ExistingTableName|\n|key|value|\n|key1|value1|\n\nq\n" +
            "!|Dictionary|Having|Name|TableName|\n|Key|Value|\n|key2|value2|\n|key3|value3|\n")]
        [DataRow(
            "Replace",
            "\nq\n|Dictionary|TableName|\n|key|value|key1|value1|\n\nq\n",
            "\nq\n!|Dictionary|Having|Name|TableName|\n|Key|Value|\n|key2|value2|\n|key3|value3|\n\nq\n")]
        [DataRow(
            "Empty",
            null,
            "!|Dictionary|Having|Name|TableName|\n|Key|Value|\n|key2|value2|\n|key3|value3|\n")]
        public void FitNessePageSaveTableToPageTests(string testCase, string inputPage, string resultPage)
        {
            var dict = new Dictionary<string, string> { { "key2", "value2" }, { "key3", "value3" } };
            var fitnessePage = new FitNessePageMock(inputPage);
            fitnessePage.SaveTableToPage("TableName", "PageName", dict);
            var expected = AddChildPartialRequest + resultPage;

            // we're only interested in the last result
            var actual = fitnessePage.UsedUri.Split(new[] { UriSeparator }, StringSplitOptions.RemoveEmptyEntries).Last();
            Assert.AreEqual(expected, actual, "Test case " + testCase);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("| xx |", false)]
        [DataRow("|xx|yy|", false)]
        [DataRow("| Dictionary |xx|", false)]
        [DataRow("|xx|yy|zz|", false)]
        [DataRow("|Dictionary|xx|yy|zz|", false)]
        [DataRow("| Dictionary |having|xx|yy|", false)]
        [DataRow("|Dictionary| given |xx|yy|", false)]
        [DataRow("|Dictionary|having|name|xx|", false)]
        [DataRow("| Dictionary | TestTable |", true)]
        [DataRow("|Dictionary|having| name |TestTable|", true)]
        [DataRow("|Dictionary| given |Name| TestTable |", true)]
        public void FitNessePageTableIsNamedTest(string testCase, bool isNamedMethod) =>
            Assert.AreEqual(isNamedMethod, (bool)_tableIsNamedmethod.Invoke(null, new object[] { testCase, "TestTable" }));
    }
}
