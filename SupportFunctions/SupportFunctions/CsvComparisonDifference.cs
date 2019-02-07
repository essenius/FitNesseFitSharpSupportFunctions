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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SupportFunctions.Model;

namespace SupportFunctions
{
    public class CsvComparisonDifference
    {
        private readonly CsvComparison _set1;
        private readonly CsvComparison _set2;
        private IEnumerable<CellComparison> _result;

        public CsvComparisonDifference(CsvComparison set1, CsvComparison set2)
        {
            _set1 = set1;
            _set2 = set2;
        }

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Comparing the difference between two comparisons."},
            {nameof(DoTable), "The result of the comparison difference in a Table Table format"},
            {nameof(ErrorCount), "The number of items with a different comparison error"},
            {nameof(Query), "Return the different errors between two comparisons in a Query Table format"}
        };

        public int ErrorCount => Result.Count();

        private IEnumerable<CellComparison> Result => _result ?? (_result = _set1.DeltaWith(_set2));

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "FitNesse API spec")]
        public Collection<object> DoTable(Collection<Collection<object>> tableIn)
        {
            var renderer = new TableRenderer<CellComparison>(CsvComparison.GetTableValues);
            return renderer.MakeTableTable(Result, tableIn);
        }

        public Collection<object> Query() => CsvComparison.MakeQueryTable(Result);

        public override string ToString() => "CsvComparisonDifference";
    }
}