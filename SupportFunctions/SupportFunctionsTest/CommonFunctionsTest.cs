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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CommonFunctionsTest
    {
        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsCloneSymbolTest() => Assert.AreEqual(5, CommonFunctions.CloneSymbol(5));

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsConcatTest() => Assert.AreEqual("abc", CommonFunctions.Concat(new[] {"a", "b", "c"}));

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsDateTests()
        {
            Date.ResetDefaultFormat();
            Assert.AreEqual("yyyy'-'MM'-'dd'T'HH':'mm':'ss", CommonFunctions.DateFormat);

            var date = CommonFunctions.Parse("28-Aug-1969");
            Assert.AreEqual(621247104000000000L, CommonFunctions.ToTicks(date));
            var date2 = CommonFunctions.AddDaysTo(-364.5, date);
            Assert.AreEqual("28-Aug-1968 12:00", CommonFunctions.DateFormatted(date2, "dd-MMM-yyyy HH:mm"));
            var date3 = CommonFunctions.AddHoursTo(1.25, date2);
            Assert.AreEqual("1968-08-28T13:15:00", date3.ToString());
            CommonFunctions.DateFormat = @"MMMM dd, yyyy";
            Assert.AreEqual("August 28, 1969", date.ToString());
        }

        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(TypeLoadException), "Could not find static class 'Wrong'")]
        public void CommonFunctionsDoOnMissingClassTest() => CommonFunctions.DoOn("Wrong.Wrong", null);

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find property, field or method 'Wrong' for type 'String'")]
        public void CommonFunctionsDoOnMissingMethodAndInputTest() => CommonFunctions.DoOn("Wrong", null);

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find property, field or method 'Wrong' for type 'Int32' or 'String'")]
        public void CommonFunctionsDoOnMissingMethodForIntTest() => CommonFunctions.DoOn("Wrong", "1");

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find property, field or method 'Wrong' for type 'String'")]
        public void CommonFunctionsDoOnMissingMethodForStringTest() => CommonFunctions.DoOn("Wrong", "hello");

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(MissingMethodException),
             "Could not find static property, field or method 'Math.Wrong'")]
        public void CommonFunctionsDoOnMissingStaticMethodTest() => CommonFunctions.DoOn("Math.Wrong", null);

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentNullException),
             "Value cannot be null.\r\nParameter name: function")]
        public void CommonFunctionsDoOnNullMethodTest() => CommonFunctions.DoOn(null, null);

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsDoOnTest()
        {
            const int maxInt = int.MaxValue;
            var maxIntString = maxInt.ToString();
            var tooBigForInt = (1L + maxInt).ToString();
            const string testString = @"abcdef";
            Assert.AreEqual("de", CommonFunctions.DoOnWithParams("Substring", testString, "3", "2"));
            Assert.AreEqual("def", CommonFunctions.DoOnWithParam("Substring", testString, "3"));
            Assert.AreEqual(6, CommonFunctions.DoOn("Length", testString));
            Assert.AreEqual(10, CommonFunctions.DoOn("Length", maxIntString));
            Assert.AreEqual("2147483647", CommonFunctions.DoOnWithParams("ToString", maxIntString));
            Assert.AreEqual("47", CommonFunctions.DoOnWithParams("Substring", maxIntString, "8"));
            Assert.AreEqual("System.Int32", CommonFunctions.DoOnWithParams("GetType", maxIntString).ToString());
            Assert.AreEqual("System.Int64", CommonFunctions.DoOnWithParams("GetType", tooBigForInt).ToString());
            Assert.AreEqual(false, CommonFunctions.DoOnWithParams("Contains", testString, "dg"));
            Assert.AreEqual(4.0, CommonFunctions.DoOnWithParams("Math.Sqrt", "16.0"));
            Assert.AreEqual((byte)255, CommonFunctions.DoOn("byte.MaxValue", null));
            Assert.AreEqual(Math.PI, CommonFunctions.DoOn("Math.PI", string.Empty));
            Assert.AreEqual(string.Empty, CommonFunctions.DoOn("Empty", null));
            Assert.AreEqual(-1M, CommonFunctions.Do("Decimal.MinusOne"));
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsEchoDictTest()
        {
            var dict = new Dictionary<string, string>
            {
                {"a", "b"},
                {"c", "d"}
            };
            Assert.AreEqual(dict, CommonFunctions.Echo(dict));
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsEchoTest() => Assert.AreEqual("abc", CommonFunctions.Echo("abc"));

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsEvaluateAsTest()
        {
            Assert.AreEqual(32769, CommonFunctions.EvaluateAs("32767+2", "int"), "32769 + 2 = 32769");
            Assert.AreEqual(5L, CommonFunctions.EvaluateAs("2 + 3", "long"), "2 + 3 = 5");
            Assert.AreEqual("23", CommonFunctions.EvaluateAs("'2' + '3'", "string"), " '2' + '3' = '23'");
            Assert.AreEqual(0.75, CommonFunctions.EvaluateAs("3 / 4", "double"), "3 / 4 = 0.75");
            Assert.AreEqual(9999999999999999999999999999M,
                CommonFunctions.EvaluateAs("9999999999999999999999999998. + 1", "decimal"),
                "99999999999999999999999998. + 1 = 99999999999999999999999999");
            Assert.IsTrue((bool)CommonFunctions.EvaluateAs("6 > 5", "bool"), "6 > 5");
            Assert.AreEqual(2, CommonFunctions.EvaluateAs("8 % 3", "System.Int32"), "8 % 3 = 2");
            Assert.AreEqual(new DateTime(1995, 5, 9), CommonFunctions.EvaluateAs("#9-May-1995#", "System.DateTime"),
                "#9-May-1995#");
            Assert.AreEqual(Date.Parse("22-Oct-1999").ToString(),
                CommonFunctions.EvaluateAs("#22-Oct-1999#", "Date").ToString(), "#22-Oct-1995#");
        }

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException),
             "Type 'WrongType' not recognized.\r\nParameter name: type")]
        public void CommonFunctionsEvaluateAsThrowsExceptionsTest() => CommonFunctions.EvaluateAs(string.Empty, "WrongType");

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsEvaluateAsWithParamTest()
        {
            var parameters = new[]
            {
                "x=5",
                "y=3",
                "z=x*2",
                "xx=x*x"
            };
            Assert.AreEqual(15, CommonFunctions.EvaluateAsWithParams("x*y", "int", parameters));
            Assert.AreEqual(10, CommonFunctions.EvaluateAsWithParams("z", "int", parameters));
            Assert.AreEqual(125, CommonFunctions.EvaluateAsWithParams("xx*x", "int", parameters));
            Assert.AreEqual(625.0, CommonFunctions.EvaluateAsWithParams("xx*xx", "double", parameters));
            // can't use powers or other math functions, unfortunately. Just the basic stuff
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsEvaluateTest()
        {
            Assert.AreEqual(5d, CommonFunctions.Calculate("2 + 3"), "2 + 3 = 5");
            Assert.AreEqual(2d, CommonFunctions.Calculate("6 / 3"), "6 / 3 = 2");
            Assert.AreEqual(2d, CommonFunctions.Calculate("8 % 3"), "8 % 3 = 2");
            Assert.AreEqual(3d, CommonFunctions.Calculate("len('abc')"), "len('abc') = 3");
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsIsTrueTest()
        {
            Assert.AreEqual(true, CommonFunctions.IsTrue("2 < 3"), "2 < 3");
            Assert.AreEqual(true, CommonFunctions.IsTrue("(true and not false) or false"),
                "(true and not(not(true)) or false");
            Assert.IsFalse(CommonFunctions.IsTrue("len('ab') = 3"), "len('ab') = 3");
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsLeftmostTest()
        {
            Assert.AreEqual(@"abcd", CommonFunctions.LeftmostOf(4, @"abcdefg"));
            Assert.AreEqual(@"abcd", CommonFunctions.LeftmostOf(5, @"abcd"));
            Assert.AreEqual("", CommonFunctions.LeftmostOf(4, ""));
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsParseFormattedTest()
        {
            var date = CommonFunctions.ParseFormatted("12-11-2010 9:8:7", "MM-dd-yyyy H:m:s");
            Assert.AreEqual("11/12/10 09:08:07 AM", date.Formatted("dd/MM/yy hh:mm:ss tt"));
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsRightmostTest()
        {
            Assert.AreEqual(@"defg", CommonFunctions.RightmostOf(4, @"abcdefg"));
            Assert.AreEqual(@"abcd", CommonFunctions.RightmostOf(5, @"abcd"));
            Assert.AreEqual("", CommonFunctions.RightmostOf(4, ""));
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsTicksTest()
        {
            var firstCheck = UniqueDateTime.NowTicks;
            ("firstCheck:" + firstCheck).Log();
            var stringDate = new DateTime(firstCheck).ToString(@"dd-MMM-yyyy HH:mm:ss.fffffff");
            stringDate.Log();
            var ticks1 = CommonFunctions.Ticks;
            ("ticks1:" + ticks1 + " (" + (ticks1 - firstCheck) + ")").Log();
            var secondCheck = UniqueDateTime.NowTicks;
            ("secondCheck:" + secondCheck + " (" + (secondCheck - ticks1) + ")").Log();
            Assert.IsTrue(firstCheck < ticks1, "firstCheck < ticks1");
            Assert.IsTrue(ticks1 < secondCheck, "ticks1 < secondCheck");
            var ticksElapsed1 = CommonFunctions.TicksSince(Date.Parse(stringDate));
            var ticksElapsed2 = CommonFunctions.TicksSince(Date.Parse(firstCheck.ToString()));
            ("ticks elapsed 1:" + ticksElapsed1).Log();
            ("ticks elapsed 2:" + ticksElapsed2).Log();

            Assert.IsTrue(ticksElapsed1 > 0);
            Assert.IsTrue(ticksElapsed2 > ticksElapsed1);
            var ticksBetween = CommonFunctions.TicksBetweenAnd(Date.Parse(firstCheck.ToString()),Date.Parse(secondCheck.ToString()));
            ("Ticks between: " + ticksBetween).Log();
            Assert.IsTrue(ticksBetween > 0);
            var ticks2 = CommonFunctions.Ticks;
            ("ticks2:" + ticks2 + " (" + (ticks2 - secondCheck) + ")").Log();
            Assert.IsTrue(ticks1 < ticks2, "ticks1 < ticks2");
            Assert.IsTrue(ticks2 - firstCheck > ticksElapsed1, "ticks2 - firstCheck > ticksElapsed");
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsToLocalTest()
        {
            var date = new Date("1/1/2018");
            var utcDate = CommonFunctions.ToLocal(date).DateTime.Ticks;
            var offset = TimeZoneInfo.Local.GetUtcOffset(date.DateTime);
            Assert.AreEqual(offset.Ticks, utcDate - date.Ticks);
            Console.WriteLine(offset.Ticks);
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsToUtcTest()
        {
            var date = new Date("1/1/2018");
            var utcDate = CommonFunctions.ToUtc(date).DateTime.Ticks;
            var offset = TimeZoneInfo.Local.GetUtcOffset(date.DateTime);
            Assert.AreEqual(offset.Ticks, date.Ticks - utcDate);
            Console.WriteLine(offset.Ticks);
        }

        [TestMethod, TestCategory("Unit")]
        public void CommonFunctionsTrimTest() => Assert.AreEqual("abc", CommonFunctions.Trim("  abc   "));
    }
}