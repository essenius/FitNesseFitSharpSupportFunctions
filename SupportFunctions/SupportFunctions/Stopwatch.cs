﻿// Copyright 2015-2019 Rik Essenius
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

namespace SupportFunctions
{
    public class Stopwatch
    {
        private readonly Dictionary<string, System.Diagnostics.Stopwatch> _stopwatchDictionary;

        public Stopwatch() => _stopwatchDictionary = new Dictionary<string, System.Diagnostics.Stopwatch>();

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Timing functions for rudimentary performance tests"},
            {nameof(ReadStopwatch), "Return the elapsed time in seconds of a stopwatch"},
            {nameof(ResetStopwatch), "Reset a stopwatch to 0"},
            {nameof(RestartStopwatch), "Reset a stopwatch to 0 and start counting"},
            {nameof(StartStopwatch), "Start (or continue) a stopwatch"},
            {nameof(StopStopwatch), "Stop a stopwatch and return elapsed time in seconds"}
        };

        private System.Diagnostics.Stopwatch GetStopwatch(string id)
        {
            System.Diagnostics.Stopwatch stopwatch;
            if (!_stopwatchDictionary.ContainsKey(id))
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                _stopwatchDictionary.Add(id, stopwatch);
            }
            else
            {
                stopwatch = _stopwatchDictionary[id];
            }
            return stopwatch;
        }

        public double ReadStopwatch(string id) => GetStopwatch(id).ElapsedMilliseconds / 1000.0;

        public void ResetStopwatch(string id) => GetStopwatch(id).Reset();

        public void RestartStopwatch(string id) => GetStopwatch(id).Restart();

        public void StartStopwatch(string id) => GetStopwatch(id).Start();

        public double StopStopwatch(string id)
        {
            GetStopwatch(id).Stop();
            return ReadStopwatch(id);
        }
    }
}