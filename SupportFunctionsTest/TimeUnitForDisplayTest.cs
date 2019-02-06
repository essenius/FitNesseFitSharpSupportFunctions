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
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    [TestClass]
    public class TimeUnitForDisplayTest
    {
        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local",
            Justification = "intentional precondition check")]
        private static void AssertEqual(double expected, double actual)
        {
            Assert.IsTrue(Math.Abs(expected - actual) <= 1e-10, $"{expected} != {actual}");
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeUnitForDisplayCaptionTest()
        {
            Assert.AreEqual("ms", new TimeUnitForDisplay(0.006).Caption);
            Assert.AreEqual("s", new TimeUnitForDisplay(100D).Caption);
            Assert.AreEqual("min", new TimeUnitForDisplay(900D).Caption);
            Assert.AreEqual("h", new TimeUnitForDisplay(50000D).Caption);
            Assert.AreEqual("d", new TimeUnitForDisplay(3D * 24D * 3600D).Caption);
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeUnitForDisplayConvertFromSecondsTest()
        {
            AssertEqual(40D, new TimeUnitForDisplay(0.006).ConvertFromSeconds(0.04));
            AssertEqual(123D, new TimeUnitForDisplay(100D).ConvertFromSeconds(123));
            AssertEqual(2.05, new TimeUnitForDisplay(900D).ConvertFromSeconds(123));
            AssertEqual(2.6, new TimeUnitForDisplay(50000D).ConvertFromSeconds(9360));
            AssertEqual(5.5, new TimeUnitForDisplay(3D * 24D * 3600D).ConvertFromSeconds(475200));
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeUnitForDisplayConvertToSecondsTest()
        {
            AssertEqual(0.04, new TimeUnitForDisplay(0.006).ConvertToSeconds(40));
            AssertEqual(123D, new TimeUnitForDisplay(100D).ConvertToSeconds(123));
            AssertEqual(123D, new TimeUnitForDisplay(900D).ConvertToSeconds(2.05));
            AssertEqual(9360D, new TimeUnitForDisplay(50000D).ConvertToSeconds(2.6));
            AssertEqual(475200D, new TimeUnitForDisplay(3D * 24D * 3600D).ConvertToSeconds(5.5));
        }
    }
}