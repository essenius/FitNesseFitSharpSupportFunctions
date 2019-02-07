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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CsvComparisonDifferenceTest
    {
        private static CsvTable CreateTestTable(string data1, string data2, string data3, string data4)
        {
            var headers = new[] {"Key", "Stream1"};
            var data = new Collection<string[]>
            {
                new[] {"Attr1", data1},
                new[] {"Attr2", data2},
                new[] {"Attr3", data3},
                new[] {"Attr4", data4}
            };
            var resultTable = new CsvTable(headers);
            foreach (var entry in data)
            {
                resultTable.Data.Add(entry);
            }
            return resultTable;
        }

        [TestMethod, TestCategory("Unit")]
        public void CsvComparisonDifferenceQueryTest()
        {
            var baseTable = CreateTestTable("100.0", "100.0", "100.0", "100.0");
            var compareTable = CreateTestTable("102.1", "100.5", "97.5", "105.1");
            var csvComparison2 = new CsvComparison(baseTable, compareTable, Tolerance.Parse("2%"));
            Assert.AreEqual(3, csvComparison2.ErrorCount(), "Error count 2%");

            var csvComparison5 = new CsvComparison(baseTable, compareTable, Tolerance.Parse("5%"));
            Assert.AreEqual(1, csvComparison5.ErrorCount(), "Error count 5%");

            var difference = new CsvComparisonDifference(csvComparison2, csvComparison5);
            Assert.AreEqual(2, difference.ErrorCount, "Error Count difference");

            var expectedDifference = new List<List<string>>
            {
                new List<string> {"2", "Attr1", "2 (B)", "Stream1", "102.1 != 100", "2.1"},
                new List<string> {"4", "Attr3", "2 (B)", "Stream1", "97.5 != 100", "2.5"}
            };

            var i = 0;
            foreach (Collection<object> row in difference.Query())
            {
                Assert.AreEqual(expectedDifference[i][0], CsvComparisonTest.QueryValue(row, "Row No"),
                    $"Query Entry #{i}.RowNo");
                Assert.AreEqual(expectedDifference[i][1], CsvComparisonTest.QueryValue(row, "Row Name"),
                    $"Query Entry #{i}.Row");
                Assert.AreEqual(expectedDifference[i][2], CsvComparisonTest.QueryValue(row, "Column No"),
                    $"Query Entry #{i}.ColumnNo");
                Assert.AreEqual(expectedDifference[i][3], CsvComparisonTest.QueryValue(row, "Column Name"),
                    $"Query Entry #{i}.Column");
                Assert.AreEqual(expectedDifference[i][4], CsvComparisonTest.QueryValue(row, "Value"),
                    $"Query Entry #{i}.Value");
                Assert.AreEqual(expectedDifference[i][5], CsvComparisonTest.QueryValue(row, "Delta"),
                    $"Query Entry #{i}.Delta");
                i++;
            }

            i = 0;
            foreach (Collection<object> row in difference.DoTable(null).Skip(1))
            {
                Assert.AreEqual("report:" + expectedDifference[i][0], row[0], $"Table Entry #{i}.RowNo");
                Assert.AreEqual("report:" + expectedDifference[i][1], row[1], $"Table Entry #{i}.Row");
                Assert.AreEqual("report:" + expectedDifference[i][2], row[2], $"Table Entry #{i}.ColumnNo");
                Assert.AreEqual("report:" + expectedDifference[i][3], row[3], $"Table Entry #{i}.Column");
                Assert.AreEqual("fail:" + expectedDifference[i][4], row[4], $"Table Entry #{i}.Value");
                Assert.AreEqual("fail:" + expectedDifference[i][5], row[5], $"Table Entry #{i}.Delta");
                i++;
            }
            Assert.AreEqual(expectedDifference.Count, difference.ErrorCount, "Table ErrorCount");
        }
    }
}