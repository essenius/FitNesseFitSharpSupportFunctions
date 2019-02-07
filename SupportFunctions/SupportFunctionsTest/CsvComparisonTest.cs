// Copyright 2017-2019 Rik Essenius
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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CsvComparisonTest
    {
        internal static object QueryValue(Collection<object> queryRow, string key) =>
            (from Collection<object> entry in queryRow where entry[0].Equals(key) select entry[1]).FirstOrDefault();

        [TestMethod, TestCategory("Integration"),
         DeploymentItem("TestData\\StreamData1.csv"),
         DeploymentItem("TestData\\StreamData2.csv")]
        public void CsvComparisonErrorsStreamDataTest()
        {
            var baseCsv = CsvTable.Parse("StreamData1.csv");
            Console.WriteLine($"Base: columns={baseCsv.ColumnCount}; rows={baseCsv.RowCount}");
            var newCsv = CsvTable.Parse("StreamData2.csv");
            Console.WriteLine($"New: columns={newCsv.ColumnCount}; rows={newCsv.RowCount}");
            var comparison = new CsvComparison(baseCsv, newCsv, Tolerance.Parse("2%;0.001"));
            Console.WriteLine($"Errors: {comparison.ErrorCount()}");
            foreach (Collection<object> errorRow in comparison.Query())
            {
                Console.WriteLine(
                    $"{QueryValue(errorRow, "Row No")} [{QueryValue(errorRow, "Row Name")}], " +
                    $"{QueryValue(errorRow, "Column No")} [{QueryValue(errorRow, "Column Name")}] " +
                    $"{QueryValue(errorRow, "Value")}" + $"{QueryValue(errorRow, "Delta Percentage")}");
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void CsvComparisonParseExists() => Assert.IsNull(CsvComparison.Parse(null));

        [TestMethod, TestCategory("Unit")]
        public void CsvComparisonTest1()
        {
            var baseHeaders = new[] {"Key", "Stream1", "Stream2", "Stream3"};
            var baseData = new Collection<string[]>
            {
                new[] {"Attr1", "1.0", "100", "3.9E+03"},
                new[] {"Attr2", "2.0", "n/a", "n/a"},
                new[] {"Attr3", "", "Vapor", "Liquid"}
            };
            var baseTable = new CsvTable(baseHeaders);
            foreach (var entry in baseData)
            {
                baseTable.Data.Add(entry);
            }

            var actualHeaders = new[] {"Key", "Stream1", "Stream2"};
            var actualdata = new Collection<string[]>
            {
                new[] {"Attr1", "1.0", "101.1"},
                new[] {"Attr4", "n/a", "0"}
            };

            var expectedResult = new List<List<string>>
            {
                new List<string> {"1", "Key", "4 (D)", "Stream3", "[Stream3] missing", ""},
                new List<string> {"2", "Attr1", "3 (C)", "Stream2", "101.1 != 100", "1.1 %"},
                new List<string> {"2", "Attr1", "4 (D)", "Stream3", "[3900] missing", ""},
                new List<string> {"3", "Attr2", "1 (A)", "Key", "[Attr4] expected [Attr2]", ""},
                new List<string> {"3", "Attr2", "2 (B)", "Stream1", "[n/a] expected [2]", ""},
                new List<string> {"3", "Attr2", "3 (C)", "Stream2", "[0] expected [n/a]", ""},
                new List<string> {"3", "Attr2", "4 (D)", "Stream3", "[n/a] missing", ""},
                new List<string> {"4", "Attr3", "1 (A)", "Key", "[Attr3] missing", ""},
                new List<string> {"4", "Attr3", "2 (B)", "Stream1", "[] missing", ""},
                new List<string> {"4", "Attr3", "3 (C)", "Stream2", "[Vapor] missing", ""},
                new List<string> {"4", "Attr3", "4 (D)", "Stream3", "[Liquid] missing", ""}
            };

            var actualTable = new CsvTable(actualHeaders);
            foreach (var entry in actualdata)
            {
                actualTable.Data.Add(entry);
            }
            var csvComparison = new CsvComparison(baseTable, actualTable, Tolerance.Parse("1%"));
            Assert.AreEqual(expectedResult.Count, csvComparison.ErrorCount(), "Error count");
            var i = 0;
            foreach (Collection<object> row in csvComparison.Query())
            {
                Assert.AreEqual(expectedResult[i][0], QueryValue(row, "Row No"), $"Query Entry #{i}.RowNo");
                Assert.AreEqual(expectedResult[i][1], QueryValue(row, "Row Name"), $"Query Entry #{i}.Row");
                Assert.AreEqual(expectedResult[i][2], QueryValue(row, "Column No"), $"Query Entry #{i}.ColumnNo");
                Assert.AreEqual(expectedResult[i][3], QueryValue(row, "Column Name"), $"Query Entry #{i}.Column");
                Assert.AreEqual(expectedResult[i][4], QueryValue(row, "Value"), $"Query Entry #{i}.Value");
                i++;
            }

            i = 0;
            foreach (Collection<object> row in csvComparison.DoTable(null).Skip(1))
            {
                Assert.AreEqual("report:" + expectedResult[i][0], row[0], $"Table Entry #{i}.RowNo");
                Assert.AreEqual("report:" + expectedResult[i][1], row[1], $"Table Entry #{i}.Row");
                Assert.AreEqual("report:" + expectedResult[i][2], row[2], $"Table Entry #{i}.ColumnNo");
                Assert.AreEqual("report:" + expectedResult[i][3], row[3], $"Table Entry #{i}.Column");
                Assert.AreEqual("fail:" + expectedResult[i][4], row[4], $"Table Entry #{i}.Value");
                if (!string.IsNullOrEmpty(expectedResult[i][5]))
                {
                    Assert.AreEqual("fail:" + expectedResult[i][5], row[6], $"Table Entry #{i}.Delta Percentage");
                }
                i++;
            }

            var desiredHeaders = new Collection<Collection<object>>
            {
                new Collection<object> {"Value", "Row No", "Column No"}
            };
            i = 0;
            var result = csvComparison.DoTable(desiredHeaders);
            var firstRow = result.FirstOrDefault() as Collection<object>;
            Assert.IsNotNull(firstRow, "with desired headers: header is null");
            Assert.AreEqual(3, firstRow.Count, "with desired headers: header count != 3");
            Assert.AreEqual("report:Value", firstRow[0], "with desired headers: row 0");
            Assert.AreEqual("report:Row No", firstRow[1], "with desired headers: row 1");
            Assert.AreEqual("report:Column No", firstRow[2], "with desired headers: row 2");

            foreach (Collection<object> row in result.Skip(1))
            {
                Assert.AreEqual("fail:" + expectedResult[i][4], row[0], $"Table Entry #{i}.Value");
                Assert.AreEqual("report:" + expectedResult[i][0], row[1], $"Table Entry #{i}.RowNo");
                Assert.AreEqual("report:" + expectedResult[i][2], row[2], $"Table Entry #{i}.ColumnNo");
                i++;
            }
        }

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException),
             "Wrong: No such header. Recognised values: Row No, Row Name, Column No, Column Name, Value, Delta, Delta %, Issue."),
         SuppressMessage("ReSharper", "UnusedVariable", Justification = "forcing exception")]
        public void CsvComparisonWrongHeaderTest()
        {
            var desiredHeaders = new Collection<Collection<object>>
            {
                new Collection<object> {"Wrong"}
            };
            var csvComparison = new CsvComparison(null, null, null);
            var result = csvComparison.DoTable(desiredHeaders);
        }
    }
}