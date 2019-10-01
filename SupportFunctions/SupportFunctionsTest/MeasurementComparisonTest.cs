// Copyright 2016-2019 Rik Essenius
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
using SupportFunctions;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class MeasurementComparisonTest
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

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

        [TestMethod, TestCategory("Unit"), DeploymentItem("SupportFunctionsTest\\TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml", "MeasurementCompare",
             DataAccessMethod.Sequential)]
        public void MeasurementComparisonTest1()
        {
            var testName = "Test " + TestContext.DataRow["testcase"];

            var expected = PrepareExpected();
            var actual = PrepareActual();

            var compareTypeString = TestContext.DataRow.ValueIfExists("compareType");
            var tolerance = Tolerance.Parse(TestContext.DataRow["tolerance"].ToString());
            var compareType = string.IsNullOrEmpty(compareTypeString) ? expected?.Value.InferType() : Type.GetType(compareTypeString);
            if (compareType == null) $"compareType == null for {testName}".Log();
            ("Precision parsed:" + tolerance.Precision).Log();

            var actualResult = new MeasurementComparison(expected, actual, tolerance, compareType);

            Assert.AreEqual(TestContext.DataRow["issue"].ToString(), actualResult.OutcomeMessage, testName + "-Issue");

            AssertStringEqual(TestContext.DataRow.ValueIfExists("resultExpectedValue"), actualResult.Value.ExpectedValueOut,
                testName + "-ExpectedValue");
            AssertStringEqual(TestContext.DataRow.ValueIfExists("resultActualValue"), actualResult.Value.ActualValueOut, testName + "-ActualValue");
            AssertStringEqual(TestContext.DataRow.ValueIfExists("resultDelta"), actualResult.Value.DeltaOut, testName + "-Delta");

            AssertBoolEqual(TestContext.DataRow.ValueIfExists("resultExpectedIsGood"), actualResult.IsGood.ExpectedValueOut,
                testName + "-ExpectedIsGood");
            AssertBoolEqual(TestContext.DataRow.ValueIfExists("resultActualIsGood"), actualResult.IsGood.ActualValueOut, testName + "-ActualIsGood");
        }

        private Measurement PrepareActual()
        {
            Measurement actual;
            if (string.IsNullOrEmpty(TestContext.DataRow["actualTimestamp"].ToString()))
            {
                actual = null;
            }
            else
            {
                var actualIsChecked = TestContext.DataRow.ValueIfExists("actualIsChecked");

                actual = new Measurement
                {
                    Timestamp = DateTime.Parse(TestContext.DataRow["actualTimestamp"].ToString()),
                    Value = TestContext.DataRow["actualValue"].ToString(),
                    IsGood = TestContext.DataRow["actualIsGood"].To<bool>(),
                    IsChecked = !string.IsNullOrEmpty(actualIsChecked) && actualIsChecked.To<bool>()
                };
            }
            return actual;
        }

        private Measurement PrepareExpected()
        {
            Measurement expected;
            if (string.IsNullOrEmpty(TestContext.DataRow["expectedTimestamp"].ToString()))
            {
                expected = null;
            }
            else
            {
                expected = new Measurement
                {
                    Timestamp = DateTime.Parse(TestContext.DataRow["expectedTimestamp"].ToString()),
                    Value = TestContext.DataRow["expectedValue"].ToString(),
                    IsGood = TestContext.DataRow["expectedIsGood"].To<bool>(),
                    IsChecked = false
                };
            }
            return expected;
        }
    }
}