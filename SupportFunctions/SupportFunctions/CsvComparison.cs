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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SupportFunctions.Model;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions
{
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class CsvComparison
    {
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
                {RowNoCaption, result => DisplayRow(result.Row).Report()},
                {RowNameCaption, result => result.RowName.Report()},
                {ColumnNoCaption, result => DisplayColumn(result.Column).Report()},
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

        public CsvComparison(CsvTable baseTable, CsvTable comparedTable, Tolerance tolerance)
        {
            _baseTable = baseTable;
            _comparedTable = comparedTable;
            _tolerance = tolerance;
            _result = null;
        }

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Comparing two CSVs"},
            {nameof(DoTable), "The result of the comparison in a Table Table format"},
            {nameof(DeltaWith), "Inernal use only"},
            {nameof(ErrorCount), "The number of items with a comparison error"},
            {nameof(MakeQueryTable), "Inernal use only"},
            {nameof(Parse), "Enables assigning a FitNesse symbol to parameters of this type"},
            {nameof(Query), "Return the errors of a CSV comparison in a Query Table format"}
        };

        private List<CellComparison> Result
        {
            get
            {
                RunComparison();
                return _result;
            }
        }

        private static string DisplayColumn(int columnNo) => Invariant($"{columnNo + 1} ({columnNo.ToExcelColumnName()})");

        // header is -1, internal data row counting starts at 0. 
        // People used to Excel will expect counting to start at 1
        private static string DisplayRow(int rowNo) => Invariant($"{rowNo + 2}");

        internal static Collection<object> MakeQueryTable(IEnumerable<CellComparison> result)
        {
            var rows = new Collection<object>();
            foreach (var entry in result)
            {
                rows.Add(QueryRow(entry));
            }
            return rows;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "input", Justification = "FitNesse requirement")]
        // We need this method to be able to assign a FitNesse symbol to parameters of this type.
        public static CsvComparison Parse(string input) => null;

        private static Collection<object> QueryRow(CellComparison row) => new Collection<object>
        {
            new Collection<object> {RowNoCaption, DisplayRow(row.Row)},
            new Collection<object> {RowNameCaption, row.RowName},
            new Collection<object> {ColumnNoCaption, DisplayColumn(row.Column)},
            new Collection<object> {ColumnNameCaption, row.ColumnName},
            new Collection<object> {ValueCaption, row.Cell.ValueMessage},
            new Collection<object> {DeltaCaption, row.Cell.DeltaMessage},
            new Collection<object> {DeltaPercentageCaption, row.Cell.DeltaPercentageMessage},
            new Collection<object> {IssueCaption, row.Cell.Outcome.ToString()}
        };

        internal IEnumerable<CellComparison> DeltaWith(CsvComparison otherComparison)
        {
            var comparer = new RowColumnComparer();
            var common = Result.Intersect(otherComparison.Result, comparer).ToList();
            var difference = Result.Except(common, comparer).Union(otherComparison.Result.Except(common, comparer));
            return difference;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Usage",
             "CA1801:ReviewUnusedParameters", MessageId = "tableIn",
             Justification = "FitNesse API spec")]
        public Collection<object> DoTable(Collection<Collection<object>> tableIn)
        {
            var renderer = new TableRenderer<CellComparison>(GetTableValues);
            return renderer.MakeTableTable(Result, tableIn);
        }

        public int ErrorCount() => Result.Count;

        public Collection<object> Query() => MakeQueryTable(Result);

        private void RunComparison()
        {
            if (_result != null)
            {
                return;
            }

            // safety net, should only occur in certain unit tests
            if (_baseTable == null)
            {
                return;
            }

            const int headerRowNo = -1;

            //Data contains a list of rows, not a list of columns
            var maxRows = Math.Max(_baseTable.RowCount, _comparedTable.RowCount);
            var maxColumns = Math.Max(_baseTable.ColumnCount, _comparedTable.ColumnCount);
            _result = new List<CellComparison>();
            var rowName = _baseTable.Header(0);
            for (var column = 0; column < maxColumns; column++)
            {
                var columnName = _baseTable.Header(column);
                var comparison = new CellComparison(headerRowNo, rowName, column, columnName, columnName,
                    _comparedTable.Header(column), _tolerance);
                if (!comparison.Cell.IsOk())
                {
                    _result.Add(comparison);
                }
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
                    var comparison = new CellComparison(row, currentRow, column, currentColumn, expectedValue,
                        actualValue, _tolerance);
                    if (!comparison.Cell.IsOk())
                    {
                        _result.Add(comparison);
                    }
                }
            }
        }

        public override string ToString() => CsvComparisonCaption;

        private class RowColumnComparer : IEqualityComparer<CellComparison>
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
}