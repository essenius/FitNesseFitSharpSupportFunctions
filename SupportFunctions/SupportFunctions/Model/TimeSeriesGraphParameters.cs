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
using System.Linq;
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class TimeSeriesGraphParameters
    {
        private const int DefaultHeight = 600;
        private const int DefaultWidth = 800;
        private const string EndTimestampKey = "ENDTIMESTAMP";
        private const string HeightKey = "HEIGHT";
        private const string MaxValueKey = "MAXVALUE";
        private const string MinValueKey = "MINVALUE";
        private const string StartTimestampKey = "STARTTIMESTAMP";
        private const string WidthKey = "WIDTH";

        private readonly Dictionary<string, string> _parameters;

        public TimeSeriesGraphParameters(Dictionary<string, string> rawParameters)
        {
            _parameters = rawParameters
                .Select(
                    x => new KeyValuePair<string, string>(x.Key.Replace(" ", string.Empty).ToUpperInvariant(), x.Value))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public DateTime? EndTimestamp => _parameters.ValueOrDefault<DateTime?>(EndTimestampKey, null);

        public int Height => _parameters.ValueOrDefault(HeightKey, DefaultHeight);

        public double? MaxValue => _parameters.ValueOrDefault<double?>(MaxValueKey, null);

        public double? MinValue => _parameters.ValueOrDefault<double?>(MinValueKey, null);

        public DateTime? StartTimestamp => _parameters.ValueOrDefault<DateTime?>(StartTimestampKey, null);

        public int Width => _parameters.ValueOrDefault(WidthKey, DefaultWidth);
    }
}