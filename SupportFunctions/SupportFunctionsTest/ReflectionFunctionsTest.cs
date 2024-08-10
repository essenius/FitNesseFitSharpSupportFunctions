// Copyright 2020-2024 Rik Essenius
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
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class ReflectionFunctionsTest
    {
        // This fails because substring can't deal with Decimals, and the fallback to doubles fails too.
        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(MissingMethodException),
            "Could not find property, field or method 'substring' for type 'String'")]
        public void ReflectionFunctionsGetOfMethodWithoutDecimalParametersTest() => ReflectionFunctions.GetOf("substring(1.0)", "a");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(TypeLoadException), "Could not find static class 'Wrong'")]
        public void ReflectionFunctionsGetOfMissingClassTest() => ReflectionFunctions.GetOf("Wrong.Wrong", null);

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(MissingMethodException),
            "Could not find property, field or method 'Wrong' for type 'String'")]
        public void ReflectionFunctionsGetOfMissingMethodAndInputTest() => ReflectionFunctions.GetOf("Wrong", null);

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(MissingMethodException),
            "Could not find property, field or method 'Wrong' for type 'Int32' or 'String'")]
        public void ReflectionFunctionsGetOfMissingMethodForIntTest() => ReflectionFunctions.GetOf("Wrong", "1");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(MissingMethodException),
            "Could not find property, field or method 'Wrong' for type 'String'")]
        public void ReflectionFunctionsGetOfMissingMethodForStringTest() => ReflectionFunctions.GetOf("Wrong", "hello");

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(MissingMemberException),
            "Could not find static property, field or method 'Math.Wrong'")]
        public void ReflectionFunctionsGetOfMissingStaticMethodTest() => ReflectionFunctions.GetOf("Math.Wrong", null);

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(ArgumentNullException),
            "'member' cannot be null in 'GetOf'")]
        public void ReflectionFunctionsGetOfNullMethodTest() => ReflectionFunctions.GetOf(null, null);

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("Length", "hello", "5", "get Property of string")]
        [DataRow("Substring(3,2)", @"abcdef", "de", "Method with 2 parameters")]
        [DataRow("Substring   ( 8 )", int.MaxValue, "47", "Method with one param, and spaces")]
        [DataRow("GetType", int.MaxValue, "System.Int32", "Method without parameters")]
        [DataRow("Math.Sqrt", "49", "7", "Method with parameter from value")]
        [DataRow("Math.Round(2)", "3.1415", "3.14", @"Method with a paramater and a value")]
        [DataRow("Math.Log10", "1000.0", "3", "Log10 (expecting double) of a decimal calculated right")]
        [DataRow("Math.Sqrt", "0x10", "4", "Method with hex parameter")]
        public void ReflectionFunctionsGetOfTest(string method, object inputObject, object expected, string message) =>
            Assert.AreEqual(expected, ReflectionFunctions.GetOf(method, inputObject.ToString()).ToString(), message);

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("Int.MaxValue", int.MaxValue, "Get Field from Int")]
        [DataRow("Math.Sqrt(49)", "7", "Get parameter from the parameter list")]
        [DataRow("String.IsNullOrEmpty()", "True", "Evaluate string method")]
        [DataRow("Math.Round( 3.14159265, 2 )", "3.14", "Use method with two parameters")]
        [DataRow("boolean.TrueString", "True", "see if we can use 'boolean' instead of 'bool")]
        [DataRow("Math.Sqrt(0x19)", "5", "Method with hex parameter")]
        public void ReflectionFunctionsGetTest(string expression, object expected, string message) =>
            Assert.AreEqual(expected.ToString(), ReflectionFunctions.Get(expression).ToString(), message);

        [TestMethod]
        public void ReflectionFunctionsGetWithParamsTest()
        {
            Assert.AreEqual(8.0, ReflectionFunctions.GetWithParams("Math.Sqrt", new object[] { "64" }), "ok");
        }
    }
}
