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

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CellComparisonTest
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit")]
        public void CellComparisonTest1()
        {
            var tolerance = Tolerance.Parse("1%");
            var comparison = new CellComparison(1, "Row", 2, "Column", "1.0", "1.009", tolerance);
            Assert.AreEqual(1, comparison.Row);
            Assert.AreEqual("Row", comparison.RowName);
            Assert.AreEqual(2, comparison.Column);
            Assert.AreEqual("Column", comparison.ColumnName);
            Assert.AreEqual("1.009 ~= 1", comparison.Cell.ValueMessage);
            Assert.AreEqual("0.009", comparison.Cell.DeltaMessage);
            Assert.AreEqual("0.9 %", comparison.Cell.DeltaPercentageMessage);
            Assert.AreEqual("pass:0.009", comparison.Cell.TableResult(comparison.Cell.DeltaMessage));
        }
    }
}