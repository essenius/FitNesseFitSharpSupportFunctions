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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class RequiresTest
    {
        [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentNullException), "'testName' cannot be null in 'RequiresNotNullTestFires'")]
        public void RequiresNotNullTestFires() => Requires.NotNull(null, "testName");

        [TestMethod]
        public void RequiresNotNullTestOk() => Requires.NotNullOrEmpty("ok", "testName");

        [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "'testName' cannot be empty in 'RequiresNotNullOrEmptyTestFiresOnEmpty'")]
        public void RequiresNotNullOrEmptyTestFiresOnEmpty() => Requires.NotNullOrEmpty(string.Empty, "testName");

        [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentNullException), "'testName' cannot be null in 'RequiresNotNullOrEmptyTestFiresOnNull'")]
        public void RequiresNotNullOrEmptyTestFiresOnNull() => Requires.NotNullOrEmpty(null, "testName");

        [TestMethod]
        public void RequiresNotEmptyestOk() => Requires.NotNullOrEmpty("ok", "testName");

        [TestMethod]
        public void RequiresConditionOk() => Requires.Condition(true, "True");

        [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "'0 > 1' not met in 'RequiresConditionFails'")]
        public void RequiresConditionFails() => Requires.Condition(0 > 1, "0 > 1");
    }
}
