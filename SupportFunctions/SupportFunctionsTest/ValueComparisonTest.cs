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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class ValueComparisonTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("0-All Good String", "0", "System.String", "Hi1", "Hi1", "None", "Hi1", "Hi1", null)]
        [DataRow("1-All Good Int", "0", "System.Int32", "12345", "12345", "None", "12345", "12345", null)]
        [DataRow("2-All Good Double", "0.001", "System.Double", "12.0", "12.0009", "WithinTolerance", "12", "12.0009", null)]
        [DataRow("3-Type Mismatch", "0", null, "12.1", "Hi1", "ValueIssue", "12.1", "Hi1", null)]
        [DataRow("4-Double Mismatch", "0.001:4", null, "12.0", "12.01", "OutsideToleranceIssue", "12", "12.01", "0.01")]
        [DataRow("5-Double Mismatch With 5 Significant Digits", "0.0012345678:5", null, "12.00345678901", "12.00567890123",
            "OutsideToleranceIssue", "12.0034568", "12.0056789", "0.0022221")]
        [DataRow("6-Double NaN", "0.0012345678:5", null, "NaN", "NaN", "None", "NaN", "NaN", null)]
        [DataRow("7-Long Mismatch", "1", null, "636188256000010001", "636188256000010000", "WithinTolerance", "636188256000010001",
            "636188256000010000", null)]
        [DataRow("8-Int Mismatch", "0", null, "11", "12", "ValueIssue", "11", "12", null)]
        [DataRow("9-Bool Mismatch", "0", null, "True", "False", "ValueIssue", "True", "False", null)]
        [DataRow("10-Missing Actual", "0", null, "11", null, "Missing", "11", null, null)]
        [DataRow("11-Missing Expected", "0", null, null, "11.0", "Surplus", null, "11", null)]
        [DataRow("12-Double 0", "2%", null, "0.00E+00", "0", "WithinTolerance", "0", "0", null)]
        [DataRow("13-Double null", "", null, null, null, "None", null, null, null)]
        [DataRow("14-Infinity Text", "0.0001", "System.Double", "Infinity", "∞", "None", "Infinity", "Infinity", null)]
        [DataRow("15--Infinity Text", "0.0001", "System.Double", "-∞", "-Infinity", "None", "-Infinity", "-Infinity", null)]
        public void ValueComparisonTest1(string testCase, string toleranceString, string compareTypeString, string expected, string actual,
            string issueString, string resultExpected, string resultActual, string resultDelta)
        {
            var testName = "Test " + testCase;

            var tolerance = Tolerance.Parse(toleranceString);
            ("Precision parsed:" + tolerance.Precision).Log();
            var issue = (CompareOutcome)Enum.Parse(typeof(CompareOutcome), issueString);
            var compareType = string.IsNullOrEmpty(compareTypeString) ? null : Type.GetType(compareTypeString);
            var comparison = new ValueComparison(expected, actual, tolerance, compareType);
            Assert.AreEqual(issue, comparison.Outcome, testName + "-Issue");
            if (!string.IsNullOrEmpty(resultExpected))
            {
                Assert.AreEqual(resultExpected, comparison.ExpectedValueOut ?? string.Empty, testName + "-Expected");
            }
            if (!string.IsNullOrEmpty(resultActual))
            {
                Assert.AreEqual(resultActual, comparison.ActualValueOut ?? string.Empty, testName + "-Actual");
            }
            if (!string.IsNullOrEmpty(resultDelta))
            {
                Assert.AreEqual(resultDelta, comparison.DeltaOut ?? string.Empty, testName + "-Delta");
            }
        }
    }
}
