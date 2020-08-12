using System.Collections.Generic;
using System.Diagnostics;

namespace SupportFunctions.Model
{
    internal class RowColumnComparer : IEqualityComparer<CellComparison>
    {
        public bool Equals(CellComparison x, CellComparison y)
        {
            Debug.Assert(x != null, nameof(x) + " != null");
            Debug.Assert(y != null, nameof(y) + " != null");
            return x.Row == y.Row && x.Column == y.Column;
        }

        public int GetHashCode(CellComparison x) => ((x.Row.GetHashCode() << 5) + x.Row.GetHashCode()) ^ x.Column.GetHashCode();
    }
}
