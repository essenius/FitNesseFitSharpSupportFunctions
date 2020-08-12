// Copyright 2017-2020 Rik Essenius
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
        [TestMethod, TestCategory("Unit")]
        public void NumericFunctionsFractionalDigitsTest()
        {
            Assert.AreEqual(4, 0.1234.FractionalDigits());
            Assert.AreEqual(0, "test".FractionalDigits());
            Assert.AreEqual(0, 1000.FractionalDigits());
            Assert.AreEqual(0, 1.234E+5.FractionalDigits());
            Assert.AreEqual(7, 1.234E-4.FractionalDigits());
            Assert.AreEqual(10, 1.234E-7.FractionalDigits());
            Assert.AreEqual(73, 1.234E-70.FractionalDigits());
            Assert.AreEqual(0, 1.234567E+70.FractionalDigits());
        }

        [TestMethod, TestCategory("Unit")]
        public void NumericFunctionsHasMinimalDifferenceWithTest()
        {
            Assert.IsTrue((0.1 + 0.2).HasMinimalDifferenceWith(0.3), "0.1 + 0.2 = 0.3");
            Assert.IsTrue((-0D).HasMinimalDifferenceWith(+0D), "-0 = +0");
            Assert.IsFalse(1.0.HasMinimalDifferenceWith(-1.0));
            const double value1 = .1 * 10;
            var value2 = 0D;
            for (var ctr = 0; ctr < 10; ctr++)
            {
                value2 += .1;
            }
            Assert.IsTrue(value1.HasMinimalDifferenceWith(value2));
            Assert.IsTrue(9.87654321E100.HasMinimalDifferenceWith(9.87654321E100 * Math.Pow(10, 5) * Math.Pow(10, -5)));
            Assert.IsFalse(0.3333333333.HasMinimalDifferenceWith(0.33333333333));
        }

        [TestMethod, TestCategory("Unit")]
        public void NumericFunctionsIsNumericTest()
        {
            object target = 1;
            Assert.IsTrue(target.IsNumeric());
            target = "a";
            Assert.IsFalse(target.IsNumeric());
            target = 1L;
            Assert.IsTrue(target.IsNumeric());
            target = 0x0f;
            Assert.IsTrue(target.IsNumeric());
            target = null;
            Assert.IsFalse(target.IsNumeric());
        }
    }
}
