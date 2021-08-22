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
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(null, null, 1, 1, -3.3, 3.3)]
        [DataRow(null, 2d, 0.95, 2, -3.25, 2)]
        [DataRow(-2d, null, -2, 1.15, -2, 3.25)]
        [DataRow(-2d, 0d, -2, 0, -2, 0)]
        [DataRow(-2d, 3d, -2, 3, -2, 3)]
        public void DimensionGetValueRangeTest(double? minIn, double? maxIn, double minOut1, double maxOut1, double minOut2, double maxOut2)
        {
            var values = new List<IMeasurementComparison> { new MeasurementComparisonMock(1, 1, CompareOutcome.None) };

            var dimension = Dimension.GetValueRange(values, minIn, maxIn);
            Assert.AreEqual(minOut1, dimension.Min, "min(1)");
            Assert.AreEqual(maxOut1, dimension.Max, "max(1)");

            values.Add(new MeasurementComparisonMock(-3, 3, CompareOutcome.None));
            dimension = Dimension.GetValueRange(values, minIn, maxIn);
            Assert.AreEqual(minOut2, dimension.Min, "min(2)");
            Assert.AreEqual(maxOut2, dimension.Max, "max(2)");
        }

        [DataTestMethod]
        [TestCategory("Unit")]
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

        [TestMethod]
        [TestCategory("Unit")]
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
