﻿// Copyright 2015-2021 Rik Essenius
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
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class StringFunctionsTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("  MAIN  ", true)]
        [DataRow("Main", true)]
        [DataRow("  Man  ", false)]
        public void StringFunctionsEqualsIgnoreCaseTest(string input, bool expected) =>
            Assert.AreEqual(expected, input.EqualsIgnoreCase("main"));
    }
}
