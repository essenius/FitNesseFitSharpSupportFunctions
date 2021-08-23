// Copyright 2017-2021 Rik Essenius
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CsvComparisonTest
    {
        [TestMethod]
        [TestCategory("Integration")]
        [DeploymentItem("TestData\\SimpleExpected.csv")]
        [DeploymentItem("TestData\\SimpleActualMissing.csv")]
        public void CsvComparisonErrorsMissingTest()
        {
            var expectedCsv = CsvTable.Parse("SimpleExpected.csv");
            var actualCsv = CsvTable.Parse("SimpleActualMissing.csv");
            var comparison = new CsvComparison(expectedCsv, actualCsv, Tolerance.Parse("2%;0.001"));
            var expected = comparison.ExpectedTable;
            Assert.AreEqual(2, expected.RowCount, "Expected RowCount");
            var actual = comparison.ActualTable;
            Assert.AreEqual(1, actual.RowCount, "Actual RowCount");
            Assert.AreEqual(4, comparison.ErrorCount(), "Error Count = 2");
            var query = comparison.Query();
            var errorRow = query[0] as Collection<object>;
            Assert.AreEqual("B1", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("Missing", QueryValue(errorRow, "Issue"));
            errorRow = query[1] as Collection<object>;
            Assert.AreEqual("B2", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("Missing", QueryValue(errorRow, "Issue"));
            errorRow = query[2] as Collection<object>;
            Assert.AreEqual("A3", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("Missing", QueryValue(errorRow, "Issue"));
            errorRow = query[3] as Collection<object>;
            Assert.AreEqual("B3", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("Missing", QueryValue(errorRow, "Issue"));
        }

        [TestMethod]
        [TestCategory("Integration")]
        [DeploymentItem("TestData\\SimpleExpected.csv")]
        [DeploymentItem("TestData\\SimpleActualSurplus.csv")]
        public void CsvComparisonErrorsSurplusTest()
        {
            var expectedCsv = CsvTable.Parse("SimpleExpected.csv");
            var actualCsv = CsvTable.Parse("SimpleActualSurplus.csv");
            var comparison = new CsvComparison(expectedCsv, actualCsv, Tolerance.Parse("2%;0.001"));
            Assert.AreEqual(3, comparison.ErrorCount(), "Error Count = 3");
            var query = comparison.Query();
            var errorRow = query[0] as Collection<object>;
            Assert.AreEqual("C1", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("Surplus", QueryValue(errorRow, "Issue"));
            errorRow = query[1] as Collection<object>;
            Assert.AreEqual("C2", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("Surplus", QueryValue(errorRow, "Issue"));
            errorRow = query[2] as Collection<object>;
            Assert.AreEqual("A4", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("Surplus", QueryValue(errorRow, "Issue"));
        }


        [TestMethod]
        [TestCategory("Integration")]
        [DeploymentItem("TestData\\StreamData1.csv")]
        [DeploymentItem("TestData\\StreamData2.csv")]
        public void CsvComparisonErrorsStreamDataTest()
        {
            var baseCsv = CsvTable.Parse("StreamData1.csv");
            Console.WriteLine($"Base: columns={baseCsv.ColumnCount}; rows={baseCsv.RowCount}");
            var newCsv = CsvTable.Parse("StreamData2.csv");
            Console.WriteLine($"New: columns={newCsv.ColumnCount}; rows={newCsv.RowCount}");
            var comparison = new CsvComparison(baseCsv, newCsv, Tolerance.Parse("2%;0.001"));
            Assert.AreEqual(1, comparison.ErrorCount(), "Error Count = 1");
            var query = comparison.Query();
            var errorRow = query[0] as Collection<object>;
            Assert.AreEqual("H2", QueryValue(errorRow, "Cell"));
            Assert.AreEqual("2", QueryValue(errorRow, "Row No"));
            Assert.AreEqual("Total Liquid Mole Fraction", QueryValue(errorRow, "Row Name"));
            Assert.AreEqual("8 (H)", QueryValue(errorRow, "Column No"));
            Assert.AreEqual("A11", QueryValue(errorRow, "Column Name"));
            Assert.AreEqual("0.01 != 0", QueryValue(errorRow, "Value"));
            Assert.AreEqual("0.01", QueryValue(errorRow, "Delta"));
            Assert.IsTrue(string.IsNullOrEmpty(QueryValue(errorRow, "Delta %").ToString()));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CsvComparisonParseExists() => Assert.IsNull(CsvComparison.Parse(null));

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(ArgumentException),
            "Wrong: No such header. Recognised values: Cell, Row No, Row Name, Column No, Column Name, Value, Delta, Delta %, Issue.")]
        public void CsvComparisonWrongHeaderTest()
        {
            var desiredHeaders = new Collection<Collection<object>> { new Collection<object> { "Wrong" } };
            var csvComparison = new CsvComparison(null, null, null);
            var _ = csvComparison.DoTable(desiredHeaders);
        }

        internal static object QueryValue(IEnumerable<object> queryRow, string key) =>
            (from Collection<object> entry in queryRow where entry[0].Equals(key) select entry[1]).FirstOrDefault();
    }
}
