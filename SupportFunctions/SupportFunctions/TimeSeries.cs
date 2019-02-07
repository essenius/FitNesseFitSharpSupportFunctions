﻿// Copyright 2016-2019 Rik Essenius
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
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Used by FitSharp")]
    public class TimeSeries
    {
        public TimeSeries() => Measurements = new Collection<Measurement>();

        public TimeSeries(string input) : this()
        {
            if (string.IsNullOrEmpty(input))
            {
                return;
            }
            var inputParts = input.Split('#');
            Path = inputParts[0].Trim();
            TimestampColumn = GetColumn(inputParts, 1, TimestampColumn);
            ValueColumn = GetColumn(inputParts, 2, ValueColumn);
            IsGoodColumn = GetColumn(inputParts, 3, IsGoodColumn);
        }

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Time series assignable to a symbol"},
            {nameof(BeginTable), "Used by Decision Table"},
            {nameof(Execute), "Used by Decision Table"},
            {nameof(FileName), "The file name of the loaded CSV file"},
            {nameof(IsGood), "Decision table column"},
            {nameof(IsGoodColumn), "Name of the Is Good column in the CSV file"},
            {
                nameof(Parse),
                "Create a new time series object from a CSV file. Input format: fileName#TimestampColumn#VaueColumn#IsGoodColumn#. If column names are omitted, default names Timestamp, Value and IsGood are taken"
            },
            {
                nameof(Path), "Path in use for this data set. Can be empty if the data is constructed via a Decision table"
            },
            {nameof(Timestamp), "Decision table column"},
            {nameof(TimestampColumn), "Name of the Timestamp column in the CSV file"},
            {nameof(Value), "Decision table column"},
            {nameof(ValueColumn), "Name of the Value column in the CSV file"}
        };

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(Path))
                {
                    return null;
                }
                var filePart = Path.Split('#')[0];
                return System.IO.Path.GetFileName(filePart);
            }
        }

        public string IsGoodColumn { get; set; } = "isgood";
        internal Collection<Measurement> Measurements { get; }
        public string Path { get; set; }
        public string TimestampColumn { get; set; } = "timestamp";
        public string ValueColumn { get; set; } = "value";

        private static string GetColumn(IReadOnlyList<string> input, int index, string defaultValue)
        {
            if (input.Count <= index)
            {
                return defaultValue;
            }
            var content = input[index].Trim();
            return string.IsNullOrEmpty(content) ? defaultValue : content;
        }

        public static TimeSeries Parse(string input) => new TimeSeries(input);

        internal void AddMeasurement(Measurement measurement) => Measurements.Add(measurement);

        internal void Load()
        {
            // the series can already be pre-populated; then _path is null
            if (string.IsNullOrEmpty(Path))
            {
                return;
            }
            var csvTable = new CsvTable();
            csvTable.LoadFrom(Path);
            var timestampIndex = csvTable.HeaderIndex(TimestampColumn);
            var valueIndex = csvTable.HeaderIndex(ValueColumn);
            var qualityIndex = csvTable.HeaderIndex(IsGoodColumn);

            Measurements.Clear();
            foreach (var entry in csvTable.Data
                .Where(t => !(string.IsNullOrEmpty(t[valueIndex]) && string.IsNullOrEmpty(t[qualityIndex]))))
            {
                var measurement = new Measurement(entry[timestampIndex], entry[valueIndex], entry[qualityIndex]);
                Measurements.Add(measurement);
            }
        }

        // keep under the hat for now.
        internal void SaveTo(string path)
        {
            var csvTable = new CsvTable(new[] {"Timestamp", "Value", "IsGood"});
            foreach (var measurement in Measurements)
            {
                csvTable.Data.Add(new[]
                    {measurement.Timestamp.ToRoundTripFormat(), measurement.Value, measurement.IsGood.ToString()});
            }
            csvTable.SaveTo(path);
        }

        // Make FitNesse objects show something meaningful
        public override string ToString() => string.IsNullOrEmpty(Path) ? "TimeSeries" : FileName;

        #region Decision Table interface

        public void BeginTable() => Measurements.Clear();

        public bool IsGood { set; get; }
        public Date Timestamp { set; get; }
        public string Value { set; get; }

        public void Execute()
        {
            var measurement = new Measurement(Timestamp, Value, IsGood);
            Measurements.Add(measurement);
        }

        public void Reset()
        {
            IsGood = true;
        }

        #endregion
    }
}