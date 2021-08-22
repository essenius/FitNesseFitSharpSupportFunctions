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
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class ExtensionFunctionsTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("0xcafe", TypeCode.Int32)]
        [DataRow("0x0x", TypeCode.String)]
        [DataRow("1", TypeCode.Int32)]
        [DataRow(int.MaxValue + 1L, TypeCode.Int64)]
        [DataRow("1.1", TypeCode.Decimal)]
        [DataRow("1E+24", TypeCode.Double)]
        [DataRow("true", TypeCode.Boolean)]
        [DataRow("-.", TypeCode.String)]
        [DataRow("x", TypeCode.String)]
        public void ExtensionFunctionsCastToInferredTypeTest(object input, TypeCode expected) =>
            Assert.AreEqual(expected, Type.GetTypeCode(input.ToString().CastToInferredType().GetType()));

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("Infinity", "Infinity")]
        [DataRow("-∞", "-Infinity")]
        [DataRow("5", "5")]
        [DataRow(true, "1")]
        [DataRow("0xfe", "-2")]
        public void ExtensionFunctionsConvertToDoubleTest(object source, string expected) =>
            Assert.AreEqual(expected, source.To<double>().ToString(CultureInfo.InvariantCulture));

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(FormatException), "Could not convert 'x' to Int32")]
        public void ExtensionFunctionsConvertToFormatExceptionTest() => "x".To<int>();

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("5", 5)]
        [DataRow(false, 0)]
        [DataRow("0xff", -1)]
        [DataRow("0xcafe", -13570)]
        [DataRow("0x0cafe", 51966)]
        [DataRow("0x7FFFFFFF", 2147483647)]
        [DataRow("0x80000000", -2147483648)]
        public void ExtensionFunctionsConvertToIntTest(object source, int expected) =>
            Assert.AreEqual(expected, source.To<int>());

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(FormatException), "Could not convert '' to Int32")]
        public void ExtensionFunctionsConvertToInvalidCastExceptionTest() => ExtensionFunctions.To<int>(null);

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("5", 5L)]
        [DataRow("0x080000000", 2147483648L)]
        [DataRow("0xFFFFFFFFFFFF0000", -65536L)]
        public void ExtensionFunctionsConvertToLongTest(object source, long expected) =>
            Assert.AreEqual(expected, source.To<long>());

        [TestMethod]
        [TestCategory("Unit")]
        public void ExtensionFunctionsConvertToTest()
        {
            Assert.AreEqual((uint)51966, "0xcafe".To<uint>());
            Assert.AreEqual(true, "True".To<bool>());
            Assert.AreEqual(636187844967890000L, "2016-12-31T12:34:56.7890".To<DateTime>().Ticks);
            Assert.AreEqual(2D, "2".To<double?>());
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("hi", null)]
        [DataRow("true", null)]
        [DataRow(true, 1d)]
        [DataRow("5", 5d)]
        public void ExtensionFunctionsConvertToWithDefaultTest(object source, double? expected) =>
            Assert.AreEqual(expected, source.ToWithDefault<double?>(null));


        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(0, "A")]
        [DataRow(25, "Z")]
        [DataRow(26, "AA")]
        [DataRow(52, "BA")]
        [DataRow(702, "AAA")]
        public void ExtensionFunctionsExcelColumnNameTest(int columnIndex, string columnName) =>
            Assert.AreEqual(columnName, columnIndex.ToExcelColumnName());

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("Double-Int", "12.5", "13", "System.Double")]
        [DataRow("Double-Bool", "12.5", "true", "System.String")]
        [DataRow("Double-Long", "1e15", "2147483648", "System.Double")]
        [DataRow("Double-String`", "12.5", "Hi1", "System.String")]
        [DataRow("Double-Double", "12.5", "13.2", "System.Double")]
        [DataRow("Int-Long", "12", "2147483648", "System.Int64")]
        [DataRow("Int-Bool", "12", "True", "System.String")]
        [DataRow("Int-Double", "12", "12.1", "System.Double")]
        [DataRow("Int-String", "12", "Hi1", "System.String")]
        [DataRow("Int-Int", "12", "13", "System.Int32")]
        [DataRow("Long-Int", "2147483648", "12", "System.Int64")]
        [DataRow("Long-Long", "2147483648", "2147483648", "System.Int64")]
        [DataRow("Long-Double", "2147483648", "12.5", "System.Double")]
        [DataRow("Long-Bool", "2147483648", "False", "System.String")]
        [DataRow("Long-String", "2147483648", "Hi1", "System.String")]
        [DataRow("Bool-String", "False", "Hi1", "System.String")]
        [DataRow("Bool-Bool", "False", "True", "System.Boolean")]
        [DataRow("Bool-Double", "False", "12.5", "System.String")]
        [DataRow("String-Double", "Hi1", "1e10", "System.String")]
        [DataRow("Double-InfT-NaN", "Infinity", "NaN", "System.Double")]
        [DataRow("Double-InfS-NaN", "∞", "NaN", "System.Double")]
        public void ExtensionFunctionsInferTypeTest(string testCase, string value1, string value2, string expectedType)
        {
            var value1Type = value1.InferType();
            var compatibleType = value2.InferType(value1Type).ToString();
            Assert.AreEqual(expectedType, compatibleType, testCase);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ExtensionFunctionsIsWithinTimeRangeTest()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var noon = today.AddHours(12);

            // Not clear to me how to get the coverage up on this function.
            // I think I have all possible cases covered, but still 2 blocks not covered
            Assert.IsTrue(noon.IsWithinTimeRange(null, null), "noon in null-null (T-T-)");
            Assert.IsFalse(noon.IsWithinTimeRange(null, today), "noon not in null-today (T-FF)");
            Assert.IsTrue(noon.IsWithinTimeRange(null, tomorrow), "noon in null-tomorrow (T-FT)");
            Assert.IsTrue(noon.IsWithinTimeRange(today, null), "noon in today-null (FTT-)");
            Assert.IsFalse(tomorrow.IsWithinTimeRange(today, noon), @"tomorrow not in today-noon (FTFF)");
            Assert.IsTrue(noon.IsWithinTimeRange(today, tomorrow), @"noon in today-tomorrow (FTFT)");
            Assert.IsFalse(noon.IsWithinTimeRange(tomorrow, null), "noon not in tomorrow-null (FF--)");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ExtensionFunctionsToTest()
        {
            var result = "hi".ToWithDefault<double?>(null);
            Assert.IsNull(result);
            Assert.AreEqual(5, "5".ToWithDefault<double?>(null));
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("Math", "Math")]
        [DataRow("System.Int32", "System.Int32")]
        [DataRow("System.Int32", "int")]
        [DataRow("System.String", "string")]
        [DataRow("System.Byte", "byte")]
        public void ExtensionFunctionsTypeNameTest(string type1, string type2) =>
            Assert.AreEqual(type1, type2.TypeName(), $"{type1} => {type2}");

        [TestMethod]
        [TestCategory("Unit")]
        public void ExtensionFunctionsValueOrDefaultTest()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "min", "1" },
                { "date", "2017-01-01" },
                { "double", "1.3" }
            };
            Assert.AreEqual(1, dictionary.ValueOrDefault("min", 100));
            Assert.AreEqual(100, dictionary.ValueOrDefault("max", 100));
            Assert.AreEqual(new DateTime(2017, 1, 1), dictionary.ValueOrDefault<DateTime?>("date", null));
            Assert.IsNull(dictionary.ValueOrDefault<DateTime?>("now", null));
            Assert.AreEqual(1.3, dictionary.ValueOrDefault("double", -1.5));
            Assert.AreEqual(-1.5, dictionary.ValueOrDefault("double1", -1.5));
        }
    }
}
