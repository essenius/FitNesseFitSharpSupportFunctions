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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SupportFunctions.Model;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions
{
    /// <summary>Compare two CSV tables</summary>
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
                {CellCaption, result => CellReference(result.Row, result.Column).Report()},
                {RowNoCaption, result => RowReference(result.Row).Report()},
                {RowNameCaption, result => result.RowName.Report()},
                {ColumnNoCaption, result => ColumnReference(result.Column).Report()},
                {ColumnNameCaption, result => result.ColumnName.Report()},
                {ValueCaption, result => result.Cell.TableResult(result.Cell.ValueMessage)},
                {DeltaCaption, result => result.Cell.TableResult(result.Cell.DeltaMessage)},
                {DeltaPercentageCaption, result => result.Cell.TableResult(result.Cell.DeltaPercentageMessage)},
                {IssueCaption, result => result.Cell.TableResult(result.Cell.Outcome.ToString())}
            };

        private readonly CsvTable _baseTable;
        private readonly CsvTable _comparedTable;
        private readonly Tolerance _tolerance;
        private List<CellComparison> _result;

        /// <summary>CSV comparison</summary>
        /// <param name="baseTable">The table being compared</param>
        /// <param name="comparedTable">the table being compared to</param>
        /// <param name="tolerance">the tolerance applied. See <see cref="Tolerance.Parse"/></param>
        public CsvComparison(CsvTable baseTable, CsvTable comparedTable, Tolerance tolerance)
        {
            _baseTable = baseTable;
            _comparedTable = comparedTable;
            _tolerance = tolerance;
            _result = null;
        }

        private List<CellComparison> Result
        {
            get
            {
                if (_result != null) return _result;
                // safety net, should only occur in certain unit tests
                if (_baseTable == null) return null;

                const int headerRowNo = -1;

                //Data contains a list of rows, not a list of columns
                var maxRows = Math.Max(_baseTable.RowCount, _comparedTable.RowCount);
                var maxColumns = Math.Max(_baseTable.ColumnCount, _comparedTable.ColumnCount);
                _result = new List<CellComparison>();
                var rowName = _baseTable.Header(0);
                for (var column = 0; column < maxColumns; column++)
                {
                    var columnName = _baseTable.Header(column);
                    var comparison = new CellComparison(headerRowNo, rowName, column, columnName, columnName, _comparedTable.Header(column),
                        _tolerance);
                    if (!comparison.Cell.IsOk()) _result.Add(comparison);
                }

                for (var row = 0; row < maxRows; row++)
                {
                    var currentRow = _baseTable.DataCell(row, 0);
                    for (var column = 0; column < _baseTable.Data[row].Length; column++)
                    {
                        var currentColumn = _baseTable.Header(column);
                        var expectedValue = _baseTable.DataCell(row, column);
                        var actualValue = _comparedTable.DataCell(row, column);
                        // reset tolerance range to force recalculating per comparison
                        _tolerance.DataRange = null;
                        var comparison = new CellComparison(row, currentRow, column, currentColumn, expectedValue, actualValue, _tolerance);
                        if (!comparison.Cell.IsOk()) _result.Add(comparison);
                    }
                }
                return _result;
            }
        }

        // the way Excel refers to individual cells, e.g. A1, AC39
        private static string CellReference(int rowNo, int columnNo) => columnNo.ToExcelColumnName() + RowReference(rowNo);

        private static string ColumnReference(int columnNo) => Invariant($"{columnNo + 1} ({columnNo.ToExcelColumnName()})");

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
        /// <returns></returns>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "FitNesse requirement"),
         SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "FitNesse requirement")]
        public static CsvComparison Parse(string input) => null;

        /// <returns>the errors of a CSV comparison in a Query Table format</returns>
        public Collection<object> Query() => MakeQueryTable(Result);

        private static Collection<object> QueryRow(CellComparison row) => new Collection<object>
        {
            new Collection<object> {CellCaption, CellReference(row.Row, row.Column)},
            new Collection<object> {RowNoCaption, RowReference(row.Row)},
            new Collection<object> {RowNameCaption, row.RowName},
            new Collection<object> {ColumnNoCaption, ColumnReference(row.Column)},
            new Collection<object> {ColumnNameCaption, row.ColumnName},
            new Collection<object> {ValueCaption, row.Cell.ValueMessage},
            new Collection<object> {DeltaCaption, row.Cell.DeltaMessage},
            new Collection<object> {DeltaPercentageCaption, row.Cell.DeltaPercentageMessage},
            new Collection<object> {IssueCaption, row.Cell.Outcome.ToString()}
        };

        // header is -1, internal data row counting starts at 0. 
        // People used to Excel will expect counting to start at 1

        /// <remarks>People used to Excel will expect counting to start at 1</remarks>
        /// <param name="rowNo">header is -1, internal data row counting starts at 0</param>
        private static string RowReference(int rowNo) => Invariant($"{rowNo + 2}");

        /// <returns>the caption of the CSV table</returns>
        public override string ToString() => CsvComparisonCaption;
    }
}