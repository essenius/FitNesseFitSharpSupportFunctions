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
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;

namespace SupportFunctionsTest
{
    [TestClass]
    public class CsvTableTest
    {
        [TestMethod, TestCategory("Integration"), DeploymentItem("TestData\\String01_TestData.csv")]
        public void CsvTableLoadQueryTableTest()
        {
            var timeSeries = CsvTable.Parse("String01_TestData.csv");
            Assert.AreEqual(7, timeSeries.ColumnCount, "ColumnCount OK");
            var collection = timeSeries.Query();
            Assert.AreEqual(62, collection.Count, "Query column count");
            var header = collection[0] as Collection<object>;
            Assert.IsNotNull(header, "Query Header not null ");
            Assert.AreEqual(7, header.Count, "Query header count");
            Assert.AreEqual("Hi7,Hi8", timeSeries.Data[60][1], "Item with comma got across OK");

            var tableCollection = timeSeries.DoTable(null);
            Assert.AreEqual(63, tableCollection.Count);
            Assert.AreEqual(7, tableCollection[0].Count, "Table column count");
            Assert.AreEqual("report:Timestamp", tableCollection[0][0], "Table Timestamp header");
            Assert.AreEqual("report:Hi8", tableCollection[62][3], "Table last row column 3");

            Assert.AreEqual(2, timeSeries.HeaderIndex("ExpectedQuality"), "HeaderIndex existing");
            try
            {
                timeSeries.HeaderIndex("Nonexisting");
                Assert.Fail("No exception raised for non-existing HeaderIndex");
            }
            catch (ArgumentException)
            {
            }

            var tempFileName = Path.GetTempFileName();
            timeSeries.SaveTo(tempFileName);
            var timeSeries2 = CsvTable.Parse(tempFileName);
            Assert.AreEqual(7, timeSeries2.ColumnCount, "Number of columns in saved and loaded CSV");
            Assert.AreEqual("Hi7,Hi8", timeSeries2.Data[60][1], "Item with comma got back OK");
            Assert.AreEqual(62, timeSeries2.Data.Count, "Number of rows in saved and loaded CSV");
            File.Delete(tempFileName);
        }
    }
}
