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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    [TestClass]
    public class TableRendererTest
    {
        private static readonly Dictionary<string, Func<Point, object>> GetTableValues =
            new Dictionary<string, Func<Point, object>>
            {
                { "X", result => result.X },
                { "Y", result => result.Y }
            };

        [TestMethod]
        [TestCategory("Unit")]
        public void TableRendererTest1()
        {
            var pointList = new List<Point>
            {
                new Point(1, 2),
                new Point(3, 4)
            };

            var tableIn = new Collection<Collection<object>>
            {
                new Collection<object> { "y", "x" }
            };

            var renderer = new TableRenderer<Point>(GetTableValues);
            var resultTable = renderer.MakeTableTable(pointList, tableIn);
            Assert.AreEqual(3, resultTable.Count);
            var header = resultTable[0] as Collection<object>;
            Assert.IsNotNull(header);
            Assert.AreEqual("report:Y", header[0]);
            Assert.AreEqual("report:X", header[1]);
            var row1 = resultTable[1] as Collection<object>;
            Assert.IsNotNull(row1);
            Assert.AreEqual(2, row1[0]);
            Assert.AreEqual(1, row1[1]);
        }
    }
}
