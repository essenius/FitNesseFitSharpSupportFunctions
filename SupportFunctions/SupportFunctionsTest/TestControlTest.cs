// Copyright 2015-2020 Rik Essenius
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
    public class TestControlTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void TestControlStopSuiteExceptionIfDoesNotThrowOnFalse() => TestControl.StopSuiteIf("false");

        [TestMethod]
        [TestCategory("Unit")]
        public void TestControlStopSuiteExceptionIfNotDoesNotThrowOnTrue() => TestControl.StopSuiteIfNot("true");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopSuiteException), "'false' evaluates to stop condition 'False'")]
        public void TestControlStopSuiteExceptionIfNotThrowsOnFalse() => TestControl.StopSuiteIfNot("false");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopSuiteException), "Could not parse '$ok' to boolean")]
        public void TestControlStopSuiteExceptionIfNotThrowsOnNonBool() => TestControl.StopSuiteIfNot("$ok");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopSuiteException), "Could not parse '$ok' to boolean")]
        public void TestControlStopSuiteExceptionIfThrowsOnNonBool() => TestControl.StopSuiteIf("$ok");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopSuiteException), "'true' evaluates to stop condition 'True'")]
        public void TestControlStopSuiteExceptionIfThrowsOnTrue() => TestControl.StopSuiteIf("true");

        [TestMethod]
        [TestCategory("Unit")]
        public void TestControlStopTestExceptionIfNotDoesNotThrowOnTrue() => TestControl.StopTestIfNot("true");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException), "'false' evaluates to stop condition 'False'")]
        public void TestControlStopTestExceptionIfNotThrowsOnFalse() => TestControl.StopTestIfNot("false");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException), "Could not parse '$ok' to boolean")]
        public void TestControlStopTestExceptionIfNotThrowsOnNonBool() => TestControl.StopTestIfNot("$ok");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException), "Could not parse '$ok' to boolean")]
        public void TestControlStopTestExceptionIfThrowsOnNonBool() => TestControl.StopTestIf("$ok");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException), "'true' evaluates to stop condition 'True'")]
        public void TestControlStopTestExceptionIfThrowsOnTrue() => TestControl.StopTestIf("true");

        [TestMethod]
        [TestCategory("Unit")]
        public void TestStopTestExceptionIfDoesNotThrowOnFalse() => TestControl.StopTestIf("false");
    }
}
