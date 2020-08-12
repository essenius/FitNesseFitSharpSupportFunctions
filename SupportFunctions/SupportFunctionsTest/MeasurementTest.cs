// Copyright 2016-2020 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class MeasurementTest
    {
        [TestMethod, TestCategory("Unit")]
        public void MeasurementValueTypeTest()
        {
            var measurement = new Measurement {Value = "12.0"};
            Assert.AreEqual(typeof(double), measurement.Value.InferType());
            measurement = new Measurement {Value = "12"};
            Assert.AreEqual(typeof(int), measurement.Value.InferType());
            measurement = new Measurement {Value = long.MaxValue.ToString()};
            Assert.AreEqual(typeof(long), measurement.Value.InferType());
            measurement = new Measurement {Value = "1a"};
            Assert.AreEqual(typeof(string), measurement.Value.InferType());
            measurement = new Measurement {Value = "true"};
            Assert.AreEqual(typeof(bool), measurement.Value.InferType());
        }
    }
}
