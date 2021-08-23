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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SupportFunctions.Model;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions
{
    /// <summary>Fixture to compare two CSV tables. Exposes Table Table and Query Table interfaces</summary>
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Local", Justification = "FitSharp would not see it")]
    public class CsvComparison
    {
        private const string CellCaption = "Cell";
        private const string ColumnNameCaption = "Column Name";
        private const string ColumnNoCaption = "Column No";
        private const string CsvComparisonCaption = "CsvComparison";
        private const string DeltaCaption = "Delta";
        private const string DeltaPercentageCaption = "Delta %";
        private const string IssueCaption = "Issue";
        private const string RowNameCaption = "Row Name";
        private const string RowNoCaption = "Row No";
        private const string ValueCaption = "Value";

        internal static readonly Dictionary<string, Func<CellComparison, object>> GetTableValues =
            new Dictionary<string, Func<CellComparison, object>>
            {
                { CellCaption, result => CellReference(result.Row, result.Column).Report() },
                { RowNoCaption, result => RowReference(result.Row).Report() },
                { RowNameCaption, result => result.RowName.Report() },
                { ColumnNoCaption, result => ColumnReference(result.Column).Report() },
                { ColumnNameCaption, result => result.ColumnName.Report() },
                { ValueCaption, result => result.Cell.TableResult(result.Cell.ValueMessage) },
                { DeltaCaption, result => result.Cell.TableResult(result.Cell.DeltaMessage) },
                { DeltaPercentageCaption, result => result.Cell.TableResult(result.Cell.DeltaPercentageMessage) },
                { IssueCaption, result => result.Cell.TableResult(result.Cell.Outcome.ToString()) }
            };

        private readonly Tolerance _tolerance;
        private List<CellComparison> _result;

        /// <summary>Entry point for the FitNesse Query or Table Table</summary>
        /// <param name="expectedTable">The table being compared</param>
        /// <param name="actualTable">the table being compared to</param>
        /// <param name="tolerance">the tolerance applied. See <see cref="Tolerance.Parse" /></param>
        public CsvComparison(CsvTable expectedTable, CsvTable actualTable, Tolerance tolerance)
        {
            ExpectedTable = expectedTable;
            ActualTable = actualTable;
            _tolerance = tolerance;
            _result = null;
        }

        /// <summary>CSV table that the base table is compared to</summary>
        public CsvTable ActualTable { get; }

        /// <summary>Base CSV table used for the comparison</summary>
        public CsvTable ExpectedTable { get; }

        /// <summary>Execute the comparison and return the result</summary>
        /// <remarks>If the comparison was already done, returns the previous result</remarks>
        private List<CellComparison> Result
        {
            get
            {
                if (_result != null) return _result;
                // safety net, should only occur in certain unit tests
                if (ExpectedTable == null) return null;

                _result = HeaderErrors();

                var maxRows = Math.Max(ExpectedTable.RowCount, ActualTable.RowCount);
                for (var row = 0; row < maxRows; row++)
                {
                    _result.AddRange(RowErrors(row));
                }
                return _result;
            }
        }

        /// <returns>the cell reference in the way Excel refers to it, e.g. A1, AC39</returns>
        private static string CellReference(int rowNo, int columnNo) => columnNo.ToExcelColumnName() + RowReference(rowNo);

        /// <param name="columnNo">the column number (lower bound 0)</param>
        /// <returns>the Excel style column reference (e.g. 0=>A, 25=>Z, 27=>AB)</returns>
        private static string ColumnReference(int columnNo) => Invariant($"{columnNo + 1} ({columnNo.ToExcelColumnName()})");

        /// <summary>Compare the comparison result with another one</summary>
        /// <param name="otherComparison">the comparison to compare with</param>
        /// <returns>a list of differences between the two results</returns>
        internal IEnumerable<CellComparison> DeltaWith(CsvComparison otherComparison)
        {
            var comparer = new RowColumnComparer();
            var common = Result.Intersect(otherComparison.Result, comparer).ToList();
            var difference = Result.Except(common, comparer).Union(otherComparison.Result.Except(common, comparer));
            return difference;
        }

        /// <summary>The result of the comparison in a Table Table format</summary>
        public Collection<object> DoTable(Collection<Collection<object>> tableIn)
        {
            var renderer = new TableRenderer<CellComparison>(GetTableValues);
            return renderer.MakeTableTable(Result, tableIn);
        }


        /// <summary>The number of items with a comparison error</summary>
        public int ErrorCount() => Result.Count;

        /// <summary>The errors, in a hashtable</summary>
        public Dictionary<string, string> Errors()
        {
            var result = new Dictionary<string, string>();
            foreach (var entry in Result)
            {
                result.Add(CellReference(entry.Row, entry.Column) + " [" + entry.ColumnName + "/" + entry.RowName + "]",
                    entry.Cell.ValueMessage + " (" +
                    (string.IsNullOrEmpty(entry.Cell.DeltaMessage) ? "" : "Delta:" + entry.Cell.DeltaMessage + ", ") +
                    (string.IsNullOrEmpty(entry.Cell.DeltaPercentageMessage) ? "" : entry.Cell.DeltaPercentageMessage + ", ") +
                    entry.Cell.Outcome + ")");
            }
            return result;
        }

        /// <summary>Execute cell comparison for the header row</summary>
        /// <returns>the list of cell comparisons that didn't pass (empty list if all passed)</returns>
        private List<CellComparison> HeaderErrors()
        {
            const int headerRowNo = -1;
            var maxColumns = Math.Max(ExpectedTable.ColumnCount, ActualTable.ColumnCount);
            var result = new List<CellComparison>();
            var rowName = ExpectedTable.Header(0);
            for (var column = 0; column < maxColumns; column++)
            {
                var columnName = ExpectedTable.Header(column);
                var comparison = new CellComparison(headerRowNo, rowName, column, columnName, columnName, ActualTable.Header(column),
                    _tolerance);
                if (!comparison.Cell.IsOk()) result.Add(comparison);
            }
            return result;
        }

        internal static Collection<object> MakeQueryTable(IEnumerable<CellComparison> result)
        {
            var rows = new Collection<object>();
            foreach (var entry in result)
            {
                rows.Add(QueryRow(entry));
            }
            return rows;
        }


        /// <summary>We need this definition for FitNesse, but we don't need the actual value</summary>
        /// <param name="input">ignored</param>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "FitNesse requirement")]
        [SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "FitNesse requirement")]
        public static CsvComparison Parse(string input) => null;

        /// <returns>the errors of a CSV comparison in a Query Table format</returns>
        public Collection<object> Query() => MakeQueryTable(Result);

        private static Collection<object> QueryRow(CellComparison row) => new Collection<object>
        {
            new Collection<object> { CellCaption, CellReference(row.Row, row.Column) },
            new Collection<object> { RowNoCaption, RowReference(row.Row) },
            new Collection<object> { RowNameCaption, row.RowName },
            new Collection<object> { ColumnNoCaption, ColumnReference(row.Column) },
            new Collection<object> { ColumnNameCaption, row.ColumnName },
            new Collection<object> { ValueCaption, row.Cell.ValueMessage },
            new Collection<object> { DeltaCaption, row.Cell.DeltaMessage },
            new Collection<object> { DeltaPercentageCaption, row.Cell.DeltaPercentageMessage },
            new Collection<object> { IssueCaption, row.Cell.Outcome.ToString() }
        };

        /// <summary>Execute cell comparisons for a data row</summary>
        /// <param name="row">the index of the row to be compared</param>
        /// <returns>the list of cell comparisons that didn't pass (empty list if all passed)</returns>
        private List<CellComparison> RowErrors(int row)
        {
            var currentRow = row < ExpectedTable.RowCount ? ExpectedTable.DataCell(row, 0) : ActualTable.DataCell(row, 0);
            var result = new List<CellComparison>();
            var maxColumns = Math.Max(ExpectedTable.Row(row).Length, ActualTable.Row(row).Length);
            for (var column = 0; column < maxColumns; column++)
            {
                var currentColumn = ExpectedTable.Header(column);
                var expectedValue = ExpectedTable.DataCell(row, column);
                var actualValue = ActualTable.DataCell(row, column);
                // reset tolerance range to force recalculating per comparison
                _tolerance.DataRange = null;
                var comparison = new CellComparison(row, currentRow, column, currentColumn, expectedValue, actualValue, _tolerance);
                if (!comparison.Cell.IsOk()) result.Add(comparison);
            }
            return result;
        }

        /// <remarks>People used to Excel will expect counting to start at 1</remarks>
        /// <param name="rowNo">header is -1, internal data row counting starts at 0</param>
        private static string RowReference(int rowNo) => Invariant($"{rowNo + 2}");

        /// <returns>the caption of the CSV table</returns>
        public override string ToString() => CsvComparisonCaption;
    }
}
