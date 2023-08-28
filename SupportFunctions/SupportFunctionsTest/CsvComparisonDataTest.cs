// Copyright 2016-2023 Rik Essenius
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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CsvComparisonDataTest
    {
        private Collection<string[]> _actualdata;
        private string[] _actualHeaders;
        private CsvTable _actualTable;
        private Collection<string[]> _baseData;

        private string[] _baseHeaders;
        private CsvTable _baseTable;
        private CsvComparison _csvComparison;
        private List<List<string>> _expectedResult;

        [TestMethod]
        [TestCategory("Unit")]
        public void CsvComparisonDataDoTableCompleteTest()
        {
            var i = 0;
            foreach (Collection<object> row in _csvComparison.DoTable(null).Skip(1))
            {
                Assert.AreEqual("report:" + _expectedResult[i][0], row[0], $"Table Entry #{i}.Cell");
                Assert.AreEqual("report:" + _expectedResult[i][1], row[1], $"Table Entry #{i}.RowNo");
                Assert.AreEqual("report:" + _expectedResult[i][2], row[2], $"Table Entry #{i}.Row");
                Assert.AreEqual("report:" + _expectedResult[i][3], row[3], $"Table Entry #{i}.ColumnNo");
                Assert.AreEqual("report:" + _expectedResult[i][4], row[4], $"Table Entry #{i}.Column");
                Assert.AreEqual("fail:" + _expectedResult[i][5], row[5], $"Table Entry #{i}.Value");
                if (!string.IsNullOrEmpty(_expectedResult[i][6]))
                {
                    Assert.AreEqual("fail:" + _expectedResult[i][6], row[7], $"Table Entry #{i}.Delta Percentage");
                }

                i++;
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CsvComparisonDataDoTablePartialTest()
        {
            var desiredHeaders = new Collection<Collection<object>>
            {
                new Collection<object> { "Value", "Row No", "Column No" }
            };
            var i = 0;
            var result = _csvComparison.DoTable(desiredHeaders);
            var firstRow = result.FirstOrDefault() as Collection<object>;
            Assert.IsNotNull(firstRow, "with desired headers: header is null");
            Assert.AreEqual(3, firstRow.Count, "with desired headers: header count != 3");
            Assert.AreEqual("report:Value", firstRow[0], "with desired headers: row 0");
            Assert.AreEqual("report:Row No", firstRow[1], "with desired headers: row 1");
            Assert.AreEqual("report:Column No", firstRow[2], "with desired headers: row 2");

            foreach (Collection<object> row in result.Skip(1))
            {
                Assert.AreEqual("fail:" + _expectedResult[i][5], row[0], $"Table Entry #{i}.Value");
                Assert.AreEqual("report:" + _expectedResult[i][1], row[1], $"Table Entry #{i}.RowNo");
                Assert.AreEqual("report:" + _expectedResult[i][3], row[2], $"Table Entry #{i}.ColumnNo");
                i++;
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CsvComparisonDataErrorsTest()
        {
            var errorExpectation = new Dictionary<string, string>
            {
                { "D1 [Stream3/Key]", "[Stream3] missing (Missing)" },
                { "C2 [Stream2/Attr1]", "101.1 != 100 (Delta:1.1, 1.1 %, OutsideToleranceIssue)" },
                { "D2 [Stream3/Attr1]", "[3900] missing (Missing)" },
                { "A3 [Key/Attr2]", "[Attr4] expected [Attr2] (ValueIssue)" },
                { "B3 [Stream1/Attr2]", "[n/a] expected [2] (ValueIssue)" },
                { "C3 [Stream2/Attr2]", "[0] expected [n/a] (ValueIssue)" },
                { "D3 [Stream3/Attr2]", "[n/a] missing (Missing)" },
                { "A4 [Key/Attr3]", "[Attr3] missing (Missing)" },
                { "B4 [Stream1/Attr3]", "[] missing (Missing)" },
                { "C4 [Stream2/Attr3]", "[Vapor] missing (Missing)" },
                { "D4 [Stream3/Attr3]", "[Liquid] missing (Missing)" }
            };
            var result = _csvComparison.Errors();
            Assert.AreEqual(errorExpectation.Count, result.Count, "Same number of errors");
            foreach (var pair in errorExpectation)
            {
                Assert.IsTrue(result.TryGetValue(pair.Key, out var value));
                Assert.AreEqual(pair.Value, value, "Value for " + pair.Key);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CsvComparisonDataQueryTest()
        {
            Assert.AreEqual(_expectedResult.Count, _csvComparison.ErrorCount(), "Error count");
            var i = 0;
            foreach (Collection<object> row in _csvComparison.Query())
            {
                Assert.AreEqual(_expectedResult[i][0], QueryValue(row, "Cell"), $"Query Entry #{i}.Cell");
                Assert.AreEqual(_expectedResult[i][1], QueryValue(row, "Row No"), $"Query Entry #{i}.RowNo");
                Assert.AreEqual(_expectedResult[i][2], QueryValue(row, "Row Name"), $"Query Entry #{i}.Row");
                Assert.AreEqual(_expectedResult[i][3], QueryValue(row, "Column No"), $"Query Entry #{i}.ColumnNo");
                Assert.AreEqual(_expectedResult[i][4], QueryValue(row, "Column Name"), $"Query Entry #{i}.Column");
                Assert.AreEqual(_expectedResult[i][5], QueryValue(row, "Value"), $"Query Entry #{i}.Value");
                i++;
            }
        }

        private static object QueryValue(IEnumerable<object> queryRow, string key) =>
            (from Collection<object> entry in queryRow where entry[0].Equals(key) select entry[1]).FirstOrDefault();

        [TestInitialize]
        public void TestInitialize()
        {
            _baseHeaders = new[] { "Key", "Stream1", "Stream2", "Stream3" };
            _baseData = new Collection<string[]>
            {
                new[] { "Attr1", "1.0", "100", "3.9E+03" },
                new[] { "Attr2", "2.0", "n/a", "n/a" },
                new[] { "Attr3", "", "Vapor", "Liquid" }
            };
            _baseTable = new CsvTable(_baseHeaders);
            foreach (var entry in _baseData)
            {
                _baseTable.Data.Add(entry);
            }

            _actualHeaders = new[] { "Key", "Stream1", "Stream2" };
            _actualdata = new Collection<string[]>
            {
                new[] { "Attr1", "1.0", "101.1" },
                new[] { "Attr4", "n/a", "0" }
            };

            _expectedResult = new List<List<string>>
            {
                new List<string> { "D1", "1", "Key", "4 (D)", "Stream3", "[Stream3] missing", "" },
                new List<string> { "C2", "2", "Attr1", "3 (C)", "Stream2", "101.1 != 100", "1.1 %" },
                new List<string> { "D2", "2", "Attr1", "4 (D)", "Stream3", "[3900] missing", "" },
                new List<string> { "A3", "3", "Attr2", "1 (A)", "Key", "[Attr4] expected [Attr2]", "" },
                new List<string> { "B3", "3", "Attr2", "2 (B)", "Stream1", "[n/a] expected [2]", "" },
                new List<string> { "C3", "3", "Attr2", "3 (C)", "Stream2", "[0] expected [n/a]", "" },
                new List<string> { "D3", "3", "Attr2", "4 (D)", "Stream3", "[n/a] missing", "" },
                new List<string> { "A4", "4", "Attr3", "1 (A)", "Key", "[Attr3] missing", "" },
                new List<string> { "B4", "4", "Attr3", "2 (B)", "Stream1", "[] missing", "" },
                new List<string> { "C4", "4", "Attr3", "3 (C)", "Stream2", "[Vapor] missing", "" },
                new List<string> { "D4", "4", "Attr3", "4 (D)", "Stream3", "[Liquid] missing", "" }
            };

            _actualTable = new CsvTable(_actualHeaders);
            foreach (var entry in _actualdata)
            {
                _actualTable.Data.Add(entry);
            }

            _csvComparison = new CsvComparison(_baseTable, _actualTable, Tolerance.Parse("1%"));
        }
    }
}
