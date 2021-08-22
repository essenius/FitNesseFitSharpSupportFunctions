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
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class ToleranceTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("AbsRelNoDigits", 25.0, "0.0123456789;0.01%", 0.0123456789, null, "0.0123456789 (0.0 %)")]
        [DataRow("Abs2Rel3_AbsWins", 25.0, "0.123:2;0.1234%:3", 0.12, 2, "0.12 (0.5 %)")]
        [DataRow("Abs3Rel2_AbsWins", 25.0, "0.0123:3;0.01234%:2", 0.0123, 4, "0.0123 (0.0 %)")]
        [DataRow("Abs2Rel3_RelWins", 100D, "0.0111:2;0.01234%:3", 0.0123, 4, "0.0123 (0.0 %)")]
        [DataRow("Abs3Rel2_RelWins", 100D, "0.0111:3;0.01234%:2", 0.012, 3, "0.012 (0.0 %)")]
        [DataRow("Abs3Rel2_NullRange", null, "0.0111:3;0.01234%:2", 0.0111, 4, "0.0111")]
        public void ToleranceParseTest(string testCase, double? range, string toleranceString,
            double expectedValue, int? expectedPrecision, string expectedRendering)
        {
            var testName = "Test: " + testCase;
            var tolerance = Tolerance.Parse(toleranceString);
            tolerance.DataRange = range;

            Assert.AreEqual(expectedValue, tolerance.Value, testName);
            Assert.AreEqual(expectedPrecision, tolerance.Precision, testName);
            Assert.AreEqual(expectedRendering, tolerance.ToString(), testName);
        }
    }
}
