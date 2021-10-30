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
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CommonFunctionsTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsCalculateTest()
        {
            Assert.AreEqual(5, CommonFunctions.Calculate("2 + 3"), "2 + 3 = 5");
            Assert.AreEqual(2.5, CommonFunctions.Calculate("5 / 2"), "5 / 2 = 2.5");
            Assert.AreEqual(2d, CommonFunctions.Calculate("6 / 3"), "6 / 3 = 2");
            Assert.AreEqual(2, CommonFunctions.Calculate("8 % 3"), "8 % 3 = 2");
            Assert.AreEqual(3, CommonFunctions.Calculate("len('abc')"), "len('abc') = 3");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsCloneSymbolTest() => Assert.AreEqual(5, CommonFunctions.CloneSymbol(5));

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsConcatenateTest() => 
            Assert.AreEqual("abc", CommonFunctions.Concatenate(new[] { "a", "b", "c" }));

        [Obsolete("Use Concatenate instead")]
        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsConcatTest() => Assert.AreEqual("ab", CommonFunctions.Concat(new[] { "a", "b" }));

        [TestMethod]
        [TestCategory("Unit")]
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

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsEchoDictTest()
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
        public void CommonFunctionsEchoTest() => Assert.AreEqual("abc", CommonFunctions.Echo("abc"));

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsEscapeRegexTest()
        {
            Assert.AreEqual(@"a\\b", CommonFunctions.RegexEscape(@"a\b"));
            Assert.AreEqual(@"a\b", CommonFunctions.RegexUnescape(@"a\\b"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsEvaluateAsTest()
        {
            Assert.AreEqual(32769, CommonFunctions.EvaluateAs("32767+2", "int"), "32767 + 2 = 32769");
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
                CommonFunctions.EvaluateAs("#22-Oct-1999#", "Date").ToString(), "#22-Oct-1999#");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(ArgumentException),
#if NET5_0
        "Type 'WrongType' not recognized. (Parameter 'type')")]
#else
            "Type 'WrongType' not recognized.\r\nParameter name: type")]
