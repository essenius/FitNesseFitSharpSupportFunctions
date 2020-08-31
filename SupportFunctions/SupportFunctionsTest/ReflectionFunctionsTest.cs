using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class ReflectionFunctionsTest
    {
        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(TypeLoadException), "Could not find static class 'Wrong'")]
        public void ReflectionFunctionsGetOfMissingClassTest() => ReflectionFunctions.GetOf("Wrong.Wrong", null);

        // This fails because substring can't deal with Decimals, and the fallback to doubles fails too.
        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find property, field or method 'substring' for type 'String'")]
        public void ReflectionFunctionsGetOfMethodWithoutDecimalParametersTest() => ReflectionFunctions.GetOf("substring(1.0)", "a");

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find property, field or method 'Wrong' for type 'String'")]
        public void ReflectionFunctionsGetOfMissingMethodAndInputTest() => ReflectionFunctions.GetOf("Wrong", null);

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find property, field or method 'Wrong' for type 'Int32' or 'String'")]
        public void ReflectionFunctionsGetOfMissingMethodForIntTest() => ReflectionFunctions.GetOf("Wrong", "1");

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find property, field or method 'Wrong' for type 'String'")]
        public void ReflectionFunctionsGetOfMissingMethodForStringTest() => ReflectionFunctions.GetOf("Wrong", "hello");

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMemberException),
             "Could not find static property, field or method 'Math.Wrong'")]
        public void ReflectionFunctionsGetOfMissingStaticMethodTest() => ReflectionFunctions.GetOf("Math.Wrong", null);

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException),
             "'member' cannot be null or empty in 'GetOf'")]
        public void ReflectionFunctionsGetOfNullMethodTest() => ReflectionFunctions.GetOf(null, null);


        [TestMethod, TestCategory("Unit")]
        public void ReflectionFunctionsGetOfTest()
        {
            Assert.AreEqual(5, ReflectionFunctions.GetOf("Length", "hello"), "get Property of string");
            Assert.AreEqual("de", ReflectionFunctions.GetOf("Substring(3,2)", "abcdef"), "Method with 2 parameters");
            var maxInt = ReflectionFunctions.Get("Int32.MaxValue");
            Assert.AreEqual("47", ReflectionFunctions.GetOf("Substring   ( 8 )", maxInt.ToString()), "Method with ona param, and spaces");
            Assert.AreEqual("System.Int32", ReflectionFunctions.GetOf("GetType", maxInt.ToString()).ToString(), "Method without parameters");
            Assert.AreEqual(7.0, ReflectionFunctions.GetOf("Math.Sqrt", "49"), "Method with parameter from value");
            Assert.AreEqual(3.14M, ReflectionFunctions.GetOf("Math.Round(2)", "3.1415"), "Method with a paramater and a value");

            // Provide a decimal value into a function that expects a double to validate it converts
            Assert.AreEqual(3.0, ReflectionFunctions.GetOf("Math.Log10", "1000.0"), "Log10 of a decimal calculated right");
        }

        [TestMethod, TestCategory("Unit")]
        public void ReflectionFunctionsGetTest()
        {
            Assert.AreEqual(int.MaxValue, ReflectionFunctions.Get("Int.MaxValue"), "Get Fleld from Int");
            Assert.AreEqual(7.0, ReflectionFunctions.Get("Math.Sqrt(49)"), "Get parameter from the parameter list");
            Assert.AreEqual(true, ReflectionFunctions.Get("String.IsNullOrEmpty()"), "Evaluate string mathod");
            var pi = ReflectionFunctions.Get("math.pi");
            Assert.AreEqual(3.14M, ReflectionFunctions.Get($"Math.Round ( {pi}, 2 )"), "Use method with two parameters");
            Assert.AreEqual("True", ReflectionFunctions.Get("boolean.TrueString"), "see if we can use 'boolean' instead of 'bool");
        }
    }
}
