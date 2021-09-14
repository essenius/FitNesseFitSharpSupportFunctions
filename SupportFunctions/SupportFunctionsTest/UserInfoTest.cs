// Copyright 2015-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class UserInfoTest
    {
        // This test causes a System.AppDomainUnloadedException in the test results

        [TestMethod]
        [TestCategory("Integration")]
        public void UserInfoBaseTest()
        {
            Debug.Print($"User name: <{UserInfo.UserName}>");
            Assert.IsFalse(string.IsNullOrEmpty(UserInfo.UserName), "User Name not empty");
            Debug.Print($"Display name: <{UserInfo.DisplayName}>");
            Assert.IsFalse(string.IsNullOrEmpty(UserInfo.DisplayName), "DisplayName not empty");
        }
    }
}