#endif
        public void CommonFunctionsEvaluateAsThrowsExceptionsTest() => 
            CommonFunctions.EvaluateAs(string.Empty, "WrongType");

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsEvaluateAsWithParamTest()
        {
            // can't use powers or other math functions, unfortunately. Just the basic stuff
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
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsEvaluateTest()
        {
            Assert.AreEqual(32769, CommonFunctions.Evaluate("32767+2"), "32769 + 2 = 32769");
            Assert.AreEqual("23", CommonFunctions.Evaluate("'2' + '3'"), " '2' + '3' = '23'");
            Assert.AreEqual(0.75, CommonFunctions.Evaluate("3 / 4"), "3 / 4 = 0.75");
            Assert.AreEqual(9999999999999999999999999999M,
                CommonFunctions.Evaluate("9999999999999999999999999998. + 1"),
                "99999999999999999999999998. + 1 = 99999999999999999999999999");
            Assert.IsTrue((bool)CommonFunctions.Evaluate("6 > 5"), "6 > 5");
            Assert.AreEqual(2, CommonFunctions.Evaluate("8 % 3"), "8 % 3 = 2");
            Assert.AreEqual(new DateTime(1995, 5, 9), CommonFunctions.Evaluate("#9-May-1995#"), "#9-May-1995#");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsEvaluateWithParamTest()
        {
            var parameters = new[]
            {
                "x=5",
                "xx=x*x"
            };
            Assert.AreEqual(15, CommonFunctions.EvaluateWithParams("x*3", parameters));
            Assert.AreEqual(125, CommonFunctions.EvaluateWithParams("xx*x", parameters));
            Assert.AreEqual(2.5, CommonFunctions.EvaluateWithParams("xx/10", parameters));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void CommonFunctionsGetOfExceptionTest()
        {
            ReflectionFunctions.Get(string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsIsTrueTest()
        {
            Assert.AreEqual(true, CommonFunctions.IsTrue("2 < 3"), "2 < 3");
            Assert.AreEqual(true, CommonFunctions.IsTrue("(true and not not true) or false"),
                "(true and not(not(true)) or false");
            Assert.IsFalse(CommonFunctions.IsTrue("len('ab') = 3"), "len('ab') <> 3");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsLeftmostTest()
        {
            Assert.AreEqual(@"abcd", CommonFunctions.LeftmostOf(4, @"abcdefg"));
            Assert.AreEqual(@"abcd", CommonFunctions.LeftmostOf(5, @"abcd"));
            Assert.AreEqual("", CommonFunctions.LeftmostOf(4, ""));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsParseFormattedTest()
        {
            var date = CommonFunctions.ParseFormatted("12-11-2010 9:8:7", "MM-dd-yyyy H:m:s");
            Assert.AreEqual("11/12/10 09:08:07 AM", date.Formatted("dd/MM/yy hh:mm:ss tt"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsRightmostTest()
        {
            Assert.AreEqual(@"defg", CommonFunctions.RightmostOf(4, @"abcdefg"));
            Assert.AreEqual(@"abcd", CommonFunctions.RightmostOf(5, @"abcd"));
            Assert.AreEqual("", CommonFunctions.RightmostOf(4, ""));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsTicksTest()
        {
            var firstCheck = UniqueDateTime.NowTicks;
            ("firstCheck:" + firstCheck).Log();
            var stringDate = new DateTime(firstCheck)
                .ToString(@"dd-MMM-yyyy HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            stringDate.Log();
            var ticks1 = CommonFunctions.Ticks;
            ("ticks1:" + ticks1 + " (" + (ticks1 - firstCheck) + ")").Log();
            var secondCheck = UniqueDateTime.NowTicks;
            ("secondCheck:" + secondCheck + " (" + (secondCheck - ticks1) + ")").Log();
            Assert.IsTrue(firstCheck < ticks1, "firstCheck < ticks1");
            Assert.IsTrue(ticks1 < secondCheck, "ticks1 < secondCheck");
            var ticksElapsed1 = CommonFunctions.TicksSince(Date.Parse(stringDate));
            var ticksElapsed2 = CommonFunctions.TicksSince(Date.Parse(firstCheck.To<string>()));
            ("ticks elapsed 1:" + ticksElapsed1).Log();
            ("ticks elapsed 2:" + ticksElapsed2).Log();

            Assert.IsTrue(ticksElapsed1 > 0);
            Assert.IsTrue(ticksElapsed2 > ticksElapsed1);
            var ticksBetween = CommonFunctions.TicksBetweenAnd(
                Date.Parse(firstCheck.To<string>()), 
                Date.Parse(secondCheck.To<string>())
            );
            ("Ticks between: " + ticksBetween).Log();
            Assert.IsTrue(ticksBetween > 0);
            var ticks2 = CommonFunctions.Ticks;
            ("ticks2:" + ticks2 + " (" + (ticks2 - secondCheck) + ")").Log();
            Assert.IsTrue(ticks1 < ticks2, "ticks1 < ticks2");
            Assert.IsTrue(ticks2 - firstCheck > ticksElapsed1, "ticks2 - firstCheck > ticksElapsed");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsToLocalTest()
        {
            var date = new Date("1/1/2018");
            var utcDate = CommonFunctions.ToLocal(date).DateTime.Ticks;
            var offset = TimeZoneInfo.Local.GetUtcOffset(date.DateTime);
            Assert.AreEqual(offset.Ticks, utcDate - date.Ticks);
            Console.WriteLine(offset.Ticks);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsToUtcTest()
        {
            var date = new Date("1/1/2018");
            var utcDate = CommonFunctions.ToUtc(date).DateTime.Ticks;
            var offset = TimeZoneInfo.Local.GetUtcOffset(date.DateTime);
            Assert.AreEqual(offset.Ticks, date.Ticks - utcDate);
            Console.WriteLine(offset.Ticks);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CommonFunctionsTrimTest() => 
            Assert.AreEqual("abc", CommonFunctions.Trim("  abc   "));
    }
}
