// Copyright 2016-2021 Rik Essenius
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class DimensionTest
    {
        [TestMethod, TestCategory("Unit")]
        public void DimensionGetValueRangeTest()
        {
            var values = new List<IMeasurementComparison> {new MeasurementComparisonMock(1, 1, CompareOutcome.None)};

            var dimension = Dimension.GetValueRange(values, null, null);
            Assert.AreEqual(1, dimension.Min);
            Assert.AreEqual(1, dimension.Max);

            dimension = Dimension.GetValueRange(values, null, 2);
            Assert.AreEqual(0.95, dimension.Min);
            Assert.AreEqual(2, dimension.Max);

            dimension = Dimension.GetValueRange(values, -2, null);
            Assert.AreEqual(-2, dimension.Min);
            Assert.AreEqual(1.15, dimension.Max);

            dimension = Dimension.GetValueRange(values, -2, 0);
            Assert.AreEqual(-2, dimension.Min);
            Assert.AreEqual(0, dimension.Max);

            dimension = Dimension.GetValueRange(values, -2, 3);
            Assert.AreEqual(-2, dimension.Min);
            Assert.AreEqual(3, dimension.Max);

            values.Add(new MeasurementComparisonMock(2, 2, CompareOutcome.None));
            dimension = Dimension.GetValueRange(values, null, null);
            Assert.AreEqual(0.95, dimension.Min);
            Assert.AreEqual(2.05, dimension.Max);
        }


        [DataTestMethod, TestCategory("Unit")]
        [DataRow(0.02, 0.002)]
        [DataRow(0.03, 0.005)]
        [DataRow(0.004, 0.0005)]
        [DataRow(0.07, 0.01)]
        [DataRow(0.09, 0.01)]
        [DataRow(0.1, 0.01)]
        [DataRow(0.11, 0.02)]
        [DataRow(5, 0.5)]
        [DataRow(235, 25)]
        [DataRow(357, 50)]
        [DataRow(798, 100)]
        [DataRow(1366, 200)]
        [DataRow(123456789, 20000000)]
        [DataRow(1.23456789e+30, 2e+29)]
        [DataRow(2.23456789e-30, 2.5e-31)]
        public void DimensionGridlineIntervalTest(double range, double expectedInterval)
        {
            var dimension = new Dimension(0, range, false);
            var actual = dimension.GridlineInterval;
            Assert.IsTrue(expectedInterval.HasMinimalDifferenceWith(actual), $"Range: {range} expected: {expectedInterval} actual: {actual}");
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
