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

        [TestMethod, TestCategory("Unit"), DeploymentItem("SupportFunctionsTest\\TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml", "MeasurementCompare",
             DataAccessMethod.Sequential)]
        public void MeasurementComparisonTest1()
        {
            var testName = "Test " + TestContext.DataRow["testcase"];

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

            var compareTypeString = TestContext.DataRow.ValueIfExists("compareType");
            var tolerance = Tolerance.Parse(TestContext.DataRow["tolerance"].ToString());
            var compareType = string.IsNullOrEmpty(compareTypeString)
                ? expected?.Value.InferType()
                : Type.GetType(compareTypeString);
            if (compareType == null)
            {
                $"compareType == null for {testName}".Log();
            }
            ("Precision parsed:" + tolerance.Precision).Log();

            var actualResult = new MeasurementComparison(expected, actual, tolerance, compareType);

            Assert.AreEqual(TestContext.DataRow["issue"].ToString(), actualResult.OutcomeMessage, testName + "-Issue");
            var resultExpectedValue = TestContext.DataRow.ValueIfExists("resultExpectedValue");
            if (!string.IsNullOrEmpty(resultExpectedValue))
            {
                Assert.AreEqual(resultExpectedValue, actualResult.Value.ExpectedValueOut ?? string.Empty,
                    testName + "-ExpectedValue");
            }
            var resultActualValue = TestContext.DataRow.ValueIfExists("resultActualValue");
            if (!string.IsNullOrEmpty(resultActualValue))
            {
                Assert.AreEqual(resultActualValue, actualResult.Value.ActualValueOut ?? string.Empty,
                    testName + "-ActualValue");
            }
            var resultDelta = TestContext.DataRow.ValueIfExists("resultDelta");
            if (!string.IsNullOrEmpty(resultDelta))
            {
                Assert.AreEqual(resultDelta, actualResult.Value.DeltaOut ?? string.Empty, testName + "-Delta");
            }
            var resultExpectedIsGood = TestContext.DataRow.ValueIfExists("resultExpectedIsGood");
            if (!string.IsNullOrEmpty(resultExpectedIsGood))
            {
                Assert.AreEqual(resultExpectedIsGood.To<bool>(), actualResult.IsGood.ExpectedValueOut.To<bool>(),
                    testName + "-ExpectedIsGood");
            }
            var resultActualIsGood = TestContext.DataRow.ValueIfExists("resultActualIsGood");
            if (!string.IsNullOrEmpty(resultActualIsGood))
            {
                Assert.AreEqual(resultActualIsGood.To<bool>(), actualResult.IsGood.ActualValueOut.To<bool>(),
                    testName + "ActualIsGood");
            }
        }
    }
}