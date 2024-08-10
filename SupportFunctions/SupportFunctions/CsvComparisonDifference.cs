// Copyright 2017-2024 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SupportFunctions.Model;

namespace SupportFunctions
{
    /// <summary>Comparing the difference between two comparisons.</summary>
    public class CsvComparisonDifference
    {
        private readonly CsvComparison _set1;
        private readonly CsvComparison _set2;
        private IEnumerable<CellComparison> _result;

        /// <summary>Initialize a CSV Comparison Difference</summary>
        /// <param name="set1">first set to compare</param>
        /// <param name="set2">second set to compare</param>
        public CsvComparisonDifference(CsvComparison set1, CsvComparison set2)
        {
            _set1 = set1;
            _set2 = set2;
        }

        /// <summary>The number of items with a different comparison error</summary>
        public int ErrorCount => Result.Count();

        private IEnumerable<CellComparison> Result => _result ??= _set1.DeltaWith(_set2);

        /// <summary>The result of the comparison difference in a Table Table format</summary>
        public Collection<object> DoTable(Collection<Collection<object>> tableIn)
        {
            var renderer = new TableRenderer<CellComparison>(CsvComparison.GetTableValues);
            return renderer.MakeTableTable(Result, tableIn);
        }

        /// <returns>the different errors between two comparisons in a Query Table format</returns>
        public Collection<object> Query() => CsvComparison.MakeQueryTable(Result);

        /// <returns>CsvComparisonDifference</returns>
        public override string ToString() => "CsvComparisonDifference";
    }
}
