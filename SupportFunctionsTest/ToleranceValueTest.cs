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

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class ToleranceValueTest
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit"), DeploymentItem("SupportFunctionsTest\\TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "ToleranceValue", DataAccessMethod.Sequential)]
        public void ToleranceValueParseTest()
        {
            var testName = "Test: " + TestContext.DataRow["testcase"];
            var range = TestContext.DataRow["range"].ToNullableDouble();
            var toleranceValue = ToleranceValue.Parse(TestContext.DataRow["tolerance"].ToString());
            toleranceValue.DataRange = range;
            var expectedValue = TestContext.DataRow["expectedOutput"].To<double>();
            Assert.AreEqual(expectedValue, toleranceValue.AppliedValue, testName);
        }
    }
}