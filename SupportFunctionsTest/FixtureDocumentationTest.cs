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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class FixtureDocumentationTest
    {
        [TestMethod, TestCategory("Unit")]
        public void FixtureCaptionExists()
        {
            Assert.AreEqual("CsvComparison", new CsvComparison(null, null, null).ToString());
            Assert.AreEqual("CsvComparisonDifference", new CsvComparisonDifference(null, null).ToString());
            Assert.AreEqual("CsvTable", new CsvTable().ToString());
            Assert.AreEqual("TimeSeries", new TimeSeries().ToString());
            Assert.AreEqual("Comparison", new TimeSeriesComparison(null, null).ToString());
        }

        [TestMethod, TestCategory("Unit")]
        public void FixtureDocumentationExists()
        {
            Assert.IsNotNull(CommonFunctions.FixtureDocumentation);
            Assert.IsNotNull(CsvComparison.FixtureDocumentation);
            Assert.IsNotNull(CsvComparisonDifference.FixtureDocumentation);
            Assert.IsNotNull(CsvTable.FixtureDocumentation);
            Assert.IsNotNull(Date.FixtureDocumentation);
            Assert.IsNotNull(DictionaryLibrary.FixtureDocumentation);
#pragma warning disable 618
            Assert.IsNotNull(EchoSupport.FixtureDocumentation);
#pragma warning restore 618
            Assert.IsNotNull(MachineInfo.FixtureDocumentation);
            Assert.IsNotNull(Stopwatch.FixtureDocumentation);
            Assert.IsNotNull(TestControl.FixtureDocumentation);
            Assert.IsNotNull(TimeSeries.FixtureDocumentation);
            Assert.IsNotNull(TimeSeriesComparison.FixtureDocumentation);
            Assert.IsNotNull(Tolerance.FixtureDocumentation);
            Assert.IsNotNull(UserInfo.FixtureDocumentation);
        }
    }
}