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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class TimeUnitForDisplayTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(0.006, "ms")]
        [DataRow(100d, "s")]
        [DataRow(900d, "min")]
        [DataRow(50000d, "h")]
        [DataRow(3D * 24D * 3600D, "d")]
        public void TimeUnitForDisplayCaptionTest(double timespan, string expectedUnit)
        {
            Assert.AreEqual(expectedUnit, new TimeUnitForDisplay(timespan).Caption, "Caption");
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(0.006, 0.04, 40d)]
        [DataRow(100d, 123d, 123d)]
        [DataRow(900d, 123d, 2.05)]
        [DataRow(50000d, 9360d, 2.6)]
        [DataRow(3d * 24d * 3600d, 475200d, 5.5)]
        public void TimeUnitForDisplayConvertFromToSecondsTest(double timespan, double valueInSeconds, double valueInUnits)
        {
            Assert.IsTrue(valueInUnits.HasMinimalDifferenceWith(
                new TimeUnitForDisplay(timespan).ConvertFromSeconds(valueInSeconds)), "From Seconds");
            Assert.IsTrue(valueInSeconds.HasMinimalDifferenceWith(
                new TimeUnitForDisplay(timespan).ConvertToSeconds(valueInUnits)), "To Seconds");
        }
    }
}
