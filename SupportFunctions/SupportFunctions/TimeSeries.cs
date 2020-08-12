// Copyright 2016-2020 Rik Essenius
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
using SupportFunctions.Utilities;

namespace SupportFunctions
{
    /// <summary>Time series assignable to a symbol</summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Used by FitSharp")]
    public class TimeSeries
    {
        /// <summary>Initialize a new TimeSeries</summary>
        public TimeSeries() => Measurements = new Collection<Measurement>();

        /// <summary>Initialize a new TimeSeries with specific path and column names</summary>
        /// <param name="input">string formatted Path#TimestampColumn#ValueColumn#IsGoodColumn</param>
        public TimeSeries(string input) : this()
        {
            if (string.IsNullOrEmpty(input)) return;
            var inputParts = input.Split('#');
            Path = inputParts[0].Trim();
            TimestampColumn = GetColumn(inputParts, 1, TimestampColumn);
            ValueColumn = GetColumn(inputParts, 2, ValueColumn);
            IsGoodColumn = GetColumn(inputParts, 3, IsGoodColumn);
        }

        /// <summary>The file name of the loaded CSV file</summary>
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(Path)) return null;
                var filePart = Path.Split('#')[0];
                return System.IO.Path.GetFileName(filePart);
            }
        }

        /// <summary>Name of the Is Good column in the CSV file</summary>
        public string IsGoodColumn { get; set; } = "isgood";

        internal Collection<Measurement> Measurements { get; }

        /// <summary>Path in use for this data set. Can be empty if the data is constructed via a Decision table</summary>
        public string Path { get; set; }

        /// <summary>Name of the Timestamp column in the CSV file</summary>
        public string TimestampColumn { get; set; } = "timestamp";

        /// <summary>Name of the Value column in the CSV file</summary>
        public string ValueColumn { get; set; } = "value";

        internal void AddMeasurement(Measurement measurement) => Measurements.Add(measurement);

        private static string GetColumn(IReadOnlyList<string> input, int index, string defaultValue)
        {
            if (input.Count <= index) return defaultValue;
            var content = input[index].Trim();
            return string.IsNullOrEmpty(content) ? defaultValue : content;
        }

        internal void Load()
        {
            // the series can already be pre-populated; then Path is null
            if (string.IsNullOrEmpty(Path)) return;
            var csvTable = new CsvTable();
            csvTable.LoadFrom(Path);
            var timestampIndex = csvTable.HeaderIndex(TimestampColumn);
            var valueIndex = csvTable.HeaderIndex(ValueColumn);
            var qualityIndex = csvTable.HeaderIndex(IsGoodColumn);

            Measurements.Clear();
            foreach (var entry in csvTable.Data.Where(t => !(string.IsNullOrEmpty(t[valueIndex]) && string.IsNullOrEmpty(t[qualityIndex]))))
            {
                var measurement = new Measurement(entry[timestampIndex], entry[valueIndex], entry[qualityIndex]);
                Measurements.Add(measurement);
            }
        }

        /// <summary>Create a new time series object from a CSV file.</summary>
        /// <param name="input">
        ///     format: fileName#TimestampColumn#ValueColumn#IsGoodColumn#.
        ///     If column names are omitted, default name Timestamp, Value and IsGood are taken
        /// </param>
        public static TimeSeries Parse(string input) => new TimeSeries(input);

        // keep under the hat for now.
        internal void SaveTo(string path)
        {
            var csvTable = new CsvTable(new[] {"Timestamp", "Value", "IsGood"});
            foreach (var measurement in Measurements)
            {
                csvTable.Data.Add(new[]
                    {measurement.Timestamp.ToRoundTripFormat(), measurement.Value, measurement.IsGood.To<string>()});
            }
            csvTable.SaveTo(path);
        }

        /// <summary>If there is a path, return that, else return "TimeSeries"</summary>
        public override string ToString() => string.IsNullOrEmpty(Path) ? "TimeSeries" : FileName;

        #region Decision Table interface

        /// <summary>Used by Decision Table</summary>
        public void BeginTable() => Measurements.Clear();

        /// <summary>Decision table column</summary>
        public bool IsGood { set; get; }

        /// <summary>Decision table column</summary>
        public Date Timestamp { set; get; }

        /// <summary>Decision table column</summary>
        public string Value { set; get; }

        /// <summary>Used by Decision Table</summary>
        public void Execute()
        {
            var measurement = new Measurement(Timestamp, Value, IsGood);
            Measurements.Add(measurement);
        }

        /// <summary>Used by Decision Table</summary>
        public void Reset() => IsGood = true;

        #endregion
    }
}
