﻿// Copyright 2015-2020 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class EchoSupportTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void EchoSupportEchoDictTest()
        {
            var dict = new Dictionary<string, string>
            {
                { "a", "b" },
                { "c", "d" }
            };
            Assert.AreEqual(dict, CommonFunctions.Echo(dict));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void EchoSupportEchoStringTest()
        {
            Assert.AreEqual("abc", CommonFunctions.Echo("abc"));
#pragma warning disable 618
            Assert.AreEqual("abc", EchoSupport.Echo("abc"));
#pragma warning restore 618
        }
    }
}
