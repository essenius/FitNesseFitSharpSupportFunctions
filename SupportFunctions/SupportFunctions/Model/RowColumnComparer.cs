using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class RowColumnComparer : IEqualityComparer<CellComparison>
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Handled by Requires.NotNull")]
        public bool Equals(CellComparison x, CellComparison y)
        {
            Requires.NotNull(x, nameof(x));
            Requires.NotNull(y, nameof(y));
            return x.Row == y.Row && x.Column == y.Column;
        }

        public int GetHashCode(CellComparison x) => ((x.Row.GetHashCode() << 5) + x.Row.GetHashCode()) ^ x.Column.GetHashCode();
    }
}
