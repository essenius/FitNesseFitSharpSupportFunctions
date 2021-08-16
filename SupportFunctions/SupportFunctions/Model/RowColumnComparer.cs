// Copyright 2017-2021 Rik Essenius
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
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class RowColumnComparer : IEqualityComparer<CellComparison>
    {
        public bool Equals(CellComparison x, CellComparison y)
        {
            Requires.NotNull(x, nameof(x));
            Requires.NotNull(y, nameof(y));
            return x.Row == y.Row && x.Column == y.Column;
        }

        public int GetHashCode(CellComparison x) => ((x.Row.GetHashCode() << 5) + x.Row.GetHashCode()) ^ x.Column.GetHashCode();
    }
}
