﻿// Copyright 2017-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

namespace SupportFunctions.Model
{
    internal class CellComparison
    {
        public CellComparison(int row, string rowName, int column, string columName, object expected, object actual,
            Tolerance tolerance = null)
        {
            Row = row;
            RowName = rowName;
            Column = column;
            ColumnName = columName;
            Cell = new ValueComparison(expected, actual, tolerance);
        }

        public ValueComparison Cell { get; }

        public int Column { get; }
        public string ColumnName { get; }
        public int Row { get; }
        public string RowName { get; }
    }
}