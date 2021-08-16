// Copyright 2016-2021 Rik Essenius
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
using System.Drawing;
using System.Linq;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctions
{
    /// <summary>Compares two time series and reports results tabularly or graphically</summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "FitSharp entry point")]
    public class TimeSeriesComparison
    {
        private const string DeltaCaption = "Delta";
        private const string DeltaPercentageCaption = "Delta %";
        private const string IsGoodCaption = "Is Good";
        private const string IssueCaption = "Issue";
        private const string NoDataCaption = "no data";
        private const string TimeSeriesComparisonCaption = "Comparison";
        private const string TimestampCaption = "Timestamp";
        private const string ValueCaption = "Value";

        private static readonly Dictionary<string, Func<IMeasurementComparison, object>> GetTableValues =
            new Dictionary<string, Func<IMeasurementComparison, object>>
            {
                {TimestampCaption, result => result.Timestamp.TableResult(result.Timestamp.ValueMessage)},
                {ValueCaption, result => result.Value.TableResult(result.Value.ValueMessage)},
                {DeltaCaption, result => result.Value.TableResult(result.Value.DeltaMessage)},
                {DeltaPercentageCaption, result => result.Value.TableResult(result.Value.DeltaPercentageMessage)},
                {IsGoodCaption, result => result.IsGood.TableResult(result.IsGood.ValueMessage)},
                {IssueCaption, result => result.TableResult(result.OutcomeMessage)}
            };

        private readonly TimeSeries _actual;
        private readonly TimeSeries _expected;
        private readonly MeasurementComparisonDictionary _result;
        private readonly Tolerance _tolerance;
        private Date _baseTimestamp;
        private Type _compareType;
        private double _maxValue;
        private double _minValue;
        private double _timeSpanSeconds;

        /// <summary>Compares two time series and reports results tabularly or graphically</summary>
        public TimeSeriesComparison(TimeSeries expected, TimeSeries actual, Tolerance tolerance)
        {
            _expected = expected ?? new TimeSeries();
            _actual = actual ?? new TimeSeries();
            _tolerance = tolerance;
            _result = new MeasurementComparisonDictionary();
        }

        /// <summary>Initialize a new TimeSeriesComparison with expected and actual time series</summary>
        /// <param name="expected">Expected time series</param>
        /// <param name="actual">Actual time series</param>
        public TimeSeriesComparison(TimeSeries expected, TimeSeries actual) : this(expected, actual, Tolerance.Parse(""))
        {
        }

        /// <summary>Timestamp of the first data point</summary>
        public Date BaseTimestamp => DoOperation(comparison => comparison._baseTimestamp);

        /// <summary>the number of failures in the comparison</summary>
        public long FailureCount => DoOperation(comparison => comparison._result.Values.Count(result => !result.IsOk()));

        /// <summary>maximal value of the time series if numerical, 0 if not numerical</summary>
        public double MaxValue => DoOperation(comparison => comparison._maxValue);

        /// <summary>minimal value of the time series if numerical, 0 if not numerical</summary>
        public double MinValue => DoOperation(comparison => comparison._minValue);

        /// <summary>The number of data points in the comparison</summary>
        public long PointCount => DoOperation(comparison => comparison._result.Count);

        private MeasurementComparisonDictionary Result
        {
            get
            {
                RunComparison();
                return _result;
            }
        }

        /// <summary>Time span in seconds between last and first data point</summary>
        public double TimeSpanSeconds => DoOperation(comparison => comparison._timeSpanSeconds);

        /// <summary>The tolerance that was used in the comparison</summary>
        public string UsedTolerance => DoOperation(comparison => comparison._tolerance.ToString());

        private void CompareExpectedToActuals(IReadOnlyDictionary<DateTime, Measurement> actualDictionary)
        {
            foreach (var expectedEntry in _expected.Measurements)
            {
                actualDictionary.TryGetValue(expectedEntry.Timestamp, out var actual);
                var comparison = new MeasurementComparison(expectedEntry, actual, _tolerance, _compareType);
                if (actual != null) actual.IsChecked = true;
                _result.AddWithCheck(expectedEntry.Timestamp, comparison, "expected");
            }
        }

        private string CreateGraph(Dictionary<string, string> rawParameters)
        {
            if (_result.Count == 0) return NoDataCaption;
            if (!_compareType.IsNumeric()) return "no numeric comparison";

            var parameters = new TimeSeriesGraphParameters(rawParameters);
            var startTimestamp = parameters.StartTimestamp;
            var endTimestamp = parameters.EndTimestamp;
            var recalculateNeeded = startTimestamp != null || endTimestamp != null;
            var minValue = GetValueWithDefault(parameters.MinValue, recalculateNeeded, MinValue);
            var maxValue = GetValueWithDefault(parameters.MaxValue, recalculateNeeded, MaxValue);
            var dataSubset = recalculateNeeded ? _result.Subset(startTimestamp, endTimestamp) : _result;
            if (dataSubset.Count == 0) return NoDataCaption;
            var yDimension = Dimension.GetExtremeValues(dataSubset.Values, minValue, maxValue);
            startTimestamp ??= dataSubset.Keys.First();
            endTimestamp ??= dataSubset.Keys.Last();

            var chart = new TimeSeriesChart();
            return chart.ChartInHtmlFor(dataSubset,
                new AxisLimits(startTimestamp.Value, endTimestamp.Value, yDimension),
                new Size(parameters.Width, parameters.Height));
        }

        private Collection<object> CreateQueryResult()
        {
            var rows = new Collection<object>();
            foreach (var entry in _result.Where(kvp => !kvp.Value.IsOk()))
            {
                var comparison = entry.Value;
                // string.Empty is to hide null values
                var cells = new Collection<object>
                {
                    new Collection<string> {TimestampCaption, comparison.Timestamp.ActualValueOut},
                    new Collection<string> {ValueCaption, string.Empty + comparison.Value.ValueMessage},
                    new Collection<string> {DeltaCaption, string.Empty + comparison.Value.DeltaMessage},
                    new Collection<string> {DeltaPercentageCaption, string.Empty + comparison.Value.DeltaPercentageMessage},
                    new Collection<string> {IsGoodCaption, string.Empty + comparison.IsGood.ValueMessage},
                    new Collection<string> {IssueCaption, comparison.OutcomeMessage}
                };
                rows.Add(cells);
            }
            return rows;
        }

        private static Dictionary<DateTime, Measurement> DictionaryFrom(IEnumerable<Measurement> measurements, string tag)
        {
            var dictionary = new Dictionary<DateTime, Measurement>();
            foreach (var measurement in measurements)
            {
                measurement.IsChecked = false;
                dictionary.AddWithCheck(measurement.Timestamp, measurement, tag);
            }
            return dictionary;
        }

        // most functions visible to FitSharp require the comparison to be run first, 
        // but we don't want to execute the comparison in the constructor. This method provides 
        // a wrapper around the comparison execution so the property/method code becomes simpler.
        private T DoOperation<T>(Func<TimeSeriesComparison, T> operation)
        {
            if (_result.Count == 0) RunComparison();
            return operation(this);
        }

        /// <summary>Table interface, providing full details about the comparison</summary>
        public Collection<object> DoTable(Collection<Collection<object>> tableIn)
        {
            var renderer = new TableRenderer<IMeasurementComparison>(GetTableValues);
            var resultTable = renderer.MakeTableTable(Result.Values, tableIn);
            return resultTable;
        }

        private static double? GetValueWithDefault(double? parameterInput, bool useNull, double fallback) =>
            parameterInput ?? (useNull ? (double?) null : fallback);

        /// <summary>html img element with an in-line base-64 html image containing a chart of the comparison</summary>
        public string Graph() => Graph(new Dictionary<string, string>());

        /// <summary>Graph with optional parameters: Width, Height, StartTimestamp, EndTimestamp, MinValue, MaxValue</summary>
        public string Graph(Dictionary<string, string> rawParameters) => DoOperation(comparison => comparison.CreateGraph(rawParameters));

        /// <summary>Shorthand for Graph with a certain width and height; other parameters default</summary>
        public string GraphX(int width, int height)
        {
            var dict = new Dictionary<string, string>
            {
                {"width", width.To<string>()},
                {"height", height.To<string>()}
            };
            return Graph(dict);
        }

        private void MarkUncoveredActualsAsSurplus(Dictionary<DateTime, Measurement> actualDictionary)
        {
            foreach (var actual in actualDictionary.Values.Where(s => !s.IsChecked))
            {
                _result.AddWithCheck(actual.Timestamp, new MeasurementComparison(null, actual, _tolerance), "surplus");
            }
        }

        private static double Max(double value1, double value2)
        {
            if (double.IsNaN(value1)) return value2;
            return double.IsNaN(value2) ? value1 : Math.Max(value1, value2);
        }

        private static double Min(double value1, double value2)
        {
            if (double.IsNaN(value1)) return value2;
            return double.IsNaN(value2) ? value1 : Math.Min(value1, value2);
        }

        /// <summary>Query interface, returning the comparison failures</summary>
        public Collection<object> Query() => DoOperation(comparison => comparison.CreateQueryResult());

        /// <summary>Execute a comparison. Normally done implicitly</summary>
        public void RunComparison()
        {
            // Load both time series if needed
            _result.Clear();
            _expected.Load();
            _actual.Load();

            if (_expected.Measurements.Count + _actual.Measurements.Count == 0) return;

            var actualDictionary = DictionaryFrom(_actual.Measurements, "actual");
            var metaDataExpected = new TimeSeriesMetadata<Measurement>(_expected.Measurements, point => point.Value);
            var metaDataActual = new TimeSeriesMetadata<Measurement>(_actual.Measurements, point => point.Value);
            _compareType = metaDataExpected.DataType ?? metaDataActual.DataType;

            // These comparisons ignore NaN values unless they are both NaN
            _minValue = Min(metaDataExpected.MinValue, metaDataActual.MinValue);
            _maxValue = Max(metaDataExpected.MaxValue, metaDataActual.MaxValue);

            // range is 0 if the extremes are NaN
            _tolerance.DataRange = metaDataExpected.Range;

            SetTimeSpanSeconds();

            CompareExpectedToActuals(actualDictionary);
            MarkUncoveredActualsAsSurplus(actualDictionary);
        }

        private void SetTimeSpanSeconds()
        {
            var baseSeries = _expected.Measurements.Count == 0 ? _actual : _expected;
            _baseTimestamp = new Date(baseSeries.Measurements.First().Timestamp);
            _timeSpanSeconds = (baseSeries.Measurements.Last().Timestamp - _baseTimestamp.DateTime).TotalSeconds;
        }

        ///<returns>a small but meaningful caption to not make FitNesse columns too large</returns>
        public override string ToString() => TimeSeriesComparisonCaption;
    }
}
