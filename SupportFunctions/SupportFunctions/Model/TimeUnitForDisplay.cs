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

using System.Collections.Generic;

namespace SupportFunctions.Model
{
    internal class TimeUnitForDisplay
    {
        private readonly Dictionary<TimeUnits, string> _captionDictionary = new Dictionary<TimeUnits, string>
        {
            {TimeUnits.Milliseconds, "ms"},
            {TimeUnits.Seconds, "s"},
            {TimeUnits.Minutes, "min"},
            {TimeUnits.Hours, "h"},
            {TimeUnits.Days, "d"}
        };

        private readonly Dictionary<TimeUnits, double> _conversionFactors = new Dictionary<TimeUnits, double>
        {
            {TimeUnits.Milliseconds, 0.001D},
            {TimeUnits.Seconds, 1D},
            {TimeUnits.Minutes, 60D},
            {TimeUnits.Hours, 3600D},
            {TimeUnits.Days, 24D * 3600D}
        };

        private readonly TimeUnits _timeUnit;

        public TimeUnitForDisplay(double range) => _timeUnit = TimeUnitFor(range);

        public string Caption => _captionDictionary[_timeUnit];

        public double ConvertFromSeconds(double secondValue) => secondValue / _conversionFactors[_timeUnit];

        public double ConvertToSeconds(double displayValue) => displayValue * _conversionFactors[_timeUnit];

        private static TimeUnits TimeUnitFor(double range)
        {
            if (range < 0.01) return TimeUnits.Milliseconds;
            if (range < 600D) return TimeUnits.Seconds;
            if (range < 12D * 3600D) return TimeUnits.Minutes;
            return range < 3D * 24D * 3600D ? TimeUnits.Hours : TimeUnits.Days;
        }

        private enum TimeUnits
        {
            Milliseconds,
            Seconds,
            Minutes,
            Hours,
            Days
        }
    }
}