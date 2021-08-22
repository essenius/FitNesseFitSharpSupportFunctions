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

namespace SupportFunctionsTest
{
    [TestClass]
    public class ToleranceValueTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("doubleAbsolute", 250D, "0.1", 0.1)]
        [DataRow("doubleAbsolute2Digits", 250D, "0.123:2", 0.12)]
        [DataRow("doubleRelative", 123.4567890, "0.1%", 0.123456789)]
        [DataRow("doubleRelative5Digits", 123.4567890, "0.1%:5", 0.12346)]
        [DataRow("doubleAbsoluteEpsilon", 1D, "epsilon", 4.94065645841247E-324)]
        [DataRow("intAbsolute", 10000D, "2", 2)]
        [DataRow("intRelative", 20000D, "0.2%", 40)]
        [DataRow("intNoTolerance", 10000D, "", 0)]
        [DataRow("doubleNoTolerance", 123.45, "", 0)]
        [DataRow("NoRange", null, "0.2%", 0)]
        public void ToleranceValueParseTest(string testCase, double? range, string tolerance, double expectedValue)
        {
            var testName = "Test: " + testCase;
            var toleranceValue = ToleranceValue.Parse(tolerance);
            toleranceValue.DataRange = range;
            Assert.AreEqual(expectedValue, toleranceValue.AppliedValue, testName);
        }
    }
}
