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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions.Model
{
    internal class TableRenderer<T>
    {
        private readonly Collection<string> _allHeaders;
        private readonly Dictionary<string, Func<T, object>> _getField;
        private Collection<string> _desiredColumns;

        public TableRenderer(Dictionary<string, Func<T, object>> getField)
        {
            _allHeaders = new Collection<string>();
            foreach (var entry in getField.Keys)
            {
                _allHeaders.Add(entry);
            }
            _getField = getField;
        }

        internal Collection<object> MakeTableTable(IEnumerable<T> input, Collection<Collection<object>> tableIn)
        {
            // FitNesse presents an empty table (no null). Checking for null just to be safe. Useful in unit tests too.
            _desiredColumns = tableIn == null || tableIn.Count == 0
                ? _allHeaders
                : new Collection<string>(tableIn[0].Select(s => s.ToString()).ToList());
            ValidateAndAlignHeaders();
            var returnValue = new Collection<object> {TableHeader()};

            foreach (var entry in input)
            {
                returnValue.Add(TableRow(entry));
            }
            return returnValue;
        }

        private Collection<object> TableHeader()
        {
            var returnValue = new Collection<object>();
            foreach (var header in _desiredColumns)
            {
                returnValue.Add(header.Report());
            }
            return returnValue;
        }

        private Collection<object> TableRow(T resultEntry)
        {
            var returnValue = new Collection<object>();
            foreach (var column in _desiredColumns)
            {
                returnValue.Add(_getField[column](resultEntry));
            }
            return returnValue;
        }

        private void ValidateAndAlignHeaders()
        {
            if (_desiredColumns.Equals(_allHeaders)) return;
            for (var i = 0; i < _desiredColumns.Count; i++)
            {
                if (!_allHeaders.Contains(_desiredColumns[i], StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        Invariant($"{_desiredColumns[i]}: No such header. Recognised values: {string.Join(", ", _allHeaders)}."));
                }
                // align the casing of the header to the specification
                _desiredColumns[i] = _getField.First(x => string.Equals(x.Key, _desiredColumns[i], StringComparison.OrdinalIgnoreCase)).Key;
            }
        }
    }
}