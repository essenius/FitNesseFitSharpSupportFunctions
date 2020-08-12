// Copyright 2016-2020 Rik Essenius
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class DimensionTest
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit")]
        public void DimensionGetExtremeValuesTest()
        {
            var values = new List<IMeasurementComparison> {new MeasurementComparisonMock(1, 1, CompareOutcome.None)};

            var dimension = Dimension.GetExtremeValues(values, null, null);
            Assert.AreEqual(1, dimension.Min);
            Assert.AreEqual(1, dimension.Max);

            dimension = Dimension.GetExtremeValues(values, null, 2);
            Assert.AreEqual(1, dimension.Min);
            Assert.AreEqual(2, dimension.Max);

            dimension = Dimension.GetExtremeValues(values, -2, null);
            Assert.AreEqual(-2, dimension.Min);
            Assert.AreEqual(1, dimension.Max);

            dimension = Dimension.GetExtremeValues(values, -2, 0);
            Assert.AreEqual(-2, dimension.Min);
            Assert.AreEqual(0, dimension.Max);

            dimension = Dimension.GetExtremeValues(values, -2, 3);
            Assert.AreEqual(-2, dimension.Min);
            Assert.AreEqual(3, dimension.Max);
        }

        [TestMethod, TestCategory("Unit"), DeploymentItem("SupportFunctionsTest\\TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "DimensionGridlineInterval", DataAccessMethod.Sequential)]
        public void DimensionGridlineIntervalTest()
        {
            var range = TestContext.DataRow["range"].To<double>();
            var expected = TestContext.DataRow["interval"].To<double>();
            var dimension = new Dimension(0, range, false);
            var actual = dimension.GridlineInterval;
            Assert.IsTrue(expected.HasMinimalDifferenceWith(actual), $"Range: {range} expected: {expected} actual: {actual}");
        }

        [TestMethod, TestCategory("Unit")]
        public void DimensionGridLineMaxMinTest()
        {
            var dimension = new Dimension(-2.6, -1.2, false);
            Assert.IsTrue(dimension.GridlineMax.HasMinimalDifferenceWith(-1.2), "max");
            Assert.IsTrue(dimension.GridlineMin.HasMinimalDifferenceWith(-2.6), "min");
            dimension = new Dimension(3, 7, false);
            Assert.IsTrue(dimension.GridlineMax.HasMinimalDifferenceWith(7), "max");
            Assert.IsTrue(dimension.GridlineMin.HasMinimalDifferenceWith(3), "min");
            dimension = new Dimension(3, 3, false);
            Assert.IsTrue(dimension.GridlineMax.IsZero(), "max zero");
            Assert.IsTrue(dimension.GridlineMin.IsZero(), "min zero");
        }
    }
}
