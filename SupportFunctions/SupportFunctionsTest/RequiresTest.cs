using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class RequiresTest
    {
        [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "'testName' cannot be null in 'RequiresNotNullTestFires'")]
        public void RequiresNotNullTestFires()
        {
            Requires.NotNull(null, "testName");
        }

        [TestMethod]
        public void RequiresNotNullTestOk()
        {
            Requires.NotNull("ok", "testName");
        }

        [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "'testName' cannot be null or empty in 'RequiresNotEmptyTestFires'")]
        public void RequiresNotEmptyTestFires()
        {
            Requires.NotEmpty(string.Empty, "testName");
        }

        [TestMethod]
        public void RequiresNotEmptyestOk()
        {
            Requires.NotEmpty("ok", "testName");
        }

        [TestMethod]
        public void RequiresConditionOk()
        {
            Requires.Condition(true, "True");
        }

        [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "'0 > 1' not met in 'RequiresConditionFails'")]
        public void RequiresConditionFails()
        {
            Requires.Condition(0 > 1, "0 > 1");
        }
    }
}
