﻿// Copyright 2017-2021 Rik Essenius
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
        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsCastToInferredTypeTest()
        {
            var longValue = (1L + int.MaxValue).ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(typeof(int), "1".CastToInferredType().GetType(), "Cast to int");
            Assert.AreEqual(typeof(long), longValue.CastToInferredType().GetType(), "Cast to long");
            Assert.AreEqual(typeof(decimal), "1.1".CastToInferredType().GetType(), "Cast to decimal");
            Assert.AreEqual(typeof(double), "1E+24".CastToInferredType().GetType(), "Cast to double");
            Assert.AreEqual(typeof(bool), "true".CastToInferredType().GetType(), "Cast to bool");
            Assert.AreEqual(typeof(string), "-.".CastToInferredType().GetType(), "Cast to string 1");
            Assert.AreEqual(typeof(string), "x".CastToInferredType().GetType(), "Cast to string 2");
        }

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(FormatException), "Could not convert 'x' to Int32")]
        public void ExtensionFunctionsConvertToFormatExceptionTest() => "x".To<int>();

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(FormatException), "Could not convert '' to Int32")]
        public void ExtensionFunctionsConvertToInvalidCastExceptionTest() => ExtensionFunctions.To<int>(null);

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsConvertToTest()
        {
            var infinityInvariant = double.Parse("Infinity", CultureInfo.InvariantCulture);
            Assert.AreEqual("Infinity", infinityInvariant.To<string>());
            var infinityCurrent = double.Parse("-∞", CultureInfo.CurrentCulture);
            Assert.AreEqual("-Infinity", infinityCurrent.To<string>());
            Assert.AreEqual(5, "5".To<int>());
            Assert.AreEqual(5L, "5".To<long>());
            Assert.AreEqual(5.0, "5".To<double>());
            Assert.AreEqual(true, "true".To<bool>());
            Assert.AreEqual(1, true.To<double>());
            Assert.AreEqual(1, true.To<double>());
            Assert.AreEqual(636187844967890000L, "2016-12-31T12:34:56.7890".To<DateTime>().Ticks);
            Assert.AreEqual(2D, "2".To<double?>());
            Assert.AreEqual(null, "hi".ToWithDefault<double?>(null));
            Assert.AreEqual(null, "true".ToWithDefault<double?>(null));
            Assert.AreEqual(1, true.ToWithDefault<double?>(null));
            Assert.AreEqual(5, "5".ToWithDefault<double?>(null));
        }

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsExcelColumnNameTest()
        {
            Assert.AreEqual("A", 0.ToExcelColumnName());
            Assert.AreEqual("Z", 25.ToExcelColumnName());
            Assert.AreEqual("AA", 26.ToExcelColumnName());
            Assert.AreEqual("BA", 52.ToExcelColumnName());
            Assert.AreEqual("AAA", 702.ToExcelColumnName());
        }

        [DataTestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsTypeNameTest()
        {
            Assert.AreEqual("Math", "Math".TypeName(), "Math => Math");
            Assert.AreEqual("System.Int32", "System.Int32".TypeName(), "int => System.Int32");
            Assert.AreEqual("System.Int32", "int".TypeName(), "int => System.Int32");
            Assert.AreEqual("System.String", "string".TypeName(), "string => System.String");
            Assert.AreEqual("System.Byte", "byte".TypeName(), "string => System.String");
        }

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsToTest()
        {
            var result = "hi".ToWithDefault<double?>(null);
            Assert.IsNull(result);
            Assert.AreEqual(5, "5".ToWithDefault<double?>(null));
        }

        [TestMethod, TestCategory("Unit")]
        public void ExtensionFunctionsValueOrDefaultTest()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"min", "1"},
                {"date", "2017-01-01"},
                {"double", "1.3"}
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
