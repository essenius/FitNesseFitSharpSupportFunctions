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
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class MeasurementComparisonTest
    {
        private static void AssertBoolEqual(string expected, string actual, string testName)
        {
            if (!string.IsNullOrEmpty(expected))
            {
                Assert.AreEqual(expected.To<bool>(), actual.To<bool>(), testName);
            }
        }

        private static void AssertStringEqual(string expected, string actual, string testName)
        {
            if (!string.IsNullOrEmpty(expected))
            {
                Assert.AreEqual(expected, actual ?? string.Empty, testName);
            }
        }

        [DataTestMethod, TestCategory("Unit")]
        [DataRow("0-All Good String", "0", "System.String", 
            "2016-12-24T00:14:00.0000000", "Hi1", true, "2016-12-24T00:14:00.0000000", "Hi1", true, "None", 
            "2016-12-24T00:14:00.0000000", null, "Hi1", "True", "Hi1", "True")]
        [DataRow("1-All Good Int", "0", "System.Int32",
            "2016-12-24T00:15:00.0000000", "12345", true, "2016-12-24T00:15:00.0000000", "12345", true, "None",
            "2016-12-24T00:15:00.0000000", null, "12345", "True", "12345", "True")]
        [DataRow("2-All Good Double", "0", "System.Int32",
            "2016-12-24T00:16:00.0000000", "12.1", true, "2016-12-24T00:16:00.0000000", "12.1", true, "None",
            "2016-12-24T00:16:00.0000000", null, "12.1", "True", "12.1", "True")]
        [DataRow("3-Type Mismatch", "0", null,
            "2016-12-24T00:17:00.0000000", "12.1", true, "2016-12-24T00:17:00.0000000", "Hi1", true, "ValueIssue",
            "2016-12-24T00:17:00.0000000", null, "12.1", "True", "Hi1", "True")]
        [DataRow("4-Double Mismatch", "0.001:4", null,
            "2016-12-24T00:18:00.0000000", "12.0", true, "2016-12-24T00:18:00.0000000", "12.01", true, "OutsideToleranceIssue",
            "2016-12-24T00:18:00.0000000", "0.01", "12", "True", "12.01", "True")]
        [DataRow("5-Double Mismatch With 5 Significant Digits", "0.0012345678:5", null,
            "2016-12-24T00:19:00.0000000", "12.00345678901", true, "2016-12-24T00:19:00.0000000", "12.00567890123", true, "OutsideToleranceIssue",
            "2016-12-24T00:19:00.0000000", "0.0022221", "12.0034568", "True", "12.0056789", "True")]
        [DataRow("6-Double NaN", "0.0012345678:5", null,
            "2016-12-24T00:20:00.0000000", "NaN", true, "2016-12-24T00:20:00.0000000", "NaN", true, "None",
            "2016-12-24T00:20:00.0000000", "", "NaN", "True", "NaN", "True")]
        [DataRow("7-Long Mismatch Within Tolerance", "1", null,
            "2016-12-24T00:21:00.0000000", "636188256000010001", true, "2016-12-24T00:21:00.0000000", "636188256000010000", true, "WithinTolerance",
            "2016-12-24T00:21:00.0000000", "1", "636188256000010001", "True", "636188256000010000", "True")]
        [DataRow("8-Int Mismatch", "0", null,
            "2016-12-24T00:22:00.0000000", "11", true, "2016-12-24T00:22:00.0000000", "12", true, "ValueIssue",
            "2016-12-24T00:22:00.0000000", null, "11", "True", "12", "True")]
        [DataRow("9-Bool Mismatch", "0", null,
            "2016-12-24T00:23:00.0000000", "True", true, "2016-12-24T00:23:00.0000000", "False", true, "ValueIssue",
            "2016-12-24T00:23:00.0000000", null, "True", "True", "False", "True")]

        [DataRow("10-Is Good Mismatch", "0", null,
            "2016-12-24T00:24:00.0000000",  "11", true, "2016-12-24T00:24:00.0000000", "11", false, "IsGoodIssue",
            "2016-12-24T00:24:00.0000000", null, "11", "True", "11", "False")]

        [DataRow("11-Missing Actual", "0", null,
            "2016-12-24T00:25:00.0000000", "11", true, null, null, null, "Missing",
            "[2016-12-24T00:25:00.0000000] missing", null, "11", "True", null, "")]
        [DataRow("12-Missing Expected", "0", null,
            null, null, null, "2016-12-24T00:26:00.0000000", "11", true, "Surplus",
            "[2016-12-24T00:26:00.0000000] surplus", null, null, "", "11", "True")]
        public void MeasurementComparisonTest1(string testCase, string toleranceString, string compareTypeString, 
            string expectedTimeStamp, string expectedValue, bool? expectedIsGood, string actualTimeStamp, string actualValue, bool? actualIsGood, string issue, 
            string resultTimestamp, string resultDelta, string resultExpectedValue, string resultExpectedIsGood, string resultActualValue, string resultActualIsGood)
        {
            var testName = "Test " + testCase;

            var expected = string.IsNullOrEmpty(expectedTimeStamp)
                ? null
                : new Measurement
                {
                    Timestamp = DateTime.Parse(expectedTimeStamp, CultureInfo.InvariantCulture),
                    Value = expectedValue,
                    IsGood = expectedIsGood ?? false,
                    IsChecked = false
                };
            var actual = string.IsNullOrEmpty(actualTimeStamp)
                ? null
                : new Measurement
                {
                    Timestamp = DateTime.Parse(actualTimeStamp, CultureInfo.InvariantCulture),
                    Value = actualValue,
                    IsGood = actualIsGood ?? false,
                    IsChecked = false
                };

        
            var tolerance = Tolerance.Parse(toleranceString);
            var compareType = string.IsNullOrEmpty(compareTypeString) ? expected?.Value.InferType() : Type.GetType(compareTypeString);
            if (compareType == null) $"compareType == null for {testName}".Log();
            ("Precision parsed:" + tolerance.Precision).Log();

            var actualResult = new MeasurementComparison(expected, actual, tolerance, compareType);

            Assert.AreEqual(issue, actualResult.OutcomeMessage, testName + "-Issue");

            AssertStringEqual(resultExpectedValue, actualResult.Value.ExpectedValueOut, testName + "-ExpectedValue");
            AssertStringEqual(resultActualValue, actualResult.Value.ActualValueOut, testName + "-ActualValue");
            AssertStringEqual(resultDelta, actualResult.Value.DeltaOut, testName + "-Delta");
            AssertBoolEqual(resultExpectedIsGood, actualResult.IsGood.ExpectedValueOut, testName + "-ExpectedIsGood");
            AssertBoolEqual(resultActualIsGood, actualResult.IsGood.ActualValueOut, testName + "-ActualIsGood");
            AssertStringEqual(resultTimestamp, actualResult.Timestamp.ValueMessage, testName + "-Timestamp");
        }
    }
}
