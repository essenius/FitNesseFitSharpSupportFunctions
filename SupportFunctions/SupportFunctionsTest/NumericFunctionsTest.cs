// Copyright 2017-2023 Rik Essenius
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
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class NumericFunctionsTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(0.1234, 4)]
        [DataRow("test", 0)]
        [DataRow(1000, 0)]
        [DataRow(1.234E+5, 0)]
        [DataRow(1.234E-4, 7)]
        [DataRow(1.234E-7, 10)]
        [DataRow(1.234E-70, 73)]
        [DataRow(1.234567E+70, 0)]
        public void NumericFunctionsFractionalDigitsTest(object value, int digits) =>
            Assert.AreEqual(digits, value.FractionalDigits());

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(0.1 + 0.2, 0.3, true, "0.1 + 0.2 = 0.3")]
        [DataRow(-0D, +0D, true, "-0 = +0")]
        [DataRow(1.0, -1.0, false, "1.0 != -1.0")]
        [DataRow(0.3333333333, 0.33333333333, false, "0.3333333333 != 0.33333333333 (one decimal more)")]
        public void NumericFunctionsHasMinimalDifferenceWithSimpleTest(double a, double b, bool expected, string description) =>
            Assert.AreEqual(expected, a.HasMinimalDifferenceWith(b), description);

        [TestMethod]
        [TestCategory("Unit")]
        public void NumericFunctionsHasMinimalDifferenceWithTest()
        {
            const double value1 = .1 * 10;
            var value2 = 0D;
            for (var ctr = 0; ctr < 10; ctr++)
            {
                value2 += .1;
            }

            Assert.IsTrue(value1.HasMinimalDifferenceWith(value2));

            Assert.IsTrue(9.87654321E100.HasMinimalDifferenceWith(9.87654321E100 * Math.Pow(10, 5) * Math.Pow(10, -5)));
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(1, true)]
        [DataRow("a", false)]
        [DataRow(2147483648L, true)]
        [DataRow("2147483649", true)]
        [DataRow("0xdeadc0de", true)]
        [DataRow("-3.27e+50", true)]
        [DataRow(".1", true)]
        [DataRow(".-1", false)]
        [DataRow(null, false)]
        public void NumericFunctionsIsNumericTest(object value, bool isNumeric)
        {
            Assert.AreEqual(isNumeric, value.IsNumeric(), $"value '{value}'");
        }
    }
}
