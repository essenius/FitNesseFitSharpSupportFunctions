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
    public class ValueComparisonTest
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit"), DeploymentItem("SupportFunctionsTest\\TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "ValueCompare", DataAccessMethod.Sequential)]
        public void ValueComparisonTest1()
        {
            var testName = "Test " + TestContext.DataRow["testcase"];
            var expected = TestContext.DataRow["expected"].ToString();
            if (string.IsNullOrEmpty(expected)) expected = null;

            var actual = TestContext.DataRow["actual"].ToString();
            if (string.IsNullOrEmpty(actual)) actual = null;

            var tolerance = Tolerance.Parse(TestContext.DataRow["tolerance"].ToString());
            ("Precision parsed:" + tolerance.Precision).Log();
            var issue = (CompareOutcome)Enum.Parse(typeof(CompareOutcome), TestContext.DataRow["issue"].ToString());

            var compareTypeString = TestContext.DataRow.ValueIfExists("compareType");
            var compareType = string.IsNullOrEmpty(compareTypeString) ? null : Type.GetType(compareTypeString);

            var comparison = new ValueComparison(expected, actual, tolerance, compareType);
            Assert.AreEqual(issue, comparison.Outcome, testName + "-Issue");
            var resultExpected = TestContext.DataRow.ValueIfExists("resultExpected");
            if (!string.IsNullOrEmpty(resultExpected))
            {
                Assert.AreEqual(resultExpected, comparison.ExpectedValueOut ?? string.Empty, testName + "-Expected");
            }
            var resultActual = TestContext.DataRow.ValueIfExists("resultActual");
            if (!string.IsNullOrEmpty(resultActual))
            {
                Assert.AreEqual(resultActual, comparison.ActualValueOut ?? string.Empty, testName + "-Actual");
            }
            var resultDelta = TestContext.DataRow.ValueIfExists("resultDelta");
            if (!string.IsNullOrEmpty(resultDelta))
            {
                Assert.AreEqual(resultDelta, comparison.DeltaOut ?? string.Empty, testName + "-Delta");
            }
        }
    }
}